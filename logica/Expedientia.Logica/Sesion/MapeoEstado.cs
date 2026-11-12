using System;
using System.Linq;
using Expedientia.Logica.Combate;
using Expedientia.Logica.Expedientes;
using Expedientia.Logica.Juridico;

namespace Expedientia.Logica.Sesion
{
    public static class MapeoEstado
    {
        public static EstadoPartida Estado(Guid id, Expediente e, int version)
        {
            var miembros = e.Miembros.ToDictionary(m => m.Ficha.Id);
            CombatienteEstado Dto(Combatiente c)
            {
                miembros.TryGetValue(c.Id, out var m);
                return new CombatienteEstado
                {
                    Id = c.Id, Nombre = c.Ficha.Nombre, Bando = c.Bando.ToString(), Especialidad = c.Ficha.Especialidad.ToString(),
                    Columna = c.Posicion.Columna, Fila = c.Posicion.Fila, Paciencia = c.Paciencia, PacienciaMaxima = c.Ficha.PacienciaMaxima,
                    Defensa = c.Ficha.Defensa, Caido = c.Caido, TurnosInmovilizado = c.TurnosInmovilizado,
                    EsBot = m?.EsBot ?? true, JugadorId = m?.JugadorId,
                    Clave = c.Ficha.Clave, Rasgo = c.Ficha.Rasgo.ToString(),
                    TurnosParaVencer = c.Ficha.Rasgo == Rasgo.Vence && !c.Caido ? Math.Max(0, ReglasHabilidades.TurnosDelPlazo - c.TurnosJugados) : (int?)null,
                    RecargaHabilidad = e.CombateActual?.RecargaHabilidad(c) ?? 0,
                    RecargaNulidad = e.CombateActual?.RecargaNulidad(c) ?? 0,
                    Alcance = c.Ficha.Alcance, Movimiento = c.Ficha.Movimiento,
                    ReaccionDisponible = c.ReaccionDisponible,
                };
            }
            CombateEstado? combate = null;
            if (e.CombateActual is { } k)
            {
                combate = new CombateEstado
                {
                    Ronda = k.Ronda, TurnoDe = k.Actual.Id, Terminado = k.Terminado, Ganador = k.Ganador?.ToString(),
                    Combatientes = k.Combatientes.Select(Dto).ToList(),
                    CasillasValidas = k.Terminado ? new System.Collections.Generic.List<int[]>() : k.CasillasValidas().Select(c => new[] { c.Columna, c.Fila }).ToList(),
                    AccionDisponible = k.Actual.AccionDisponible, MovimientoRestante = k.Actual.MovimientoRestante, CasacionUsada = k.CasacionUsada,
                    NulidadPosible = k.HayAccionEnemigaQueAnular,
                    Requisito = k.Requisito is { Visible: true } rq ? rq.RequisitoActual : null,
                    RequisitoDescripcion = k.Requisito == null ? null : k.Requisito.Visible ? k.Requisito.DescripcionActual : "Nadie sabe qué exige ahora. Un golpe observado lo revela.",
                };
            }
            var s = e.Sentencia;
            var proximo = e.Estudio.Proximo;
            MejoraEstado M(Mejora m) { var d = Mejoras.Datos(m); return new MejoraEstado { Id = m.ToString(), Nombre = d.Nombre, Descripcion = d.Descripcion, Rara = d.Rara }; }
            return new EstadoPartida
            {
                Id = id, CasoId = e.Caso.Id, Titulo = e.Caso.Titulo, Premisa = e.Caso.Premisa,
                Salas = e.Salas.Select(x => x.ToString()).ToList(), IndiceSala = e.IndiceSala,
                SalaActual = e.IndiceSala >= 0 ? e.SalaActual.ToString() : null, SalaResuelta = e.SalaResuelta, TextoSala = e.TextoSala,
                MinutosRestantes = e.MinutosRestantes,
                PruebasJuridicasRestantes = ControlPruebasJuridicas.MaximoPorPartida - e.PruebasJuridicas.Usadas,
                TieneEscritoPreFirmado = e.TieneEscritoPreFirmado, TieneCodigoAnotado = e.TieneCodigoAnotado,
                HonorariosDisponibles = e.HonorariosDisponibles,
                MejorasOfrecidas = e.MejorasOfrecidas.Select(M).ToList(),
                MejorasTomadas = e.MejorasTomadas.Select(M).ToList(),
                PuedeVolverASortear = e.PuedeVolverASortear,
                Estudio = new EstudioEstado
                {
                    Honorarios = e.Estudio.Honorarios, Experiencia = e.Estudio.Experiencia, Nivel = e.Estudio.Nivel,
                    Reputacion = e.Estudio.Reputacion, ExpedientesCerrados = e.Estudio.ExpedientesCerrados,
                    ProximoDesbloqueo = proximo?.Nombre, ProximoDesbloqueoDescripcion = proximo?.Descripcion,
                    FaltanParaDesbloqueo = proximo == null ? 0 : proximo.ExpedientesNecesarios - e.Estudio.ExpedientesCerrados,
                },
                Equipo = e.Equipo.Select(Dto).ToList(),
                Combate = combate,
                Sentencia = s == null ? null : new SentenciaEstado { Fundada = s.Fundada, Motivo = s.Motivo, Honorarios = s.Honorarios, Experiencia = s.Experiencia },
                Version = version,
            };
        }
    }
}
