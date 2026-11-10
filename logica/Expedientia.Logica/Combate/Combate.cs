using System;
using System.Collections.Generic;
using System.Linq;
using Expedientia.Logica.Dados;
using Expedientia.Logica.Personajes;

namespace Expedientia.Logica.Combate
{
    public sealed class ResultadoAccion
    {
        public bool Valida { get; }
        public string? Motivo { get; }
        public IReadOnlyList<EventoCombate> Eventos { get; }
        private ResultadoAccion(bool v, string? m, IReadOnlyList<EventoCombate> e) { Valida = v; Motivo = m; Eventos = e; }
        internal static ResultadoAccion Ok(List<EventoCombate> e) => new ResultadoAccion(true, null, e);
        internal static ResultadoAccion Rechazada(string motivo) => new ResultadoAccion(false, motivo, Array.Empty<EventoCombate>());
    }

    /// <summary>
    /// Combate por turnos: cada combatiente tiene 1 acción, su movimiento y 1 reacción por ronda;
    /// el orden lo decide la iniciativa. El servidor es la autoridad: valida cada acción y emite eventos.
    /// </summary>
    public sealed class Combate
    {
        private readonly IGeneradorAleatorio _azar;
        private readonly List<Combatiente> _todos;
        private List<Combatiente> _orden = new List<Combatiente>();
        private int _indice;
        private bool _actuoEsteTurno;
        private readonly Dictionary<string, (string objetivo, Modo modo)> _ultimoAtaqueFallido = new Dictionary<string, (string, Modo)>();

        public int Ronda { get; private set; }
        public bool Terminado { get; private set; }
        public Bando? Ganador { get; private set; }
        public bool CasacionUsada { get; private set; }
        public IReadOnlyList<Combatiente> Combatientes => _todos;
        public IReadOnlyList<Combatiente> Orden => _orden;
        public Combatiente Actual => _orden[_indice];

        /// <summary>Mecánica especial del jefe de la sala, si la hay.</summary>
        public IMecanicaDeJefe? Mecanica { get; }
        internal IGeneradorAleatorio Azar => _azar;

        public Combate(IGeneradorAleatorio azar, IEnumerable<Combatiente> combatientes, IMecanicaDeJefe? mecanica = null)
        {
            Mecanica = mecanica;
            _azar = azar ?? throw new ArgumentNullException(nameof(azar));
            _todos = combatientes.ToList();
            if (_todos.Select(c => c.Id).Distinct().Count() != _todos.Count) throw new ArgumentException("Ids repetidos.");
            if (_todos.Any(c => !Cuadricula.Contiene(c.Posicion))) throw new ArgumentException("Hay combatientes fuera de la cuadrícula.");
            if (_todos.Select(c => c.Posicion).Distinct().Count() != _todos.Count) throw new ArgumentException("Dos combatientes en la misma casilla.");
            if (!_todos.Any(c => c.Bando == Bando.Estudio) || !_todos.Any(c => c.Bando == Bando.Sistema)) throw new ArgumentException("Faltan combatientes de un bando.");
        }

        public IReadOnlyList<EventoCombate> Iniciar()
        {
            if (Ronda != 0) throw new InvalidOperationException("El combate ya empezó.");
            var porId = _todos.ToDictionary(c => c.Id);
            _orden = Iniciativa.Ordenar(_azar, _todos.Select(c => new Participante(c.Id, c.Ficha.Atributos.Modificador(Atributo.AgilidadProcesal))))
                .Select(t => porId[t.Participante.Id]).ToList();
            Ronda = 1;
            _indice = 0;
            var eventos = new List<EventoCombate>();
            EmpezarTurno(eventos);
            return eventos;
        }

        public Combatiente Buscar(string id) => _todos.FirstOrDefault(c => c.Id == id) ?? throw new ArgumentException($"No existe {id}.");

        public bool PuedeMoverA(Casilla destino, out string? motivo)
        {
            var yo = Actual;
            motivo = null;
            if (Terminado) motivo = "El combate terminó.";
            else if (yo.Inmovilizado) motivo = "Está inmovilizado.";
            else if (!Cuadricula.Contiene(destino)) motivo = "Fuera de la cuadrícula.";
            else if (_todos.Any(c => !c.Caido && c.Posicion == destino)) motivo = "Casilla ocupada.";
            else if (yo.Posicion.DistanciaA(destino) > yo.MovimientoRestante) motivo = "No le alcanza el movimiento.";
            return motivo == null;
        }

        /// <summary>Casillas a las que puede moverse ahora (para iluminarlas en la interfaz).</summary>
        public IEnumerable<Casilla> CasillasValidas()
        {
            for (var x = 0; x < Cuadricula.Columnas; x++)
                for (var y = 0; y < Cuadricula.Filas; y++)
                {
                    var c = new Casilla(x, y);
                    if (c != Actual.Posicion && PuedeMoverA(c, out _)) yield return c;
                }
        }

        public ResultadoAccion Mover(Casilla destino)
        {
            if (!PuedeMoverA(destino, out var motivo)) return ResultadoAccion.Rechazada(motivo!);
            var yo = Actual;
            var desde = yo.Posicion;
            yo.MovimientoRestante -= desde.DistanciaA(destino);
            yo.Posicion = destino;
            _actuoEsteTurno = true;
            return ResultadoAccion.Ok(new List<EventoCombate> { new Movio(yo.Id, desde, destino) });
        }

        public ResultadoAccion Atacar(string objetivoId, Modo modo = Modo.Normal)
        {
            var yo = Actual;
            var rechazo = ValidarAccion(yo) ?? ValidarObjetivoEnemigo(yo, objetivoId, yo.Ficha.Alcance);
            if (rechazo != null) return ResultadoAccion.Rechazada(rechazo);
            yo.AccionDisponible = false;
            _actuoEsteTurno = true;
            var eventos = new List<EventoCombate>();
            ResolverAtaque(yo, Buscar(objetivoId), modo, eventos);
            return ResultadoAccion.Ok(eventos);
        }

        public ResultadoAccion UsarHabilidad(Habilidad habilidad, string objetivoId)
        {
            var yo = Actual;
            switch (habilidad)
            {
                case Habilidad.MedidaCautelar:
                    return Inmovilizar(yo, Especialidad.Civilista, habilidad, objetivoId);
                case Habilidad.Queja:
                    return Inmovilizar(yo, Especialidad.Administrativista, habilidad, objetivoId);
                default:
                    return ResultadoAccion.Rechazada("Esa habilidad se usa como reacción.");
            }
        }

        /// <summary>
        /// Recurso de apelación (reacción del Administrativista): el aliado repite su último ataque fallido de esta ronda.
        /// Puede usarse fuera del turno del Administrativista.
        /// </summary>
        public ResultadoAccion RecursoDeApelacion(string administrativistaId, string aliadoId)
        {
            if (Terminado) return ResultadoAccion.Rechazada("El combate terminó.");
            var adm = Buscar(administrativistaId);
            if (adm.Ficha.Especialidad != Especialidad.Administrativista) return ResultadoAccion.Rechazada("Solo el Administrativista apela.");
            if (adm.Caido) return ResultadoAccion.Rechazada("Está caído.");
            if (!adm.ReaccionDisponible) return ResultadoAccion.Rechazada("Ya usó su reacción.");
            var aliado = Buscar(aliadoId);
            if (aliado.Bando != adm.Bando) return ResultadoAccion.Rechazada("Solo se apela por un aliado.");
            if (!_ultimoAtaqueFallido.TryGetValue(aliadoId, out var previo)) return ResultadoAccion.Rechazada("No hay una tirada fallida que apelar.");
            var objetivo = Buscar(previo.objetivo);
            if (objetivo.Caido) return ResultadoAccion.Rechazada("El objetivo ya cayó.");
            adm.ReaccionDisponible = false;
            _ultimoAtaqueFallido.Remove(aliadoId);
            var eventos = new List<EventoCombate> { new UsoHabilidad(adm.Id, Habilidad.RecursoDeApelacion, aliadoId) };
            ResolverAtaque(aliado, objetivo, previo.modo, eventos);
            return ResultadoAccion.Ok(eventos);
        }

        /// <summary>
        /// Casación: una vez por partida para todo el equipo. Primero se debe superar una tirada de Oratoria
        /// contra dificultad 18; si se admite, el aliado repite su último ataque fallido.
        /// </summary>
        public ResultadoAccion Casacion(string solicitanteId, string aliadoId)
        {
            if (Terminado) return ResultadoAccion.Rechazada("El combate terminó.");
            if (CasacionUsada) return ResultadoAccion.Rechazada("La Casación ya se usó en esta partida.");
            var sol = Buscar(solicitanteId);
            var aliado = Buscar(aliadoId);
            if (sol.Bando != Bando.Estudio || aliado.Bando != Bando.Estudio || sol.Caido) return ResultadoAccion.Rechazada("Solo el estudio puede interponer Casación.");
            if (!_ultimoAtaqueFallido.TryGetValue(aliadoId, out var previo)) return ResultadoAccion.Rechazada("No hay una tirada fallida que casar.");
            CasacionUsada = true;
            var eventos = new List<EventoCombate> { new UsoHabilidad(sol.Id, Habilidad.Casacion, aliadoId) };
            var admision = Personajes.Pruebas.DeAtributo(_azar, sol.Ficha.Atributos, Atributo.Oratoria, sol.Ficha.Nivel, true, ReglasHabilidades.DificultadCasacion);
            if (!admision.Exito) return ResultadoAccion.Ok(eventos);
            _ultimoAtaqueFallido.Remove(aliadoId);
            ResolverAtaque(aliado, Buscar(previo.objetivo), previo.modo, eventos);
            return ResultadoAccion.Ok(eventos);
        }

        public IReadOnlyList<EventoCombate> TerminarTurno()
        {
            if (Terminado) throw new InvalidOperationException("El combate terminó.");
            var eventos = new List<EventoCombate>();
            var yo = Actual;
            // Solo cuenta el silencio voluntario: estar inmovilizado no es "no actuar".
            yo.TurnosSinActuar = _actuoEsteTurno || yo.Inmovilizado ? 0 : yo.TurnosSinActuar + 1;
            if (yo.TurnosInmovilizado > 0) yo.TurnosInmovilizado--;
            if (RevisarSilencioAdministrativo(eventos)) return eventos;

            do
            {
                _indice++;
                if (_indice >= _orden.Count)
                {
                    _indice = 0;
                    Ronda++;
                    _ultimoAtaqueFallido.Clear();
                }
            } while (Actual.Caido);
            EmpezarTurno(eventos);
            return eventos;
        }

        private void EmpezarTurno(List<EventoCombate> eventos)
        {
            var yo = Actual;
            yo.AccionDisponible = !yo.Inmovilizado;
            yo.MovimientoRestante = yo.Inmovilizado ? 0 : yo.Ficha.Movimiento;
            yo.ReaccionDisponible = true;
            _actuoEsteTurno = false;
            eventos.Add(new InicioDeTurno(yo.Id, Ronda));
            if (Mecanica != null && Mecanica.JefeId == yo.Id && !yo.Inmovilizado && !yo.Caido)
                Mecanica.AlInicioDelTurnoDelJefe(this, eventos);
        }

        /// <summary>Casillas libres de la cuadrícula, en orden de fila y columna.</summary>
        public IEnumerable<Casilla> CasillasLibres()
        {
            for (var y = 0; y < Cuadricula.Filas; y++)
                for (var x = 0; x < Cuadricula.Columnas; x++)
                {
                    var c = new Casilla(x, y);
                    if (!_todos.Any(o => !o.Caido && o.Posicion == c)) yield return c;
                }
        }

        /// <summary>Suma un combatiente a mitad del combate; actúa al final de la ronda.</summary>
        internal void AgregarRefuerzo(Combatiente nuevo, List<EventoCombate> eventos)
        {
            if (_todos.Any(c => c.Id == nuevo.Id)) throw new ArgumentException("Id repetido.");
            _todos.Add(nuevo);
            _orden.Add(nuevo);
            eventos.Add(new Aparecio(nuevo.Id, nuevo.Ficha.Nombre, nuevo.Posicion));
        }

        private ResultadoAccion Inmovilizar(Combatiente yo, Especialidad requerida, Habilidad h, string objetivoId)
        {
            if (yo.Ficha.Especialidad != requerida) return ResultadoAccion.Rechazada($"{h} es de la especialidad {requerida}.");
            var rechazo = ValidarAccion(yo) ?? ValidarObjetivoEnemigo(yo, objetivoId, ReglasHabilidades.AlcanceDeHabilidad);
            if (rechazo != null) return ResultadoAccion.Rechazada(rechazo);
            if (yo.RondaUltimaHabilidad.HasValue && Ronda - yo.RondaUltimaHabilidad.Value < ReglasHabilidades.EnfriamientoEnRondas)
                return ResultadoAccion.Rechazada("La habilidad se está recargando.");
            yo.RondaUltimaHabilidad = Ronda;
            yo.AccionDisponible = false;
            _actuoEsteTurno = true;
            var obj = Buscar(objetivoId);
            obj.TurnosInmovilizado = Math.Max(obj.TurnosInmovilizado, ReglasHabilidades.TurnosDeInmovilizacion);
            return ResultadoAccion.Ok(new List<EventoCombate>
            {
                new UsoHabilidad(yo.Id, h, objetivoId),
                new Inmovilizado(objetivoId, obj.TurnosInmovilizado),
            });
        }

        private void ResolverAtaque(Combatiente atacante, Combatiente objetivo, Modo modo, List<EventoCombate> eventos)
        {
            var f = atacante.Ficha;
            var r = Personajes.Pruebas.Ataque(_azar, f.Atributos, f.AtributoDeAtaque, f.Nivel, objetivo.Ficha.Defensa, modo);
            eventos.Add(new Ataco(atacante.Id, objetivo.Id, r.Tirada.Natural, r.Total, r.Exito, r.Tirada.Momento));
            if (!r.Exito)
            {
                _ultimoAtaqueFallido[atacante.Id] = (objetivo.Id, modo);
                return;
            }
            _ultimoAtaqueFallido.Remove(atacante.Id);
            var danio = Personajes.Pruebas.Danio(_azar, f.DadosDeDanio, f.CarasDeDanio, f.Atributos.Modificador(f.AtributoDeAtaque), r.EsCritico);
            objetivo.Paciencia = Math.Max(0, objetivo.Paciencia - danio);
            eventos.Add(new RecibioDanio(objetivo.Id, danio, objetivo.Paciencia));
            if (objetivo.Caido)
            {
                eventos.Add(new Cayo(objetivo.Id));
                RevisarFin(eventos);
            }
        }

        private void RevisarFin(List<EventoCombate> eventos)
        {
            foreach (var bando in new[] { Bando.Estudio, Bando.Sistema })
            {
                if (_todos.Where(c => c.Bando == bando).All(c => c.Caido))
                {
                    var ganador = bando == Bando.Estudio ? Bando.Sistema : Bando.Estudio;
                    Finalizar(ganador, ganador == Bando.Estudio ? "Fundado: el equipo venció" : "El equipo cayó", eventos);
                    return;
                }
            }
        }

        /// <summary>Pasiva del Administrativista: un enemigo que no actúa 2 turnos seguidos le da la sala al equipo.</summary>
        private bool RevisarSilencioAdministrativo(List<EventoCombate> eventos)
        {
            var hayAdministrativista = _todos.Any(c => c.Bando == Bando.Estudio && !c.Caido && c.Ficha.Especialidad == Especialidad.Administrativista);
            if (!hayAdministrativista) return false;
            if (_todos.Any(c => c.Bando == Bando.Sistema && !c.Caido && c.TurnosSinActuar >= ReglasHabilidades.TurnosDeSilencio))
            {
                Finalizar(Bando.Estudio, "Silencio administrativo", eventos);
                return true;
            }
            return false;
        }

        private void Finalizar(Bando ganador, string motivo, List<EventoCombate> eventos)
        {
            if (Terminado) return;
            Terminado = true;
            Ganador = ganador;
            eventos.Add(new FinDeCombate(ganador, motivo));
        }

        private string? ValidarAccion(Combatiente yo)
        {
            if (Terminado) return "El combate terminó.";
            if (yo.Inmovilizado) return "Está inmovilizado.";
            if (!yo.AccionDisponible) return "Ya usó su acción.";
            return null;
        }

        private string? ValidarObjetivoEnemigo(Combatiente yo, string objetivoId, int alcance)
        {
            var obj = _todos.FirstOrDefault(c => c.Id == objetivoId);
            if (obj == null) return "Objetivo inexistente.";
            if (obj.Bando == yo.Bando) return "El objetivo es un aliado.";
            if (obj.Caido) return "El objetivo ya cayó.";
            if (yo.Posicion.DistanciaA(obj.Posicion) > alcance) return "Fuera de alcance.";
            return null;
        }
    }
}
