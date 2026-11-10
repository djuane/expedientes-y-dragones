using System;
using System.Collections.Generic;

namespace Expedientia.Logica.Sesion
{
    /// <summary>
    /// Acción de un jugador. La misma forma sirve para el servidor (JSON) y para el modo sin conexión del celular.
    /// Tipos: avanzar, mover, atacar, habilidad, apelar, casacion, terminarTurno, piloto, tramite, audiencia,
    /// descansar, comprar, salirTienda, coimero, pedirPregunta, responder, nulidad.
    /// </summary>
    public sealed class AccionJuego
    {
        public string Tipo { get; set; } = "";
        public string? Miembro { get; set; }
        public string? Objetivo { get; set; }
        public int? Columna { get; set; }
        public int? Fila { get; set; }
        public string? Habilidad { get; set; }
        public string? Opcion { get; set; }
        public string? Objeto { get; set; }
        public string? Decision { get; set; }
        public int? Indice { get; set; }
    }

    public sealed class EventoJuego
    {
        public string Tipo { get; set; } = "";
        public object? Datos { get; set; }
    }

    public sealed class CombatienteEstado
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public string Bando { get; set; } = "";
        public string Especialidad { get; set; } = "";
        public int Columna { get; set; }
        public int Fila { get; set; }
        public int Paciencia { get; set; }
        public int PacienciaMaxima { get; set; }
        public int Defensa { get; set; }
        public bool Caido { get; set; }
        public int TurnosInmovilizado { get; set; }
        public bool EsBot { get; set; }
        public string? JugadorId { get; set; }
        /// <summary>Clave del dibujo (civilista, formalismo, plazo…).</summary>
        public string Clave { get; set; } = "";
        /// <summary>Mecánica propia (Vence, NotificaAlAzar, ExigeRequisito, Empuja, SeFracciona) o Ninguno.</summary>
        public string Rasgo { get; set; } = "Ninguno";
        /// <summary>Plazo que Vence: turnos propios que le quedan antes de vencer; null si no vence.</summary>
        public int? TurnosParaVencer { get; set; }
        public int RecargaHabilidad { get; set; }
        public int RecargaNulidad { get; set; }
        public bool ReaccionDisponible { get; set; }
    }

    public sealed class CombateEstado
    {
        public int Ronda { get; set; }
        public string TurnoDe { get; set; } = "";
        public bool Terminado { get; set; }
        public string? Ganador { get; set; }
        public List<CombatienteEstado> Combatientes { get; set; } = new List<CombatienteEstado>();
        public List<int[]> CasillasValidas { get; set; } = new List<int[]>();
        public bool AccionDisponible { get; set; }
        public int MovimientoRestante { get; set; }
        public bool CasacionUsada { get; set; }
        /// <summary>Hay una acción enemiga reciente que la Civilista puede anular.</summary>
        public bool NulidadPosible { get; set; }
        /// <summary>Requisito Imposible: lo que exige ahora (EnPersona, Virtual, ConConstancia), solo si el equipo lo conoce.</summary>
        public string? Requisito { get; set; }
        public string? RequisitoDescripcion { get; set; }
    }

    public sealed class EstudioEstado
    {
        public int Honorarios { get; set; }
        public int Experiencia { get; set; }
        public int Nivel { get; set; }
        public int Reputacion { get; set; }
        public int ExpedientesCerrados { get; set; }
    }

    public sealed class SentenciaEstado
    {
        public bool Fundada { get; set; }
        public string Motivo { get; set; } = "";
        public int Honorarios { get; set; }
        public int Experiencia { get; set; }
    }

    public sealed class EstadoPartida
    {
        public Guid Id { get; set; }
        public string CasoId { get; set; } = "";
        public string Titulo { get; set; } = "";
        public string Premisa { get; set; } = "";
        public List<string> Salas { get; set; } = new List<string>();
        public int IndiceSala { get; set; }
        public string? SalaActual { get; set; }
        public bool SalaResuelta { get; set; }
        /// <summary>Descripción de la sala actual (trama).</summary>
        public string TextoSala { get; set; } = "";
        public int? MinutosRestantes { get; set; }
        public int PruebasJuridicasRestantes { get; set; }
        public bool TieneEscritoPreFirmado { get; set; }
        public bool TieneCodigoAnotado { get; set; }
        public int HonorariosDisponibles { get; set; }
        public EstudioEstado Estudio { get; set; } = new EstudioEstado();
        public List<CombatienteEstado> Equipo { get; set; } = new List<CombatienteEstado>();
        public CombateEstado? Combate { get; set; }
        public SentenciaEstado? Sentencia { get; set; }
        public int Version { get; set; }
    }

    public sealed class PreguntaVisible
    {
        public string Id { get; set; } = "";
        public string Materia { get; set; } = "";
        public string Texto { get; set; } = "";
        public List<string> Opciones { get; set; } = new List<string>();
        public int Segundos { get; set; }
        public bool Borrador { get; set; }
    }

    public sealed class CorreccionPregunta
    {
        public string Resultado { get; set; } = "";
        public int Correcta { get; set; }
        public string Respuesta { get; set; } = "";
        public string BaseLegal { get; set; } = "";
        public string EnlaceOficial { get; set; } = "";
        public string VerificadoPor { get; set; } = "";
        public string? FechaVerificacion { get; set; }
        public bool VentajaParaLaProximaTirada { get; set; }
    }

    public sealed class ExtraRespuesta
    {
        public PreguntaVisible? Pregunta { get; set; }
        public CorreccionPregunta? Correccion { get; set; }
    }

    public sealed class RespuestaAccion
    {
        public bool Valida { get; set; }
        public string? Motivo { get; set; }
        public List<EventoJuego> Eventos { get; set; } = new List<EventoJuego>();
        public EstadoPartida Estado { get; set; } = new EstadoPartida();
        public ExtraRespuesta? Extra { get; set; }
    }
}
