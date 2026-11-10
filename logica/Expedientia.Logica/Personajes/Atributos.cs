using System;

namespace Expedientia.Logica.Personajes
{
    /// <summary>Los seis atributos del SRD con nombre de oficio.</summary>
    public enum Atributo
    {
        Firmeza,          // Fuerza
        AgilidadProcesal, // Destreza
        Aguante,          // Constitución
        Doctrina,         // Inteligencia
        Criterio,         // Sabiduría
        Oratoria,         // Carisma
    }

    public sealed class Atributos
    {
        private readonly int[] _valores = new int[6];

        public Atributos(int firmeza, int agilidadProcesal, int aguante, int doctrina, int criterio, int oratoria)
        {
            Fijar(Atributo.Firmeza, firmeza);
            Fijar(Atributo.AgilidadProcesal, agilidadProcesal);
            Fijar(Atributo.Aguante, aguante);
            Fijar(Atributo.Doctrina, doctrina);
            Fijar(Atributo.Criterio, criterio);
            Fijar(Atributo.Oratoria, oratoria);
        }

        public int Valor(Atributo a) => _valores[(int)a];
        public int Modificador(Atributo a) => Reglas.Modificador(Valor(a));

        private void Fijar(Atributo a, int valor)
        {
            if (valor < 1 || valor > 30) throw new ArgumentOutOfRangeException(a.ToString(), "Los atributos van de 1 a 30.");
            _valores[(int)a] = valor;
        }
    }

    public static class Reglas
    {
        public const int NivelMaximo = 20;

        /// <summary>Modificador del SRD: (valor − 10) / 2 redondeado hacia abajo.</summary>
        public static int Modificador(int valor) => (int)Math.Floor((valor - 10) / 2.0);

        /// <summary>Bonificador de competencia del SRD: +2 en niveles 1–4 y sube 1 cada 4 niveles hasta +6.</summary>
        public static int Competencia(int nivel)
        {
            if (nivel < 1 || nivel > NivelMaximo) throw new ArgumentOutOfRangeException(nameof(nivel));
            return 2 + (nivel - 1) / 4;
        }

        /// <summary>Título de carrera según el nivel (GDD: practicante, bachiller, abogado, asociado, socio).</summary>
        public static string TituloDeCarrera(int nivel)
        {
            Competencia(nivel); // valida el rango
            if (nivel <= 4) return "Practicante";
            if (nivel <= 8) return "Bachiller";
            if (nivel <= 12) return "Abogado";
            if (nivel <= 16) return "Asociado";
            return "Socio";
        }
    }
}
