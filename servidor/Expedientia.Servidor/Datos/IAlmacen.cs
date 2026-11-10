namespace Expedientia.Servidor.Datos;

public interface IAlmacen
{
    Task PrepararAsync();
    Task<Jugador> CrearJugadorAsync(string alias);
    Task<Jugador?> BuscarPorTokenAsync(string token);
    Task<EstudioGuardado> LeerEstudioAsync(Guid jugadorId);
    Task GuardarEstudioAsync(EstudioGuardado estudio);
    Task GuardarResultadoAsync(ResultadoGuardado resultado);
    Task<IReadOnlyList<ResultadoGuardado>> ResultadosAsync(Guid jugadorId);
    Task<IReadOnlyList<PreguntaGuardada>> PreguntasAsync();
    Task<PreguntaGuardada?> PreguntaAsync(string id);
    Task GuardarPreguntaAsync(PreguntaGuardada pregunta);
}

internal static class Tokens
{
    public static string Nuevo() => Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24)).ToLowerInvariant();
}
