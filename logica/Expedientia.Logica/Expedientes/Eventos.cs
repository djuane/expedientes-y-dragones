using System.Collections.Generic;
using Expedientia.Logica.Combate;

namespace Expedientia.Logica.Expedientes
{
    public sealed class SalaIniciada : EventoCombate { public int Indice { get; } public TipoSala Tipo { get; } public SalaIniciada(int i, TipoSala t) { Indice = i; Tipo = t; } }
    public sealed class SalaSuperada : EventoCombate { public int Indice { get; } public string Detalle { get; } public SalaSuperada(int i, string d) { Indice = i; Detalle = d; } }
    public sealed class RelojAvanzo : EventoCombate { public int MinutosRestantes { get; } public RelojAvanzo(int m) { MinutosRestantes = m; } }
    public sealed class ComentarioDeDonEusebio : EventoCombate { public string Texto { get; } public ComentarioDeDonEusebio(string t) { Texto = t; } }
    public sealed class SentenciaDictada : EventoCombate { public Sentencia Sentencia { get; } public SentenciaDictada(Sentencia s) { Sentencia = s; } }
    /// <summary>Una línea de la trama: la capa visual la muestra con el retrato del personaje.</summary>
    public sealed class Dialogo : EventoCombate { public string Personaje { get; } public string Texto { get; } public Dialogo(string p, string t) { Personaje = p; Texto = t; } }
    /// <summary>Al ganar un combate el equipo elige 1 de estas mejoras.</summary>
    public sealed class MejorasOfrecidas : EventoCombate { public IReadOnlyList<Mejora> Opciones { get; } public MejorasOfrecidas(IReadOnlyList<Mejora> o) { Opciones = o; } }
    public sealed class MejoraElegida : EventoCombate
    {
        public Mejora Mejora { get; } public string Nombre { get; } public string Descripcion { get; }
        public MejoraElegida(Mejora m) { Mejora = m; Nombre = Mejoras.Datos(m).Nombre; Descripcion = Mejoras.Datos(m).Descripcion; }
    }
    /// <summary>El estudio ganó algo para siempre al cerrar este expediente.</summary>
    public sealed class DesbloqueoGanado : EventoCombate
    {
        public string Nombre { get; } public string Descripcion { get; }
        public DesbloqueoGanado(DatosDesbloqueo d) { Nombre = d.Nombre; Descripcion = d.Descripcion; }
    }
}
