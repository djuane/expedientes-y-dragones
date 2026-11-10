using System;
using System.Collections.Generic;
using System.Linq;

namespace Expedientia.Logica.Combate
{
    /// <summary>
    /// Bot para enemigos y para completar el equipo. Usa las mismas acciones que un jugador,
    /// así que no puede hacer nada que el servidor no valide.
    /// Estudio: inmoviliza al rival más peligroso, prioriza el Plazo que está por vencer, busca la distancia que exige
    /// el jefe, ataca y apela los fallos de sus aliados; la Civilista bot anula los golpes fuertes con Nulidad.
    /// Sistema: se acerca al rival más cercano y lo ataca (la Notificación Defectuosa elige a quien no corresponde).
    /// </summary>
    public static class Bot
    {
        /// <param name="combate">Combate en curso; juega el turno de quien le toca.</param>
        /// <param name="esBot">Dice si un combatiente del estudio lo maneja un bot (para que no gaste reacciones de un jugador). Null = todos.</param>
        public static IReadOnlyList<EventoCombate> JugarTurno(Combate combate, Func<string, bool>? esBot = null)
        {
            esBot ??= _ => true;
            var eventos = new List<EventoCombate>();
            var yo = combate.Actual;
            if (!yo.Inmovilizado && !yo.Caido && !combate.Terminado)
            {
                var rivales = combate.Combatientes.Where(c => c.Bando != yo.Bando && !c.Caido).ToList();
                if (yo.Bando == Bando.Estudio) IntentarHabilidad(combate, yo, rivales, eventos);
                var objetivo = yo.Bando == Bando.Estudio ? ElegirObjetivoDelEstudio(combate, yo, rivales) : ElegirObjetivoDelSistema(combate, yo, rivales);
                if (objetivo != null && !combate.Terminado)
                {
                    var (minimo, maximo) = DistanciaDeseada(combate, yo, objetivo);
                    var actual = yo.Posicion.DistanciaA(objetivo.Posicion);
                    if (actual < minimo || actual > maximo)
                    {
                        var mejor = combate.CasillasValidas()
                            .OrderBy(c => Desvio(c.DistanciaA(objetivo.Posicion), minimo, maximo))
                            .ThenBy(c => yo.Posicion.DistanciaA(c))
                            .ThenBy(c => c.Fila).ThenBy(c => c.Columna)
                            .Cast<Casilla?>()
                            .FirstOrDefault();
                        if (mejor.HasValue && Desvio(mejor.Value.DistanciaA(objetivo.Posicion), minimo, maximo) < Desvio(actual, minimo, maximo))
                            eventos.AddRange(combate.Mover(mejor.Value).Eventos);
                    }
                    if (yo.AccionDisponible)
                    {
                        var ataque = combate.Atacar(objetivo.Id);
                        if (ataque.Valida)
                        {
                            eventos.AddRange(ataque.Eventos);
                            if (yo.Bando == Bando.Estudio) IntentarApelacion(combate, yo, eventos);
                            else IntentarNulidad(combate, esBot, eventos);
                        }
                    }
                }
            }
            if (!combate.Terminado) eventos.AddRange(combate.TerminarTurno());
            if (!combate.Terminado) IntentarNulidad(combate, esBot, eventos);
            return eventos;
        }

        private static int Desvio(int distancia, int minimo, int maximo) =>
            distancia < minimo ? minimo - distancia : distancia > maximo ? distancia - maximo : 0;

        /// <summary>Distancia a la que conviene golpear: normalmente hasta el alcance; frente al Requisito Imposible, la que exige.</summary>
        private static (int minimo, int maximo) DistanciaDeseada(Combate combate, Combatiente yo, Combatiente objetivo)
        {
            var req = combate.Requisito;
            if (yo.Bando == Bando.Estudio && req != null && objetivo.Id == ((IMecanicaDeJefe)req).JefeId)
            {
                if (req.RequisitoActual == RequisitoImposible.EnPersona) return (1, 1);
                if (req.RequisitoActual == RequisitoImposible.Virtual) return (2, Math.Max(2, yo.Ficha.Alcance));
            }
            return (1, yo.Ficha.Alcance);
        }

        private static Combatiente? ElegirObjetivoDelSistema(Combate combate, Combatiente yo, List<Combatiente> rivales)
        {
            if (rivales.Count == 0) return null;
            if (yo.Ficha.Rasgo == Rasgo.NotificaAlAzar)
            {
                var orden = rivales.OrderBy(c => c.Id, StringComparer.Ordinal).ToList();
                return orden[combate.Azar.Entre(0, orden.Count - 1)];
            }
            return rivales
                .OrderBy(c => yo.Posicion.DistanciaA(c.Posicion))
                .ThenBy(c => c.Paciencia)
                .ThenBy(c => c.Id, StringComparer.Ordinal)
                .First();
        }

        /// <summary>
        /// El estudio prioriza: el Plazo a punto de vencer, luego a quien puede dañar de verdad (el jefe con su requisito,
        /// el Formalismo solo con ventaja), y entre ellos al más cercano y débil.
        /// </summary>
        private static Combatiente? ElegirObjetivoDelEstudio(Combate combate, Combatiente yo, List<Combatiente> rivales)
        {
            var req = combate.Requisito;
            int Puntaje(Combatiente c)
            {
                var p = yo.Posicion.DistanciaA(c.Posicion) * 10 + c.Paciencia;
                if (c.Ficha.Rasgo == Rasgo.Vence && !c.Inmovilizado) p -= 15 * c.TurnosJugados;
                if (c.Inmovilizado) p -= 8;
                if (c.Ficha.Rasgo == Rasgo.ExigeRequisito && !c.Inmovilizado) p += 6;
                if (req != null && c.Id == ((IMecanicaDeJefe)req).JefeId)
                {
                    var danable = req.RequisitoActual != RequisitoImposible.ConConstancia || c.Inmovilizado;
                    p += danable ? -20 : 60;
                }
                return p;
            }
            return rivales.OrderBy(Puntaje).ThenBy(c => c.Id, StringComparer.Ordinal).FirstOrDefault();
        }

        private static void IntentarHabilidad(Combate combate, Combatiente yo, List<Combatiente> rivales, List<EventoCombate> eventos)
        {
            var habilidad = yo.Ficha.Especialidad switch
            {
                Especialidad.Civilista => Habilidad.MedidaCautelar,
                Especialidad.Administrativista => Habilidad.Queja,
                _ => (Habilidad?)null,
            };
            if (habilidad == null || rivales.Count < 2) return;
            if (combate.RecargaHabilidad(yo) > 0) return;
            var peligroso = rivales
                .Where(r => !r.Inmovilizado && yo.Posicion.DistanciaA(r.Posicion) <= ReglasHabilidades.AlcanceDeHabilidad)
                .OrderByDescending(r => r.Ficha.Rasgo == Rasgo.Vence && r.TurnosJugados >= ReglasHabilidades.TurnosDelPlazo - 1)
                .ThenByDescending(r => r.Ficha.PacienciaMaxima)
                .ThenBy(r => r.Id, StringComparer.Ordinal)
                .FirstOrDefault();
            if (peligroso == null) return;
            var r = combate.UsarHabilidad(habilidad.Value, peligroso.Id);
            if (r.Valida) eventos.AddRange(r.Eventos);
        }

        /// <summary>Si el bot falló, un Administrativista aliado con reacción disponible apela.</summary>
        private static void IntentarApelacion(Combate combate, Combatiente atacante, List<EventoCombate> eventos)
        {
            var ultimo = eventos.OfType<Ataco>().LastOrDefault();
            if (ultimo == null || ultimo.Acierto || combate.Terminado) return;
            var adm = combate.Combatientes.FirstOrDefault(c => c.Bando == Bando.Estudio && !c.Caido
                && c.Ficha.Especialidad == Especialidad.Administrativista && c.ReaccionDisponible);
            if (adm == null) return;
            var r = combate.RecursoDeApelacion(adm.Id, atacante.Id);
            if (r.Valida) eventos.AddRange(r.Eventos);
        }

        /// <summary>Una Civilista manejada por bot anula el golpe enemigo si quitó 4 o más de paciencia o tumbó a alguien.</summary>
        private static void IntentarNulidad(Combate combate, Func<string, bool> esBot, List<EventoCombate> eventos)
        {
            if (combate.Terminado || !combate.HayAccionEnemigaQueAnular) return;
            var afectados = eventos.OfType<RecibioDanio>().Where(d => combate.Buscar(d.Id).Bando == Bando.Estudio).ToList();
            var grave = afectados.Sum(d => d.Cantidad) >= 4 || afectados.Any(d => d.PacienciaRestante == 0);
            if (!grave) return;
            var civ = combate.Combatientes.FirstOrDefault(c => c.Bando == Bando.Estudio && !c.Caido && esBot(c.Id)
                && c.Ficha.Especialidad == Especialidad.Civilista && c.ReaccionDisponible && combate.RecargaNulidad(c) == 0);
            if (civ == null) return;
            var r = combate.Nulidad(civ.Id);
            if (r.Valida) eventos.AddRange(r.Eventos);
        }
    }
}
