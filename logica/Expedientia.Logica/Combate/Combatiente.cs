using System;
using Expedientia.Logica.Personajes;

namespace Expedientia.Logica.Combate
{
    public enum Bando { Estudio, Sistema }

    public enum Especialidad { Ninguna, Civilista, Administrativista }

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

        public FichaCombatiente(string id, string nombre, Bando bando, Especialidad especialidad, Atributos atributos,
            int nivel, int paciencia, int defensa, int movimiento, Atributo atributoDeAtaque,
            int dadosDeDanio = 1, int carasDeDanio = 6, int alcance = 1)
        {
            if (paciencia < 1) throw new ArgumentOutOfRangeException(nameof(paciencia));
            if (movimiento < 0) throw new ArgumentOutOfRangeException(nameof(movimiento));
            if (alcance < 1) throw new ArgumentOutOfRangeException(nameof(alcance));
            Reglas.Competencia(nivel);
            Id = id; Nombre = nombre; Bando = bando; Especialidad = especialidad; Atributos = atributos;
            Nivel = nivel; PacienciaMaxima = paciencia; Defensa = defensa; Movimiento = movimiento;
            AtributoDeAtaque = atributoDeAtaque; DadosDeDanio = dadosDeDanio; CarasDeDanio = carasDeDanio; Alcance = alcance;
        }
    }

    /// <summary>Estado de un combatiente durante la partida.</summary>
    public sealed class Combatiente
    {
        public FichaCombatiente Ficha { get; }
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
