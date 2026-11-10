using Expedientia.Logica.Dados;
using Expedientia.Logica.Personajes;

namespace Expedientia.Logica.Pruebas;

public class ReglasPruebas
{
    private static Atributos Civilista() => new(firmeza: 8, agilidadProcesal: 12, aguante: 13, doctrina: 16, criterio: 14, oratoria: 10);

    [Theory]
    [InlineData(1, -5)] [InlineData(8, -1)] [InlineData(9, -1)] [InlineData(10, 0)]
    [InlineData(11, 0)] [InlineData(16, 3)] [InlineData(20, 5)] [InlineData(30, 10)]
    public void Modificador_del_SRD(int valor, int mod) => Assert.Equal(mod, Reglas.Modificador(valor));

    [Theory]
    [InlineData(1, 2)] [InlineData(4, 2)] [InlineData(5, 3)] [InlineData(9, 4)]
    [InlineData(13, 5)] [InlineData(17, 6)] [InlineData(20, 6)]
    public void Competencia_por_nivel(int nivel, int bono) => Assert.Equal(bono, Reglas.Competencia(nivel));

    [Theory]
    [InlineData(1, "Practicante")] [InlineData(5, "Bachiller")] [InlineData(12, "Abogado")]
    [InlineData(13, "Asociado")] [InlineData(20, "Socio")]
    public void Titulos_de_carrera(int nivel, string titulo) => Assert.Equal(titulo, Reglas.TituloDeCarrera(nivel));

    [Fact]
    public void Nivel_fuera_de_rango_falla() => Assert.Throws<ArgumentOutOfRangeException>(() => Reglas.Competencia(21));

    [Fact]
    public void Atributo_fuera_de_rango_falla() => Assert.Throws<ArgumentOutOfRangeException>(() => new Atributos(0, 10, 10, 10, 10, 10));

    [Fact]
    public void Prueba_de_atributo_suma_modificador_y_competencia()
    {
        // Doctrina 16 (+3) + competencia nivel 1 (+2) + 10 = 15 contra dificultad 15
        var r = Personajes.Pruebas.DeAtributo(new FalsoAzar(10), Civilista(), Atributo.Doctrina, 1, true, 15);
        Assert.Equal(15, r.Total);
        Assert.True(r.Exito);
    }

    [Fact]
    public void Ataque_con_20_natural_acierta_siempre_y_es_Fundado()
    {
        var r = Personajes.Pruebas.Ataque(new FalsoAzar(20), Civilista(), Atributo.Doctrina, 1, defensa: 30);
        Assert.True(r.Exito);
        Assert.True(r.EsCritico);
    }

    [Fact]
    public void Ataque_con_1_natural_falla_siempre()
    {
        var r = Personajes.Pruebas.Ataque(new FalsoAzar(1), Civilista(), Atributo.Doctrina, 20, defensa: 2);
        Assert.False(r.Exito);
    }

    [Fact]
    public void Prueba_de_atributo_con_20_no_es_exito_automatico()
    {
        // Firmeza 8 (−1), sin competencia: 20 − 1 = 19 < 25
        var r = Personajes.Pruebas.DeAtributo(new FalsoAzar(20), Civilista(), Atributo.Firmeza, 1, false, 25);
        Assert.False(r.Exito);
    }

    [Fact]
    public void Danio_critico_duplica_dados_no_modificador()
    {
        Assert.Equal(4 + 3, Personajes.Pruebas.Danio(new FalsoAzar(4), 1, 6, 3, critico: false));
        Assert.Equal(4 + 5 + 3, Personajes.Pruebas.Danio(new FalsoAzar(4, 5), 1, 6, 3, critico: true));
    }

    [Fact]
    public void Danio_nunca_es_negativo() => Assert.Equal(0, Personajes.Pruebas.Danio(new FalsoAzar(1), 1, 4, -5, false));
}
