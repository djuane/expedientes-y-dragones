using System;
using System.Collections.Generic;
using System.Linq;
using Expedientia.Logica.Combate;
using Expedientia.Logica.Dados;
using Expedientia.Logica.Expedientes;
using Expedientia.Logica.Juridico;

namespace Expedientia.Logica.Sesion
{
    /// <summary>
    /// Traduce acciones de jugadores a la lógica del juego y juega los turnos de bots y enemigos.
    /// Lo usan el servidor (partidas en línea) y el celular (partidas sin conexión), con las mismas reglas.
    /// No es seguro para hilos: quien lo usa debe serializar las llamadas.
    /// </summary>
    public sealed class MotorDePartida
    {
        public const string MarcaBorrador = "BORRADOR SIN VERIFICAR";
        public static readonly TimeSpan TiempoPorTurno = TimeSpan.FromSeconds(30);

        private readonly HashSet<string> _ventajaPorPrueba = new HashSet<string>();
        private (Pregunta pregunta, DateTime inicio, string miembro)? _preguntaPendiente;
        private string? _turnoAnterior;

        public Guid Id { get; }
        public Expediente Expediente { get; }
        public int Version { get; private set; }
        public DateTime InicioDelTurno { get; private set; }

        public MotorDePartida(Guid id, Expediente expediente, DateTime ahora)
        {
            Id = id; Expediente = expediente; InicioDelTurno = ahora;
        }

        public bool Participa(string jugadorId) => Expediente.Miembros.Any(m => m.JugadorId == jugadorId);

        public RespuestaAccion Iniciar(DateTime ahora)
        {
            if (Expediente.IndiceSala >= 0) throw new InvalidOperationException("La partida ya empezó.");
            var eventos = new List<EventoCombate>(Expediente.Avanzar());
            AutoJugar(eventos, ahora);
            return Cerrar(eventos, null);
        }

        /// <summary>Otro jugador toma el personaje de un bot.</summary>
        public string? Unirse(string jugadorId, string especialidad)
        {
            if (Participa(jugadorId)) return null;
            var libre = Expediente.Miembros.FirstOrDefault(m => m.EsBot && string.Equals(m.Ficha.Especialidad.ToString(), especialidad, StringComparison.OrdinalIgnoreCase))
                        ?? Expediente.Miembros.FirstOrDefault(m => m.EsBot);
            if (libre == null) return "La partida está completa.";
            Expediente.AsignarJugador(libre.Ficha.Id, jugadorId);
            Version++;
            return null;
        }

        /// <summary>Ejecuta una acción. <c>elegirPregunta</c> devuelve una pregunta jugable del calabozo, o null si no hay.</summary>
        public RespuestaAccion Ejecutar(string jugadorId, AccionJuego a, DateTime ahora, Func<string, Pregunta?> elegirPregunta)
        {
            try
            {
                if (!Participa(jugadorId)) return Rechazo("No participas en esta partida.");
                if (Expediente.Terminado) return Rechazo("El expediente está cerrado.");
                var eventos = new List<EventoCombate>();
                ExtraRespuesta? extra = null;
                var combate = Expediente.CombateActual;
                var miembro = ElegirMiembro(jugadorId, a.Miembro);

                switch (a.Tipo)
                {
                    case "avanzar":
                        if (!Expediente.SalaResuelta) return Rechazo("La sala no está resuelta.");
                        eventos.AddRange(Expediente.Avanzar());
                        break;

                    case "mover":
                    case "atacar":
                    case "habilidad":
                    case "terminarTurno":
                    case "piloto":
                    {
                        if (combate == null || combate.Terminado) return Rechazo("No hay combate en curso.");
                        if (!EsSuTurno(jugadorId, combate)) return Rechazo("No es tu turno.");
                        var actor = combate.Actual.Id;
                        ResultadoAccion? r = null;
                        switch (a.Tipo)
                        {
                            case "mover":
                                if (a.Columna == null || a.Fila == null) return Rechazo("Falta la casilla.");
                                r = combate.Mover(new Casilla(a.Columna.Value, a.Fila.Value));
                                break;
                            case "atacar":
                                r = combate.Atacar(a.Objetivo ?? "", ModoPara(actor, a.Opcion == "escrito"));
                                break;
                            case "habilidad":
                                if (!Enum.TryParse<Habilidad>(a.Habilidad, true, out var h)) return Rechazo("Habilidad desconocida.");
                                r = combate.UsarHabilidad(h, a.Objetivo ?? "");
                                break;
                            case "terminarTurno":
                                eventos.AddRange(combate.TerminarTurno());
                                break;
                            default:
                                eventos.AddRange(Bot.JugarTurno(combate, ManejaUnBot));
                                break;
                        }
                        if (r != null)
                        {
                            if (!r.Valida) return Rechazo(r.Motivo!);
                            eventos.AddRange(r.Eventos);
                        }
                        break;
                    }

                    case "apelar":
                    {
                        if (combate == null) return Rechazo("No hay combate en curso.");
                        var adm = Expediente.Miembros.FirstOrDefault(m => m.JugadorId == jugadorId && m.Ficha.Especialidad == Especialidad.Administrativista);
                        if (adm == null) return Rechazo("Solo el Administrativista apela.");
                        var r = combate.RecursoDeApelacion(adm.Ficha.Id, a.Objetivo ?? "");
                        if (!r.Valida) return Rechazo(r.Motivo!);
                        eventos.AddRange(r.Eventos);
                        break;
                    }

                    case "nulidad":
                    {
                        if (combate == null) return Rechazo("No hay combate en curso.");
                        var civ = Expediente.Miembros.FirstOrDefault(m => m.JugadorId == jugadorId && m.Ficha.Especialidad == Especialidad.Civilista);
                        if (civ == null) return Rechazo("Solo la Civilista deduce nulidad.");
                        var r = combate.Nulidad(civ.Ficha.Id);
                        if (!r.Valida) return Rechazo(r.Motivo!);
                        eventos.AddRange(r.Eventos);
                        break;
                    }

                    case "casacion":
                    {
                        if (combate == null || miembro == null) return Rechazo("No hay combate en curso.");
                        var r = combate.Casacion(miembro, a.Objetivo ?? miembro);
                        if (!r.Valida) return Rechazo(r.Motivo!);
                        eventos.AddRange(r.Eventos);
                        break;
                    }

                    case "tramite":
                        if (miembro == null) return Rechazo("Elige un personaje.");
                        eventos.AddRange(Expediente.ResolverTramite(miembro, ModoPara(miembro, a.Opcion == "escrito")));
                        break;

                    case "audiencia":
                        if (miembro == null) return Rechazo("Elige un personaje.");
                        var opcion = a.Opcion == "confrontar" ? OpcionAudiencia.Confrontar : OpcionAudiencia.RevisarExpediente;
                        eventos.AddRange(Expediente.ResolverAudiencia(miembro, opcion, ModoPara(miembro, false)));
                        break;

                    case "descansar":
                        eventos.AddRange(Expediente.Descansar());
                        break;

                    case "comprar":
                        if (!Enum.TryParse<ObjetoTienda>(a.Objeto, true, out var objeto)) return Rechazo("Objeto desconocido.");
                        if (!Expediente.Comprar(objeto, out var motivoCompra)) return Rechazo(motivoCompra);
                        break;

                    case "salirTienda":
                        eventos.AddRange(Expediente.SalirDeLaTienda());
                        break;

                    case "coimero":
                        if (!Enum.TryParse<DecisionCoimero>(a.Decision, true, out var d)) return Rechazo("Decisión desconocida.");
                        eventos.AddRange(Expediente.DecidirCoimero(d));
                        break;

                    case "pedirPregunta":
                    {
                        if (miembro == null) return Rechazo("Elige un personaje.");
                        if (!Expediente.PruebasJuridicas.Disponible) return Rechazo("Ya se usaron las 3 pruebas jurídicas.");
                        if (_preguntaPendiente != null) return Rechazo("Ya hay una pregunta en curso.");
                        var p = elegirPregunta(Expediente.Caso.Calabozo);
                        if (p == null) return Rechazo("Todavía no hay preguntas verificadas para este calabozo.");
                        _preguntaPendiente = (p, ahora, miembro);
                        return new RespuestaAccion
                        {
                            Valida = true,
                            Estado = Estado(),
                            Extra = new ExtraRespuesta
                            {
                                Pregunta = new PreguntaVisible
                                {
                                    Id = p.Id, Materia = p.Materia, Texto = p.Texto, Opciones = p.Opciones.ToList(),
                                    Segundos = (int)ControlPruebasJuridicas.TiempoLimite.TotalSeconds,
                                    Borrador = p.VerificadoPor == MarcaBorrador,
                                },
                            },
                        };
                    }

                    case "responder":
                    {
                        if (!(_preguntaPendiente is { } pend)) return Rechazo("No hay pregunta en curso.");
                        _preguntaPendiente = null;
                        var res = Expediente.PruebasJuridicas.Evaluar(pend.pregunta, a.Indice, ahora - pend.inicio);
                        if (res == ResultadoRespuesta.Acierto) _ventajaPorPrueba.Add(pend.miembro);
                        var p = pend.pregunta;
                        var borrador = p.VerificadoPor == MarcaBorrador;
                        extra = new ExtraRespuesta
                        {
                            Correccion = new CorreccionPregunta
                            {
                                Resultado = res.ToString(),
                                Correcta = p.IndiceCorrecto,
                                Respuesta = p.Opciones[p.IndiceCorrecto],
                                BaseLegal = p.BaseLegal,
                                EnlaceOficial = p.EnlaceOficial,
                                VerificadoPor = p.VerificadoPor ?? "",
                                FechaVerificacion = borrador ? null : p.FechaVerificacion?.ToString("yyyy-MM-dd"),
                                VentajaParaLaProximaTirada = res == ResultadoRespuesta.Acierto,
                            },
                        };
                        break;
                    }

                    default:
                        return Rechazo($"Acción desconocida: {a.Tipo}.");
                }

                AutoJugar(eventos, ahora);
                return Cerrar(eventos, extra);
            }
            catch (InvalidOperationException ex) { return Rechazo(ex.Message); }
        }

        /// <summary>Si un jugador no actúa en 30 segundos, un bot juega su turno. Devuelve null si no hizo nada.</summary>
        public RespuestaAccion? RevisarTiempo(DateTime ahora)
        {
            var c = Expediente.CombateActual;
            if (Expediente.Terminado || c == null || c.Terminado || ahora - InicioDelTurno < TiempoPorTurno) return null;
            var eventos = new List<EventoCombate>(Bot.JugarTurno(c, ManejaUnBot));
            AutoJugar(eventos, ahora);
            return Cerrar(eventos, null);
        }

        /// <summary>Los aliados de paso (Suboficial Quispe) y los miembros sin jugador los maneja un bot.</summary>
        private bool ManejaUnBot(string id) => Expediente.Miembros.FirstOrDefault(m => m.Ficha.Id == id)?.EsBot ?? true;

        public EstadoPartida Estado() => MapeoEstado.Estado(Id, Expediente, Version);

        private string? ElegirMiembro(string jugador, string? pedido)
        {
            var mios = Expediente.Miembros.Where(m => m.JugadorId == jugador).Select(m => m.Ficha.Id).ToList();
            if (pedido != null) return mios.Contains(pedido) ? pedido : null;
            var c = Expediente.CombateActual;
            if (c != null && mios.Contains(c.Actual.Id)) return c.Actual.Id;
            return mios.FirstOrDefault();
        }

        private bool EsSuTurno(string jugador, Combate.Combate c) =>
            Expediente.Miembros.Any(m => m.Ficha.Id == c.Actual.Id && m.JugadorId == jugador);

        private Modo ModoPara(string miembro, bool usarEscrito)
        {
            var ventaja = _ventajaPorPrueba.Remove(miembro);
            if (usarEscrito && Expediente.UsarEscritoPreFirmado()) ventaja = true;
            return ventaja ? Modo.Ventaja : Modo.Normal;
        }

        private void AutoJugar(List<EventoCombate> eventos, DateTime ahora)
        {
            var guardia = 0;
            while (guardia++ < 2000)
            {
                var c = Expediente.CombateActual;
                if (c == null || Expediente.Terminado) break;
                if (c.Terminado) { eventos.AddRange(Expediente.RevisarCombate()); break; }
                var m = Expediente.Miembros.FirstOrDefault(x => x.Ficha.Id == c.Actual.Id);
                if (m != null && !m.EsBot) break;
                eventos.AddRange(Bot.JugarTurno(c, ManejaUnBot));
            }
            var turno = Expediente.CombateActual?.Actual.Id;
            if (turno != _turnoAnterior) { _turnoAnterior = turno; InicioDelTurno = ahora; }
        }

        private RespuestaAccion Cerrar(List<EventoCombate> eventos, ExtraRespuesta? extra)
        {
            Version++;
            return new RespuestaAccion
            {
                Valida = true,
                Eventos = eventos.Select(e => new EventoJuego { Tipo = e.GetType().Name, Datos = e }).ToList(),
                Estado = Estado(),
                Extra = extra,
            };
        }

        private RespuestaAccion Rechazo(string motivo) => new RespuestaAccion { Valida = false, Motivo = motivo, Estado = Estado() };
    }
}
