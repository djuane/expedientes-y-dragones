using System.Text.Json;
using Npgsql;

namespace Expedientia.Servidor.Datos;

/// <summary>Almacén en PostgreSQL. Crea sus tablas al arrancar si no existen.</summary>
public sealed class AlmacenPostgres : IAlmacen
{
    private readonly NpgsqlDataSource _fuente;
    public AlmacenPostgres(string cadena) { _fuente = NpgsqlDataSource.Create(cadena); }

    public async Task PrepararAsync()
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS jugadores (
                id uuid PRIMARY KEY, alias text NOT NULL, token text NOT NULL UNIQUE, creado timestamptz NOT NULL);
            CREATE TABLE IF NOT EXISTS estudios (
                jugador_id uuid PRIMARY KEY REFERENCES jugadores(id), honorarios int NOT NULL, experiencia int NOT NULL,
                reputacion int NOT NULL, disciplinario int NOT NULL, cerrados int NOT NULL);
            CREATE TABLE IF NOT EXISTS resultados (
                partida_id uuid NOT NULL, jugador_id uuid NOT NULL REFERENCES jugadores(id), caso_id text NOT NULL, semilla int NOT NULL,
                fundada boolean NOT NULL, motivo text NOT NULL, honorarios int NOT NULL, experiencia int NOT NULL, fecha timestamptz NOT NULL,
                PRIMARY KEY (partida_id, jugador_id));
            CREATE TABLE IF NOT EXISTS preguntas (
                id text PRIMARY KEY, calabozo text NOT NULL, estado text NOT NULL, datos jsonb NOT NULL, actualizado timestamptz NOT NULL DEFAULT now());
            """;
        await using var cmd = _fuente.CreateCommand(sql);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<Jugador> CrearJugadorAsync(string alias)
    {
        var j = new Jugador(Guid.NewGuid(), alias, Tokens.Nuevo(), DateTime.UtcNow);
        await using var cmd = _fuente.CreateCommand("INSERT INTO jugadores (id, alias, token, creado) VALUES ($1, $2, $3, $4)");
        cmd.Parameters.AddWithValue(j.Id); cmd.Parameters.AddWithValue(j.Alias); cmd.Parameters.AddWithValue(j.Token); cmd.Parameters.AddWithValue(j.Creado);
        await cmd.ExecuteNonQueryAsync();
        return j;
    }

    public async Task<Jugador?> BuscarPorTokenAsync(string token)
    {
        await using var cmd = _fuente.CreateCommand("SELECT id, alias, token, creado FROM jugadores WHERE token = $1");
        cmd.Parameters.AddWithValue(token);
        await using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? new Jugador(r.GetGuid(0), r.GetString(1), r.GetString(2), r.GetDateTime(3)) : null;
    }

    public async Task<EstudioGuardado> LeerEstudioAsync(Guid jugadorId)
    {
        await using var cmd = _fuente.CreateCommand("SELECT honorarios, experiencia, reputacion, disciplinario, cerrados FROM estudios WHERE jugador_id = $1");
        cmd.Parameters.AddWithValue(jugadorId);
        await using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync()
            ? new EstudioGuardado(jugadorId, r.GetInt32(0), r.GetInt32(1), r.GetInt32(2), r.GetInt32(3), r.GetInt32(4))
            : new EstudioGuardado(jugadorId, 0, 0, 50, 0, 0);
    }

    public async Task GuardarEstudioAsync(EstudioGuardado e)
    {
        await using var cmd = _fuente.CreateCommand("""
            INSERT INTO estudios (jugador_id, honorarios, experiencia, reputacion, disciplinario, cerrados) VALUES ($1,$2,$3,$4,$5,$6)
            ON CONFLICT (jugador_id) DO UPDATE SET honorarios=$2, experiencia=$3, reputacion=$4, disciplinario=$5, cerrados=$6
            """);
        cmd.Parameters.AddWithValue(e.JugadorId); cmd.Parameters.AddWithValue(e.Honorarios); cmd.Parameters.AddWithValue(e.Experiencia);
        cmd.Parameters.AddWithValue(e.Reputacion); cmd.Parameters.AddWithValue(e.Disciplinario); cmd.Parameters.AddWithValue(e.Cerrados);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task GuardarResultadoAsync(ResultadoGuardado x)
    {
        await using var cmd = _fuente.CreateCommand("""
            INSERT INTO resultados (partida_id, jugador_id, caso_id, semilla, fundada, motivo, honorarios, experiencia, fecha)
            VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9) ON CONFLICT DO NOTHING
            """);
        cmd.Parameters.AddWithValue(x.PartidaId); cmd.Parameters.AddWithValue(x.JugadorId); cmd.Parameters.AddWithValue(x.CasoId);
        cmd.Parameters.AddWithValue(x.Semilla); cmd.Parameters.AddWithValue(x.Fundada); cmd.Parameters.AddWithValue(x.Motivo);
        cmd.Parameters.AddWithValue(x.Honorarios); cmd.Parameters.AddWithValue(x.Experiencia); cmd.Parameters.AddWithValue(x.Fecha);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<IReadOnlyList<ResultadoGuardado>> ResultadosAsync(Guid jugadorId)
    {
        await using var cmd = _fuente.CreateCommand("SELECT partida_id, jugador_id, caso_id, semilla, fundada, motivo, honorarios, experiencia, fecha FROM resultados WHERE jugador_id = $1 ORDER BY fecha DESC LIMIT 100");
        cmd.Parameters.AddWithValue(jugadorId);
        var lista = new List<ResultadoGuardado>();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync())
            lista.Add(new ResultadoGuardado(r.GetGuid(0), r.GetGuid(1), r.GetString(2), r.GetInt32(3), r.GetBoolean(4), r.GetString(5), r.GetInt32(6), r.GetInt32(7), r.GetDateTime(8)));
        return lista;
    }

    public async Task<IReadOnlyList<PreguntaGuardada>> PreguntasAsync()
    {
        await using var cmd = _fuente.CreateCommand("SELECT datos FROM preguntas ORDER BY id");
        var lista = new List<PreguntaGuardada>();
        await using var r = await cmd.ExecuteReaderAsync();
        while (await r.ReadAsync()) lista.Add(JsonSerializer.Deserialize<PreguntaGuardada>(r.GetString(0), Json.Opciones)!);
        return lista;
    }

    public async Task<PreguntaGuardada?> PreguntaAsync(string id)
    {
        await using var cmd = _fuente.CreateCommand("SELECT datos FROM preguntas WHERE id = $1");
        cmd.Parameters.AddWithValue(id);
        var datos = await cmd.ExecuteScalarAsync() as string;
        return datos == null ? null : JsonSerializer.Deserialize<PreguntaGuardada>(datos, Json.Opciones);
    }

    public async Task GuardarPreguntaAsync(PreguntaGuardada p)
    {
        await using var cmd = _fuente.CreateCommand("""
            INSERT INTO preguntas (id, calabozo, estado, datos, actualizado) VALUES ($1,$2,$3,$4::jsonb, now())
            ON CONFLICT (id) DO UPDATE SET calabozo=$2, estado=$3, datos=$4::jsonb, actualizado=now()
            """);
        cmd.Parameters.AddWithValue(p.Id); cmd.Parameters.AddWithValue(p.Calabozo); cmd.Parameters.AddWithValue(p.Estado);
        cmd.Parameters.AddWithValue(JsonSerializer.Serialize(p, Json.Opciones));
        await cmd.ExecuteNonQueryAsync();
    }
}
