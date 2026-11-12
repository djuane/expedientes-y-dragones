using Expedientia.Logica.Combate;
using Expedientia.Logica.Expedientes;

namespace Expedientia.Logica.Pruebas;

/// <summary>Mejoras al ganar cada combate y desbloqueos del estudio entre partidas (v0.11).</summary>
public class MejorasPruebas
{
    private static List<MiembroDelEquipo> Equipo() => new()
    {
        new MiembroDelEquipo(Catalogo.Civilista("civ", "Dra. Civil"), "jugador-1"),
        new MiembroDelEquipo(Catalogo.Administrativista("adm", "Dr. Adm"), null),
    };

    /// <summary>Juega con bots hasta que se ofrezcan mejoras (o termine).</summary>
    private static Expediente HastaLaPrimeraOferta(int semilla, EstadoEstudio? estudio = null)
    {
        var exp = new Expediente(CatalogoCasos.Buscar("escrito-fantasma"), semilla, Equipo(), estudio);
        exp.Avanzar();
        if (exp.MejorasOfrecidas.Count > 0) return exp;
        for (var i = 0; i < 2000 && exp.MejorasOfrecidas.Count == 0 && !exp.Terminado; i++)
        {
            Bot.JugarTurno(exp.CombateActual!);
            exp.RevisarCombate();
        }
        return exp;
    }

    [Fact]
    public void Ganar_un_combate_ofrece_tres_mejoras_distintas()
    {
        for (var s = 0; s < 30; s++)
        {
            var exp = HastaLaPrimeraOferta(s);
            if (exp.Terminado) continue;
            Assert.Equal(3, exp.MejorasOfrecidas.Count);
            Assert.Equal(3, exp.MejorasOfrecidas.Distinct().Count());
            Assert.DoesNotContain(exp.MejorasOfrecidas, m => Mejoras.Datos(m).Rara); // sin la biblioteca no salen raras
        }
    }

    [Fact]
    public void Las_mejoras_cambian_la_ficha_de_todo_el_equipo()
    {
        var c = new Combatiente(Catalogo.Civilista("c", "C"), new Casilla(0, 0));
        var antes = c.Ficha;
        Mejoras.Aplicar(Mejora.TernoAMedida, c);
        Mejoras.Aplicar(Mejora.DoctrinaActualizada, c);
        Mejoras.Aplicar(Mejora.ZapatosComodos, c);
        Mejoras.Aplicar(Mejora.SelloDeUrgencia, c);
        Mejoras.Aplicar(Mejora.JurisprudenciaVinculante, c);
        Mejoras.Aplicar(Mejora.CodigoComentado, c);
        Assert.Equal(antes.Defensa + 1, c.Ficha.Defensa);
        Assert.Equal(antes.CarasDeDanio + 2, c.Ficha.CarasDeDanio);
        Assert.Equal(antes.Movimiento + 1, c.Ficha.Movimiento);
        Assert.Equal(antes.Alcance + 1, c.Ficha.Alcance);
        Assert.Equal(antes.DadosDeDanio + 1, c.Ficha.DadosDeDanio);
        Assert.Equal(antes.Atributos.Modificador(antes.AtributoDeAtaque) + 1, c.Ficha.Atributos.Modificador(antes.AtributoDeAtaque));
        Assert.Equal(antes.Id, c.Ficha.Id);

        c.Paciencia = 3;
        Mejoras.Aplicar(Mejora.TermoDeCafe, c);
        Assert.Equal(antes.PacienciaMaxima + 5, c.Ficha.PacienciaMaxima);
        Assert.Equal(8, c.Paciencia);
    }

    [Fact]
    public void Elegir_mejora_la_aplica_y_avanzar_sin_elegir_la_toma_el_juego()
    {
        var exp = HastaLaPrimeraOferta(3);
        Assert.False(exp.Terminado);
        var oferta = exp.MejorasOfrecidas.ToList();
        var ev = exp.ElegirMejora(oferta[2]);
        Assert.IsType<MejoraElegida>(ev[0]);
        Assert.Empty(exp.MejorasOfrecidas);
        Assert.Equal(new[] { oferta[2] }, exp.MejorasTomadas);
        Assert.Throws<InvalidOperationException>(() => exp.ElegirMejora(oferta[0]));

        var otra = HastaLaPrimeraOferta(3);
        otra.Avanzar();
        Assert.Single(otra.MejorasTomadas);
    }

    [Fact]
    public void El_asistente_practicante_reincorpora_a_los_caidos()
    {
        var c = new Combatiente(Catalogo.Administrativista("a", "A"), new Casilla(0, 0)) { Paciencia = 0 };
        Assert.True(c.Caido);
        Mejoras.Aplicar(Mejora.AsistentePracticante, c);
        Assert.Equal(c.Ficha.PacienciaMaxima, c.Paciencia);
    }

    [Fact]
    public void Los_desbloqueos_se_ganan_cerrando_expedientes()
    {
        var nuevo = new EstadoEstudio();
        Assert.False(nuevo.Tiene(Desbloqueo.MejoraDeEntrada));
        Assert.Equal(Desbloqueo.MejoraDeEntrada, nuevo.Proximo!.Desbloqueo);

        var veterano = EstadoEstudio.Restaurar(0, 0, 50, 0, 3);
        Assert.True(veterano.Tiene(Desbloqueo.SegundaOpinion));
        Assert.False(veterano.Tiene(Desbloqueo.OficinaPropia));
        Assert.Equal(Desbloqueo.OficinaPropia, veterano.Proximo!.Desbloqueo);
        Assert.Null(EstadoEstudio.Restaurar(0, 0, 50, 0, 9).Proximo);
    }

    [Fact]
    public void Con_el_maletin_se_elige_una_mejora_al_entrar_y_con_segunda_opinion_se_resortea_una_vez()
    {
        var estudio = EstadoEstudio.Restaurar(0, 0, 50, 0, 3);
        var exp = new Expediente(CatalogoCasos.Buscar("escrito-fantasma"), 7, Equipo(), estudio);
        exp.Avanzar();
        Assert.Equal(3, exp.MejorasOfrecidas.Count);
        Assert.NotNull(exp.CombateActual); // se elige con el primer combate ya en pantalla
        Assert.True(exp.PuedeVolverASortear);
        exp.VolverASortear();
        Assert.Equal(3, exp.MejorasOfrecidas.Count);
        Assert.False(exp.PuedeVolverASortear);
        Assert.Throws<InvalidOperationException>(() => exp.VolverASortear());
    }

    [Fact]
    public void Oficina_propia_suma_paciencia_al_empezar()
    {
        var estudio = EstadoEstudio.Restaurar(0, 0, 50, 0, 5);
        var exp = new Expediente(CatalogoCasos.Buscar("escrito-fantasma"), 1, Equipo(), estudio);
        var civ = exp.Equipo.First(c => c.Id == "civ");
        Assert.Equal(Catalogo.Civilista("x", "x").PacienciaMaxima + 4, civ.Ficha.PacienciaMaxima);
        Assert.Equal(civ.Ficha.PacienciaMaxima, civ.Paciencia);
    }

    [Fact]
    public void Ganar_el_expediente_que_completa_un_desbloqueo_lo_anuncia()
    {
        for (var s = 0; s < 40; s++)
        {
            var estudio = new EstadoEstudio();
            var exp = ExpedientePruebas.JugarCompleto(CatalogoCasos.Buscar("escrito-fantasma"), s, estudio: estudio);
            if (!exp.Sentencia!.Fundada) continue;
            Assert.True(estudio.Tiene(Desbloqueo.MejoraDeEntrada));
            return;
        }
        Assert.Fail("Ningún expediente salió Fundado en 40 semillas.");
    }
}
