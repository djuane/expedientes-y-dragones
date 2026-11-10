using Expedientia.Logica.Dados;

namespace Expedientia.Logica.Pruebas;

public class DadosPruebas
{
    [Fact]
    public void Ventaja_toma_el_mayor() => Assert.Equal(17, Dado.D20(new FalsoAzar(4, 17), Modo.Ventaja).Natural);

    [Fact]
    public void Desventaja_toma_el_menor() => Assert.Equal(4, Dado.D20(new FalsoAzar(4, 17), Modo.Desventaja).Natural);

    [Fact]
    public void Normal_tira_un_solo_dado()
    {
        var t = Dado.D20(new FalsoAzar(9), Modo.Normal);
        Assert.Equal(9, t.Natural);
        Assert.Null(t.Segundo);
    }

    [Theory]
    [InlineData(20, Momento.Fundado)]
    [InlineData(1, Momento.Improcedente)]
    [InlineData(10, Momento.Ninguno)]
    public void Momentos_especiales(int natural, Momento esperado) =>
        Assert.Equal(esperado, Dado.D20(new FalsoAzar(natural)).Momento);

    [Theory]
    [InlineData(true, false, Modo.Ventaja)]
    [InlineData(false, true, Modo.Desventaja)]
    [InlineData(true, true, Modo.Normal)]
    [InlineData(false, false, Modo.Normal)]
    public void Ventaja_y_desventaja_se_anulan(bool v, bool d, Modo esperado) => Assert.Equal(esperado, Dado.Combinar(v, d));

    [Fact]
    public void Misma_semilla_mismas_tiradas()
    {
        var a = new GeneradorConSemilla(42);
        var b = new GeneradorConSemilla(42);
        for (var i = 0; i < 100; i++) Assert.Equal(Dado.Tirar(a, 20), Dado.Tirar(b, 20));
    }

    [Fact]
    public void D20_siempre_entre_1_y_20()
    {
        var azar = new GeneradorConSemilla(7);
        var vistos = new HashSet<int>();
        for (var i = 0; i < 2000; i++)
        {
            var n = Dado.Tirar(azar, 20);
            Assert.InRange(n, 1, 20);
            vistos.Add(n);
        }
        Assert.Equal(20, vistos.Count);
    }

    [Fact]
    public void Suma_de_varios_dados() => Assert.Equal(9, Dado.Tirar(new FalsoAzar(3, 6), 2, 6));
}
