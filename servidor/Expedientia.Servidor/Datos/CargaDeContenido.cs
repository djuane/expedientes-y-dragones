using System.Text.Json;

namespace Expedientia.Servidor.Datos;

/// <summary>Carga las fichas JSON de contenido/preguntas al almacén (solo las que aún no existen).</summary>
public static class CargaDeContenido
{
    public static async Task<int> CargarPreguntasAsync(IAlmacen almacen, string? carpeta, ILogger log)
    {
        if (string.IsNullOrWhiteSpace(carpeta) || !Directory.Exists(carpeta)) return 0;
        var nuevas = 0;
        foreach (var archivo in Directory.GetFiles(carpeta, "*.json").Where(f => !Path.GetFileName(f).StartsWith("plantilla")))
        {
            try
            {
                var p = JsonSerializer.Deserialize<PreguntaGuardada>(await File.ReadAllTextAsync(archivo), Json.Opciones);
                if (p == null || string.IsNullOrWhiteSpace(p.Id) || await almacen.PreguntaAsync(p.Id) != null) continue;
                await almacen.GuardarPreguntaAsync(p);
                nuevas++;
            }
            catch (Exception ex) { log.LogWarning(ex, "No se pudo leer {Archivo}", archivo); }
        }
        return nuevas;
    }
}
