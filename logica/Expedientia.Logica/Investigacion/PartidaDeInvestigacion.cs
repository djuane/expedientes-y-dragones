using System;
using System.Collections.Generic;
using System.Linq;
using Expedientia.Logica.Expedientes;

namespace Expedientia.Logica.Investigacion
{
    public enum Fase { Investigacion, Audiencia, Sentencia }

    /// <summary>Lo que pasó al hacer algo: las líneas que se muestran y lo que cambió.</summary>
    public sealed class Resultado
    {
        public IReadOnlyList<LineaDeGuion> Lineas { get; }
        /// <summary>Prueba o pista nueva conseguida; null si no hubo.</summary>
        public Prueba? Nueva { get; internal set; }
        /// <summary>Al presentar: la prueba desmintió la frase.</summary>
        public bool Contradiccion { get; internal set; }
        public int CredibilidadPerdida { get; internal set; }
        /// <summary>Se rompió el testimonio y empieza el siguiente (o termina la audiencia).</summary>
        public bool TestimonioRoto { get; internal set; }
        /// <summary>El juez pide fundamentar: la capa visual puede hacer una prueba jurídica (acierto = credibilidad).</summary>
        public bool PideFundamento { get; internal set; }
        public bool SeAcaboElPlazo { get; internal set; }
        internal Resultado(IEnumerable<LineaDeGuion> lineas) { Lineas = lineas.ToList(); }
    }

    /// <summary>
    /// Una partida del modo investigación: se recorren lugares con un plazo en horas para juntar pruebas,
    /// y en la audiencia se desmiente a los testigos presentando la prueba justa en la frase justa.
    /// Errar cuesta credibilidad; sin credibilidad, el caso se pierde.
    /// </summary>
    public sealed class PartidaDeInvestigacion
    {
        public const int CredibilidadMaxima = 5;
        public const string Jueza = Guion.Valdivia;

        private readonly HashSet<string> _tengo = new HashSet<string>();
        private readonly List<string> _ordenObtenidas = new List<string>();
        private readonly HashSet<string> _vistos = new HashSet<string>();
        private readonly HashSet<(int, int)> _repreguntadas = new HashSet<(int, int)>();
        private int _pistasDeLion;

        public CasoDeInvestigacion Caso { get; }
        public EstadoEstudio Estudio { get; }
        public Fase Fase { get; private set; } = Fase.Investigacion;
        public int HorasRestantes { get; private set; }
        public int Credibilidad { get; private set; } = CredibilidadMaxima;
        public int IndiceTestimonio { get; private set; }
        public int Errores { get; private set; }
        public bool? Fundado { get; private set; }
        public int Honorarios { get; private set; }
        public int Experiencia { get; private set; }
        public bool UsoAtajos { get; private set; }

        public Testimonio? TestimonioActual => Fase == Fase.Audiencia ? Caso.Testimonios[IndiceTestimonio] : null;
        public IReadOnlyList<Prueba> Obtenidas => _ordenObtenidas.Select(Caso.Prueba).ToList();
        public IReadOnlyList<Prueba> Presentables => Obtenidas.Where(p => p.Presentable).ToList();
        public bool Tiene(string id) => _tengo.Contains(id);
        public bool Visto(string hallazgo) => _vistos.Contains(hallazgo);
        public bool Repreguntada(int linea) => _repreguntadas.Contains((IndiceTestimonio, linea));

        /// <summary>Estrellas de 1 a 3 al ganar: sin errores 3, con uno o dos 2, más 1.</summary>
        public int Estrellas => Fundado != true ? 0 : Errores == 0 ? 3 : Errores <= 2 ? 2 : 1;

        public PartidaDeInvestigacion(CasoDeInvestigacion caso, EstadoEstudio? estudio = null)
        {
            Caso = caso ?? throw new ArgumentNullException(nameof(caso));
            Estudio = estudio ?? new EstadoEstudio();
            HorasRestantes = caso.HorasDePlazo;
            foreach (var p in caso.PruebasIniciales) Obtener(p);
        }

        public IReadOnlyList<Lugar> LugaresAbiertos =>
            Caso.Lugares.Where(l => l.Requiere == null || _tengo.Contains(l.Requiere)).ToList();

        /// <summary>Acciones que se pueden hacer ahora en un lugar (las ya hechas no se repiten).</summary>
        public IReadOnlyList<Hallazgo> Acciones(string lugarId)
        {
            var l = Caso.Lugares.First(x => x.Id == lugarId);
            if (l.Requiere != null && !_tengo.Contains(l.Requiere)) return Array.Empty<Hallazgo>();
            return l.Hallazgos.Where(h => !_vistos.Contains(h.Id) && (h.Requiere == null || _tengo.Contains(h.Requiere))).ToList();
        }

        /// <summary>¿Queda algo por hacer en el lugar? (La capa visual lo marca para no hacer buscar a ciegas.)</summary>
        public bool QuedaAlgo(string lugarId) => Acciones(lugarId).Count > 0;

        public Resultado Investigar(string hallazgoId)
        {
            Exigir(Fase.Investigacion);
            var lugar = Caso.Lugares.FirstOrDefault(l => l.Hallazgos.Any(h => h.Id == hallazgoId)) ?? throw new InvalidOperationException("No existe esa acción.");
            var h = lugar.Hallazgos.First(x => x.Id == hallazgoId);
            if (!Acciones(lugar.Id).Contains(h)) throw new InvalidOperationException("Esa acción no está disponible.");
            if (h.Horas > HorasRestantes) throw new InvalidOperationException("No alcanza el tiempo para eso.");
            _vistos.Add(h.Id);
            HorasRestantes -= h.Horas;
            if (h.Atajo) { UsoAtajos = true; Estudio.ExpedienteDisciplinario += 10; }
            var r = new Resultado(h.Lineas);
            if (h.Da != null && !_tengo.Contains(h.Da)) { Obtener(h.Da); r.Nueva = Caso.Prueba(h.Da); }
            if (HorasRestantes == 0) { r.SeAcaboElPlazo = true; }
            return r;
        }

        /// <summary>Lion señala dónde hay algo importante que falta (gratis, tres veces por caso).</summary>
        public const int PistasDeLionPorCaso = 3;
        public int PistasDeLionRestantes => PistasDeLionPorCaso - _pistasDeLion;

        public Resultado PedirPistaALion()
        {
            Exigir(Fase.Investigacion);
            if (PistasDeLionRestantes <= 0)
                return new Resultado(new[] { new LineaDeGuion(Guion.Lion, "Ya les dije lo que sé. Ahora toca leer, socios.") });
            _pistasDeLion++;
            foreach (var l in LugaresAbiertos)
            {
                var clave = Acciones(l.Id).FirstOrDefault(h => h.Clave && h.Horas <= HorasRestantes);
                if (clave != null)
                    return new Resultado(new[] { new LineaDeGuion(Guion.Lion, $"Yo que ustedes iría a {l.Nombre}. Ahí hay algo que va a pesar en la audiencia: «{clave.Accion}».") });
            }
            return new Resultado(new[] { new LineaDeGuion(Guion.Lion, "Con lo que tenemos podemos ganar. Lo demás es contexto. Vamos a la audiencia cuando quieran.") });
        }

        /// <summary>
        /// Cierra la investigación: se arma el escrito eligiendo qué se pide al juzgado, y empieza la audiencia.
        /// Pedir algo equivocado no pierde el caso, pero la audiencia empieza con un punto menos de credibilidad.
        /// </summary>
        public Resultado PresentarEscrito(int pretension)
        {
            Exigir(Fase.Investigacion);
            if (pretension < 0 || pretension >= Caso.Pretensiones.Count) throw new ArgumentOutOfRangeException(nameof(pretension));
            var p = Caso.Pretensiones[pretension];
            Fase = Fase.Audiencia;
            IndiceTestimonio = 0;
            var lineas = new List<LineaDeGuion>(p.Comentario);
            if (!p.Correcta) { Credibilidad -= 1; Errores++; }
            lineas.AddRange(Caso.AntesDeLaAudiencia);
            lineas.AddRange(Caso.Testimonios[0].Antes);
            return new Resultado(lineas) { CredibilidadPerdida = p.Correcta ? 0 : 1 };
        }

        public Resultado Repreguntar(int linea)
        {
            var t = TestimonioActual ?? throw new InvalidOperationException("No hay audiencia en curso.");
            if (linea < 0 || linea >= t.Lineas.Count) throw new ArgumentOutOfRangeException(nameof(linea));
            _repreguntadas.Add((IndiceTestimonio, linea));
            return new Resultado(t.Lineas[linea].Repregunta);
        }

        /// <summary>Presenta una prueba contra una frase del testimonio.</summary>
        public Resultado Presentar(int linea, string pruebaId)
        {
            var t = TestimonioActual ?? throw new InvalidOperationException("No hay audiencia en curso.");
            if (linea < 0 || linea >= t.Lineas.Count) throw new ArgumentOutOfRangeException(nameof(linea));
            if (!_tengo.Contains(pruebaId)) throw new InvalidOperationException("No tienen esa prueba.");
            var prueba = Caso.Prueba(pruebaId);
            if (!prueba.Presentable) throw new InvalidOperationException("Las pistas no se presentan; las pruebas sí.");
            var frase = t.Lineas[linea];

            if (prueba.Viciada)
            {
                var r = Fallo(2, new[]
                {
                    new LineaDeGuion(Jueza, "¿Una constancia «agilizada»? Esto no tiene firma, ni registro, ni vergüenza."),
                    new LineaDeGuion(Jueza, "Doctor, voy a oficiar al Colegio. Y su credibilidad en esta sala acaba de caer."),
                });
                Estudio.ExpedienteDisciplinario += 10;
                return r;
            }
            if (frase.Contradice != pruebaId)
            {
                var objecion = new[]
                {
                    new LineaDeGuion(Jueza, $"Doctor, «{prueba.Nombre}» no tiene que ver con lo que acaba de decir el testigo."),
                    new LineaDeGuion(Jueza, "No me haga perder el tiempo. Se lo anoto."),
                };
                return Fallo(1, objecion);
            }

            var lineas = new List<LineaDeGuion>(frase.AlContradecir);
            IndiceTestimonio++;
            var termino = IndiceTestimonio >= Caso.Testimonios.Count;
            if (termino) Cerrar(true, lineas);
            else lineas.AddRange(Caso.Testimonios[IndiceTestimonio].Antes);
            // Entre un testimonio y otro, la jueza puede pedir el fundamento jurídico (una prueba jurídica del banco).
            return new Resultado(lineas) { Contradiccion = true, TestimonioRoto = true, PideFundamento = !termino };
        }

        /// <summary>Resultado de la prueba jurídica que pidió la jueza: acertar devuelve un punto de credibilidad.</summary>
        public void FundamentoRespondido(bool acierto)
        {
            if (Fase != Fase.Audiencia) return;
            if (acierto) Credibilidad = Math.Min(CredibilidadMaxima, Credibilidad + 1);
        }

        private Resultado Fallo(int puntos, LineaDeGuion[] lineas)
        {
            Errores++;
            Credibilidad = Math.Max(0, Credibilidad - puntos);
            var todas = new List<LineaDeGuion>(lineas);
            if (Credibilidad == 0) Cerrar(false, todas);
            return new Resultado(todas) { CredibilidadPerdida = puntos };
        }

        private void Cerrar(bool fundado, List<LineaDeGuion> lineas)
        {
            Fase = Fase.Sentencia;
            Fundado = fundado;
            lineas.AddRange(fundado ? Caso.Victoria : Caso.Derrota);
            Experiencia = fundado ? 200 + 50 * Credibilidad : 60;
            Honorarios = fundado ? 100 + 20 * Credibilidad + 5 * HorasRestantes : 20;
            if (UsoAtajos) Honorarios /= 2;
            Estudio.Honorarios += Honorarios;
            Estudio.Experiencia += Experiencia;
            if (fundado)
            {
                Estudio.ExpedientesCerrados++;
                Estudio.CambiarReputacion(UsoAtajos ? -5 : +3);
            }
            else Estudio.CambiarReputacion(-1);
        }

        private void Obtener(string id)
        {
            if (_tengo.Add(id)) _ordenObtenidas.Add(id);
        }

        private void Exigir(Fase fase)
        {
            if (Fase != fase) throw new InvalidOperationException(fase == Fase.Investigacion ? "La investigación ya terminó." : "No hay audiencia en curso.");
        }
    }
}
