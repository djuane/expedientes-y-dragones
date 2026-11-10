using Expedientia.Logica.Combate;
using Expedientia.Logica.Expedientes;
using Expedientia.Logica.Juridico;
using Expedientia.Logica.Sesion;

namespace Expedientia.Logica.Pruebas;

public class MotorPruebas
{
    private static MotorDePartida Nuevo(int semilla) => new(Guid.NewGuid(), new Expediente(CatalogoCasos.Buscar("escrito-fantasma"), semilla, new[]
    {
        new MiembroDelEquipo(Catalogo.Civilista("p1", "Yo"), "yo"),
        new MiembroDelEquipo(Catalogo.Administrativista("p2", "Bot"), null),
    }), DateTime.UnixEpoch);

    private static AccionJuego Siguiente(EstadoPartida e) => e.SalaResuelta ? new AccionJuego { Tipo = "avanzar" } : e.SalaActual switch
    {
        "Combate" or "Jefe" or "RedDelCoimero" => new AccionJuego { Tipo = "piloto" },
        "Tramite" => new AccionJuego { Tipo = "tramite" },
        "Audiencia" => new AccionJuego { Tipo = "audiencia", Opcion = "revisar" },
        "Cafeteria" => new AccionJuego { Tipo = "descansar" },
        "Libreria" => new AccionJuego { Tipo = "salirTienda" },
        _ => new AccionJuego { Tipo = "coimero", Decision = "rechazar" },
    };

    [Fact]
    public void Partida_sin_conexion_completa_con_el_mismo_motor_del_servidor()
    {
        var m = Nuevo(4);
        var e = m.Iniciar(DateTime.UnixEpoch).Estado;
        for (var i = 0; i < 1000 && e.Sentencia == null; i++)
        {
            var r = m.Ejecutar("yo", Siguiente(e), DateTime.UnixEpoch, _ => null);
            Assert.True(r.Valida, r.Motivo);
            e = r.Estado;
        }
        Assert.NotNull(e.Sentencia);
    }

    [Fact]
    public void Sin_preguntas_verificadas_no_hay_prueba_juridica()
    {
        var m = Nuevo(4);
        m.Iniciar(DateTime.UnixEpoch);
        var r = m.Ejecutar("yo", new AccionJuego { Tipo = "pedirPregunta" }, DateTime.UnixEpoch, _ => null);
        Assert.False(r.Valida);
    }

    [Fact]
    public void Respuesta_tardia_es_tiempo_agotado_segun_el_reloj_del_motor()
    {
        var m = Nuevo(4);
        m.Iniciar(DateTime.UnixEpoch);
        var p = new Pregunta("x", "m", "Mesa de Partes", "t", new[] { "a", "b", "c", "d" }, 1, "b", "https://ejemplo.invalid", EstadoPregunta.Verificada, "Revisor", new DateTime(2026, 11, 10));
        var pedida = m.Ejecutar("yo", new AccionJuego { Tipo = "pedirPregunta" }, DateTime.UnixEpoch, _ => p);
        Assert.Null(pedida.Extra!.Correccion);
        Assert.Equal(4, pedida.Extra.Pregunta!.Opciones.Count);
        var r = m.Ejecutar("yo", new AccionJuego { Tipo = "responder", Indice = 1 }, DateTime.UnixEpoch.AddSeconds(25), _ => p);
        Assert.Equal("TiempoAgotado", r.Extra!.Correccion!.Resultado);
    }

    [Fact]
    public void Un_jugador_ajeno_no_puede_actuar()
    {
        var m = Nuevo(4);
        m.Iniciar(DateTime.UnixEpoch);
        Assert.False(m.Ejecutar("otro", new AccionJuego { Tipo = "piloto" }, DateTime.UnixEpoch, _ => null).Valida);
    }
}
