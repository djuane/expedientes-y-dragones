using Expedientia.Logica.Combate;

namespace Expedientia.Logica.Expedientes
{
    public sealed class SalaIniciada : EventoCombate { public int Indice { get; } public TipoSala Tipo { get; } public SalaIniciada(int i, TipoSala t) { Indice = i; Tipo = t; } }
    public sealed class SalaSuperada : EventoCombate { public int Indice { get; } public string Detalle { get; } public SalaSuperada(int i, string d) { Indice = i; Detalle = d; } }
    public sealed class RelojAvanzo : EventoCombate { public int MinutosRestantes { get; } public RelojAvanzo(int m) { MinutosRestantes = m; } }
    public sealed class ComentarioDeDonEusebio : EventoCombate { public string Texto { get; } public ComentarioDeDonEusebio(string t) { Texto = t; } }
    public sealed class SentenciaDictada : EventoCombate { public Sentencia Sentencia { get; } public SentenciaDictada(Sentencia s) { Sentencia = s; } }
}
