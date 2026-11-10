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
    }

    public static class ReglasHabilidades
    {
        public const int TurnosDeInmovilizacion = 2;
        public const int AlcanceDeHabilidad = 3;
        /// <summary>Pasiva Silencio administrativo: si un enemigo no actúa 2 turnos, el equipo gana la sala.</summary>
        public const int TurnosDeSilencio = 2;
        public const int DificultadCasacion = 18;
    }
}
