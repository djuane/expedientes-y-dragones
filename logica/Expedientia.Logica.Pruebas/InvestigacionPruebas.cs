using Expedientia.Logica.Expedientes;
using Expedientia.Logica.Investigacion;

namespace Expedientia.Logica.Pruebas;

/// <summary>Modo investigar y litigar (v0.13): plazo en horas, pruebas, escrito y audiencia con contradicciones.</summary>
public class InvestigacionPruebas
{
    private static readonly CasoDeInvestigacion Caso = CasosDeInvestigacion.EscritoFantasma;

    /// <summary>Hace las acciones clave en el orden en que se van abriendo (como lo haría quien sigue las pistas de Lion).</summary>
    private static void InvestigarLoClave(PartidaDeInvestigacion p)
    {
        for (var vuelta = 0; vuelta < 20; vuelta++)
        {
            var siguiente = p.LugaresAbiertos.SelectMany(l => p.Acciones(l.Id)).FirstOrDefault(h => h.Clave);
            if (siguiente == null) return;
            p.Investigar(siguiente.Id);
        }
    }

    private static void GanarLaAudiencia(PartidaDeInvestigacion p)
    {
        while (p.Fase == Fase.Audiencia)
        {
            var t = p.TestimonioActual!;
            var i = t.Lineas.ToList().FindIndex(l => l.Contradice != null);
            var r = p.Presentar(i, t.Lineas[i].Contradice!);
            Assert.True(r.Contradiccion);
        }
    }

    [Fact]
    public void El_caso_se_gana_con_las_pruebas_clave_dentro_del_plazo_y_da_tres_estrellas()
    {
        var estudio = new EstadoEstudio();
        var p = new PartidaDeInvestigacion(Caso, estudio);
        InvestigarLoClave(p);
        foreach (var t in Caso.Testimonios)
            foreach (var l in t.Lineas.Where(l => l.Contradice != null)) Assert.True(p.Tiene(l.Contradice!), l.Contradice);
        Assert.True(p.HorasRestantes >= 0);
        p.PresentarEscrito(Caso.Pretensiones.ToList().FindIndex(x => x.Correcta));
        GanarLaAudiencia(p);
        Assert.Equal(Fase.Sentencia, p.Fase);
        Assert.True(p.Fundado);
        Assert.Equal(3, p.Estrellas);
        Assert.Equal(1, estudio.ExpedientesCerrados);
        Assert.True(estudio.Honorarios > 0);
    }

    [Fact]
    public void Cada_accion_cuesta_horas_y_no_se_repite()
    {
        var p = new PartidaDeInvestigacion(Caso);
        var r = p.Investigar("casa-cliente");
        Assert.Equal(Caso.HorasDePlazo - 1, p.HorasRestantes);
        Assert.Equal("tintoreria", r.Nueva!.Id);
        Assert.DoesNotContain(p.Acciones("casa"), h => h.Id == "casa-cliente");
        Assert.Throws<InvalidOperationException>(() => p.Investigar("casa-cliente"));
        p.Investigar("estudio-gonzabio"); // conversar en el estudio no cuesta horas
        Assert.Equal(Caso.HorasDePlazo - 1, p.HorasRestantes);
    }

    [Fact]
    public void Los_lugares_y_acciones_se_abren_con_pistas()
    {
        var p = new PartidaDeInvestigacion(Caso);
        Assert.DoesNotContain(p.LugaresAbiertos, l => l.Id == "tintoreria");
        Assert.DoesNotContain(p.LugaresAbiertos, l => l.Id == "oficina");
        Assert.DoesNotContain(p.Acciones("mesa"), h => h.Id == "mesa-rosita-bitacora");
        p.Investigar("casa-cliente");
        Assert.Contains(p.LugaresAbiertos, l => l.Id == "tintoreria");
        p.Investigar("soporte-tecnico");
        Assert.Contains(p.Acciones("mesa"), h => h.Id == "mesa-rosita-bitacora");
    }

    [Fact]
    public void Sin_tiempo_no_se_puede_seguir_investigando()
    {
        var p = new PartidaDeInvestigacion(Caso);
        var hechas = 0;
        while (p.HorasRestantes > 0)
        {
            var h = p.LugaresAbiertos.SelectMany(l => p.Acciones(l.Id)).First(x => x.Horas > 0);
            var r = p.Investigar(h.Id);
            hechas++;
            if (p.HorasRestantes == 0) Assert.True(r.SeAcaboElPlazo);
        }
        Assert.Equal(Caso.HorasDePlazo, hechas);
        var cara = p.LugaresAbiertos.SelectMany(l => p.Acciones(l.Id)).FirstOrDefault(x => x.Horas > 0);
        if (cara != null) Assert.Throws<InvalidOperationException>(() => p.Investigar(cara.Id));
    }

    [Fact]
    public void Lion_senala_un_lugar_con_algo_clave_y_tiene_tres_pistas()
    {
        var p = new PartidaDeInvestigacion(Caso);
        var r = p.PedirPistaALion();
        Assert.Contains("«", r.Lineas[0].Texto);
        p.PedirPistaALion();
        p.PedirPistaALion();
        Assert.Equal(0, p.PistasDeLionRestantes);
        Assert.Contains("Ya les dije", p.PedirPistaALion().Lineas[0].Texto);
    }

    [Fact]
    public void Presentar_la_prueba_equivocada_cuesta_credibilidad_y_sin_credibilidad_se_pierde()
    {
        var estudio = new EstadoEstudio();
        var p = new PartidaDeInvestigacion(Caso, estudio);
        p.PresentarEscrito(Caso.Pretensiones.ToList().FindIndex(x => x.Correcta));
        Assert.Equal(PartidaDeInvestigacion.CredibilidadMaxima, p.Credibilidad);
        for (var i = 0; i < PartidaDeInvestigacion.CredibilidadMaxima; i++)
        {
            var r = p.Presentar(0, "fotocopia");
            Assert.False(r.Contradiccion);
            Assert.Equal(1, r.CredibilidadPerdida);
        }
        Assert.Equal(Fase.Sentencia, p.Fase);
        Assert.False(p.Fundado);
        Assert.Equal(0, estudio.ExpedientesCerrados);
        Assert.Throws<InvalidOperationException>(() => p.Presentar(0, "fotocopia"));
    }

    [Fact]
    public void Un_escrito_mal_pedido_resta_credibilidad_y_quita_estrellas()
    {
        var p = new PartidaDeInvestigacion(Caso);
        InvestigarLoClave(p);
        var r = p.PresentarEscrito(Caso.Pretensiones.ToList().FindIndex(x => !x.Correcta));
        Assert.Equal(1, r.CredibilidadPerdida);
        GanarLaAudiencia(p);
        Assert.True(p.Fundado);
        Assert.Equal(2, p.Estrellas);
    }

    [Fact]
    public void La_constancia_del_Coimero_es_un_atajo_que_sale_caro()
    {
        var estudio = new EstadoEstudio();
        var p = new PartidaDeInvestigacion(Caso, estudio);
        p.Investigar("pasillo-coimero");
        Assert.True(p.UsoAtajos);
        Assert.Equal(10, estudio.ExpedienteDisciplinario);
        p.PresentarEscrito(0);
        var r = p.Presentar(1, "constancia");
        Assert.Equal(2, r.CredibilidadPerdida);
        Assert.Equal(20, estudio.ExpedienteDisciplinario);
    }

    [Fact]
    public void Las_pistas_no_se_presentan_y_repreguntar_no_cuesta()
    {
        var p = new PartidaDeInvestigacion(Caso);
        p.Investigar("casa-cliente");
        Assert.DoesNotContain(p.Presentables, x => x.Id == "tintoreria");
        p.PresentarEscrito(0);
        Assert.Throws<InvalidOperationException>(() => p.Presentar(0, "tintoreria"));
        var r = p.Repreguntar(1);
        Assert.NotEmpty(r.Lineas);
        Assert.True(p.Repreguntada(1));
        Assert.Equal(PartidaDeInvestigacion.CredibilidadMaxima, p.Credibilidad);
    }

    [Fact]
    public void Romper_un_testimonio_pide_fundamento_y_acertar_devuelve_credibilidad()
    {
        var p = new PartidaDeInvestigacion(Caso);
        InvestigarLoClave(p);
        p.PresentarEscrito(0);
        p.Presentar(0, "fotocopia");
        Assert.Equal(4, p.Credibilidad);
        var r = p.Presentar(1, "bitacora");
        Assert.True(r.TestimonioRoto);
        Assert.True(r.PideFundamento);
        Assert.Equal(1, p.IndiceTestimonio);
        p.FundamentoRespondido(true);
        Assert.Equal(5, p.Credibilidad);
    }

    [Fact]
    public void Un_caso_mal_armado_no_se_puede_crear()
    {
        Assert.Throws<ArgumentException>(() => new Testimonio("x", "t", Array.Empty<LineaDeGuion>(),
            new[] { new LineaDeTestimonio("a", Array.Empty<LineaDeGuion>()) }));
    }
}
