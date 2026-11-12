using System;
using System.Collections.Generic;
using System.Linq;

namespace Expedientia.Logica.Expedientes
{
    /// <summary>Lo que el estudio gana para siempre al ir cerrando expedientes. Es la meta entre partidas.</summary>
    public enum Desbloqueo { MejoraDeEntrada, BibliotecaDelEstudio, SegundaOpinion, OficinaPropia }

    public sealed class DatosDesbloqueo
    {
        public Desbloqueo Desbloqueo { get; }
        public int ExpedientesNecesarios { get; }
        public string Nombre { get; }
        public string Descripcion { get; }
        internal DatosDesbloqueo(Desbloqueo d, int n, string nombre, string descripcion)
        { Desbloqueo = d; ExpedientesNecesarios = n; Nombre = nombre; Descripcion = descripcion; }
    }

    /// <summary>Progreso que se conserva entre expedientes.</summary>
    public sealed class EstadoEstudio
    {
        // Tabla de experiencia del SRD 5.2 (niveles 1 a 20).
        private static readonly int[] UmbralesDeNivel =
        {
            0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000,
            85000, 100000, 120000, 140000, 165000, 195000, 225000, 265000, 305000, 355000,
        };

        public int Honorarios { get; internal set; }
        public int Experiencia { get; internal set; }
        /// <summary>Visible, de 0 a 100.</summary>
        public int Reputacion { get; private set; } = 50;
        /// <summary>Oculto al jugador; se alimenta de las ofertas del Coimero aceptadas.</summary>
        public int ExpedienteDisciplinario { get; internal set; }
        public int ExpedientesCerrados { get; internal set; }

        public int Nivel
        {
            get
            {
                var nivel = 1;
                for (var i = 1; i < UmbralesDeNivel.Length; i++) if (Experiencia >= UmbralesDeNivel[i]) nivel = i + 1;
                return nivel;
            }
        }

        /// <summary>Desbloqueos en orden; cada uno se gana al llegar a esa cantidad de expedientes cerrados (Fundado).</summary>
        public static readonly IReadOnlyList<DatosDesbloqueo> Desbloqueos = new[]
        {
            new DatosDesbloqueo(Desbloqueo.MejoraDeEntrada, 1, "Maletín del estudio", "Eliges una mejora antes del primer combate."),
            new DatosDesbloqueo(Desbloqueo.BibliotecaDelEstudio, 2, "Biblioteca del estudio", "Aparecen mejoras raras en el sorteo."),
            new DatosDesbloqueo(Desbloqueo.SegundaOpinion, 3, "Segunda opinión", "Una vez por expediente puedes volver a sortear las mejoras."),
            new DatosDesbloqueo(Desbloqueo.OficinaPropia, 5, "Oficina propia", "Todo el equipo empieza con +4 de paciencia máxima."),
        };

        public bool Tiene(Desbloqueo d) => ExpedientesCerrados >= Desbloqueos.First(x => x.Desbloqueo == d).ExpedientesNecesarios;

        /// <summary>El próximo desbloqueo por ganar, o null si ya los tiene todos.</summary>
        public DatosDesbloqueo? Proximo => Desbloqueos.FirstOrDefault(d => ExpedientesCerrados < d.ExpedientesNecesarios);

        public static int ExperienciaParaNivel(int nivel) => UmbralesDeNivel[nivel - 1];

        internal void CambiarReputacion(int delta) => Reputacion = Math.Max(0, Math.Min(100, Reputacion + delta));

        /// <summary>Con buena reputación aparecen los aliados honestos (Suboficial Quispe, Especialista Mendoza).</summary>
        public bool AliadosHonestosDisponibles => Reputacion >= 70;

        public static EstadoEstudio Restaurar(int honorarios, int experiencia, int reputacion, int disciplinario, int cerrados)
        {
            var e = new EstadoEstudio { Honorarios = honorarios, Experiencia = experiencia, ExpedienteDisciplinario = disciplinario, ExpedientesCerrados = cerrados };
            e.Reputacion = Math.Max(0, Math.Min(100, reputacion));
            return e;
        }
    }
}
