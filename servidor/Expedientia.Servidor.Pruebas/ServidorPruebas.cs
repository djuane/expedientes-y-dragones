using System.Net;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Expedientia.Servidor.Pruebas;

public class Fabrica : WebApplicationFactory<Program>
{
    public static readonly string Carpeta = Path.Combine(Path.GetTempPath(), "eyd-preguntas-" + Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        Directory.CreateDirectory(Carpeta);
        File.WriteAllText(Path.Combine(Carpeta, "mp-900.json"), """
            {"id":"mp-900","materia":"Procesal civil","calabozo":"Mesa de Partes","pregunta":"Pregunta de prueba",
             "opciones":["A","B","C","D"],"indice_correcto":2,"base_legal":"Norma de prueba","enlace_oficial":"",
             "verificado_por":"","fecha_verificacion":"","estado":"borrador"}
            """);
        builder.ConfigureAppConfiguration((_, c) => c.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["Juego:PermitirBorradores"] = "true",
            ["Juego:CarpetaPreguntas"] = Carpeta,
            ["Juego:ClavePanel"] = "clave-de-prueba",
            ["ConnectionStrings:Juego"] = Environment.GetEnvironmentVariable("EYD_PRUEBAS_POSTGRES") ?? "",
        }));
    }
}

public class ServidorPruebas : IClassFixture<Fabrica>
{
    private readonly Fabrica _f;
    public ServidorPruebas(Fabrica f) { _f = f; }

    private async Task<(HttpClient http, string token)> Jugador(string alias = "Dra. Prueba")
    {
        var http = _f.CreateClient();
        var r = await http.PostAsJsonAsync("/api/jugadores", new { alias });
        r.EnsureSuccessStatusCode();
        var j = await r.Content.ReadFromJsonAsync<JsonObject>();
        var token = j!["token"]!.GetValue<string>();
        http.DefaultRequestHeaders.Add("X-Token", token);
        return (http, token);
    }

    private static async Task<JsonObject> Accion(HttpClient http, string id, object accion, bool exigirValida = true)
    {
        var r = await http.PostAsJsonAsync($"/api/partidas/{id}/acciones", accion);
        var cuerpo = (await r.Content.ReadFromJsonAsync<JsonObject>())!;
        if (exigirValida) Assert.True(r.IsSuccessStatusCode, cuerpo.ToJsonString());
        return cuerpo;
    }

    [Fact]
    public async Task Salud_responde()
    {
        var r = await _f.CreateClient().GetFromJsonAsync<JsonObject>("/salud");
        Assert.Equal("ok", r!["estado"]!.GetValue<string>());
    }

    [Fact]
    public async Task Sin_token_no_se_crea_partida()
    {
        var r = await _f.CreateClient().PostAsJsonAsync("/api/partidas", new { caso_id = "escrito-fantasma", especialidad = "Civilista" });
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Lista_los_casos_del_prototipo()
    {
        var casos = await _f.CreateClient().GetFromJsonAsync<JsonArray>("/api/casos");
        Assert.Equal(3, casos!.Count);
    }

    [Fact]
    public async Task Partida_completa_por_la_api_con_piloto_automatico()
    {
        var (http, _) = await Jugador();
        var r = await http.PostAsJsonAsync("/api/partidas", new { caso_id = "escrito-fantasma", especialidad = "Civilista", semilla = 11, bots = 1 });
        var inicio = (await r.Content.ReadFromJsonAsync<JsonObject>())!;
        Assert.True(r.IsSuccessStatusCode, inicio.ToJsonString());
        var estado = inicio["estado"]!.AsObject();
        var id = estado["id"]!.GetValue<string>();

        var pasos = 0;
        while (estado["sentencia"] == null && pasos++ < 1000)
        {
            var sala = estado["sala_actual"]!.GetValue<string>();
            var resuelta = estado["sala_resuelta"]!.GetValue<bool>();
            object accion = resuelta ? new { tipo = "avanzar" } : sala switch
            {
                "Combate" or "Jefe" or "RedDelCoimero" => new { tipo = "piloto" },
                "Tramite" => new { tipo = "tramite" },
                "Audiencia" => new { tipo = "audiencia", opcion = "revisar" },
                "Cafeteria" => new { tipo = "descansar" },
                "Libreria" => new { tipo = "salirTienda" },
                "Coimero" => new { tipo = "coimero", decision = "rechazar" },
                _ => throw new Exception(sala),
            };
            estado = (await Accion(http, id, accion))["estado"]!.AsObject();
        }
        Assert.NotNull(estado["sentencia"]);

        var yo = await http.GetFromJsonAsync<JsonObject>("/api/yo");
        Assert.Single(yo!["resultados"]!.AsArray());
        Assert.True(yo["estudio"]!["experiencia"]!.GetValue<int>() > 0);
    }

    [Fact]
    public async Task Prueba_juridica_da_ventaja_y_muestra_la_base_legal()
    {
        var (http, _) = await Jugador();
        var inicio = (await (await http.PostAsJsonAsync("/api/partidas", new { caso_id = "escrito-fantasma", especialidad = "Civilista", semilla = 5 })).Content.ReadFromJsonAsync<JsonObject>())!;
        var id = inicio["estado"]!["id"]!.GetValue<string>();
        var pedida = await Accion(http, id, new { tipo = "pedirPregunta" });
        var pregunta = pedida["extra"]!["pregunta"]!;
        Assert.Equal("Pregunta de prueba", pregunta["texto"]!.GetValue<string>());
        Assert.True(pregunta["borrador"]!.GetValue<bool>());
        Assert.Null(pregunta["indice_correcto"]); // la respuesta no viaja al cliente
        var resp = await Accion(http, id, new { tipo = "responder", indice = 2 });
        Assert.Equal("Acierto", resp["extra"]!["correccion"]!["resultado"]!.GetValue<string>());
        Assert.Equal("Norma de prueba", resp["extra"]!["correccion"]!["base_legal"]!.GetValue<string>());
        Assert.Equal(2, resp["estado"]!["pruebas_juridicas_restantes"]!.GetValue<int>());
    }

    [Fact]
    public async Task No_se_juega_fuera_de_turno_ni_en_partida_ajena()
    {
        var (http, _) = await Jugador("Uno");
        var (otro, _) = await Jugador("Dos");
        var inicio = (await (await http.PostAsJsonAsync("/api/partidas", new { caso_id = "escrito-fantasma", especialidad = "Administrativista", semilla = 3 })).Content.ReadFromJsonAsync<JsonObject>())!;
        var id = inicio["estado"]!["id"]!.GetValue<string>();
        var ajena = await Accion(otro, id, new { tipo = "piloto" }, exigirValida: false);
        Assert.False(ajena["valida"]!.GetValue<bool>());
        var mal = await Accion(http, id, new { tipo = "mover", columna = 99, fila = 99 }, exigirValida: false);
        Assert.False(mal["valida"]!.GetValue<bool>());
    }

    [Fact]
    public async Task Segundo_jugador_se_une_y_reemplaza_al_bot()
    {
        var (http, _) = await Jugador("Anfitrión");
        var (otro, _) = await Jugador("Invitada");
        var inicio = (await (await http.PostAsJsonAsync("/api/partidas", new { caso_id = "escrito-fantasma", especialidad = "Civilista", semilla = 8 })).Content.ReadFromJsonAsync<JsonObject>())!;
        var id = inicio["estado"]!["id"]!.GetValue<string>();
        var r = await otro.PostAsJsonAsync($"/api/partidas/{id}/unirse", new { especialidad = "Administrativista" });
        var estado = (await r.Content.ReadFromJsonAsync<JsonObject>())!;
        Assert.True(r.IsSuccessStatusCode, estado.ToJsonString());
        Assert.All(estado["equipo"]!.AsArray(), m => Assert.False(m!["es_bot"]!.GetValue<bool>()));
    }

    [Fact]
    public async Task WebSocket_recibe_el_estado_y_ejecuta_acciones()
    {
        var (http, token) = await Jugador();
        var inicio = (await (await http.PostAsJsonAsync("/api/partidas", new { caso_id = "escrito-fantasma", especialidad = "Civilista", semilla = 21 })).Content.ReadFromJsonAsync<JsonObject>())!;
        var id = inicio["estado"]!["id"]!.GetValue<string>();
        var ws = _f.Server.CreateWebSocketClient();
        using var socket = await ws.ConnectAsync(new Uri(_f.Server.BaseAddress, $"ws/partidas/{id}?token={token}"), CancellationToken.None);
        var primero = await Leer(socket);
        Assert.Equal(id, primero["estado"]!["id"]!.GetValue<string>());
        await socket.SendAsync(Encoding.UTF8.GetBytes("""{"tipo":"piloto"}"""), WebSocketMessageType.Text, true, CancellationToken.None);
        var segundo = await Leer(socket);
        Assert.True(segundo["estado"]!["version"]!.GetValue<int>() > primero["estado"]!["version"]!.GetValue<int>());
    }

    private static async Task<JsonObject> Leer(WebSocket s)
    {
        var buffer = new byte[256 * 1024];
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var total = 0;
        WebSocketReceiveResult r;
        do { r = await s.ReceiveAsync(buffer.AsMemory(total).ToArray() is var _ ? new ArraySegment<byte>(buffer, total, buffer.Length - total) : default, cts.Token); total += r.Count; }
        while (!r.EndOfMessage);
        return JsonNode.Parse(Encoding.UTF8.GetString(buffer, 0, total))!.AsObject();
    }

    [Fact]
    public async Task Panel_exige_clave_y_no_publica_sin_revisor()
    {
        var http = _f.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        Assert.Equal(HttpStatusCode.Unauthorized, (await http.GetAsync("/panel/")).StatusCode);
        http.DefaultRequestHeaders.Authorization = new("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes("panel:clave-de-prueba")));
        Assert.Equal(HttpStatusCode.OK, (await http.GetAsync("/panel/")).StatusCode);
        var form = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["id"] = "mp-901", ["materia"] = "Procesal civil", ["calabozo"] = "Mesa de Partes", ["pregunta"] = "¿?",
            ["opcion0"] = "a", ["opcion1"] = "b", ["opcion2"] = "c", ["opcion3"] = "d", ["correcta"] = "1", ["estado"] = "verificada",
        });
        var r = await http.PostAsync("/panel/editar", form);
        Assert.Contains("abogado revisor", await r.Content.ReadAsStringAsync());
    }
}
