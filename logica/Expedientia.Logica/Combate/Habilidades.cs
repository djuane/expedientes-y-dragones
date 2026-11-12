namespace Expedientia.Logica.Combate
{
    /// <summary>Habilidades del prototipo (Civilista y Administrativista) y la compartida Casación.</summary>
    public enum Habilidad
    {
        /// <summary>Civilista: inmoviliza a un enemigo 2 turnos.</summary>
        MedidaCautelar,
        /// <summary>Administrativista: un aliado repite su última tirada de ataque fallida (usa la reacción).</summary>
        RecursoDeApelacion,
        /// <summary>Administrativista: inmoviliza 2 turnos a un enemigo.</summary>
        Queja,
        /// <summary>Equipo: una sola repetición de tirada por partida, con dificultad muy alta.</summary>
        Casacion,
        /// <summary>Civilista (reacción): anula la última acción de un enemigo y devuelve la paciencia que quitó.</summary>
        Nulidad,
    }

    public static class ReglasHabilidades
    {
        public const int TurnosDeInmovilizacion = 2;
        public const int AlcanceDeHabilidad = 3;
        /// <summary>Pasiva Silencio administrativo: si un enemigo no actúa 2 turnos, el equipo gana la sala.</summary>
        public const int TurnosDeSilencio = 2;
        public const int DificultadCasacion = 18;
        /// <summary>Medida cautelar y Queja se recargan en 3 rondas, para que no se encadenen sin fin.</summary>
        public const int EnfriamientoEnRondas = 3;
        /// <summary>La Nulidad se recarga en 3 rondas.</summary>
        public const int EnfriamientoNulidad = 3;
        /// <summary>El Plazo que Vence explota al empezar este turno propio si sigue en pie (inmovilizado, el plazo se suspende).</summary>
        public const int TurnosDelPlazo = 3;
        /// <summary>Un Fraccionador se parte como máximo en estas copias.</summary>
        public const int MaximoDeCopias = 2;
    }
}
