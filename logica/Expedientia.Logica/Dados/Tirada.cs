using System;

namespace Expedientia.Logica.Dados
{
    public enum Modo { Normal, Ventaja, Desventaja }

    /// <summary>Momento especial del dado (pilar "el dado como espectáculo").</summary>
    public enum Momento { Ninguno, Fundado, Improcedente }

    /// <summary>Resultado de un d20 con ventaja o desventaja, listo para que la capa visual lo anime.</summary>
    public sealed class TiradaD20
    {
        public int Primero { get; }
        public int? Segundo { get; }
        public Modo Modo { get; }
        public int Natural { get; }
        public Momento Momento => Natural == 20 ? Momento.Fundado : Natural == 1 ? Momento.Improcedente : Momento.Ninguno;

        internal TiradaD20(int primero, int? segundo, Modo modo)
        {
            Primero = primero; Segundo = segundo; Modo = modo;
            Natural = modo switch
            {
                Modo.Ventaja => Math.Max(primero, segundo!.Value),
                Modo.Desventaja => Math.Min(primero, segundo!.Value),
                _ => primero,
            };
        }
    }

    public static class Dado
    {
        public static int Tirar(IGeneradorAleatorio azar, int caras)
        {
            if (caras < 2) throw new ArgumentOutOfRangeException(nameof(caras));
            return azar.Entre(1, caras);
        }

        /// <summary>Suma de <paramref name="cantidad"/> dados de <paramref name="caras"/> caras (por ejemplo, 2d6).</summary>
        public static int Tirar(IGeneradorAleatorio azar, int cantidad, int caras)
        {
            if (cantidad < 1) throw new ArgumentOutOfRangeException(nameof(cantidad));
            var total = 0;
            for (var i = 0; i < cantidad; i++) total += Tirar(azar, caras);
            return total;
        }

        public static TiradaD20 D20(IGeneradorAleatorio azar, Modo modo = Modo.Normal)
        {
            var primero = Tirar(azar, 20);
            int? segundo = modo == Modo.Normal ? (int?)null : Tirar(azar, 20);
            return new TiradaD20(primero, segundo, modo);
        }

        /// <summary>Regla del SRD: ventaja y desventaja a la vez se anulan, sin importar cuántas fuentes haya.</summary>
        public static Modo Combinar(bool hayVentaja, bool hayDesventaja)
        {
            if (hayVentaja == hayDesventaja) return Modo.Normal;
            return hayVentaja ? Modo.Ventaja : Modo.Desventaja;
        }
    }
}
