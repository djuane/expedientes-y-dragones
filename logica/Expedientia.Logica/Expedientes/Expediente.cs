using System;
using System.Collections.Generic;
using System.Linq;
using Expedientia.Logica.Combate;
using Expedientia.Logica.Dados;
using Expedientia.Logica.Juridico;
using Expedientia.Logica.Personajes;
using CombateMotor = Expedientia.Logica.Combate.Combate;

namespace Expedientia.Logica.Expedientes
{
    public enum TipoSala { Combate, Tramite, Audiencia, Libreria, Cafeteria, Coimero, RedDelCoimero, Jefe }

    public enum OpcionAudiencia { Confrontar, RevisarExpediente }

    public enum DecisionCoimero { Aceptar, Rechazar, Denunciar }

    public enum ObjetoTienda { EscritoPreFirmado, CafeDoble, CodigoAnotado }

    public sealed class Sentencia
    {
        public bool Fundada { get; }
        public string Motivo { get; }
        public int Honorarios { get; }
        public int Experiencia { get; }
        public Sentencia(bool fundada, string motivo, int honorarios, int experiencia)
        { Fundada = fundada; Motivo = motivo; Honorarios = honorarios; Experiencia = experiencia; }
    }

    public sealed class MiembroDelEquipo
    {
        public FichaCombatiente Ficha { get; }
        /// <summary>Id del jugador humano; null si lo maneja un bot.</summary>
        public string? JugadorId { get; internal set; }
        public bool EsBot => JugadorId == null;
        public MiembroDelEquipo(FichaCombatiente ficha, string? jugadorId) { Ficha = ficha; JugadorId = jugadorId; }
    }

    /// <summary>
    /// Una partida: el equipo recorre de 8 a 12 salas y cierra el caso venciendo al jefe.
    /// Todo el azar sale de la semilla, así el servidor puede reproducir la partida.
    /// </summary>
    public sealed class Expediente
    {
        public const int CostoEscritoPreFirmado = 30;
        public const int CostoCafeDoble = 20;
        public const int CostoCodigoAnotado = 40;

        private readonly IGeneradorAleatorio _azar;
        private readonly int _semilla;
        private readonly List<TipoSala> _salas;
        private readonly Dictionary<string, Combatiente> _equipo;
        private int _contadorEnemigos;
        private bool _ventajaProximaPrueba;
        private bool _escritoPreFirmado;
        private int _honorariosGanados;
        private readonly List<Mejora> _ofertas = new List<Mejora>();
        private readonly List<Mejora> _tomadas = new List<Mejora>();
        private bool _resorteoUsado;
        private int _combatesGanados;

        public Caso Caso { get; }
        public EstadoEstudio Estudio { get; }
        public IReadOnlyList<MiembroDelEquipo> Miembros { get; }
        public IReadOnlyList<TipoSala> Salas => _salas;
        public int IndiceSala { get; private set; } = -1;
        public TipoSala SalaActual => _salas[IndiceSala];
        public CombateMotor? CombateActual { get; private set; }
        public bool SalaResuelta { get; private set; }
        public int? MinutosRestantes { get; private set; }
        public Sentencia? Sentencia { get; private set; }
        public bool Terminado => Sentencia != null;
        public ControlPruebasJuridicas PruebasJuridicas { get; } = new ControlPruebasJuridicas();
        public IReadOnlyCollection<Combatiente> Equipo => _equipo.Values;

        /// <summary>Mejoras por elegir ahora (vacío si no hay elección pendiente).</summary>
        public IReadOnlyList<Mejora> MejorasOfrecidas => _ofertas;
        public IReadOnlyList<Mejora> MejorasTomadas => _tomadas;
        public bool PuedeVolverASortear => _ofertas.Count > 0 && !_resorteoUsado && Estudio.Tiene(Desbloqueo.SegundaOpinion);

        /// <summary>Cómo se ve la sala actual (trama); la capa visual lo muestra tal cual.</summary>
        public string TextoSala { get; private set; } = "";

        public Expediente(Caso caso, int semilla, IEnumerable<MiembroDelEquipo> miembros, EstadoEstudio? estudio = null)
        {
            Caso = caso ?? throw new ArgumentNullException(nameof(caso));
            Miembros = miembros.ToList();
            if (Miembros.Count < 1 || Miembros.Count > 4) throw new ArgumentException("El equipo es de 1 a 4 personajes.");
            if (Miembros.Any(m => m.Ficha.Bando != Bando.Estudio)) throw new ArgumentException("El equipo es del estudio.");
            Estudio = estudio ?? new EstadoEstudio();
            _azar = new GeneradorConSemilla(semilla);
            _semilla = semilla;
            _equipo = Miembros.ToDictionary(m => m.Ficha.Id, m => new Combatiente(m.Ficha, new Casilla(0, 0)));
            _salas = GenerarSalas(_azar);
            MinutosRestantes = caso.MinutosDePlazo;
            if (Estudio.Tiene(Desbloqueo.OficinaPropia))
                foreach (var c in _equipo.Values) { c.Ficha = c.Ficha.Con(paciencia: c.Ficha.PacienciaMaxima + 4); c.Paciencia = c.Ficha.PacienciaMaxima; }
        }

        public MiembroDelEquipo Miembro(string id) => Miembros.First(m => m.Ficha.Id == id);

        /// <summary>Un jugador toma el personaje de un bot (o un bot toma el de un jugador que se fue, con null).</summary>
        public void AsignarJugador(string miembroId, string? jugadorId) => Miembro(miembroId).JugadorId = jugadorId;

        /// <summary>8 a 12 salas: empieza con combate, termina con el jefe, siempre hay una cafetería y una de cada 3 partidas trae al Coimero.</summary>
        private static List<TipoSala> GenerarSalas(IGeneradorAleatorio azar)
        {
            var total = azar.Entre(8, 12);
            var medio = new List<TipoSala> { TipoSala.Tramite, TipoSala.Audiencia, TipoSala.Cafeteria, TipoSala.Libreria };
            if (azar.Entre(1, 3) == 1) medio.Add(TipoSala.Coimero);
            var pozo = new[] { TipoSala.Combate, TipoSala.Combate, TipoSala.Combate, TipoSala.Tramite, TipoSala.Audiencia };
            while (medio.Count < total - 2) medio.Add(pozo[azar.Entre(0, pozo.Length - 1)]);
            for (var i = medio.Count - 1; i > 0; i--)
            {
                var j = azar.Entre(0, i);
                (medio[i], medio[j]) = (medio[j], medio[i]);
            }
            var salas = new List<TipoSala> { TipoSala.Combate };
            salas.AddRange(medio);
            salas.Add(TipoSala.Jefe);
            return salas;
        }

        /// <summary>Pasa a la siguiente sala (la primera vez, entra a la sala 1).</summary>
        public IReadOnlyList<EventoCombate> Avanzar()
        {
            if (Terminado) throw new InvalidOperationException("El expediente está cerrado.");
            if (IndiceSala >= 0 && !SalaResuelta) throw new InvalidOperationException("La sala actual no está resuelta.");
            var eventos = new List<EventoCombate>();
            // Si nadie eligió la mejora, el juego elige por el equipo (como un bot).
            if (_ofertas.Count > 0) eventos.AddRange(ElegirMejora(Mejoras.ElegirComoBot(_ofertas, _equipo.Values, MinutosRestantes)));
            if (IndiceSala == _salas.Count - 1) { Cerrar(true, "Expediente cerrado: Fundado", eventos); return eventos; }
            var guion = Guion.De(Caso.Id);
            var primera = IndiceSala < 0;
            if (primera) Decir(guion.Llegada, eventos);
            IndiceSala++;
            SalaResuelta = false;
            CombateActual = null;
            TextoSala = Guion.DescribirSala(Caso.Id, SalaActual, _semilla, IndiceSala);
            eventos.Add(new SalaIniciada(IndiceSala, SalaActual));
            switch (SalaActual)
            {
                case TipoSala.Combate:
                    PrepararCombate(eventos, NuevosEnemigos(CantidadDeEnemigos()), null);
                    break;
                case TipoSala.RedDelCoimero:
                {
                    var red = new List<FichaCombatiente>();
                    for (var i = 0; i < CantidadDeEnemigos() + 1; i++) red.Add(Catalogo.Tramitador($"e{++_contadorEnemigos}"));
                    var aliados = new List<FichaCombatiente>();
                    if (Estudio.AliadosHonestosDisponibles)
                    {
                        aliados.Add(Catalogo.SuboficialQuispe("quispe"));
                        eventos.Add(new Dialogo(Guion.QuispeLlega.Personaje, Guion.QuispeLlega.Texto));
                    }
                    PrepararCombate(eventos, red, null, aliados);
                    break;
                }
                case TipoSala.Jefe:
                    PrepararJefe(eventos, guion);
                    break;
                case TipoSala.Coimero:
                    eventos.Add(new Dialogo(Guion.Coimero, "Doctor, por una propina su escrito entra primero. Nadie se entera."));
                    break;
            }
            if (primera && Estudio.Tiene(Desbloqueo.MejoraDeEntrada)) Ofrecer(eventos);
            return eventos;
        }

        private void PrepararJefe(List<EventoCombate> eventos, GuionDeCaso guion)
        {
            if (Caso.Jefe == CatalogoCasos.JefeRequisitoImposible)
            {
                var jefe = Catalogo.RequisitoImposible("jefe", 16 + 6 * Miembros.Count + 3 * _tomadas.Count + 3 * Fama);
                var conMendoza = Estudio.AliadosHonestosDisponibles;
                var inicial = new[] { RequisitoImposible.EnPersona, RequisitoImposible.Virtual }[_azar.Entre(0, 1)];
                var mecanica = new RequisitoImposible(jefe.Id, inicial, n => Catalogo.Formalismo($"req-{n}"), conMendoza, maximoDeRefuerzos: Math.Max(1, Miembros.Count / 2));
                TextoSala = "Jefe: El Requisito Imposible. Cada 2 rondas cambia lo que exige para recibir daño.";
                Decir(guion.Jefe, eventos);
                if (conMendoza) eventos.Add(new Dialogo(Guion.MendozaRevela.Personaje, Guion.MendozaRevela.Texto));
                PrepararCombate(eventos, new List<FichaCombatiente> { jefe }, mecanica);
            }
            else
            {
                var jefe = Catalogo.ColaInfinita("jefe", 18 + 6 * Miembros.Count + 3 * _tomadas.Count + 3 * Fama);
                TextoSala = "Jefe: La Cola Infinita. Cada turno se suma un usuario a la fila.";
                Decir(guion.Jefe, eventos);
                PrepararCombate(eventos, new List<FichaCombatiente> { jefe },
                    new ColaInfinita(jefe.Id, Catalogo.EsbirroDeLaCola, maximo: 3 + Miembros.Count));
            }
        }

        private static void Decir(IEnumerable<LineaDeGuion> lineas, List<EventoCombate> eventos)
        {
            foreach (var l in lineas) eventos.Add(new Dialogo(l.Personaje, l.Texto));
        }

        /// <summary>Enemigos por sala de combate según el tamaño del equipo (ajustado con la simulación de partidas).</summary>
        /// <summary>
        /// Enemigos por sala de combate según el tamaño del equipo, y uno más a partir del segundo combate ganado:
        /// el equipo se hace fuerte con las mejoras y la burocracia responde (ajustado con la simulación de partidas).
        /// </summary>
        private int CantidadDeEnemigos() => Miembros.Count switch { 1 => 1, 2 => 2, 3 => 3, _ => 5 } + (_combatesGanados >= 1 ? 1 : 0) + (_combatesGanados >= 3 ? 1 : 0);

        /// <summary>
        /// Fama del estudio: cuantos más expedientes cerró, más se resiste la burocracia (más paciencia en los enemigos).
        /// Compensa los desbloqueos para que el juego no se vuelva un paseo.
        /// </summary>
        private int Fama => Math.Min(6, Estudio.ExpedientesCerrados);

        private List<FichaCombatiente> NuevosEnemigos(int cantidad)
        {
            var lista = new List<FichaCombatiente>();
            var contrataciones = Caso.Calabozo == CatalogoCasos.SalaDeContrataciones;
            for (var i = 0; i < cantidad; i++)
            {
                _contadorEnemigos++;
                var id = $"e{_contadorEnemigos}";
                var tirada = _azar.Entre(1, 3);
                var ficha = contrataciones
                    ? tirada switch { 1 => Catalogo.VuelvaManiana(id), 2 => Catalogo.Fraccionador(id), _ => Catalogo.Formalismo(id) }
                    : tirada switch { 1 => Catalogo.PlazoQueVence(id), 2 => Catalogo.NotificacionDefectuosa(id), _ => Catalogo.Formalismo(id) };
                lista.Add(Fama > 0 ? ficha.Con(paciencia: ficha.PacienciaMaxima + Fama) : ficha);
            }
            return lista;
        }

        private void PrepararCombate(List<EventoCombate> eventos, List<FichaCombatiente> enemigos, IMecanicaDeJefe? mecanica, List<FichaCombatiente>? aliados = null)
        {
            var vivos = _equipo.Values.Where(c => !c.Caido).ToList();
            var columnasEquipo = new[] { 1, 4, 2, 3 };
            for (var i = 0; i < vivos.Count; i++)
            {
                vivos[i].Posicion = new Casilla(columnasEquipo[i], Cuadricula.Filas - 1);
                vivos[i].TurnosInmovilizado = 0;
                vivos[i].TurnosSinActuar = 0;
                vivos[i].RondaUltimaHabilidad = null;
            }
            var columnasEnemigo = new[] { 2, 0, 5, 3, 1, 4 };
            var participantes = new List<Combatiente>(vivos);
            var columnasAliados = new[] { 0, 5 };
            if (aliados != null)
                for (var i = 0; i < aliados.Count; i++)
                    participantes.Add(new Combatiente(aliados[i], new Casilla(columnasAliados[i % 2], Cuadricula.Filas - 1 - i / 2)));
            for (var i = 0; i < enemigos.Count; i++)
                participantes.Add(new Combatiente(enemigos[i], new Casilla(columnasEnemigo[i % 6], i / 6)));
            CombateActual = new CombateMotor(_azar, participantes, mecanica);
            eventos.AddRange(CombateActual.Iniciar());
        }

        private void Ofrecer(List<EventoCombate> eventos)
        {
            _ofertas.Clear();
            _ofertas.AddRange(Mejoras.Sortear(_azar, Estudio.Tiene(Desbloqueo.BibliotecaDelEstudio), _tomadas.Where(EsUnica).ToList()));
            if (_ofertas.Count > 0) eventos.Add(new MejorasOfrecidas(_ofertas.ToList()));
        }

        /// <summary>Las que no se apilan no vuelven a salir (el resto sí: dos tazas de café se notan).</summary>
        private static bool EsUnica(Mejora m) => m == Mejora.SelloDeUrgencia || m == Mejora.ZapatosComodos;

        /// <summary>El equipo se queda con una de las mejoras ofrecidas.</summary>
        public IReadOnlyList<EventoCombate> ElegirMejora(Mejora mejora)
        {
            if (Terminado) throw new InvalidOperationException("El expediente está cerrado.");
            if (!_ofertas.Contains(mejora)) throw new InvalidOperationException("Esa mejora no está entre las ofrecidas.");
            _ofertas.Clear();
            _tomadas.Add(mejora);
            if (mejora == Mejora.AgendaDePlazos)
            {
                if (MinutosRestantes.HasValue) MinutosRestantes += 25; else _honorariosGanados += 25;
            }
            else foreach (var c in _equipo.Values) Mejoras.Aplicar(mejora, c);
            var eventos = new List<EventoCombate> { new MejoraElegida(mejora) };
            if (mejora == Mejora.AgendaDePlazos && MinutosRestantes.HasValue) eventos.Add(new RelojAvanzo(MinutosRestantes.Value));
            return eventos;
        }

        /// <summary>Segunda opinión (desbloqueo): una vez por expediente se vuelven a sortear las mejoras.</summary>
        public IReadOnlyList<EventoCombate> VolverASortear()
        {
            if (!PuedeVolverASortear) throw new InvalidOperationException("No se puede volver a sortear ahora.");
            _resorteoUsado = true;
            var antes = _ofertas.ToList();
            var eventos = new List<EventoCombate>();
            Ofrecer(eventos);
            // Que salga al menos una distinta si hay de dónde.
            if (_ofertas.SequenceEqual(antes)) { eventos.Clear(); Ofrecer(eventos); }
            return eventos;
        }

        /// <summary>Después de cada acción de combate: si terminó, resuelve la sala o archiva el caso.</summary>
        public IReadOnlyList<EventoCombate> RevisarCombate()
        {
            var eventos = new List<EventoCombate>();
            if (CombateActual == null || !CombateActual.Terminado || SalaResuelta || Terminado) return eventos;
            var rondas = CombateActual.Ronda;
            if (CombateActual.Ganador == Bando.Estudio)
            {
                SalaResuelta = true;
                var botin = SalaActual == TipoSala.Jefe ? 60 : SalaActual == TipoSala.RedDelCoimero ? 50 : 15;
                _honorariosGanados += botin;
                if (SalaActual == TipoSala.RedDelCoimero) Estudio.CambiarReputacion(+10);
                // Respiro tras la victoria: los caídos se reincorporan con un cuarto de su paciencia.
                foreach (var m in _equipo.Values.Where(x => x.Caido)) m.Paciencia = Math.Max(1, m.Ficha.PacienciaMaxima / 4);
                eventos.Add(new SalaSuperada(IndiceSala, "Combate ganado"));
                ConsumirMinutos(4 + rondas * 2, eventos);
                _combatesGanados++;
                if (SalaActual != TipoSala.Jefe && !Terminado) Ofrecer(eventos);
            }
            else
            {
                Cerrar(false, "Archivado: el equipo cayó", eventos);
            }
            return eventos;
        }

        /// <summary>
        /// Trámite: prueba de Criterio contra dificultad 12. Una prueba jurídica acertada antes da ventaja.
        /// Si falla, cuesta tiempo y paciencia, pero se sigue adelante.
        /// </summary>
        public IReadOnlyList<EventoCombate> ResolverTramite(string miembroId, Modo modo = Modo.Normal)
        {
            ExigirSala(TipoSala.Tramite);
            var c = _equipo[miembroId];
            var r = Personajes.Pruebas.DeAtributo(_azar, c.Ficha.Atributos, Atributo.Criterio, c.Ficha.Nivel, true, 12, ConVentajaGuardada(modo));
            var eventos = new List<EventoCombate>();
            if (r.Exito) eventos.Add(new SalaSuperada(IndiceSala, $"Requisito correcto ({r.Total})"));
            else
            {
                foreach (var m in _equipo.Values.Where(x => !x.Caido)) m.Paciencia = Math.Max(1, m.Paciencia - 2);
                eventos.Add(new SalaSuperada(IndiceSala, $"Observado: subsanan a la segunda ({r.Total})"));
            }
            SalaResuelta = true;
            ConsumirMinutos(r.Exito ? 10 : 20, eventos);
            return eventos;
        }

        /// <summary>Audiencia: el Cliente cambia su versión. Confrontarlo (Oratoria) o revisar el expediente (Criterio, puede ganar tiempo).</summary>
        public IReadOnlyList<EventoCombate> ResolverAudiencia(string miembroId, OpcionAudiencia opcion, Modo modo = Modo.Normal)
        {
            ExigirSala(TipoSala.Audiencia);
            var c = _equipo[miembroId];
            var atributo = opcion == OpcionAudiencia.Confrontar ? Atributo.Oratoria : Atributo.Criterio;
            var r = Personajes.Pruebas.DeAtributo(_azar, c.Ficha.Atributos, atributo, c.Ficha.Nivel, true, 13, ConVentajaGuardada(modo));
            var eventos = new List<EventoCombate>();
            SalaResuelta = true;
            if (r.Exito && opcion == OpcionAudiencia.RevisarExpediente)
            {
                eventos.Add(new SalaSuperada(IndiceSala, "Encontraron una fecha que les da más tiempo"));
                if (MinutosRestantes.HasValue) { MinutosRestantes += 20; eventos.Add(new RelojAvanzo(MinutosRestantes.Value)); }
                else _honorariosGanados += 10;
            }
            else if (r.Exito)
            {
                _honorariosGanados += 20;
                eventos.Add(new SalaSuperada(IndiceSala, "El Cliente reconoce la versión correcta"));
            }
            else
            {
                eventos.Add(new SalaSuperada(IndiceSala, "La audiencia se reprograma"));
                ConsumirMinutos(10, eventos);
            }
            ConsumirMinutos(10, eventos);
            return eventos;
        }

        /// <summary>Cafetería del juzgado: todos recuperan la mitad de su paciencia máxima.</summary>
        public IReadOnlyList<EventoCombate> Descansar()
        {
            ExigirSala(TipoSala.Cafeteria);
            foreach (var m in _equipo.Values.Where(x => !x.Caido))
                m.Paciencia = Math.Min(m.Ficha.PacienciaMaxima, m.Paciencia + (m.Ficha.PacienciaMaxima + 1) / 2);
            SalaResuelta = true;
            var eventos = new List<EventoCombate> { new SalaSuperada(IndiceSala, "Un café y a seguir") };
            ConsumirMinutos(15, eventos);
            return eventos;
        }

        /// <summary>Librería jurídica (o el Ekeko): se paga con honorarios del juego, nunca con dinero real.</summary>
        public bool Comprar(ObjetoTienda objeto, out string motivo)
        {
            ExigirSala(TipoSala.Libreria);
            var costo = objeto switch
            {
                ObjetoTienda.EscritoPreFirmado => CostoEscritoPreFirmado,
                ObjetoTienda.CafeDoble => CostoCafeDoble,
                _ => CostoCodigoAnotado,
            };
            var disponible = Estudio.Honorarios + _honorariosGanados;
            if (disponible < costo) { motivo = "No alcanzan los honorarios."; return false; }
            _honorariosGanados -= costo;
            switch (objeto)
            {
                case ObjetoTienda.EscritoPreFirmado: _escritoPreFirmado = true; break;
                case ObjetoTienda.CafeDoble:
                    foreach (var m in _equipo.Values.Where(x => !x.Caido)) m.Paciencia = m.Ficha.PacienciaMaxima;
                    break;
                case ObjetoTienda.CodigoAnotado: _ventajaProximaPrueba = true; break;
            }
            motivo = "Comprado.";
            return true;
        }

        public IReadOnlyList<EventoCombate> SalirDeLaTienda()
        {
            ExigirSala(TipoSala.Libreria);
            SalaResuelta = true;
            var eventos = new List<EventoCombate> { new SalaSuperada(IndiceSala, "Salen de la librería") };
            ConsumirMinutos(5, eventos);
            return eventos;
        }

        /// <summary>
        /// El Coimero tienta; no ataca. Aceptar ayuda hoy y suma al Expediente Disciplinario oculto;
        /// rechazar sube la Reputación; denunciar abre una sala extra más difícil con la mejor recompensa.
        /// </summary>
        public IReadOnlyList<EventoCombate> DecidirCoimero(DecisionCoimero decision)
        {
            ExigirSala(TipoSala.Coimero);
            var eventos = new List<EventoCombate>();
            switch (decision)
            {
                case DecisionCoimero.Aceptar:
                    foreach (var m in _equipo.Values.Where(x => !x.Caido)) m.Paciencia = m.Ficha.PacienciaMaxima;
                    Estudio.ExpedienteDisciplinario += 10;
                    eventos.Add(new SalaSuperada(IndiceSala, "El trámite \"se agilizó\""));
                    break;
                case DecisionCoimero.Rechazar:
                    Estudio.CambiarReputacion(+3);
                    eventos.Add(new SalaSuperada(IndiceSala, "Rechazaron la oferta"));
                    break;
                case DecisionCoimero.Denunciar:
                    Estudio.CambiarReputacion(+2);
                    _salas.Insert(IndiceSala + 1, TipoSala.RedDelCoimero);
                    eventos.Add(new SalaSuperada(IndiceSala, "Denunciaron: la red del Coimero los espera"));
                    eventos.Add(new Dialogo(Guion.Eusebio, "Bien hecho, doctores. Ahora prepárense: la red no se va a dejar."));
                    break;
            }
            SalaResuelta = true;
            ConsumirMinutos(5, eventos);
            return eventos;
        }

        /// <summary>El "Escrito pre-firmado" comprado en la tienda se gasta para tener ventaja en una tirada.</summary>
        public bool UsarEscritoPreFirmado()
        {
            if (!_escritoPreFirmado) return false;
            _escritoPreFirmado = false;
            return true;
        }

        public bool TieneEscritoPreFirmado => _escritoPreFirmado;
        public bool TieneCodigoAnotado => _ventajaProximaPrueba;
        public int HonorariosDisponibles => Estudio.Honorarios + _honorariosGanados;

        private Modo ConVentajaGuardada(Modo modo)
        {
            if (!_ventajaProximaPrueba) return modo;
            _ventajaProximaPrueba = false;
            return Dado.Combinar(true, modo == Modo.Desventaja);
        }

        private void ConsumirMinutos(int minutos, List<EventoCombate> eventos)
        {
            if (!MinutosRestantes.HasValue || Terminado) return;
            MinutosRestantes = Math.Max(0, MinutosRestantes.Value - minutos);
            eventos.Add(new RelojAvanzo(MinutosRestantes.Value));
            if (MinutosRestantes == 0)
            {
                Cerrar(false, "Improcedente por extemporáneo", eventos);
            }
        }

        private void Cerrar(bool fundada, string motivo, List<EventoCombate> eventos)
        {
            if (Terminado) return;
            var salasSuperadas = IndiceSala + (SalaResuelta ? 1 : 0);
            var honorarios = Math.Max(0, 100 + _honorariosGanados + 10 * salasSuperadas);
            var experiencia = 50 * Math.Max(1, salasSuperadas) + (fundada ? 150 : 0);
            if (!fundada) { honorarios /= 4; experiencia /= 2; }
            if (Estudio.ExpedienteDisciplinario >= 30 && _azar.Entre(1, 2) == 1)
            {
                honorarios /= 2;
                eventos.Add(new Dialogo(Guion.Eusebio, "Llegó una investigación disciplinaria. La multa sale de los honorarios."));
            }
            var guion = Guion.De(Caso.Id);
            Decir(fundada ? guion.Victoria : guion.Derrota, eventos);
            Sentencia = new Sentencia(fundada, motivo, honorarios, experiencia);
            Estudio.Honorarios += honorarios;
            Estudio.Experiencia += experiencia;
            if (fundada)
            {
                Estudio.ExpedientesCerrados++;
                Estudio.CambiarReputacion(+1);
                var nuevo = EstadoEstudio.Desbloqueos.FirstOrDefault(d => d.ExpedientesNecesarios == Estudio.ExpedientesCerrados);
                if (nuevo != null) eventos.Add(new DesbloqueoGanado(nuevo));
            }
            CombateActual = null;
            _ofertas.Clear();
            eventos.Add(new SentenciaDictada(Sentencia));
        }

        private void ExigirSala(TipoSala tipo)
        {
            if (Terminado) throw new InvalidOperationException("El expediente está cerrado.");
            if (IndiceSala < 0 || SalaActual != tipo) throw new InvalidOperationException($"La sala actual no es {tipo}.");
            if (SalaResuelta) throw new InvalidOperationException("La sala ya se resolvió.");
        }
    }
}
