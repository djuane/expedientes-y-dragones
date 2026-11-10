using Expedientia.Logica.Combate;
using Expedientia.Logica.Expedientes;

namespace Expedientia.Servidor.Partidas;

public sealed record CombatienteDto(string Id, string Nombre, string Bando, string Especialidad, int Columna, int Fila,
    int Paciencia, int PacienciaMaxima, int Defensa, bool Caido, int TurnosInmovilizado, bool EsBot, string? JugadorId);

public sealed record CombateDto(int Ronda, string TurnoDe, bool Terminado, string? Ganador, IReadOnlyList<CombatienteDto> Combatientes,
    IReadOnlyList<int[]> CasillasValidas, bool AccionDisponible, int MovimientoRestante, bool CasacionUsada);

public sealed record EstudioDto(int Honorarios, int Experiencia, int Nivel, int Reputacion, int ExpedientesCerrados);

public sealed record EstadoPartida(Guid Id, string CasoId, string Titulo, string Premisa, IReadOnlyList<string> Salas, int IndiceSala,
    string? SalaActual, bool SalaResuelta, int? MinutosRestantes, int PruebasJuridicasRestantes, bool TieneEscritoPreFirmado,
    bool TieneCodigoAnotado, int HonorariosDisponibles, EstudioDto Estudio, IReadOnlyList<CombatienteDto> Equipo, CombateDto? Combate,
    object? Sentencia, int Version);

internal static class Mapeo
{
    public static EstadoPartida Estado(Guid id, Expediente e, int version)
    {
        var miembros = e.Miembros.ToDictionary(m => m.Ficha.Id);
        CombatienteDto Dto(Combatiente c)
        {
            miembros.TryGetValue(c.Id, out var m);
            return new CombatienteDto(c.Id, c.Ficha.Nombre, c.Bando.ToString(), c.Ficha.Especialidad.ToString(), c.Posicion.Columna, c.Posicion.Fila,
                c.Paciencia, c.Ficha.PacienciaMaxima, c.Ficha.Defensa, c.Caido, c.TurnosInmovilizado, m?.EsBot ?? true, m?.JugadorId);
        }
        CombateDto? combate = null;
        if (e.CombateActual is { } k)
        {
            combate = new CombateDto(k.Ronda, k.Actual.Id, k.Terminado, k.Ganador?.ToString(), k.Combatientes.Select(Dto).ToList(),
                k.Terminado ? Array.Empty<int[]>() : k.CasillasValidas().Select(c => new[] { c.Columna, c.Fila }).ToList(),
                k.Actual.AccionDisponible, k.Actual.MovimientoRestante, k.CasacionUsada);
        }
        var s = e.Sentencia;
        return new EstadoPartida(id, e.Caso.Id, e.Caso.Titulo, e.Caso.Premisa, e.Salas.Select(x => x.ToString()).ToList(), e.IndiceSala,
            e.IndiceSala >= 0 ? e.SalaActual.ToString() : null, e.SalaResuelta, e.MinutosRestantes,
            Expedientia.Logica.Juridico.ControlPruebasJuridicas.MaximoPorPartida - e.PruebasJuridicas.Usadas,
            e.TieneEscritoPreFirmado, e.TieneCodigoAnotado, e.HonorariosDisponibles,
            new EstudioDto(e.Estudio.Honorarios, e.Estudio.Experiencia, e.Estudio.Nivel, e.Estudio.Reputacion, e.Estudio.ExpedientesCerrados),
            e.Equipo.Select(Dto).ToList(), combate,
            s == null ? null : new { s.Fundada, s.Motivo, s.Honorarios, s.Experiencia }, version);
    }

    public static EventoDto Evento(EventoCombate ev) => new(ev.GetType().Name, ev);
}
