using System;
using System.Collections.Generic;
using System.Linq;
using Expedientia.Logica.Combate;

namespace Expedientia.Logica.Expedientes
{
    /// <summary>
    /// Mejoras que el equipo elige (1 de 3) al ganar cada combate. Valen para todo el equipo y solo durante el expediente.
    /// Son el gancho de cada partida: ninguna se arma igual que la anterior.
    /// </summary>
    public enum Mejora
    {
        DoctrinaActualizada,
        TernoAMedida,
        TermoDeCafe,
        ZapatosComodos,
        SelloDeUrgencia,
        AgendaDePlazos,
        CodigoComentado,
        // Raras: entran al sorteo con el desbloqueo «Biblioteca del estudio».
        AsistentePracticante,
        JurisprudenciaVinculante,
        ArgumentoDeCierre,
    }

    public sealed class DatosMejora
    {
        public Mejora Mejora { get; }
        public string Nombre { get; }
        public string Descripcion { get; }
        public bool Rara { get; }
        internal DatosMejora(Mejora m, string nombre, string descripcion, bool rara = false)
        { Mejora = m; Nombre = nombre; Descripcion = descripcion; Rara = rara; }
    }

    public static class Mejoras
    {
        public const int OfertasPorSala = 3;

        private static readonly DatosMejora[] Tabla =
        {
            new DatosMejora(Mejora.DoctrinaActualizada, "Doctrina actualizada", "Los golpes del equipo pegan más: el dado de daño sube un tamaño."),
            new DatosMejora(Mejora.TernoAMedida, "Terno a medida", "+1 de defensa para todo el equipo."),
            new DatosMejora(Mejora.TermoDeCafe, "Termo de café", "+5 de paciencia máxima y recuperan 5 ahora."),
            new DatosMejora(Mejora.ZapatosComodos, "Zapatos cómodos", "+1 casilla de movimiento por turno."),
            new DatosMejora(Mejora.SelloDeUrgencia, "Sello de urgencia", "+1 de alcance para atacar."),
            new DatosMejora(Mejora.AgendaDePlazos, "Agenda de plazos", "+25 minutos al reloj del caso (sin reloj: S/ 25 en honorarios)."),
            new DatosMejora(Mejora.CodigoComentado, "Código comentado", "+1 a la tirada de ataque (mejor atributo de ataque)."),
            new DatosMejora(Mejora.AsistentePracticante, "Asistente practicante", "Todos recuperan toda la paciencia y los caídos vuelven.", true),
            new DatosMejora(Mejora.JurisprudenciaVinculante, "Jurisprudencia vinculante", "Un dado de daño más en cada golpe.", true),
            new DatosMejora(Mejora.ArgumentoDeCierre, "Argumento de cierre", "+8 de paciencia máxima y +1 de defensa.", true),
        };

        public static DatosMejora Datos(Mejora m) => Tabla[(int)m];

        public static IReadOnlyList<DatosMejora> Todas => Tabla;

        internal static List<Mejora> Sortear(Dados.IGeneradorAleatorio azar, bool conRaras, ICollection<Mejora> excluir)
        {
            var pozo = Tabla.Where(d => (conRaras || !d.Rara) && !excluir.Contains(d.Mejora)).Select(d => d.Mejora).ToList();
            var elegidas = new List<Mejora>();
            while (elegidas.Count < OfertasPorSala && pozo.Count > 0)
            {
                var i = azar.Entre(0, pozo.Count - 1);
                elegidas.Add(pozo[i]);
                pozo.RemoveAt(i);
            }
            return elegidas;
        }

        /// <summary>Aplica la mejora a un miembro del equipo (cambia su ficha).</summary>
        internal static void Aplicar(Mejora m, Combatiente c)
        {
            var f = c.Ficha;
            switch (m)
            {
                case Mejora.DoctrinaActualizada: c.Ficha = f.Con(caras: Math.Min(12, f.CarasDeDanio + 2)); break;
                case Mejora.TernoAMedida: c.Ficha = f.Con(defensa: f.Defensa + 1); break;
                case Mejora.TermoDeCafe:
                    c.Ficha = f.Con(paciencia: f.PacienciaMaxima + 5);
                    if (!c.Caido) c.Paciencia = Math.Min(c.Ficha.PacienciaMaxima, c.Paciencia + 5);
                    break;
                case Mejora.ZapatosComodos: c.Ficha = f.Con(movimiento: f.Movimiento + 1); break;
                case Mejora.SelloDeUrgencia: c.Ficha = f.Con(alcance: f.Alcance + 1); break;
                case Mejora.CodigoComentado: c.Ficha = f.Con(atributos: f.Atributos.Con(f.AtributoDeAtaque, +2)); break;
                case Mejora.AsistentePracticante: c.Paciencia = f.PacienciaMaxima; break;
                case Mejora.JurisprudenciaVinculante: c.Ficha = f.Con(dados: f.DadosDeDanio + 1); break;
                case Mejora.ArgumentoDeCierre:
                    c.Ficha = f.Con(paciencia: f.PacienciaMaxima + 8, defensa: f.Defensa + 1);
                    if (!c.Caido) c.Paciencia += 8;
                    break;
            }
        }

        /// <summary>Lo que elegiría un bot (o el juego si nadie elige): cura si el equipo está mal; si no, la que más pega.</summary>
        public static Mejora ElegirComoBot(IReadOnlyList<Mejora> ofertas, IEnumerable<Combatiente> equipo, int? minutosRestantes)
        {
            var lista = equipo.ToList();
            var salud = lista.Count == 0 ? 1.0 : lista.Average(c => Math.Max(0, c.Paciencia) / (double)c.Ficha.PacienciaMaxima);
            int Puntaje(Mejora m) => m switch
            {
                Mejora.AsistentePracticante => salud < 0.5 ? 100 : 30,
                Mejora.TermoDeCafe => salud < 0.5 ? 90 : 55,
                Mejora.AgendaDePlazos => minutosRestantes.HasValue && minutosRestantes < 50 ? 95 : 10,
                Mejora.JurisprudenciaVinculante => 85,
                Mejora.ArgumentoDeCierre => 80,
                Mejora.DoctrinaActualizada => 70,
                Mejora.CodigoComentado => 65,
                Mejora.TernoAMedida => 60,
                Mejora.SelloDeUrgencia => 40,
                Mejora.ZapatosComodos => 35,
                _ => 0,
            };
            return ofertas.OrderByDescending(Puntaje).First();
        }
    }
}
