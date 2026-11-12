using System;
using Expedientia.Logica.Personajes;

namespace Expedientia.Logica.Combate
{
    public enum Bando { Estudio, Sistema }

    public enum Especialidad { Ninguna, Civilista, Administrativista }

    /// <summary>Mecánica propia de cada enemigo del GDD. La capa visual la muestra; la lógica la aplica.</summary>
    public enum Rasgo
    {
        Ninguno,
        /// <summary>Plazo que Vence: si no cae en sus primeros turnos, vence y golpea a todo el que esté cerca.</summary>
        Vence,
        /// <summary>Notificación Defectuosa: ataca a quien no corresponde (objetivo al azar).</summary>
        NotificaAlAzar,
        /// <summary>Formalismo: sin el "requisito correcto" (ventaja o Fundado) solo recibe la mitad del daño.</summary>
        ExigeRequisito,
        /// <summary>"Vuelva Mañana": empuja hacia atrás al rival más cercano.</summary>
        Empuja,
        /// <summary>Fraccionador: cada vez que recibe daño y sigue en pie, se parte en una copia pequeña.</summary>
        SeFracciona,
    }

    /// <summary>Datos fijos de un combatiente, independientes de cómo se dibuja.</summary>
    public sealed class FichaCombatiente
    {
        public string Id { get; }
        public string Nombre { get; }
        public Bando Bando { get; }
        public Especialidad Especialidad { get; }
        public Atributos Atributos { get; }
        public int Nivel { get; }
        public int PacienciaMaxima { get; }
        public int Defensa { get; }
        public int Movimiento { get; }
        public Atributo AtributoDeAtaque { get; }
        public int DadosDeDanio { get; }
        public int CarasDeDanio { get; }
        public int Alcance { get; }
        public Rasgo Rasgo { get; }
        /// <summary>Clave estable para que la capa visual elija el dibujo (no cambia aunque cambie el nombre).</summary>
        public string Clave { get; }

        public FichaCombatiente(string id, string nombre, Bando bando, Especialidad especialidad, Atributos atributos,
            int nivel, int paciencia, int defensa, int movimiento, Atributo atributoDeAtaque,
            int dadosDeDanio = 1, int carasDeDanio = 6, int alcance = 1, Rasgo rasgo = Rasgo.Ninguno, string? clave = null)
        {
            if (paciencia < 1) throw new ArgumentOutOfRangeException(nameof(paciencia));
            if (movimiento < 0) throw new ArgumentOutOfRangeException(nameof(movimiento));
            if (alcance < 1) throw new ArgumentOutOfRangeException(nameof(alcance));
            Reglas.Competencia(nivel);
            Id = id; Nombre = nombre; Bando = bando; Especialidad = especialidad; Atributos = atributos;
            Nivel = nivel; PacienciaMaxima = paciencia; Defensa = defensa; Movimiento = movimiento;
            AtributoDeAtaque = atributoDeAtaque; DadosDeDanio = dadosDeDanio; CarasDeDanio = carasDeDanio; Alcance = alcance;
            Rasgo = rasgo;
            Clave = clave ?? (especialidad != Especialidad.Ninguna ? especialidad.ToString().ToLowerInvariant() : nombre.ToLowerInvariant());
        }

        /// <summary>Copia con algunos números cambiados (lo usan las mejoras del expediente).</summary>
        internal FichaCombatiente Con(int? paciencia = null, int? defensa = null, int? movimiento = null, int? dados = null,
            int? caras = null, int? alcance = null, Atributos? atributos = null) =>
            new FichaCombatiente(Id, Nombre, Bando, Especialidad, atributos ?? Atributos, Nivel, paciencia ?? PacienciaMaxima,
                defensa ?? Defensa, movimiento ?? Movimiento, AtributoDeAtaque, dados ?? DadosDeDanio, caras ?? CarasDeDanio,
                alcance ?? Alcance, Rasgo, Clave);
    }

    /// <summary>Estado de un combatiente durante la partida.</summary>
    public sealed class Combatiente
    {
        /// <summary>Se reemplaza al ganar una mejora (la ficha es inmutable; la mejora crea otra con los números nuevos).</summary>
        public FichaCombatiente Ficha { get; internal set; }
        public string Id => Ficha.Id;
        public Bando Bando => Ficha.Bando;
        public Casilla Posicion { get; internal set; }
        public int Paciencia { get; internal set; }
        public bool Caido => Paciencia <= 0;

        /// <summary>Turnos propios que le quedan inmovilizado (Medida cautelar, Queja).</summary>
        public int TurnosInmovilizado { get; internal set; }
        public bool Inmovilizado => TurnosInmovilizado > 0;

        /// <summary>Turnos seguidos sin actuar (para la pasiva Silencio administrativo).</summary>
        public int TurnosSinActuar { get; internal set; }

        /// <summary>Ronda en que usó por última vez una habilidad con enfriamiento.</summary>
        public int? RondaUltimaHabilidad { get; internal set; }

        /// <summary>Turnos propios que ya empezó en este combate (el Plazo que Vence los cuenta).</summary>
        public int TurnosJugados { get; internal set; }

        /// <summary>Copias que ya generó (Fraccionador).</summary>
        public int Copias { get; internal set; }

        /// <summary>Ronda en que usó por última vez la Nulidad (Civilista).</summary>
        public int? RondaUltimaNulidad { get; internal set; }

        public bool AccionDisponible { get; internal set; }
        public int MovimientoRestante { get; internal set; }
        public bool ReaccionDisponible { get; internal set; }

        public Combatiente(FichaCombatiente ficha, Casilla posicion)
        {
            Ficha = ficha;
            Posicion = posicion;
            Paciencia = ficha.PacienciaMaxima;
            ReaccionDisponible = true;
        }
    }
}
