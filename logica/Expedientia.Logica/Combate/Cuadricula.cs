using System;

namespace Expedientia.Logica.Combate
{
    /// <summary>Casilla de la cuadrícula de combate (columna, fila).</summary>
    public readonly struct Casilla : IEquatable<Casilla>
    {
        public int Columna { get; }
        public int Fila { get; }
        public Casilla(int columna, int fila) { Columna = columna; Fila = fila; }

        /// <summary>Distancia en casillas; las diagonales cuentan 1, como en el SRD.</summary>
        public int DistanciaA(Casilla otra) => Math.Max(Math.Abs(Columna - otra.Columna), Math.Abs(Fila - otra.Fila));

        public bool Equals(Casilla o) => Columna == o.Columna && Fila == o.Fila;
        public override bool Equals(object? obj) => obj is Casilla c && Equals(c);
        public override int GetHashCode() => Columna * 397 ^ Fila;
        public override string ToString() => $"({Columna},{Fila})";
        public static bool operator ==(Casilla a, Casilla b) => a.Equals(b);
        public static bool operator !=(Casilla a, Casilla b) => !a.Equals(b);
    }

    /// <summary>Tablero vertical de 6 columnas por 8 filas (GDD).</summary>
    public static class Cuadricula
    {
        public const int Columnas = 6;
        public const int Filas = 8;
        public static bool Contiene(Casilla c) => c.Columna >= 0 && c.Columna < Columnas && c.Fila >= 0 && c.Fila < Filas;
    }
}
