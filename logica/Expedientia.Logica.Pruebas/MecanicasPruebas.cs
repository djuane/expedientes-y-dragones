using Expedientia.Logica.Combate;
using Expedientia.Logica.Dados;
using Expedientia.Logica.Expedientes;
using CombateMotor = Expedientia.Logica.Combate.Combate;

namespace Expedientia.Logica.Pruebas;

/// <summary>Mecánicas de enemigos del GDD, Nulidad, Requisito Imposible y trama (v0.8.0).</summary>
public class MecanicasPruebas
{
    private static CombateMotor Preparar(int semilla, params Combatiente[] c)
    {
        var combate = new CombateMotor(new GeneradorConSemilla(semilla), c);
        combate.Iniciar();
        return combate;
    }

    /// <summary>Pasa turnos sin hacer nada hasta que le toque a <paramref name="id"/>.</summary>
    private static List<EventoCombate> PasarHasta(CombateMotor c, string id)
    {
        var eventos = new List<EventoCombate>();
        var guardia = 0;
        while (c.Actual.Id != id && !c.Terminado && guardia++ < 50) eventos.AddRange(c.TerminarTurno());
        return eventos;
    }

    [Fact]
    public void El_Plazo_que_Vence_explota_en_su_tercer_turno_y_golpea_a_quien_esta_cerca()
    {
        var civ = new Combatiente(Catalogo.Civilista("civ", "Civ"), new Casilla(2, 6));
        var lejos = new Combatiente(Catalogo.Civilista("civ2", "Civ 2"), new Casilla(5, 0));
        var plazo = new Combatiente(Catalogo.PlazoQueVence("p"), new Casilla(2, 5));
        var otro = new Combatiente(Catalogo.Formalismo("f"), new Casilla(0, 0));
        var c = Preparar(3, civ, lejos, plazo, otro);
        var eventos = new List<EventoCombate>();
        var guardia = 0;
        while (!eventos.OfType<Vencio>().Any() && guardia++ < 40) eventos.AddRange(c.TerminarTurno());
        Assert.Contains(eventos, e => e is Vencio v && v.Id == "p");
        Assert.True(plazo.Caido);
        Assert.Equal(3, plazo.TurnosJugados);
        Assert.True(civ.Paciencia < civ.Ficha.PacienciaMaxima);          // estaba a 1 casilla
        Assert.Equal(lejos.Ficha.PacienciaMaxima, lejos.Paciencia);      // estaba lejos
        Assert.True(c.HayAccionEnemigaQueAnular);
    }

    [Fact]
    public void Inmovilizado_el_plazo_se_suspende()
    {
        var civ = new Combatiente(Catalogo.Civilista("civ", "Civ"), new Casilla(2, 6));
        var plazo = new Combatiente(Catalogo.PlazoQueVence("p"), new Casilla(2, 4));
        var otro = new Combatiente(Catalogo.Formalismo("f"), new Casilla(0, 0));
        var c = Preparar(5, civ, plazo, otro);
        PasarHasta(c, "civ");
        Assert.True(c.UsarHabilidad(Habilidad.MedidaCautelar, "p").Valida);
        var antes = plazo.TurnosJugados;
        PasarHasta(c, "p");
        Assert.Equal(antes, plazo.TurnosJugados);
        Assert.False(plazo.Caido);
    }

    [Fact]
    public void El_Formalismo_sin_requisito_recibe_la_mitad_y_con_ventaja_completo()
    {
        int observados = 0, completos = 0;
        for (var s = 0; s < 200; s++)
        {
            var civ = new Combatiente(Catalogo.Civilista("civ", "Civ"), new Casilla(2, 6));
            var f = new Combatiente(Catalogo.Formalismo("f"), new Casilla(2, 5));
            var c = Preparar(s, civ, f);
            PasarHasta(c, "civ");
            var conVentaja = s % 2 == 0;
            var r = c.Atacar("f", conVentaja ? Modo.Ventaja : Modo.Normal);
            var ataque = r.Eventos.OfType<Ataco>().Single();
            if (!ataque.Acierto) continue;
            var observado = r.Eventos.OfType<Observado>().Any();
            if (conVentaja || ataque.Momento == Momento.Fundado) { Assert.False(observado); completos++; }
            else { Assert.True(observado); observados++; }
        }
        Assert.True(observados > 10 && completos > 10);
    }

    [Fact]
    public void Atacar_a_un_inmovilizado_es_con_ventaja_y_cumple_el_requisito()
    {
        var aciertos = 0;
        for (var s = 0; s < 60; s++)
        {
            var civ = new Combatiente(Catalogo.Civilista("civ", "Civ"), new Casilla(2, 6));
            var f = new Combatiente(Catalogo.Formalismo("f"), new Casilla(2, 5));
            var otro = new Combatiente(Catalogo.Formalismo("g"), new Casilla(5, 0));
            var c = Preparar(s, civ, f, otro);
            PasarHasta(c, "civ");
            Assert.True(c.UsarHabilidad(Habilidad.MedidaCautelar, "f").Valida);
            c.TerminarTurno();
            PasarHasta(c, "civ");
            Assert.True(f.Inmovilizado);
            var r = c.Atacar("f");
            if (!r.Eventos.OfType<Ataco>().Single().Acierto) continue;
            aciertos++;
            Assert.DoesNotContain(r.Eventos, e => e is Observado);
        }
        Assert.True(aciertos > 20);
    }

    [Fact]
    public void La_Nulidad_devuelve_la_paciencia_quitada_y_se_recarga()
    {
        for (var s = 0; s < 100; s++)
        {
            var civ = new Combatiente(Catalogo.Civilista("civ", "Civ"), new Casilla(2, 6));
            var adm = new Combatiente(Catalogo.Administrativista("adm", "Adm"), new Casilla(3, 6));
            var f = new Combatiente(Catalogo.Formalismo("f"), new Casilla(2, 5));
            var c = Preparar(s, civ, adm, f);
            PasarHasta(c, "f");
            var r = c.Atacar(civ.Id);
            if (!r.Eventos.OfType<RecibioDanio>().Any()) continue;
            var herida = civ.Paciencia;
            var n = c.Nulidad("civ");
            Assert.True(n.Valida);
            Assert.Equal(civ.Ficha.PacienciaMaxima, civ.Paciencia);
            Assert.True(civ.Paciencia > herida);
            Assert.False(c.HayAccionEnemigaQueAnular);
            Assert.False(c.Nulidad("civ").Valida);                 // ya usó la reacción y no hay nada que anular
            Assert.False(c.Nulidad("adm").Valida);                 // solo la Civilista
            Assert.Equal(ReglasHabilidades.EnfriamientoNulidad, c.RecargaNulidad(civ));
            return;
        }
        Assert.Fail("Ninguna semilla produjo un golpe enemigo.");
    }

    [Theory]
    [InlineData(RequisitoImposible.EnPersona, 1, false, true)]
    [InlineData(RequisitoImposible.EnPersona, 2, false, false)]
    [InlineData(RequisitoImposible.Virtual, 1, false, false)]
    [InlineData(RequisitoImposible.Virtual, 3, false, true)]
    [InlineData(RequisitoImposible.ConConstancia, 1, false, false)]
    [InlineData(RequisitoImposible.ConConstancia, 1, true, true)]
    public void El_Requisito_Imposible_solo_admite_golpes_que_cumplen(string requisito, int distancia, bool ventaja, bool admite)
    {
        var jefe = new Combatiente(Catalogo.RequisitoImposible("jefe"), new Casilla(2, 2));
        var civ = new Combatiente(Catalogo.Civilista("civ", "Civ"), new Casilla(2, 2 + distancia));
        var m = new RequisitoImposible("jefe", requisito, n => Catalogo.Formalismo($"r{n}"), siempreVisible: false);
        Assert.Equal(admite, m.Admite(civ, jefe, ventaja));
        Assert.True(m.Admite(civ, new Combatiente(Catalogo.Formalismo("x"), new Casilla(0, 0)), false)); // a otros no les exige nada
        Assert.False(m.Visible);
    }

    [Fact]
    public void El_Fraccionador_se_parte_al_recibir_dano()
    {
        for (var s = 0; s < 100; s++)
        {
            var civ = new Combatiente(Catalogo.Civilista("civ", "Civ"), new Casilla(2, 6));
            var fr = new Combatiente(Catalogo.Fraccionador("fr"), new Casilla(2, 5));
            var c = Preparar(s, civ, fr);
            PasarHasta(c, "civ");
            var r = c.Atacar("fr", Modo.Ventaja);
            if (fr.Caido || !r.Eventos.OfType<RecibioDanio>().Any()) continue;
            Assert.Contains(r.Eventos, e => e is Aparecio a && a.Nombre == "Fracción");
            Assert.Equal(1, fr.Copias);
            return;
        }
        Assert.Fail("Ninguna semilla dejó al Fraccionador herido y en pie.");
    }

    [Fact]
    public void Caso_3_se_juega_completo_con_su_jefe_y_su_calabozo()
    {
        var caso = CatalogoCasos.Buscar("buena-pro-imposible");
        Assert.Equal(CatalogoCasos.SalaDeContrataciones, caso.Calabozo);
        var fundados = 0;
        for (var s = 0; s < 60; s++)
        {
            var exp = ExpedientePruebas.JugarCompleto(caso, s);
            if (exp.Sentencia!.Fundada) fundados++;
        }
        Assert.InRange(fundados, 15, 59);
    }

    [Fact]
    public void La_trama_habla_al_llegar_describe_cada_sala_y_cierra_con_epilogo()
    {
        var exp = new Expediente(CatalogoCasos.Buscar("escrito-fantasma"), 7, new[]
        {
            new MiembroDelEquipo(Catalogo.Civilista("civ", "Civ"), "j1"),
            new MiembroDelEquipo(Catalogo.Administrativista("adm", "Adm"), null),
        });
        var inicio = exp.Avanzar();
        Assert.Contains(inicio, e => e is Dialogo d && d.Personaje == Guion.Cliente);
        Assert.False(string.IsNullOrWhiteSpace(exp.TextoSala));
        Assert.NotEmpty(Guion.Prologo);
        Assert.Contains(Guion.Prologo, l => l.Personaje == Guion.Gonzabio);
        foreach (var g in Guion.Casos.Values) Assert.Contains(g.Llegada, l => l.Personaje == Guion.Gonzabio);
        foreach (var c in CatalogoCasos.Todos) Assert.True(Guion.Casos.ContainsKey(c.Id), $"Falta el guion de {c.Id}");
    }

    [Fact]
    public void Con_buena_reputacion_el_Suboficial_Quispe_ayuda_contra_la_red_del_Coimero()
    {
        var estudio = EstadoEstudio.Restaurar(0, 0, 80, 0, 0);
        for (var s = 0; s < 400; s++)
        {
            var exp = new Expediente(CatalogoCasos.Buscar("escrito-fantasma"), s, new[]
            {
                new MiembroDelEquipo(Catalogo.Civilista("civ", "Civ"), null),
                new MiembroDelEquipo(Catalogo.Administrativista("adm", "Adm"), null),
            }, estudio);
            if (!exp.Salas.Contains(TipoSala.Coimero)) continue;
            exp.Avanzar();
            var pasos = 0;
            while (!exp.Terminado && pasos++ < 5000)
            {
                if (exp.SalaResuelta) { var ev = exp.Avanzar(); if (exp.SalaActual == TipoSala.RedDelCoimero) {
                        Assert.Contains(exp.CombateActual!.Combatientes, c => c.Id == "quispe" && c.Bando == Bando.Estudio);
                        Assert.Contains(ev, e => e is Dialogo d && d.Personaje == Guion.Quispe);
                        return; } continue; }
                switch (exp.SalaActual)
                {
                    case TipoSala.Combate: case TipoSala.Jefe: Bot.JugarTurno(exp.CombateActual!); exp.RevisarCombate(); break;
                    case TipoSala.Tramite: exp.ResolverTramite("adm"); break;
                    case TipoSala.Audiencia: exp.ResolverAudiencia("adm", OpcionAudiencia.RevisarExpediente); break;
                    case TipoSala.Cafeteria: exp.Descansar(); break;
                    case TipoSala.Libreria: exp.SalirDeLaTienda(); break;
                    case TipoSala.Coimero: exp.DecidirCoimero(DecisionCoimero.Denunciar); break;
                }
            }
        }
        Assert.Fail("Ninguna semilla llegó a la red del Coimero.");
    }
}
