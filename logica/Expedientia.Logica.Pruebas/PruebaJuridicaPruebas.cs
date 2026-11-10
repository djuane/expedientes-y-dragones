using Expedientia.Logica.Dados;
using Expedientia.Logica.Juridico;

namespace Expedientia.Logica.Pruebas;

public class PruebaJuridicaPruebas
{
    private static readonly string[] Opciones = { "3 días", "5 días", "10 días", "15 días" };

    private static Pregunta Verificada() => new("mp-001", "Procesal civil", "Mesa de Partes",
        "Pregunta de ejemplo", Opciones, 2, "Norma de ejemplo", "https://ejemplo.invalid/norma",
        EstadoPregunta.Verificada, "Revisor de prueba", new DateTime(2026, 11, 10));

    [Fact]
    public void Acierto_da_ventaja()
    {
        var c = new ControlPruebasJuridicas();
        var r = c.Evaluar(Verificada(), 2, TimeSpan.FromSeconds(8));
        Assert.Equal(ResultadoRespuesta.Acierto, r);
        Assert.Equal(Modo.Ventaja, ControlPruebasJuridicas.ModoDeTirada(r));
    }

    [Fact]
    public void Fallo_da_tirada_normal_sin_castigo()
    {
        var r = new ControlPruebasJuridicas().Evaluar(Verificada(), 0, TimeSpan.FromSeconds(8));
        Assert.Equal(ResultadoRespuesta.Fallo, r);
        Assert.Equal(Modo.Normal, ControlPruebasJuridicas.ModoDeTirada(r));
    }

    [Fact]
    public void Pasados_20_segundos_es_tiempo_agotado()
    {
        var r = new ControlPruebasJuridicas().Evaluar(Verificada(), 2, TimeSpan.FromSeconds(21));
        Assert.Equal(ResultadoRespuesta.TiempoAgotado, r);
        Assert.Equal(Modo.Normal, ControlPruebasJuridicas.ModoDeTirada(r));
    }

    [Fact]
    public void Maximo_tres_por_partida()
    {
        var c = new ControlPruebasJuridicas();
        for (var i = 0; i < 3; i++) c.Evaluar(Verificada(), 2, TimeSpan.FromSeconds(5));
        Assert.False(c.Disponible);
        Assert.Throws<InvalidOperationException>(() => c.Evaluar(Verificada(), 2, TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void Borrador_no_entra_al_juego()
    {
        var borrador = new Pregunta("mp-002", "Procesal civil", "Mesa de Partes", "x", Opciones, 0, "", "", EstadoPregunta.Borrador);
        Assert.False(borrador.PuedeJugarse);
        Assert.Throws<InvalidOperationException>(() => new ControlPruebasJuridicas().Evaluar(borrador, 0, TimeSpan.Zero));
    }

    [Fact]
    public void Verificada_exige_revisor_fecha_y_enlace() =>
        Assert.Throws<ArgumentException>(() => new Pregunta("mp-003", "m", "c", "t", Opciones, 0, "b", "", EstadoPregunta.Verificada, "Revisor", DateTime.Today));

    [Fact]
    public void Exige_cuatro_opciones() =>
        Assert.Throws<ArgumentException>(() => new Pregunta("mp-004", "m", "c", "t", new[] { "a", "b" }, 0, "b", "e", EstadoPregunta.Borrador));
}
