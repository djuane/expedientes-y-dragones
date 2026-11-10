using Expedientia.Logica.Dados;

namespace Expedientia.Logica.Combate
{
    /// <summary>
    /// La lógica solo emite eventos; la capa visual (2D hoy, 3D mañana) decide cómo animarlos.
    /// </summary>
    public abstract class EventoCombate { }

    public sealed class InicioDeTurno : EventoCombate { public string Id { get; } public int Ronda { get; } public InicioDeTurno(string id, int ronda) { Id = id; Ronda = ronda; } }
    public sealed class Movio : EventoCombate { public string Id { get; } public Casilla Desde { get; } public Casilla Hasta { get; } public Movio(string id, Casilla d, Casilla h) { Id = id; Desde = d; Hasta = h; } }
    public sealed class Ataco : EventoCombate
    {
        public string Atacante { get; } public string Objetivo { get; } public int Natural { get; } public int Total { get; } public bool Acierto { get; } public Momento Momento { get; }
        public Ataco(string a, string o, int n, int t, bool ac, Momento m) { Atacante = a; Objetivo = o; Natural = n; Total = t; Acierto = ac; Momento = m; }
    }
    public sealed class RecibioDanio : EventoCombate { public string Id { get; } public int Cantidad { get; } public int PacienciaRestante { get; } public RecibioDanio(string id, int c, int r) { Id = id; Cantidad = c; PacienciaRestante = r; } }
    public sealed class Cayo : EventoCombate { public string Id { get; } public Cayo(string id) { Id = id; } }
    public sealed class UsoHabilidad : EventoCombate { public string Id { get; } public Habilidad Habilidad { get; } public string? Objetivo { get; } public UsoHabilidad(string id, Habilidad h, string? o) { Id = id; Habilidad = h; Objetivo = o; } }
    public sealed class Inmovilizado : EventoCombate { public string Id { get; } public int Turnos { get; } public Inmovilizado(string id, int t) { Id = id; Turnos = t; } }
    public sealed class Aparecio : EventoCombate { public string Id { get; } public string Nombre { get; } public Casilla Casilla { get; } public Aparecio(string id, string n, Casilla c) { Id = id; Nombre = n; Casilla = c; } }
    public sealed class FinDeCombate : EventoCombate { public Bando Ganador { get; } public string Motivo { get; } public FinDeCombate(Bando g, string m) { Ganador = g; Motivo = m; } }
}
