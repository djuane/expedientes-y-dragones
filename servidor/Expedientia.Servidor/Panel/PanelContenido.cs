using System.Net;
using System.Text;
using Expedientia.Servidor.Datos;
using Expedientia.Servidor.Partidas;

namespace Expedientia.Servidor.Panel;

/// <summary>
/// Panel de contenido: redactar, verificar y publicar preguntas sin actualizar la app.
/// Protegido con usuario "panel" y la clave Juego:ClavePanel (sin clave, el panel queda apagado).
/// </summary>
public static class PanelContenido
{
    private static readonly string[] Estados = { "borrador", "verificada", "desactualizada" };

    public static void Mapear(WebApplication app, OpcionesJuego opciones)
    {
        var grupo = app.MapGroup("/panel").AddEndpointFilter(async (ctx, next) =>
        {
            if (string.IsNullOrWhiteSpace(opciones.ClavePanel)) return Results.NotFound();
            if (!Autorizado(ctx.HttpContext, opciones.ClavePanel))
            {
                ctx.HttpContext.Response.Headers.WWWAuthenticate = "Basic realm=\"Panel\", charset=\"UTF-8\"";
                return Results.Unauthorized();
            }
            return await next(ctx);
        });

        grupo.MapGet("/", async (IAlmacen almacen) =>
        {
            var preguntas = await almacen.PreguntasAsync();
            var sb = new StringBuilder();
            sb.Append("<h1>Banco de preguntas</h1><p><a href='/panel/editar'>Nueva pregunta</a></p>");
            sb.Append($"<p>{preguntas.Count(p => p.Estado == "verificada")} verificadas de {preguntas.Count}.</p>");
            sb.Append("<table><tr><th>Id</th><th>Calabozo</th><th>Pregunta</th><th>Estado</th><th>Verificó</th></tr>");
            foreach (var p in preguntas)
                sb.Append($"<tr class='{E(p.Estado)}'><td><a href='/panel/editar?id={Uri.EscapeDataString(p.Id)}'>{E(p.Id)}</a></td><td>{E(p.Calabozo)}</td><td>{E(p.Pregunta)}</td><td>{E(p.Estado)}</td><td>{E(p.VerificadoPor)} {E(p.FechaVerificacion)}</td></tr>");
            sb.Append("</table>");
            return Html("Panel de contenido", sb.ToString());
        });

        grupo.MapGet("/editar", async (string? id, IAlmacen almacen) =>
        {
            var p = id == null ? new PreguntaGuardada { Calabozo = "Mesa de Partes", Opciones = new() { "", "", "", "" } } : await almacen.PreguntaAsync(id);
            if (p == null) return Results.NotFound();
            while (p.Opciones.Count < 4) p.Opciones.Add("");
            return Html("Editar pregunta", Formulario(p, null));
        });

        grupo.MapPost("/editar", async (HttpRequest req, IAlmacen almacen) =>
        {
            var f = await req.ReadFormAsync();
            var p = new PreguntaGuardada
            {
                Id = f["id"].ToString().Trim(),
                Materia = f["materia"].ToString().Trim(),
                Calabozo = f["calabozo"].ToString().Trim(),
                Pregunta = f["pregunta"].ToString().Trim(),
                Opciones = Enumerable.Range(0, 4).Select(i => f[$"opcion{i}"].ToString().Trim()).ToList(),
                IndiceCorrecto = int.TryParse(f["correcta"], out var c) ? c : 0,
                Explicacion = f["explicacion"].ToString().Trim(),
                BaseLegal = f["base_legal"].ToString().Trim(),
                EnlaceOficial = f["enlace_oficial"].ToString().Trim(),
                VerificadoPor = f["verificado_por"].ToString().Trim(),
                FechaVerificacion = f["fecha_verificacion"].ToString().Trim(),
                Estado = Estados.Contains(f["estado"].ToString()) ? f["estado"].ToString() : "borrador",
            };
            var error = Validar(p);
            if (error != null) return Html("Editar pregunta", Formulario(p, error));
            await almacen.GuardarPreguntaAsync(p);
            return Results.Redirect("/panel/");
        });
    }

    /// <summary>Regla del GDD: ninguna pregunta se publica sin revisor, fecha y enlace oficial.</summary>
    public static string? Validar(PreguntaGuardada p)
    {
        if (string.IsNullOrWhiteSpace(p.Id) || p.Id.Any(ch => !(char.IsLetterOrDigit(ch) || ch == '-'))) return "El id solo admite letras, números y guiones.";
        if (string.IsNullOrWhiteSpace(p.Pregunta)) return "Falta el texto de la pregunta.";
        if (p.Opciones.Count != 4 || p.Opciones.Any(string.IsNullOrWhiteSpace)) return "Se necesitan 4 opciones.";
        if (p.IndiceCorrecto is < 0 or > 3) return "Respuesta correcta inválida.";
        if (p.Estado == "verificada")
        {
            if (string.IsNullOrWhiteSpace(p.VerificadoPor)) return "Una pregunta verificada necesita el nombre del abogado revisor.";
            if (!DateTime.TryParse(p.FechaVerificacion, out _)) return "Una pregunta verificada necesita la fecha de verificación (AAAA-MM-DD).";
            if (!Uri.TryCreate(p.EnlaceOficial, UriKind.Absolute, out var u) || u.Scheme != "https") return "Una pregunta verificada necesita el enlace oficial (https).";
            if (string.IsNullOrWhiteSpace(p.BaseLegal)) return "Una pregunta verificada necesita su base legal.";
        }
        return null;
    }

    private static bool Autorizado(HttpContext ctx, string clave)
    {
        var h = ctx.Request.Headers.Authorization.ToString();
        if (!h.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase)) return false;
        try
        {
            var partes = Encoding.UTF8.GetString(Convert.FromBase64String(h[6..])).Split(':', 2);
            return partes.Length == 2 && partes[0] == "panel" &&
                   System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(partes[1]), Encoding.UTF8.GetBytes(clave));
        }
        catch (FormatException) { return false; }
    }

    private static string Formulario(PreguntaGuardada p, string? error)
    {
        string Campo(string nombre, string etiqueta, string valor) => $"<label>{etiqueta}<input name='{nombre}' value='{E(valor)}'></label>";
        var sb = new StringBuilder("<h1>Pregunta</h1>");
        if (error != null) sb.Append($"<p class='error'>{E(error)}</p>");
        sb.Append("<form method='post'>");
        sb.Append(Campo("id", "Id (ej. mp-001)", p.Id));
        sb.Append(Campo("materia", "Materia", p.Materia));
        sb.Append(Campo("calabozo", "Calabozo", p.Calabozo));
        sb.Append($"<label>Pregunta<textarea name='pregunta'>{E(p.Pregunta)}</textarea></label>");
        for (var i = 0; i < 4; i++)
            sb.Append($"<label><input type='radio' name='correcta' value='{i}' {(p.IndiceCorrecto == i ? "checked" : "")}> Opción {(char)('A' + i)} (marcar si es la correcta)<input name='opcion{i}' value='{E(p.Opciones[i])}'></label>");
        sb.Append($"<label>Explicación (2 líneas)<textarea name='explicacion'>{E(p.Explicacion)}</textarea></label>");
        sb.Append(Campo("base_legal", "Base legal (norma, artículo)", p.BaseLegal));
        sb.Append(Campo("enlace_oficial", "Enlace oficial (SPIJ, El Peruano, tribunal)", p.EnlaceOficial));
        sb.Append(Campo("verificado_por", "Verificado por (abogado)", p.VerificadoPor));
        sb.Append(Campo("fecha_verificacion", "Fecha de verificación (AAAA-MM-DD)", p.FechaVerificacion));
        sb.Append("<label>Estado<select name='estado'>");
        foreach (var e in Estados) sb.Append($"<option {(p.Estado == e ? "selected" : "")}>{e}</option>");
        sb.Append("</select></label><button>Guardar</button> <a href='/panel/'>Volver</a></form>");
        return sb.ToString();
    }

    private static string E(string? s) => WebUtility.HtmlEncode(s ?? "");

    private static IResult Html(string titulo, string cuerpo) => Results.Content($$"""
        <!doctype html><html lang="es"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1">
        <title>{{titulo}}</title><style>
        body{font-family:system-ui,sans-serif;max-width:960px;margin:0 auto;padding:16px;background:#EDE3CC;color:#0E0C1A}
        h1{color:#0E0C1A} a{color:#C8442F} table{border-collapse:collapse;width:100%} td,th{border-bottom:1px solid #c9bc9c;padding:6px;text-align:left;vertical-align:top}
        tr.borrador td{background:#fff3b0} tr.desactualizada td{color:#888;text-decoration:line-through}
        label{display:block;margin:10px 0} input,textarea,select{display:block;width:100%;padding:6px;box-sizing:border-box} textarea{min-height:70px}
        input[type=radio]{display:inline;width:auto} button{background:#0E0C1A;color:#E2B04A;border:0;padding:10px 18px;margin-top:10px}
        .error{background:#C8442F;color:#fff;padding:8px}
        </style></head><body>{{cuerpo}}</body></html>
        """, "text/html; charset=utf-8");
}
