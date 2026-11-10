using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Expedientia.Logica.Combate;
using Expedientia.Logica.Dados;
using Expedientia.Logica.Expedientes;
using Expedientia.Logica.Juridico;
using Expedientia.Servidor.Datos;

namespace Expedientia.Servidor.Partidas;

/// <summary>
/// Una partida viva en el servidor. El servidor es la autoridad: valida cada acción con la librería de lógica,
/// juega los turnos de bots y enemigos, y avisa a todos los jugadores conectados.
/// </summary>
public sealed class SesionPartida
{
    public static readonly TimeSpan TiempoPorTurno = TimeSpan.FromSeconds(30);

    private readonly SemaphoreSlim _cerrojo = new(1, 1);
    private readonly List<WebSocket> _conectados = new();
    private readonly HashSet<string> _ventajaPorPrueba = new();
    private (Pregunta pregunta, DateTime inicio, string miembro)? _preguntaPendiente;
    private readonly Func<SesionPartida, Task> _alTerminar;
    private readonly Func<string, Task<Pregunta?>> _elegirPregunta;
    private bool _resultadoGuardado;

    public Guid Id { get; }
    public int Semilla { get; }
    public Expediente Expediente { get; }
    public int Version { get; private set; }
    public DateTime UltimaActividad { get; private set; } = DateTime.UtcNow;
    public DateTime InicioDelTurno { get; private set; } = DateTime.UtcNow;
    private string? _turnoAnterior;

    public SesionPartida(Guid id, int semilla, Expediente expediente, Func<string, Task<Pregunta?>> elegirPregunta, Func<SesionPartida, Task> alTerminar)
    {
        Id = id; Semilla = semilla; Expediente = expediente; _elegirPregunta = elegirPregunta; _alTerminar = alTerminar;
    }

    public EstadoPartida Estado() => Mapeo.Estado(Id, Expediente, Version);

    public bool Participa(Guid jugadorId) => Expediente.Miembros.Any(m => m.JugadorId == jugadorId.ToString());

    /// <summary>Primera sala y turnos automáticos iniciales.</summary>
    public async Task<RespuestaAccion> IniciarAsync()
    {
        await _cerrojo.WaitAsync();
        try
        {
            var eventos = new List<EventoCombate>();
            eventos.AddRange(Expediente.Avanzar());
            AutoJugar(eventos);
            return await CerrarAsync(true, null, eventos, null);
        }
        finally { _cerrojo.Release(); }
    }

    /// <summary>Otro jugador toma el personaje de un bot (hasta 4 jugadores).</summary>
    public async Task<(bool ok, string? motivo)> UnirseAsync(Guid jugadorId, string especialidad)
    {
        await _cerrojo.WaitAsync();
        try
        {
            if (Participa(jugadorId)) return (true, null);
            var libre = Expediente.Miembros.FirstOrDefault(m => m.EsBot && string.Equals(m.Ficha.Especialidad.ToString(), especialidad, StringComparison.OrdinalIgnoreCase))
                        ?? Expediente.Miembros.FirstOrDefault(m => m.EsBot);
            if (libre == null) return (false, "La partida está completa.");
            Expediente.AsignarJugador(libre.Ficha.Id, jugadorId.ToString());
            Version++;
            await DifundirAsync(new RespuestaAccion(true, null, Array.Empty<EventoDto>(), Estado()));
            return (true, null);
        }
        finally { _cerrojo.Release(); }
    }

    public async Task<RespuestaAccion> EjecutarAsync(Guid jugadorId, AccionPedido a)
    {
        await _cerrojo.WaitAsync();
        try
        {
            UltimaActividad = DateTime.UtcNow;
            if (!Participa(jugadorId)) return Rechazo("No participas en esta partida.");
            if (Expediente.Terminado) return Rechazo("El expediente está cerrado.");
            var jugador = jugadorId.ToString();
            var eventos = new List<EventoCombate>();
            object? extra = null;
            string? motivo = null;
            var combate = Expediente.CombateActual;
            var miembroElegido = ElegirMiembro(jugador, a.Miembro);

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
                    if (!EsSuTurno(jugador, combate)) return Rechazo("No es tu turno.");
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
                        case "piloto":
                            eventos.AddRange(Bot.JugarTurno(combate));
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
                    var adm = Expediente.Miembros.FirstOrDefault(m => m.JugadorId == jugador && m.Ficha.Especialidad == Especialidad.Administrativista);
                    if (adm == null) return Rechazo("Solo el Administrativista apela.");
                    var r = combate.RecursoDeApelacion(adm.Ficha.Id, a.Objetivo ?? "");
                    if (!r.Valida) return Rechazo(r.Motivo!);
                    eventos.AddRange(r.Eventos);
                    break;
                }

                case "casacion":
                {
                    if (combate == null || miembroElegido == null) return Rechazo("No hay combate en curso.");
                    var r = combate.Casacion(miembroElegido, a.Objetivo ?? miembroElegido);
                    if (!r.Valida) return Rechazo(r.Motivo!);
                    eventos.AddRange(r.Eventos);
                    break;
                }

                case "tramite":
                    if (miembroElegido == null) return Rechazo("Elige un personaje.");
                    eventos.AddRange(Expediente.ResolverTramite(miembroElegido, ModoPara(miembroElegido, a.Opcion == "escrito")));
                    break;

                case "audiencia":
                    if (miembroElegido == null) return Rechazo("Elige un personaje.");
                    var opcion = a.Opcion == "confrontar" ? OpcionAudiencia.Confrontar : OpcionAudiencia.RevisarExpediente;
                    eventos.AddRange(Expediente.ResolverAudiencia(miembroElegido, opcion, ModoPara(miembroElegido, false)));
                    break;

                case "descansar":
                    eventos.AddRange(Expediente.Descansar());
                    break;

                case "comprar":
                    if (!Enum.TryParse<ObjetoTienda>(a.Objeto, true, out var objeto)) return Rechazo("Objeto desconocido.");
                    if (!Expediente.Comprar(objeto, out motivo)) return Rechazo(motivo);
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
                    if (miembroElegido == null) return Rechazo("Elige un personaje.");
                    if (!Expediente.PruebasJuridicas.Disponible) return Rechazo("Ya se usaron las 3 pruebas jurídicas.");
                    if (_preguntaPendiente != null) return Rechazo("Ya hay una pregunta en curso.");
                    var calabozo = Expediente.Caso.Calabozo;
                    var p = await _elegirPregunta(calabozo);
                    if (p == null) return Rechazo("Todavía no hay preguntas verificadas para este calabozo.");
                    _preguntaPendiente = (p, DateTime.UtcNow, miembroElegido);
                    extra = new { pregunta = new { p.Id, p.Materia, p.Texto, p.Opciones, segundos = (int)ControlPruebasJuridicas.TiempoLimite.TotalSeconds, borrador = p.VerificadoPor == PreguntasDeJuego.MarcaBorrador } };
                    return new RespuestaAccion(true, null, Array.Empty<EventoDto>(), Estado(), extra);
                }

                case "responder":
                {
                    if (_preguntaPendiente is not { } pend) return Rechazo("No hay pregunta en curso.");
                    _preguntaPendiente = null;
                    var tiempo = DateTime.UtcNow - pend.inicio;
                    var res = Expediente.PruebasJuridicas.Evaluar(pend.pregunta, a.Indice, tiempo);
                    if (res == ResultadoRespuesta.Acierto) _ventajaPorPrueba.Add(pend.miembro);
                    var p = pend.pregunta;
                    extra = new
                    {
                        resultado = res.ToString(),
                        correcta = p.IndiceCorrecto,
                        respuesta = p.Opciones[p.IndiceCorrecto],
                        p.BaseLegal,
                        p.EnlaceOficial,
                        verificadoPor = p.VerificadoPor,
                        fechaVerificacion = p.VerificadoPor == PreguntasDeJuego.MarcaBorrador ? null : p.FechaVerificacion?.ToString("yyyy-MM-dd"),
                        ventajaParaLaProximaTirada = res == ResultadoRespuesta.Acierto,
                    };
                    break;
                }

                default:
                    return Rechazo($"Acción desconocida: {a.Tipo}.");
            }

            AutoJugar(eventos);
            return await CerrarAsync(true, motivo, eventos, extra);
        }
        catch (InvalidOperationException ex) { return Rechazo(ex.Message); }
        finally { _cerrojo.Release(); }
    }

    /// <summary>Si un jugador no actúa en 30 segundos, un bot juega su turno (regla del GDD).</summary>
    public async Task<bool> RevisarTiempoAsync(DateTime ahora)
    {
        if (!await _cerrojo.WaitAsync(0)) return false;
        try
        {
            var c = Expediente.CombateActual;
            if (Expediente.Terminado || c == null || c.Terminado || ahora - InicioDelTurno < TiempoPorTurno) return false;
            var eventos = new List<EventoCombate>(Bot.JugarTurno(c));
            AutoJugar(eventos);
            await CerrarAsync(true, null, eventos, null);
            return true;
        }
        finally { _cerrojo.Release(); }
    }

    public void Conectar(WebSocket ws) { lock (_conectados) _conectados.Add(ws); }
    public void Desconectar(WebSocket ws) { lock (_conectados) _conectados.Remove(ws); }

    private string? ElegirMiembro(string jugador, string? pedido)
    {
        var mios = Expediente.Miembros.Where(m => m.JugadorId == jugador).Select(m => m.Ficha.Id).ToList();
        if (pedido != null) return mios.Contains(pedido) ? pedido : null;
        var c = Expediente.CombateActual;
        if (c != null && mios.Contains(c.Actual.Id)) return c.Actual.Id;
        return mios.FirstOrDefault();
    }

    private bool EsSuTurno(string jugador, Expedientia.Logica.Combate.Combate c) =>
        Expediente.Miembros.Any(m => m.Ficha.Id == c.Actual.Id && m.JugadorId == jugador);

    private Modo ModoPara(string miembro, bool usarEscrito)
    {
        var ventaja = _ventajaPorPrueba.Remove(miembro);
        if (usarEscrito && Expediente.UsarEscritoPreFirmado()) ventaja = true;
        return ventaja ? Modo.Ventaja : Modo.Normal;
    }

    /// <summary>Juega bots y enemigos hasta que le toque a un humano o termine el combate.</summary>
    private void AutoJugar(List<EventoCombate> eventos)
    {
        var guardia = 0;
        while (guardia++ < 2000)
        {
            var c = Expediente.CombateActual;
            if (c == null || Expediente.Terminado) break;
            if (c.Terminado) { eventos.AddRange(Expediente.RevisarCombate()); break; }
            var miembro = Expediente.Miembros.FirstOrDefault(m => m.Ficha.Id == c.Actual.Id);
            if (miembro != null && !miembro.EsBot) break;
            eventos.AddRange(Bot.JugarTurno(c));
        }
        var turno = Expediente.CombateActual?.Actual.Id;
        if (turno != _turnoAnterior) { _turnoAnterior = turno; InicioDelTurno = DateTime.UtcNow; }
    }

    private async Task<RespuestaAccion> CerrarAsync(bool valida, string? motivo, List<EventoCombate> eventos, object? extra)
    {
        Version++;
        var respuesta = new RespuestaAccion(valida, motivo, eventos.Select(Mapeo.Evento).ToList(), Estado(), extra);
        if (Expediente.Terminado && !_resultadoGuardado)
        {
            _resultadoGuardado = true;
            await _alTerminar(this);
        }
        await DifundirAsync(respuesta with { Extra = null });
        return respuesta;
    }

    private RespuestaAccion Rechazo(string motivo) => new(false, motivo, Array.Empty<EventoDto>(), Estado());

    private async Task DifundirAsync(RespuestaAccion r)
    {
        WebSocket[] destino;
        lock (_conectados) destino = _conectados.Where(w => w.State == WebSocketState.Open).ToArray();
        if (destino.Length == 0) return;
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(r, Json.Opciones));
        foreach (var ws in destino)
        {
            try { await ws.SendAsync(bytes, WebSocketMessageType.Text, true, CancellationToken.None); }
            catch (WebSocketException) { Desconectar(ws); }
        }
    }
}
