using Expedientia.Logica.Combate;
using Expedientia.Logica.Dados;
using Expedientia.Logica.Expedientes;

namespace Expedientia.Logica.Pruebas;

public class ExpedientePruebas
{
    private static List<MiembroDelEquipo> Equipo() => new()
    {
        new MiembroDelEquipo(Catalogo.Civilista("civ", "Dra. Civil"), "jugador-1"),
        new MiembroDelEquipo(Catalogo.Administrativista("adm", "Dr. Adm"), null),
    };

    /// <summary>Juega un expediente completo con decisiones automáticas, como haría un bot.</summary>
    internal static Expediente JugarCompleto(Caso caso, int semilla, DecisionCoimero decision = DecisionCoimero.Rechazar, EstadoEstudio? estudio = null)
    {
        var exp = new Expediente(caso, semilla, Equipo(), estudio);
        var pasos = 0;
        exp.Avanzar();
        while (!exp.Terminado && pasos++ < 5000)
        {
            if (!exp.SalaResuelta)
            {
                switch (exp.SalaActual)
                {
                    case TipoSala.Combate:
                    case TipoSala.Jefe:
                    case TipoSala.RedDelCoimero:
                        Bot.JugarTurno(exp.CombateActual!);
                        exp.RevisarCombate();
                        break;
                    case TipoSala.Tramite: exp.ResolverTramite("adm"); break;
                    case TipoSala.Audiencia: exp.ResolverAudiencia("adm", OpcionAudiencia.RevisarExpediente); break;
                    case TipoSala.Cafeteria: exp.Descansar(); break;
                    case TipoSala.Libreria: exp.SalirDeLaTienda(); break;
                    case TipoSala.Coimero: exp.DecidirCoimero(decision); break;
                }
            }
            else if (!exp.Terminado) exp.Avanzar();
        }
        Assert.True(exp.Terminado, $"Semilla {semilla}: el expediente no terminó.");
        return exp;
    }

    [Fact]
    public void Genera_entre_8_y_12_salas_con_combate_al_inicio_y_jefe_al_final()
    {
        for (var s = 0; s < 200; s++)
        {
            var exp = new Expediente(CatalogoCasos.Buscar("escrito-fantasma"), s, Equipo());
            Assert.InRange(exp.Salas.Count, 8, 12);
            Assert.Equal(TipoSala.Combate, exp.Salas[0]);
            Assert.Equal(TipoSala.Jefe, exp.Salas[^1]);
            Assert.Contains(TipoSala.Cafeteria, exp.Salas);
        }
    }

    [Fact]
    public void El_Coimero_aparece_en_alrededor_de_un_tercio_de_las_partidas()
    {
        var conCoimero = Enumerable.Range(0, 600).Count(s => new Expediente(CatalogoCasos.Buscar("escrito-fantasma"), s, Equipo()).Salas.Contains(TipoSala.Coimero));
        Assert.InRange(conCoimero, 140, 260);
    }

    [Fact]
    public void Expedientes_completos_terminan_siempre_con_sentencia()
    {
        var fundados = 0;
        for (var s = 0; s < 100; s++)
        {
            var exp = JugarCompleto(CatalogoCasos.Buscar("escrito-fantasma"), s);
            Assert.NotNull(exp.Sentencia);
            if (exp.Sentencia!.Fundada) fundados++;
        }
        // Equilibrio inicial: con bots, el equipo gana la mayoría pero no siempre.
        Assert.InRange(fundados, 40, 100);
    }

    [Fact]
    public void Plazo_en_llamas_puede_vencer_por_extemporaneo()
    {
        var motivos = Enumerable.Range(0, 100).Select(s => JugarCompleto(CatalogoCasos.Buscar("plazo-en-llamas"), s).Sentencia!.Motivo).ToList();
        Assert.Contains(motivos, m => m == "Improcedente por extemporáneo" || m.StartsWith("Expediente cerrado"));
        Assert.All(motivos, m => Assert.False(string.IsNullOrEmpty(m)));
    }

    [Fact]
    public void Misma_semilla_misma_sentencia()
    {
        var a = JugarCompleto(CatalogoCasos.Buscar("escrito-fantasma"), 77).Sentencia!;
        var b = JugarCompleto(CatalogoCasos.Buscar("escrito-fantasma"), 77).Sentencia!;
        Assert.Equal((a.Fundada, a.Motivo, a.Honorarios, a.Experiencia), (b.Fundada, b.Motivo, b.Honorarios, b.Experiencia));
    }

    [Fact]
    public void Aceptar_al_Coimero_suma_al_expediente_disciplinario_y_rechazar_sube_reputacion()
    {
        var semilla = Enumerable.Range(0, 200).First(s => new Expediente(CatalogoCasos.Buscar("escrito-fantasma"), s, Equipo()).Salas.Contains(TipoSala.Coimero));
        var acepta = JugarCompleto(CatalogoCasos.Buscar("escrito-fantasma"), semilla, DecisionCoimero.Aceptar);
        Assert.Equal(10, acepta.Estudio.ExpedienteDisciplinario);
        var rechaza = JugarCompleto(CatalogoCasos.Buscar("escrito-fantasma"), semilla, DecisionCoimero.Rechazar);
        Assert.Equal(0, rechaza.Estudio.ExpedienteDisciplinario);
        Assert.True(rechaza.Estudio.Reputacion > 50);
    }

    [Fact]
    public void Denunciar_abre_la_sala_de_la_red_del_Coimero()
    {
        var semilla = Enumerable.Range(0, 200).First(s => new Expediente(CatalogoCasos.Buscar("escrito-fantasma"), s, Equipo()).Salas.Contains(TipoSala.Coimero));
        var exp = JugarCompleto(CatalogoCasos.Buscar("escrito-fantasma"), semilla, DecisionCoimero.Denunciar);
        var i = exp.Salas.ToList().IndexOf(TipoSala.Coimero);
        Assert.Equal(TipoSala.RedDelCoimero, exp.Salas[i + 1]);
    }

    [Fact]
    public void El_estudio_conserva_progreso_entre_expedientes()
    {
        var estudio = new EstadoEstudio();
        JugarCompleto(CatalogoCasos.Buscar("escrito-fantasma"), 1, estudio: estudio);
        var tras1 = estudio.Experiencia;
        JugarCompleto(CatalogoCasos.Buscar("escrito-fantasma"), 2, estudio: estudio);
        Assert.True(estudio.Experiencia > tras1);
        Assert.True(estudio.Honorarios > 0);
    }

    [Theory]
    [InlineData(0, 1)] [InlineData(299, 1)] [InlineData(300, 2)] [InlineData(2700, 4)] [InlineData(355000, 20)]
    public void Nivel_segun_experiencia_del_SRD(int xp, int nivel) =>
        Assert.Equal(nivel, EstadoEstudio.Restaurar(0, xp, 50, 0, 0).Nivel);

    [Fact]
    public void No_se_avanza_sin_resolver_la_sala()
    {
        var exp = new Expediente(CatalogoCasos.Buscar("escrito-fantasma"), 3, Equipo());
        exp.Avanzar();
        Assert.Throws<InvalidOperationException>(() => exp.Avanzar());
    }

    [Fact]
    public void La_Cola_Infinita_suma_usuarios_a_la_fila()
    {
        var jefe = Catalogo.ColaInfinita("jefe");
        var mecanica = new ColaInfinita("jefe", Catalogo.EsbirroDeLaCola, maximo: 2);
        var c = new Expedientia.Logica.Combate.Combate(new GeneradorConSemilla(5), new[]
        {
            new Combatiente(Catalogo.Civilista("civ", "C"), new Casilla(0, 7)),
            new Combatiente(jefe, new Casilla(2, 0)),
        }, mecanica);
        c.Iniciar();
        var eventos = new List<EventoCombate>();
        for (var i = 0; i < 8 && !c.Terminado; i++)
        {
            if (c.Actual.Id == "civ") eventos.AddRange(c.TerminarTurno());
            else eventos.AddRange(Bot.JugarTurno(c));
        }
        Assert.Equal(2, mecanica.Creados);
        Assert.Equal(2, eventos.OfType<Aparecio>().Count());
    }
}
