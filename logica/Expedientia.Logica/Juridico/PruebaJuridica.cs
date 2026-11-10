using System;
using System.Collections.Generic;
using Expedientia.Logica.Dados;

namespace Expedientia.Logica.Juridico
{
    public enum EstadoPregunta { Borrador, Verificada, Desactualizada }

    /// <summary>Ficha de pregunta según el GDD. Solo las verificadas pueden entrar al juego.</summary>
    public sealed class Pregunta
    {
        public string Id { get; }
        public string Materia { get; }
        public string Calabozo { get; }
        public string Texto { get; }
        public IReadOnlyList<string> Opciones { get; }
        public int IndiceCorrecto { get; }
        public string BaseLegal { get; }
        public string EnlaceOficial { get; }
        public string? VerificadoPor { get; }
        public DateTime? FechaVerificacion { get; }
        public EstadoPregunta Estado { get; }

        public Pregunta(string id, string materia, string calabozo, string texto, IReadOnlyList<string> opciones,
            int indiceCorrecto, string baseLegal, string enlaceOficial, EstadoPregunta estado,
            string? verificadoPor = null, DateTime? fechaVerificacion = null)
        {
            if (opciones == null || opciones.Count != 4) throw new ArgumentException("Cada pregunta lleva exactamente 4 opciones.", nameof(opciones));
            if (indiceCorrecto < 0 || indiceCorrecto > 3) throw new ArgumentOutOfRangeException(nameof(indiceCorrecto));
            if (estado == EstadoPregunta.Verificada && (string.IsNullOrWhiteSpace(verificadoPor) || fechaVerificacion == null || string.IsNullOrWhiteSpace(enlaceOficial)))
                throw new ArgumentException("Una pregunta verificada necesita revisor, fecha y enlace oficial.");
            Id = id; Materia = materia; Calabozo = calabozo; Texto = texto; Opciones = opciones;
            IndiceCorrecto = indiceCorrecto; BaseLegal = baseLegal; EnlaceOficial = enlaceOficial;
            Estado = estado; VerificadoPor = verificadoPor; FechaVerificacion = fechaVerificacion;
        }

        public bool PuedeJugarse => Estado == EstadoPregunta.Verificada;
    }

    public enum ResultadoRespuesta { Acierto, Fallo, TiempoAgotado }

    /// <summary>
    /// Reglas de la prueba jurídica: máximo 3 por partida, 20 segundos, acierto = ventaja,
    /// fallo o tiempo agotado = tirada normal (nunca castiga a quien no sabe).
    /// </summary>
    public sealed class ControlPruebasJuridicas
    {
        public const int MaximoPorPartida = 3;
        public static readonly TimeSpan TiempoLimite = TimeSpan.FromSeconds(20);

        public int Usadas { get; private set; }
        public bool Disponible => Usadas < MaximoPorPartida;

        public ResultadoRespuesta Evaluar(Pregunta pregunta, int? indiceElegido, TimeSpan tiempoUsado)
        {
            if (!pregunta.PuedeJugarse) throw new InvalidOperationException("La pregunta no está verificada y no puede entrar al juego.");
            if (!Disponible) throw new InvalidOperationException("Ya se usaron las 3 pruebas jurídicas de la partida.");
            Usadas++;
            if (indiceElegido == null || tiempoUsado > TiempoLimite) return ResultadoRespuesta.TiempoAgotado;
            return indiceElegido.Value == pregunta.IndiceCorrecto ? ResultadoRespuesta.Acierto : ResultadoRespuesta.Fallo;
        }

        public static Modo ModoDeTirada(ResultadoRespuesta r) => r == ResultadoRespuesta.Acierto ? Modo.Ventaja : Modo.Normal;
    }
}
