namespace Expedientia.Servidor.Partidas;

public sealed record CrearJugadorPedido(string Alias);
public sealed record CrearPartidaPedido(string CasoId, string Especialidad, int? Semilla, int? Bots);
public sealed record UnirsePedido(string Especialidad);

/// <summary>
/// Acción de un jugador. Tipos: avanzar, mover, atacar, habilidad, apelar, casacion, terminarTurno,
/// tramite, audiencia, descansar, comprar, salirTienda, coimero, pedirPregunta, responder, piloto.
/// </summary>
public sealed class AccionPedido
{
    public string Tipo { get; set; } = "";
    public string? Miembro { get; set; }
    public string? Objetivo { get; set; }
    public int? Columna { get; set; }
    public int? Fila { get; set; }
    public string? Habilidad { get; set; }
    public string? Opcion { get; set; }
    public string? Objeto { get; set; }
    public string? Decision { get; set; }
    public int? Indice { get; set; }
}

public sealed record EventoDto(string Tipo, object Datos);

public sealed record RespuestaAccion(bool Valida, string? Motivo, IReadOnlyList<EventoDto> Eventos, EstadoPartida Estado, object? Extra = null);
