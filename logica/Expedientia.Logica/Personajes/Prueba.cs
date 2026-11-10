using Expedientia.Logica.Dados;

namespace Expedientia.Logica.Personajes
{
    /// <summary>Resultado de una tirada contra una defensa o una dificultad.</summary>
    public sealed class ResultadoPrueba
    {
        public TiradaD20 Tirada { get; }
        public int Bonificador { get; }
        public int Objetivo { get; }
        public int Total => Tirada.Natural + Bonificador;

        /// <summary>
        /// En ataques, 20 natural acierta siempre y 1 natural falla siempre (SRD).
        /// En pruebas de atributo se compara solo el total.
        /// </summary>
        public bool Exito { get; }
        public bool EsCritico => EsAtaque && Tirada.Momento == Momento.Fundado;
        public bool EsAtaque { get; }

        internal ResultadoPrueba(TiradaD20 tirada, int bonificador, int objetivo, bool esAtaque)
        {
            Tirada = tirada; Bonificador = bonificador; Objetivo = objetivo; EsAtaque = esAtaque;
            if (esAtaque && tirada.Momento == Momento.Fundado) Exito = true;
            else if (esAtaque && tirada.Momento == Momento.Improcedente) Exito = false;
            else Exito = Total >= objetivo;
        }
    }

    public static class Pruebas
    {
        /// <summary>Tirada base: d20 + modificador + competencia (si aplica) contra la dificultad.</summary>
        public static ResultadoPrueba DeAtributo(IGeneradorAleatorio azar, Atributos atributos, Atributo atributo,
            int nivel, bool competente, int dificultad, Modo modo = Modo.Normal)
        {
            var bono = atributos.Modificador(atributo) + (competente ? Reglas.Competencia(nivel) : 0);
            return new ResultadoPrueba(Dado.D20(azar, modo), bono, dificultad, esAtaque: false);
        }

        /// <summary>Ataque: d20 + modificador + competencia contra la defensa del objetivo.</summary>
        public static ResultadoPrueba Ataque(IGeneradorAleatorio azar, Atributos atributos, Atributo atributo,
            int nivel, int defensa, Modo modo = Modo.Normal)
        {
            var bono = atributos.Modificador(atributo) + Reglas.Competencia(nivel);
            return new ResultadoPrueba(Dado.D20(azar, modo), bono, defensa, esAtaque: true);
        }

        /// <summary>Daño: en un "Fundado" se tiran el doble de dados (crítico del SRD); el modificador no se duplica.</summary>
        public static int Danio(IGeneradorAleatorio azar, int cantidadDados, int caras, int modificador, bool critico)
        {
            var total = Dado.Tirar(azar, critico ? cantidadDados * 2 : cantidadDados, caras) + modificador;
            return total < 0 ? 0 : total;
        }
    }
}
