using System.Collections.Generic;
using System.Linq;
using Expedientia.Logica.Dados;

namespace Expedientia.Logica.Personajes
{
    public sealed class Participante
    {
        public string Id { get; }
        public int ModificadorAgilidad { get; }
        public Participante(string id, int modificadorAgilidad) { Id = id; ModificadorAgilidad = modificadorAgilidad; }
    }

    public sealed class TurnoIniciativa
    {
        public Participante Participante { get; }
        public int Total { get; }
        internal int Desempate { get; }
        internal TurnoIniciativa(Participante p, int total, int desempate) { Participante = p; Total = total; Desempate = desempate; }
    }

    public static class Iniciativa
    {
        /// <summary>
        /// d20 + Agilidad procesal. Empates: gana el mayor modificador; si persiste, se tira un d20 de desempate.
        /// El resultado es determinista para una misma semilla, como exige el servidor.
        /// </summary>
        public static IReadOnlyList<TurnoIniciativa> Ordenar(IGeneradorAleatorio azar, IEnumerable<Participante> participantes)
        {
            var turnos = participantes
                .Select(p => new TurnoIniciativa(p, Dado.Tirar(azar, 20) + p.ModificadorAgilidad, Dado.Tirar(azar, 20)))
                .ToList();
            return turnos
                .OrderByDescending(t => t.Total)
                .ThenByDescending(t => t.Participante.ModificadorAgilidad)
                .ThenByDescending(t => t.Desempate)
                .ThenBy(t => t.Participante.Id, System.StringComparer.Ordinal)
                .ToList();
        }
    }
}
