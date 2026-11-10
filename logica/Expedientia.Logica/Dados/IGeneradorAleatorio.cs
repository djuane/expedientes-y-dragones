namespace Expedientia.Logica.Dados
{
    /// <summary>Fuente de azar inyectable: el servidor usa una semilla por partida y las pruebas, valores fijos.</summary>
    public interface IGeneradorAleatorio
    {
        /// <summary>Entero entre <paramref name="minimo"/> y <paramref name="maximo"/>, ambos incluidos.</summary>
        int Entre(int minimo, int maximo);
    }

    /// <summary>Generador reproducible a partir de una semilla (misma semilla, mismas tiradas).</summary>
    public sealed class GeneradorConSemilla : IGeneradorAleatorio
    {
        private readonly System.Random _random;
        public GeneradorConSemilla(int semilla) { _random = new System.Random(semilla); }
        public int Entre(int minimo, int maximo) => _random.Next(minimo, maximo + 1);
    }
}
