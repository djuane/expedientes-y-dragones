using System.Collections.Generic;
using System.Linq;

namespace Expedientia.Logica.Combate
{
    /// <summary>
    /// Bot para enemigos y para completar el equipo. Usa las mismas acciones que un jugador,
    /// así que no puede hacer nada que el servidor no valide.
    /// Estudio: inmoviliza al rival más peligroso con su habilidad, se acerca y ataca; apela los fallos de sus aliados.
    /// Sistema: se acerca al rival más cercano y lo ataca.
    /// </summary>
    public static class Bot
    {
        public static IReadOnlyList<EventoCombate> JugarTurno(Combate combate)
        {
            var eventos = new List<EventoCombate>();
            var yo = combate.Actual;
            if (!yo.Inmovilizado)
            {
                var rivales = combate.Combatientes.Where(c => c.Bando != yo.Bando && !c.Caido).ToList();
                if (yo.Bando == Bando.Estudio) IntentarHabilidad(combate, yo, rivales, eventos);
                var objetivo = rivales
                    .OrderBy(c => yo.Posicion.DistanciaA(c.Posicion))
                    .ThenBy(c => c.Paciencia)
                    .ThenBy(c => c.Id, System.StringComparer.Ordinal)
                    .FirstOrDefault();
                if (objetivo != null && !combate.Terminado)
                {
                    if (yo.Posicion.DistanciaA(objetivo.Posicion) > yo.Ficha.Alcance)
                    {
                        var mejor = combate.CasillasValidas()
                            .OrderBy(c => System.Math.Abs(c.DistanciaA(objetivo.Posicion) - yo.Ficha.Alcance))
                            .ThenBy(c => yo.Posicion.DistanciaA(c))
                            .ThenBy(c => c.Fila).ThenBy(c => c.Columna)
                            .Cast<Casilla?>()
                            .FirstOrDefault();
                        if (mejor.HasValue && mejor.Value.DistanciaA(objetivo.Posicion) < yo.Posicion.DistanciaA(objetivo.Posicion))
                            eventos.AddRange(combate.Mover(mejor.Value).Eventos);
                    }
                    if (yo.AccionDisponible)
                    {
                        var ataque = combate.Atacar(objetivo.Id);
                        if (ataque.Valida)
                        {
                            eventos.AddRange(ataque.Eventos);
                            if (yo.Bando == Bando.Estudio) IntentarApelacion(combate, yo, eventos);
                        }
                    }
                }
            }
            if (!combate.Terminado) eventos.AddRange(combate.TerminarTurno());
            return eventos;
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
            if (yo.RondaUltimaHabilidad.HasValue && combate.Ronda - yo.RondaUltimaHabilidad.Value < ReglasHabilidades.EnfriamientoEnRondas) return;
            var peligroso = rivales
                .Where(r => !r.Inmovilizado && yo.Posicion.DistanciaA(r.Posicion) <= ReglasHabilidades.AlcanceDeHabilidad)
                .OrderByDescending(r => r.Ficha.PacienciaMaxima)
                .ThenBy(r => r.Id, System.StringComparer.Ordinal)
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
    }
}
