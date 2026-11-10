using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Expedientia.Logica.Expedientes;
using Expedientia.Servidor.Datos;
using Expedientia.Servidor.Panel;
using Expedientia.Servidor.Partidas;

var builder = WebApplication.CreateBuilder(args);

// Las opciones se leen al resolver los servicios, para respetar toda la configuración (archivo, entorno y pruebas).
builder.Services.AddSingleton(sp =>
{
    var c = sp.GetRequiredService<IConfiguration>();
    return new OpcionesJuego
    {
        PermitirBorradores = c.GetValue("Juego:PermitirBorradores", false),
        CarpetaPreguntas = c["Juego:CarpetaPreguntas"],
        ClavePanel = c["Juego:ClavePanel"],
    };
});
builder.Services.AddSingleton<IAlmacen>(sp =>
{
    var cadena = sp.GetRequiredService<IConfiguration>().GetConnectionString("Juego");
    return string.IsNullOrWhiteSpace(cadena) ? new AlmacenMemoria() : new AlmacenPostgres(cadena);
});
builder.Services.AddSingleton<GestorPartidas>();
builder.Services.AddHostedService<RelojDeTurnos>();
builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = Json.Opciones.PropertyNamingPolicy;
    foreach (var c in Json.Opciones.Converters) o.SerializerOptions.Converters.Add(c);
});

var app = builder.Build();

var almacen = app.Services.GetRequiredService<IAlmacen>();
var opciones = app.Services.GetRequiredService<OpcionesJuego>();
await almacen.PrepararAsync();
var cargadas = await CargaDeContenido.CargarPreguntasAsync(almacen, opciones.CarpetaPreguntas, app.Logger);
app.Logger.LogInformation("Almacén: {Tipo}. Preguntas nuevas cargadas: {N}. Borradores permitidos: {B}",
    almacen.GetType().Name, cargadas, opciones.PermitirBorradores);

app.UseWebSockets(new WebSocketOptions { KeepAliveInterval = TimeSpan.FromSeconds(20) });

async Task<Jugador?> Autenticar(HttpContext ctx)
{
    var token = ctx.Request.Headers["X-Token"].FirstOrDefault() ?? ctx.Request.Query["token"].FirstOrDefault();
    return string.IsNullOrWhiteSpace(token) ? null : await almacen.BuscarPorTokenAsync(token);
}

app.MapGet("/", () => Results.Text("Expedientes y Dragones: servidor de partidas", "text/plain; charset=utf-8"));
app.MapGet("/salud", () => Results.Ok(new { estado = "ok", version = typeof(Program).Assembly.GetName().Version?.ToString(), almacen = almacen.GetType().Name }));

app.MapGet("/api/casos", () => CatalogoCasos.Todos.Select(c => new { c.Id, c.Numero, c.Titulo, c.Premisa, c.Calabozo, c.Jefe, c.MinutosDePlazo, c.EnPrototipo }));

app.MapPost("/api/jugadores", async (CrearJugadorPedido p) =>
{
    var alias = (p.Alias ?? "").Trim();
    if (alias.Length is < 2 or > 30) return Results.BadRequest(new { motivo = "El alias debe tener de 2 a 30 caracteres." });
    var j = await almacen.CreateJugadorSeguro(alias);
    return Results.Ok(new { jugadorId = j.Id, j.Alias, j.Token });
});

app.MapGet("/api/yo", async (HttpContext ctx) =>
{
    var j = await Autenticar(ctx);
    if (j == null) return Results.Unauthorized();
    var e = await almacen.LeerEstudioAsync(j.Id);
    var est = EstadoEstudio.Restaurar(e.Honorarios, e.Experiencia, e.Reputacion, e.Disciplinario, e.Cerrados);
    return Results.Ok(new
    {
        jugadorId = j.Id, j.Alias,
        estudio = new { est.Honorarios, est.Experiencia, est.Nivel, est.Reputacion, est.ExpedientesCerrados },
        resultados = await almacen.ResultadosAsync(j.Id),
    });
});

app.MapPost("/api/partidas", async (HttpContext ctx, CrearPartidaPedido p, GestorPartidas gestor) =>
{
    var j = await Autenticar(ctx);
    if (j == null) return Results.Unauthorized();
    var (sesion, inicio, error) = await gestor.CrearAsync(j, p);
    return sesion == null ? Results.BadRequest(new { motivo = error }) : Results.Ok(inicio);
});

app.MapGet("/api/partidas/{id:guid}", async (HttpContext ctx, Guid id, GestorPartidas gestor) =>
{
    var j = await Autenticar(ctx);
    if (j == null) return Results.Unauthorized();
    var s = gestor.Buscar(id);
    return s == null ? Results.NotFound() : Results.Ok(s.Estado());
});

app.MapPost("/api/partidas/{id:guid}/unirse", async (HttpContext ctx, Guid id, UnirsePedido p, GestorPartidas gestor) =>
{
    var j = await Autenticar(ctx);
    if (j == null) return Results.Unauthorized();
    var s = gestor.Buscar(id);
    if (s == null) return Results.NotFound();
    var (ok, motivo) = await s.UnirseAsync(j.Id, p.Especialidad);
    return ok ? Results.Ok(s.Estado()) : Results.BadRequest(new { motivo });
});

app.MapPost("/api/partidas/{id:guid}/acciones", async (HttpContext ctx, Guid id, AccionPedido a, GestorPartidas gestor) =>
{
    var j = await Autenticar(ctx);
    if (j == null) return Results.Unauthorized();
    var s = gestor.Buscar(id);
    if (s == null) return Results.NotFound();
    var r = await s.EjecutarAsync(j.Id, a);
    return r.Valida ? Results.Ok(r) : Results.BadRequest(r);
});

// WebSocket: el cliente envía las mismas acciones en JSON y recibe cada cambio de la partida.
app.Map("/ws/partidas/{id:guid}", async (HttpContext ctx, Guid id, GestorPartidas gestor) =>
{
    if (!ctx.WebSockets.IsWebSocketRequest) { ctx.Response.StatusCode = 400; return; }
    var j = await Autenticar(ctx);
    var s = gestor.Buscar(id);
    if (j == null || s == null || !s.Participa(j.Id)) { ctx.Response.StatusCode = 403; return; }
    using var ws = await ctx.WebSockets.AcceptWebSocketAsync();
    s.Conectar(ws);
    try
    {
        var inicial = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new RespuestaAccion(true, null, Array.Empty<EventoDto>(), s.Estado()), Json.Opciones));
        await ws.SendAsync(inicial, WebSocketMessageType.Text, true, ctx.RequestAborted);
        var buffer = new byte[16 * 1024];
        while (ws.State == WebSocketState.Open)
        {
            var recibido = await ws.ReceiveAsync(buffer, ctx.RequestAborted);
            if (recibido.MessageType == WebSocketMessageType.Close) break;
            AccionPedido? accion = null;
            try { accion = JsonSerializer.Deserialize<AccionPedido>(buffer.AsSpan(0, recibido.Count), Json.Opciones); } catch (JsonException) { }
            if (accion == null) continue;
            var r = await s.EjecutarAsync(j.Id, accion);
            // La respuesta propia (con la pregunta o su corrección) solo va a quien actuó; el resto recibe la difusión.
            if (!r.Valida || r.Extra != null)
                await ws.SendAsync(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(r, Json.Opciones)), WebSocketMessageType.Text, true, ctx.RequestAborted);
        }
    }
    catch (OperationCanceledException) { }
    catch (WebSocketException) { }
    finally { s.Desconectar(ws); }
});

PanelContenido.Mapear(app, opciones);

app.Run();

public partial class Program { }

internal static class AlmacenExtensiones
{
    public static Task<Jugador> CreateJugadorSeguro(this IAlmacen a, string alias) =>
        a.CrearJugadorAsync(System.Net.WebUtility.HtmlEncode(alias));
}
