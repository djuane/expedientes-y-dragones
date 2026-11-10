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

    /// <summary>Mecánica de jefe que decide qué golpes le hacen daño.</summary>
    public interface IExigeRequisito
    {
        /// <summary>¿Este golpe cumple el requisito vigente?</summary>
        bool Admite(Combatiente atacante, Combatiente objetivo, bool conVentaja);
        string RequisitoActual { get; }
        string DescripcionActual { get; }
        /// <summary>Si el equipo ya conoce el requisito vigente (lo revela la Especialista Mendoza o un golpe observado).</summary>
        bool Visible { get; }
        void Revelar();
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

    /// <summary>
    /// El Requisito Imposible (jefe de la Sala de Contrataciones): cada 2 rondas cambia la condición para recibir daño
    /// y suma un Formalismo a la sala. Los golpes que no cumplen el requisito vigente quedan "observados" y no hacen daño.
    /// </summary>
    public sealed class RequisitoImposible : IMecanicaDeJefe, IExigeRequisito
    {
        public const string EnPersona = "EnPersona";
        public const string Virtual = "Virtual";
        public const string ConConstancia = "ConConstancia";

        private static readonly string[] Requisitos = { EnPersona, Virtual, ConConstancia };
        private readonly Func<int, FichaCombatiente> _crearRefuerzo;
        private readonly bool _siempreVisible;
        private int _rondaDelCambio;
        private int _refuerzos;

        public string JefeId { get; }
        public string RequisitoActual { get; private set; }
        public bool Visible { get; private set; }
        public int MaximoDeRefuerzos { get; }

        public RequisitoImposible(string jefeId, string inicial, Func<int, FichaCombatiente> crearRefuerzo, bool siempreVisible, int maximoDeRefuerzos = 2)
        {
            JefeId = jefeId; RequisitoActual = inicial; _crearRefuerzo = crearRefuerzo;
            _siempreVisible = siempreVisible; Visible = siempreVisible; MaximoDeRefuerzos = maximoDeRefuerzos;
            _rondaDelCambio = 1;
        }

        public string DescripcionActual => Describir(RequisitoActual);

        public static string Describir(string requisito) => requisito switch
        {
            EnPersona => "Solo acepta escritos presentados en persona: golpes desde una casilla vecina.",
            Virtual => "Solo acepta la mesa de partes virtual: golpes desde 2 casillas o más.",
            _ => "Exige constancia: solo golpes con ventaja (prueba jurídica, escrito pre-firmado o el jefe inmovilizado).",
        };

        public bool Admite(Combatiente atacante, Combatiente objetivo, bool conVentaja)
        {
            if (objetivo.Id != JefeId) return true;
            var distancia = atacante.Posicion.DistanciaA(objetivo.Posicion);
            return RequisitoActual switch
            {
                EnPersona => distancia <= 1,
                Virtual => distancia >= 2,
                _ => conVentaja,
            };
        }

        public void Revelar() => Visible = true;

        public void AlInicioDelTurnoDelJefe(Combate combate, List<EventoCombate> eventos)
        {
            if (combate.Ronda - _rondaDelCambio < 2) return;
            _rondaDelCambio = combate.Ronda;
            var opciones = Requisitos.Where(r => r != RequisitoActual).ToArray();
            RequisitoActual = opciones[combate.Azar.Entre(0, opciones.Length - 1)];
            Visible = _siempreVisible;
            eventos.Add(new RequisitoCambio(JefeId, RequisitoActual, Visible ? DescripcionActual : "Cambió el requisito. Nadie sabe cuál es."));
            if (_refuerzos >= MaximoDeRefuerzos) return;
            var jefe = combate.Buscar(JefeId);
            var libre = combate.CasillasLibres()
                .OrderBy(c => c.DistanciaA(jefe.Posicion)).ThenBy(c => c.Fila).ThenBy(c => c.Columna)
                .Cast<Casilla?>().FirstOrDefault();
            if (libre == null) return;
            _refuerzos++;
            combate.AgregarRefuerzo(new Combatiente(_crearRefuerzo(_refuerzos), libre.Value), eventos);
        }
    }
}
