using System;

namespace Expedientia.Logica.Combate
{
    /// <summary>
    /// Caminos por el tablero. Se avanza casilla a casilla (diagonales incluidas, como en el SRD):
    /// un rival bloquea el paso; un aliado deja pasar pero no se puede terminar en su casilla.
    /// La búsqueda es un relleno recursivo en profundidad con poda: cada casilla recuerda el mayor
    /// movimiento con que se llegó a ella y solo se vuelve a explorar si ahora se llega con más.
    /// No reserva memoria: trabaja sobre arreglos de 48 casillas que el combate reutiliza.
    /// </summary>
    public static class Rutas
    {
        public const byte Libre = 0, Aliado = 1, Rival = 2;
        public const int Casillas = Cuadricula.Columnas * Cuadricula.Filas;

        public static int Indice(Casilla c) => c.Fila * Cuadricula.Columnas + c.Columna;
        public static Casilla Casilla(int indice) => new Casilla(indice % Cuadricula.Columnas, indice / Cuadricula.Columnas);

        /// <summary>
        /// Llena <paramref name="restante"/> con el movimiento que sobra al llegar a cada casilla (-1 si no se llega).
        /// </summary>
        public static void Explorar(Casilla origen, int movimiento, ReadOnlySpan<byte> ocupacion, Span<int> restante)
        {
            restante.Fill(-1);
            Visitar(origen.Columna, origen.Fila, movimiento, ocupacion, restante);
        }

        private static void Visitar(int x, int y, int quedan, ReadOnlySpan<byte> ocupacion, Span<int> restante)
        {
            var i = y * Cuadricula.Columnas + x;
            if (quedan <= restante[i]) return; // ya se llegó aquí con igual o más movimiento: poda
            restante[i] = quedan;
            if (quedan == 0) return;
            for (var dy = -1; dy <= 1; dy++)
            {
                var ny = y + dy;
                if (ny < 0 || ny >= Cuadricula.Filas) continue;
                for (var dx = -1; dx <= 1; dx++)
                {
                    var nx = x + dx;
                    if ((dx | dy) == 0 || nx < 0 || nx >= Cuadricula.Columnas) continue;
                    if (ocupacion[ny * Cuadricula.Columnas + nx] == Rival) continue;
                    Visitar(nx, ny, quedan - 1, ocupacion, restante);
                }
            }
        }
    }
}
