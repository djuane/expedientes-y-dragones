using System.Collections.Concurrent;
using Expedientia.Logica.Combate;
using Expedientia.Logica.Expedientes;
using Expedientia.Logica.Juridico;
using Expedientia.Servidor.Datos;

namespace Expedientia.Servidor.Partidas;

public sealed class OpcionesJuego
{
    /// <summary>Solo en desarrollo: deja entrar preguntas en borrador, marcadas como tales.</summary>
    public bool PermitirBorradores { get; set; }
    public string? CarpetaPreguntas { get; set; }
    public string? ClavePanel { get; set; }
}

public sealed class GestorPartidas
{
    private readonly ConcurrentDictionary<Guid, SesionPartida> _partidas = new();
    private readonly IAlmacen _almacen;
    private readonly OpcionesJuego _opciones;
    private readonly ILogger<GestorPartidas> _log;

    public GestorPartidas(IAlmacen almacen, OpcionesJuego opciones, ILogger<GestorPartidas> log)
    {
        _almacen = almacen; _opciones = opciones; _log = log;
    }

    public IEnumerable<SesionPartida> Activas => _partidas.Values;

    public SesionPartida? Buscar(Guid id) => _partidas.TryGetValue(id, out var s) ? s : null;

    public async Task<(SesionPartida? sesion, Expedientia.Logica.Sesion.RespuestaAccion? inicio, string? error)> CrearAsync(Jugador jugador, CrearPartidaPedido pedido)
    {
        var caso = CatalogoCasos.Todos.FirstOrDefault(c => c.Id == pedido.CasoId);
        if (caso == null) return (null, null, "Caso desconocido.");
        if (!Enum.TryParse<Especialidad>(pedido.Especialidad, true, out var esp) || esp == Especialidad.Ninguna)
            return (null, null, "Especialidad no disponible en el prototipo (Civilista o Administrativista).");
        var guardado = await _almacen.LeerEstudioAsync(jugador.Id);
        var estudio = EstadoEstudio.Restaurar(guardado.Honorarios, guardado.Experiencia, guardado.Reputacion, guardado.Disciplinario, guardado.Cerrados);
        var nivel = estudio.Nivel;
        var bots = Math.Clamp(pedido.Bots ?? 1, 0, 3);
        var miembros = new List<MiembroDelEquipo> { new(Ficha(esp, "p1", jugador.Alias, nivel), jugador.Id.ToString()) };
        for (var i = 0; i < bots; i++)
        {
            var otra = (i % 2 == 0) == (esp == Especialidad.Civilista) ? Especialidad.Administrativista : Especialidad.Civilista;
            miembros.Add(new MiembroDelEquipo(Ficha(otra, $"p{i + 2}", otra == Especialidad.Civilista ? "Bot Civilista" : "Bot Administrativista", nivel), null));
        }
        var semilla = pedido.Semilla ?? Random.Shared.Next();
        var id = Guid.NewGuid();
        var sesion = new SesionPartida(id, semilla, new Expediente(caso, semilla, miembros, estudio), ElegirPreguntaAsync, GuardarResultadoAsync);
        _partidas[id] = sesion;
        var inicio = await sesion.IniciarAsync();
        _log.LogInformation("Partida {Id} creada: caso {Caso}, semilla {Semilla}", id, caso.Id, semilla);
        return (sesion, inicio, null);
    }

    private static FichaCombatiente Ficha(Especialidad e, string id, string nombre, int nivel) =>
        e == Especialidad.Civilista ? Catalogo.Civilista(id, nombre, nivel) : Catalogo.Administrativista(id, nombre, nivel);

    private async Task<Pregunta?> ElegirPreguntaAsync(string calabozo)
    {
        var candidatas = (await _almacen.PreguntasAsync())
            .Where(p => string.Equals(p.Calabozo, calabozo, StringComparison.OrdinalIgnoreCase))
            .Select(p => PreguntasDeJuego.AJuego(p, _opciones.PermitirBorradores))
            .Where(p => p != null)
            .ToList();
        return candidatas.Count == 0 ? null : candidatas[Random.Shared.Next(candidatas.Count)];
    }

    private async Task GuardarResultadoAsync(SesionPartida s)
    {
        var e = s.Expediente;
        var sentencia = e.Sentencia!;
        foreach (var jugadorId in e.Miembros.Where(m => !m.EsBot).Select(m => Guid.Parse(m.JugadorId!)).Distinct())
        {
            await _almacen.GuardarResultadoAsync(new ResultadoGuardado(s.Id, jugadorId, e.Caso.Id, s.Semilla, sentencia.Fundada, sentencia.Motivo,
                sentencia.Honorarios, sentencia.Experiencia, DateTime.UtcNow));
            // El progreso del estudio es del creador; los invitados suman honorarios y experiencia a su propio estudio.
            var previo = await _almacen.LeerEstudioAsync(jugadorId);
            var esCreador = e.Miembros[0].JugadorId == jugadorId.ToString();
            await _almacen.GuardarEstudioAsync(esCreador
                ? new EstudioGuardado(jugadorId, e.Estudio.Honorarios, e.Estudio.Experiencia, e.Estudio.Reputacion, e.Estudio.ExpedienteDisciplinario, e.Estudio.ExpedientesCerrados)
                : previo with { Honorarios = previo.Honorarios + sentencia.Honorarios, Experiencia = previo.Experiencia + sentencia.Experiencia, Cerrados = previo.Cerrados + (sentencia.Fundada ? 1 : 0) });
        }
        _log.LogInformation("Partida {Id} cerrada: {Motivo}", s.Id, sentencia.Motivo);
    }

    /// <summary>Quita de memoria las partidas cerradas o abandonadas hace más de 2 horas.</summary>
    public void Limpiar(DateTime ahora)
    {
        foreach (var (id, s) in _partidas)
            if (ahora - s.UltimaActividad > TimeSpan.FromHours(2)) _partidas.TryRemove(id, out _);
    }
}

/// <summary>Cada segundo revisa turnos vencidos (30 s) y limpia partidas viejas.</summary>
public sealed class RelojDeTurnos : BackgroundService
{
    private readonly GestorPartidas _gestor;
    public RelojDeTurnos(GestorPartidas gestor) { _gestor = gestor; }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var vueltas = 0;
        while (!ct.IsCancellationRequested)
        {
            await Task.Delay(1000, ct).ContinueWith(_ => { });
            var ahora = DateTime.UtcNow;
            foreach (var s in _gestor.Activas) await s.RevisarTiempoAsync(ahora);
            if (++vueltas % 600 == 0) _gestor.Limpiar(ahora);
        }
    }
}
