namespace Expedientia.Servidor.Datos;

public sealed record Jugador(Guid Id, string Alias, string Token, DateTime Creado);

public sealed record EstudioGuardado(Guid JugadorId, int Honorarios, int Experiencia, int Reputacion, int Disciplinario, int Cerrados);

public sealed record ResultadoGuardado(Guid PartidaId, Guid JugadorId, string CasoId, int Semilla, bool Fundada, string Motivo, int Honorarios, int Experiencia, DateTime Fecha);

/// <summary>Ficha de pregunta tal como la guarda el panel (formato de contenido/preguntas/plantilla.json).</summary>
public sealed class PreguntaGuardada
{
    public string Id { get; set; } = "";
    public string Materia { get; set; } = "";
    public string Calabozo { get; set; } = "";
    public string Pregunta { get; set; } = "";
    public List<string> Opciones { get; set; } = new();
    public int IndiceCorrecto { get; set; }
    public string Explicacion { get; set; } = "";
    public string BaseLegal { get; set; } = "";
    public string EnlaceOficial { get; set; } = "";
    public string VerificadoPor { get; set; } = "";
    public string FechaVerificacion { get; set; } = "";
    public string Estado { get; set; } = "borrador";
}
