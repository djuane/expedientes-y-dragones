namespace Expedientia.Servidor.Partidas;

public sealed record CrearJugadorPedido(string Alias);
public sealed record CrearPartidaPedido(string CasoId, string Especialidad, int? Semilla, int? Bots);
public sealed record UnirsePedido(string Especialidad);
