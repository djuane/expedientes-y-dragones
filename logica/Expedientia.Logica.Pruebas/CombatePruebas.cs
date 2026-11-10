using Expedientia.Logica.Combate;
using Expedientia.Logica.Dados;
using Expedientia.Logica.Personajes;
using CombateMotor = Expedientia.Logica.Combate.Combate;

namespace Expedientia.Logica.Pruebas;

public class CombatePruebas
{
    private static Atributos Base(int doctrina = 16, int criterio = 14, int agilidad = 12, int oratoria = 10) =>
        new(10, agilidad, 12, doctrina, criterio, oratoria);

    private static Combatiente Civilista(Casilla c, string id = "civ") =>
        new(new FichaCombatiente(id, "Civilista", Bando.Estudio, Especialidad.Civilista, Base(agilidad: 20), 1, 12, 13, 4, Atributo.Doctrina, alcance: 2), c);

    private static Combatiente Administrativista(Casilla c, string id = "adm") =>
        new(new FichaCombatiente(id, "Administrativista", Bando.Estudio, Especialidad.Administrativista, Base(agilidad: 18), 1, 11, 12, 4, Atributo.Criterio, alcance: 2), c);

    private static Combatiente Formalismo(Casilla c, string id = "f1", int paciencia = 6) =>
        new(new FichaCombatiente(id, "Formalismo", Bando.Sistema, Especialidad.Ninguna, Base(agilidad: 1), 1, paciencia, 10, 3, Atributo.Firmeza), c);

    // Iniciativa: cada combatiente consume 2 valores (d20 y desempate).
    private static CombateMotor Preparar(int[] valores, params Combatiente[] c)
    {
        var combate = new CombateMotor(new FalsoAzar(valores), c);
        combate.Iniciar();
        return combate;
    }

    [Fact]
    public void Distancia_cuenta_diagonales_como_una_casilla() =>
        Assert.Equal(3, new Casilla(0, 0).DistanciaA(new Casilla(3, 2)));

    [Fact]
    public void Rechaza_combatientes_en_la_misma_casilla() =>
        Assert.Throws<ArgumentException>(() => new CombateMotor(new FalsoAzar(), new[] { Civilista(new Casilla(0, 0)), Formalismo(new Casilla(0, 0)) }));

    [Fact]
    public void Movimiento_valido_y_limites()
    {
        var c = Preparar(new[] { 10, 1, 10, 1 }, Civilista(new Casilla(0, 7)), Formalismo(new Casilla(5, 0)));
        Assert.Equal("civ", c.Actual.Id);
        Assert.False(c.Mover(new Casilla(0, 2)).Valida);  // 5 casillas: no alcanza
        Assert.False(c.Mover(new Casilla(6, 7)).Valida);  // fuera de la cuadrícula
        Assert.True(c.Mover(new Casilla(2, 5)).Valida);   // 2 casillas en diagonal
        Assert.Equal(2, c.Actual.MovimientoRestante);
        Assert.False(c.Mover(new Casilla(2, 1)).Valida);  // le quedan 2
    }

    [Fact]
    public void No_se_mueve_a_casilla_ocupada()
    {
        var c = Preparar(new[] { 10, 1, 10, 1 }, Civilista(new Casilla(0, 7)), Formalismo(new Casilla(1, 6)));
        Assert.False(c.Mover(new Casilla(1, 6)).Valida);
    }

    [Fact]
    public void Ataque_que_acierta_resta_paciencia_y_puede_derribar()
    {
        // iniciativa civ, f1; ataque 15 + 5 = 20 vs 10; daño 1d6=6 + 3 = 9 ≥ 6
        var c = Preparar(new[] { 10, 1, 10, 1, 15, 6 }, Civilista(new Casilla(0, 1)), Formalismo(new Casilla(0, 0)));
        var r = c.Atacar("f1");
        Assert.True(r.Valida);
        Assert.Contains(r.Eventos, e => e is Cayo);
        Assert.True(c.Terminado);
        Assert.Equal(Bando.Estudio, c.Ganador);
    }

    [Fact]
    public void Una_sola_accion_por_turno()
    {
        var c = Preparar(new[] { 10, 1, 10, 1, 2 }, Civilista(new Casilla(0, 1)), Formalismo(new Casilla(0, 0), paciencia: 50));
        Assert.True(c.Atacar("f1").Valida);
        Assert.False(c.Atacar("f1").Valida);
    }

    [Fact]
    public void Fuera_de_alcance_se_rechaza()
    {
        var c = Preparar(new[] { 10, 1, 10, 1 }, Civilista(new Casilla(0, 7)), Formalismo(new Casilla(0, 0)));
        Assert.Equal("Fuera de alcance.", c.Atacar("f1").Motivo);
    }

    [Fact]
    public void Fundado_duplica_los_dados_de_danio()
    {
        // 20 natural; daño 2d6 = 1 + 1, + 3 = 5
        var c = Preparar(new[] { 10, 1, 10, 1, 20, 1, 1 }, Civilista(new Casilla(0, 1)), Formalismo(new Casilla(0, 0), paciencia: 50));
        var r = c.Atacar("f1");
        Assert.Equal(Momento.Fundado, r.Eventos.OfType<Ataco>().Single().Momento);
        Assert.Equal(5, r.Eventos.OfType<RecibioDanio>().Single().Cantidad);
    }

    [Fact]
    public void Medida_cautelar_inmoviliza_dos_turnos_del_enemigo()
    {
        var c = Preparar(new[] { 10, 1, 10, 1 }, Civilista(new Casilla(0, 3)), Formalismo(new Casilla(0, 0), paciencia: 50));
        var r = c.UsarHabilidad(Habilidad.MedidaCautelar, "f1");
        Assert.True(r.Valida);
        c.TerminarTurno();
        Assert.Equal("f1", c.Actual.Id);
        Assert.False(c.Actual.AccionDisponible);
        Assert.False(c.Mover(new Casilla(1, 0)).Valida);
        c.TerminarTurno(); // turno 1 inmovilizado
        c.TerminarTurno(); // civ
        Assert.True(c.Actual.Inmovilizado); // f1 sigue: segundo turno
        c.TerminarTurno();
        c.TerminarTurno();
        Assert.False(c.Actual.Inmovilizado); // ya libre
    }

    [Fact]
    public void Habilidad_de_otra_especialidad_se_rechaza()
    {
        var c = Preparar(new[] { 10, 1, 10, 1 }, Civilista(new Casilla(0, 3)), Formalismo(new Casilla(0, 0)));
        Assert.False(c.UsarHabilidad(Habilidad.Queja, "f1").Valida);
    }

    [Fact]
    public void Silencio_administrativo_gana_la_sala_si_el_enemigo_no_actua_dos_turnos()
    {
        // adm primero; Queja inmoviliza a f1 dos turnos → no actúa → victoria
        var c = Preparar(new[] { 10, 1, 10, 1 }, Administrativista(new Casilla(0, 3)), Formalismo(new Casilla(0, 0), paciencia: 50));
        Assert.True(c.UsarHabilidad(Habilidad.Queja, "f1").Valida);
        c.TerminarTurno();  // f1 (1)
        c.TerminarTurno();  // adm
        c.TerminarTurno();  // f1 (2)
        var fin = c.TerminarTurno();
        Assert.True(c.Terminado);
        Assert.Equal("Silencio administrativo", fin.OfType<FinDeCombate>().Single().Motivo);
    }

    [Fact]
    public void Sin_Administrativista_no_hay_silencio()
    {
        var c = Preparar(new[] { 10, 1, 10, 1 }, Civilista(new Casilla(0, 3)), Formalismo(new Casilla(0, 0), paciencia: 50));
        c.UsarHabilidad(Habilidad.MedidaCautelar, "f1");
        for (var i = 0; i < 6; i++) c.TerminarTurno();
        Assert.False(c.Terminado);
    }

    [Fact]
    public void Recurso_de_apelacion_repite_un_ataque_fallido_una_vez_por_ronda()
    {
        // Orden: civ (agilidad 20), adm (18), f1. civ falla con 2 y apela adm: 15 acierta, daño 3+3
        var valores = new[] { 10, 1, 10, 1, 10, 1, 2, 15, 3 };
        var c = Preparar(valores, Civilista(new Casilla(0, 1)), Administrativista(new Casilla(3, 3)), Formalismo(new Casilla(0, 0), paciencia: 50));
        Assert.Equal("civ", c.Actual.Id);
        Assert.False(c.Atacar("f1").Eventos.OfType<Ataco>().Single().Acierto);
        var apel = c.RecursoDeApelacion("adm", "civ");
        Assert.True(apel.Valida);
        Assert.Equal(44, c.Buscar("f1").Paciencia);
        Assert.False(c.RecursoDeApelacion("adm", "civ").Valida);
    }

    [Fact]
    public void Casacion_solo_una_vez_por_partida()
    {
        // civ falla (2); Casación: Oratoria 10 (+0) + 2 con 18 = 20 ≥ 18 admite; repite ataque 15; daño 1+3
        var valores = new[] { 10, 1, 10, 1, 2, 18, 15, 1 };
        var c = Preparar(valores, Civilista(new Casilla(0, 1)), Formalismo(new Casilla(0, 0), paciencia: 50));
        c.Atacar("f1");
        Assert.True(c.Casacion("civ", "civ").Valida);
        Assert.Equal(46, c.Buscar("f1").Paciencia);
        Assert.True(c.CasacionUsada);
        Assert.False(c.Casacion("civ", "civ").Valida);
    }

    [Fact]
    public void Bots_terminan_un_combate_completo_con_semilla()
    {
        for (var semilla = 0; semilla < 50; semilla++)
        {
            var c = new CombateMotor(new GeneradorConSemilla(semilla), new[]
            {
                Civilista(new Casilla(1, 7)), Administrativista(new Casilla(4, 7)),
                Formalismo(new Casilla(0, 0), "f1"), Formalismo(new Casilla(3, 0), "f2"), Formalismo(new Casilla(5, 0), "f3"),
            });
            c.Iniciar();
            var pasos = 0;
            while (!c.Terminado && pasos++ < 500) Bot.JugarTurno(c);
            Assert.True(c.Terminado, $"La semilla {semilla} no terminó.");
        }
    }

    [Fact]
    public void Misma_semilla_misma_partida()
    {
        string Jugar(int semilla)
        {
            var c = new CombateMotor(new GeneradorConSemilla(semilla), new[]
            {
                Civilista(new Casilla(1, 7)), Formalismo(new Casilla(0, 0), "f1"), Formalismo(new Casilla(3, 0), "f2"),
            });
            c.Iniciar();
            var log = new List<string>();
            while (!c.Terminado) log.AddRange(Bot.JugarTurno(c).Select(e => e.GetType().Name));
            return string.Join(",", log) + c.Ganador;
        }
        Assert.Equal(Jugar(123), Jugar(123));
    }
}
