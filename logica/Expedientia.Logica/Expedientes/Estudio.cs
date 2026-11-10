using System;

namespace Expedientia.Logica.Expedientes
{
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
