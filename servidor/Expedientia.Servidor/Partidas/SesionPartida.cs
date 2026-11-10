using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Expedientia.Logica.Expedientes;
using Expedientia.Logica.Juridico;
using Expedientia.Logica.Sesion;
using Expedientia.Servidor.Datos;

namespace Expedientia.Servidor.Partidas;

/// <summary>
/// Partida viva en el servidor: serializa las acciones, usa el mismo motor que el celular
/// y avisa a todos los jugadores conectados por WebSocket.
/// </summary>
public sealed class SesionPartida
{
    private readonly SemaphoreSlim _cerrojo = new(1, 1);
    private readonly List<WebSocket> _conectados = new();
    private readonly Func<SesionPartida, Task> _alTerminar;
    private readonly Func<string, Task<Pregunta?>> _elegirPregunta;
    private readonly MotorDePartida _motor;
    private bool _resultadoGuardado;

    public Guid Id => _motor.Id;
    public int Semilla { get; }
    public Expediente Expediente => _motor.Expediente;
    public DateTime UltimaActividad { get; private set; } = DateTime.UtcNow;

    public SesionPartida(Guid id, int semilla, Expediente expediente, Func<string, Task<Pregunta?>> elegirPregunta, Func<SesionPartida, Task> alTerminar)
    {
        _motor = new MotorDePartida(id, expediente, DateTime.UtcNow);
        Semilla = semilla; _elegirPregunta = elegirPregunta; _alTerminar = alTerminar;
    }

    public EstadoPartida Estado() => _motor.Estado();
    public bool Participa(Guid jugadorId) => _motor.Participa(jugadorId.ToString());

    public Task<RespuestaAccion> IniciarAsync() => ConCerrojo(async () => await Publicar(_motor.Iniciar(DateTime.UtcNow)));

    public Task<string?> UnirseAsync(Guid jugadorId, string especialidad) => ConCerrojo(async () =>
    {
        var error = _motor.Unirse(jugadorId.ToString(), especialidad);
        if (error == null) await DifundirAsync(new RespuestaAccion { Valida = true, Estado = Estado() });
        return error;
    });

    public Task<RespuestaAccion> EjecutarAsync(Guid jugadorId, AccionJuego a) => ConCerrojo(async () =>
    {
        UltimaActividad = DateTime.UtcNow;
        Pregunta? pregunta = null;
        if (a.Tipo == "pedirPregunta" && _motor.Participa(jugadorId.ToString())) pregunta = await _elegirPregunta(Expediente.Caso.Calabozo);
        var r = _motor.Ejecutar(jugadorId.ToString(), a, DateTime.UtcNow, _ => pregunta);
        return r.Valida && a.Tipo != "pedirPregunta" ? await Publicar(r) : r;
    });

    public async Task RevisarTiempoAsync(DateTime ahora)
    {
        if (!await _cerrojo.WaitAsync(0)) return;
        try
        {
            var r = _motor.RevisarTiempo(ahora);
            if (r != null) await Publicar(r);
        }
        finally { _cerrojo.Release(); }
    }

    public void Conectar(WebSocket ws) { lock (_conectados) _conectados.Add(ws); }
    public void Desconectar(WebSocket ws) { lock (_conectados) _conectados.Remove(ws); }

    private async Task<T> ConCerrojo<T>(Func<Task<T>> f)
    {
        await _cerrojo.WaitAsync();
        try { return await f(); }
        finally { _cerrojo.Release(); }
    }

    private async Task<RespuestaAccion> Publicar(RespuestaAccion r)
    {
        if (Expediente.Terminado && !_resultadoGuardado)
        {
            _resultadoGuardado = true;
            await _alTerminar(this);
        }
        // La corrección de la pregunta solo la ve quien respondió; el resto recibe eventos y estado.
        await DifundirAsync(new RespuestaAccion { Valida = r.Valida, Motivo = r.Motivo, Eventos = r.Eventos, Estado = r.Estado });
        return r;
    }

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
