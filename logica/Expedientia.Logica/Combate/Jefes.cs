using System;
using System.Collections.Generic;
using System.Linq;

namespace Expedientia.Logica.Combate
{
    public interface IMecanicaDeJefe
    {
        string JefeId { get; }
        void AlInicioDelTurnoDelJefe(Combate combate, List<EventoCombate> eventos);
    }

    /// <summary>
    /// La Cola Infinita (jefe de Mesa de Partes): cada turno suma un enemigo menor a la fila,
    /// hasta un tope para que la sala siempre pueda ganarse.
    /// </summary>
    public sealed class ColaInfinita : IMecanicaDeJefe
    {
        private readonly Func<int, FichaCombatiente> _crearEsbirro;
        private int _creados;
        public string JefeId { get; }
        public int Maximo { get; }
        public int Creados => _creados;

        public ColaInfinita(string jefeId, Func<int, FichaCombatiente> crearEsbirro, int maximo = 4)
        {
            JefeId = jefeId; _crearEsbirro = crearEsbirro; Maximo = maximo;
        }

        public void AlInicioDelTurnoDelJefe(Combate combate, List<EventoCombate> eventos)
        {
            var vivos = combate.Combatientes.Count(c => c.Bando == Bando.Sistema && !c.Caido && c.Id != JefeId);
            if (_creados >= Maximo || vivos >= 3) return;
            var jefe = combate.Buscar(JefeId);
            var libre = combate.CasillasLibres()
                .OrderBy(c => c.DistanciaA(jefe.Posicion))
                .ThenBy(c => c.Fila).ThenBy(c => c.Columna)
                .Cast<Casilla?>().FirstOrDefault();
            if (libre == null) return;
            _creados++;
            combate.AgregarRefuerzo(new Combatiente(_crearEsbirro(_creados), libre.Value), eventos);
        }
    }
}
