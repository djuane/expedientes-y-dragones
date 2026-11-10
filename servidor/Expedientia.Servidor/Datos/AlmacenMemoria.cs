using System.Collections.Concurrent;

namespace Expedientia.Servidor.Datos;

/// <summary>Almacén en memoria: para pruebas y para correr sin base de datos.</summary>
public sealed class AlmacenMemoria : IAlmacen
{
    private readonly ConcurrentDictionary<Guid, Jugador> _jugadores = new();
    private readonly ConcurrentDictionary<Guid, EstudioGuardado> _estudios = new();
    private readonly ConcurrentBag<ResultadoGuardado> _resultados = new();
    private readonly ConcurrentDictionary<string, PreguntaGuardada> _preguntas = new();

    public Task PrepararAsync() => Task.CompletedTask;

    public Task<Jugador> CrearJugadorAsync(string alias)
    {
        var j = new Jugador(Guid.NewGuid(), alias, Tokens.Nuevo(), DateTime.UtcNow);
        _jugadores[j.Id] = j;
        return Task.FromResult(j);
    }

    public Task<Jugador?> BuscarPorTokenAsync(string token) =>
        Task.FromResult(_jugadores.Values.FirstOrDefault(j => j.Token == token));

    public Task<EstudioGuardado> LeerEstudioAsync(Guid jugadorId) =>
        Task.FromResult(_estudios.TryGetValue(jugadorId, out var e) ? e : new EstudioGuardado(jugadorId, 0, 0, 50, 0, 0));

    public Task GuardarEstudioAsync(EstudioGuardado estudio) { _estudios[estudio.JugadorId] = estudio; return Task.CompletedTask; }

    public Task GuardarResultadoAsync(ResultadoGuardado resultado) { _resultados.Add(resultado); return Task.CompletedTask; }

    public Task<IReadOnlyList<ResultadoGuardado>> ResultadosAsync(Guid jugadorId) =>
        Task.FromResult<IReadOnlyList<ResultadoGuardado>>(_resultados.Where(r => r.JugadorId == jugadorId).OrderByDescending(r => r.Fecha).ToList());

    public Task<IReadOnlyList<PreguntaGuardada>> PreguntasAsync() =>
        Task.FromResult<IReadOnlyList<PreguntaGuardada>>(_preguntas.Values.OrderBy(p => p.Id).ToList());

    public Task<PreguntaGuardada?> PreguntaAsync(string id) => Task.FromResult(_preguntas.TryGetValue(id, out var p) ? p : null);

    public Task GuardarPreguntaAsync(PreguntaGuardada pregunta) { _preguntas[pregunta.Id] = pregunta; return Task.CompletedTask; }
}
