using Expedientia.Logica.Combate;
using Expedientia.Logica.Dados;
using CombateMotor = Expedientia.Logica.Combate.Combate;

namespace Expedientia.Logica.Pruebas;

/// <summary>Caminos por el tablero (v0.9.0): los rivales bloquean, los aliados dejan pasar.</summary>
public class RutasPruebas
{
    private static int[] Explorar(Casilla origen, int movimiento, params (Casilla c, byte quien)[] ocupadas)
    {
        var ocupacion = new byte[Rutas.Casillas];
        foreach (var (c, quien) in ocupadas) ocupacion[Rutas.Indice(c)] = quien;
        var restante = new int[Rutas.Casillas];
        Rutas.Explorar(origen, movimiento, ocupacion, restante);
        return restante;
    }

    [Fact]
    public void En_tablero_libre_el_costo_es_la_distancia_con_diagonales()
    {
        var r = Explorar(new Casilla(0, 0), 3);
        for (var i = 0; i < Rutas.Casillas; i++)
        {
            var d = new Casilla(0, 0).DistanciaA(Rutas.Casilla(i));
            Assert.Equal(d <= 3 ? 3 - d : -1, r[i]);
        }
    }

    [Fact]
    public void Una_fila_de_rivales_es_un_muro()
    {
        var muro = Enumerable.Range(0, Cuadricula.Columnas).Select(x => (new Casilla(x, 1), Rutas.Rival)).ToArray();
        var r = Explorar(new Casilla(2, 0), 8, muro);
        for (var i = 0; i < Rutas.Casillas; i++)
            if (Rutas.Casilla(i).Fila >= 2) Assert.Equal(-1, r[i]);
    }

    [Fact]
    public void Los_aliados_dejan_pasar_y_los_rivales_no()
    {
        var rodeo = new[] { new Casilla(1, 0), new Casilla(0, 1), new Casilla(1, 1) };
        var conAliados = Explorar(new Casilla(0, 0), 2, rodeo.Select(c => (c, Rutas.Aliado)).ToArray());
        Assert.Equal(0, conAliados[Rutas.Indice(new Casilla(2, 2))]);
        var conRivales = Explorar(new Casilla(0, 0), 2, rodeo.Select(c => (c, Rutas.Rival)).ToArray());
        Assert.Equal(-1, conRivales[Rutas.Indice(new Casilla(2, 2))]);
    }

    [Fact]
    public void Rodear_un_obstaculo_cuesta_mas_movimiento()
    {
        var muro = new[] { (new Casilla(1, 1), Rutas.Rival), (new Casilla(2, 1), Rutas.Rival), (new Casilla(3, 1), Rutas.Rival) };
        Assert.Equal(2, Explorar(new Casilla(2, 0), 6, muro)[Rutas.Indice(new Casilla(2, 2))]); // 4 pasos, no 2
        Assert.Equal(-1, Explorar(new Casilla(2, 0), 3, muro)[Rutas.Indice(new Casilla(2, 2))]);
    }

    [Fact]
    public void El_combate_no_deja_atravesar_rivales_y_cobra_el_rodeo()
    {
        var civ = new Combatiente(Catalogo.Civilista("civ", "Civ"), new Casilla(1, 0));
        var aliado = new Combatiente(Catalogo.Civilista("civ2", "Civ 2"), new Casilla(5, 7));
        var f1 = new Combatiente(Catalogo.Formalismo("f1"), new Casilla(0, 1));
        var f2 = new Combatiente(Catalogo.Formalismo("f2"), new Casilla(1, 1));
        var f3 = new Combatiente(Catalogo.Formalismo("f3"), new Casilla(2, 1));
        var c = new CombateMotor(new GeneradorConSemilla(1), new[] { civ, aliado, f1, f2, f3 });
        c.Iniciar();
        var guardia = 0;
        while (c.Actual.Id != "civ" && guardia++ < 20) c.TerminarTurno();
        Assert.Equal("civ", c.Actual.Id);
        // (2,2) está a 2 casillas en línea recta, pero el muro obliga a rodear por (2,0) y (3,1): 3 pasos.
        Assert.False(c.PuedeMoverA(new Casilla(0, 2), out var motivo));
        Assert.Equal("El camino está bloqueado.", motivo);
        Assert.Equal(0, c.MovimientoAlLlegar(new Casilla(2, 2)));
        Assert.DoesNotContain(new Casilla(0, 2), c.CasillasValidas());
        Assert.True(c.Mover(new Casilla(2, 2)).Valida);
        Assert.Equal(0, civ.MovimientoRestante);
    }
}
