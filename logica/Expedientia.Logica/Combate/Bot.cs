using System.Collections.Generic;
using System.Linq;

namespace Expedientia.Logica.Combate
{
    /// <summary>
    /// Bot sencillo para enemigos y para completar el equipo: se acerca al rival más cercano y lo ataca.
    /// Usa las mismas acciones que un jugador, así que no puede hacer nada que el servidor no valide.
    /// </summary>
    public static class Bot
    {
        public static IReadOnlyList<EventoCombate> JugarTurno(Combate combate)
        {
            var eventos = new List<EventoCombate>();
            var yo = combate.Actual;
            if (!yo.Inmovilizado)
            {
                var objetivo = combate.Combatientes
                    .Where(c => c.Bando != yo.Bando && !c.Caido)
                    .OrderBy(c => yo.Posicion.DistanciaA(c.Posicion))
                    .ThenBy(c => c.Paciencia)
                    .ThenBy(c => c.Id, System.StringComparer.Ordinal)
                    .FirstOrDefault();
                if (objetivo != null)
                {
                    if (yo.Posicion.DistanciaA(objetivo.Posicion) > yo.Ficha.Alcance)
                    {
                        var mejor = combate.CasillasValidas()
                            .OrderBy(c => c.DistanciaA(objetivo.Posicion))
                            .ThenBy(c => yo.Posicion.DistanciaA(c))
                            .ThenBy(c => c.Fila).ThenBy(c => c.Columna)
                            .Cast<Casilla?>()
                            .FirstOrDefault();
                        if (mejor.HasValue && mejor.Value.DistanciaA(objetivo.Posicion) < yo.Posicion.DistanciaA(objetivo.Posicion))
                            eventos.AddRange(combate.Mover(mejor.Value).Eventos);
                    }
                    var ataque = combate.Atacar(objetivo.Id);
                    if (ataque.Valida) eventos.AddRange(ataque.Eventos);
                }
            }
            if (!combate.Terminado) eventos.AddRange(combate.TerminarTurno());
            return eventos;
        }
    }
}
