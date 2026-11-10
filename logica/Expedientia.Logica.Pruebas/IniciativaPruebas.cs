using Expedientia.Logica.Dados;
using Expedientia.Logica.Personajes;

namespace Expedientia.Logica.Pruebas;

public class IniciativaPruebas
{
    [Fact]
    public void Ordena_por_total_descendente()
    {
        // Cada participante consume 2 valores: d20 de iniciativa y d20 de desempate.
        var azar = new FalsoAzar(5, 1, 15, 1, 10, 1);
        var orden = Iniciativa.Ordenar(azar, new[] { new Participante("a", 0), new Participante("b", 0), new Participante("c", 0) });
        Assert.Equal(new[] { "b", "c", "a" }, orden.Select(t => t.Participante.Id));
    }

    [Fact]
    public void Empate_lo_gana_el_mayor_modificador()
    {
        var azar = new FalsoAzar(12, 20, 10, 1); // a: 12+0=12 ; b: 10+2=12
        var orden = Iniciativa.Ordenar(azar, new[] { new Participante("a", 0), new Participante("b", 2) });
        Assert.Equal("b", orden[0].Participante.Id);
    }

    [Fact]
    public void Es_determinista_con_la_misma_semilla()
    {
        var gente = Enumerable.Range(1, 6).Select(i => new Participante($"p{i}", i % 3)).ToArray();
        var x = Iniciativa.Ordenar(new GeneradorConSemilla(99), gente).Select(t => t.Participante.Id);
        var y = Iniciativa.Ordenar(new GeneradorConSemilla(99), gente).Select(t => t.Participante.Id);
        Assert.Equal(x, y);
    }
}
