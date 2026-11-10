using Expedientia.Logica.Juridico;
using Expedientia.Servidor.Datos;

namespace Expedientia.Servidor.Partidas;

internal static class PreguntasDeJuego
{
    public const string MarcaBorrador = Expedientia.Logica.Sesion.MotorDePartida.MarcaBorrador;

    /// <summary>
    /// Convierte la ficha guardada en pregunta de juego. Solo entran las verificadas; los borradores entran
    /// únicamente si el servidor lo permite (entorno de desarrollo) y van marcados como tales.
    /// </summary>
    public static Pregunta? AJuego(PreguntaGuardada p, bool permitirBorradores)
    {
        if (p.Opciones.Count != 4 || p.Estado == "desactualizada") return null;
        try
        {
            if (p.Estado == "verificada")
            {
                if (!DateTime.TryParse(p.FechaVerificacion, out var fecha)) return null;
                return new Pregunta(p.Id, p.Materia, p.Calabozo, p.Pregunta, p.Opciones, p.IndiceCorrecto, p.BaseLegal, p.EnlaceOficial,
                    EstadoPregunta.Verificada, p.VerificadoPor, fecha);
            }
            if (!permitirBorradores) return null;
            var enlace = string.IsNullOrWhiteSpace(p.EnlaceOficial) ? "pendiente" : p.EnlaceOficial;
            return new Pregunta(p.Id, p.Materia, p.Calabozo, p.Pregunta, p.Opciones, p.IndiceCorrecto, p.BaseLegal, enlace,
                EstadoPregunta.Verificada, MarcaBorrador, DateTime.UnixEpoch);
        }
        catch (ArgumentException) { return null; }
    }
}
