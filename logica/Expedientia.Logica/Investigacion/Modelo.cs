using System;
using System.Collections.Generic;
using System.Linq;
using Expedientia.Logica.Expedientes;

namespace Expedientia.Logica.Investigacion
{
    /// <summary>
    /// Algo que el estudio consigue al investigar. Las pruebas se presentan en la audiencia;
    /// las pistas no se presentan, solo abren lugares o conversaciones.
    /// </summary>
    public sealed class Prueba
    {
        public string Id { get; }
        public string Nombre { get; }
        public string Descripcion { get; }
        public bool Presentable { get; }
        /// <summary>Prueba conseguida con un atajo (el Coimero): presentarla es grave.</summary>
        public bool Viciada { get; }
        public Prueba(string id, string nombre, string descripcion, bool presentable = true, bool viciada = false)
        { Id = id; Nombre = nombre; Descripcion = descripcion; Presentable = presentable; Viciada = viciada; }
    }

    /// <summary>Una acción dentro de un lugar: examinar algo o hablar con alguien. Cuesta horas del plazo.</summary>
    public sealed class Hallazgo
    {
        public string Id { get; }
        /// <summary>Texto del botón («Hablar con Rosita»).</summary>
        public string Accion { get; }
        public IReadOnlyList<LineaDeGuion> Lineas { get; }
        /// <summary>Prueba o pista que se obtiene; null si es solo conversación.</summary>
        public string? Da { get; }
        /// <summary>Prueba o pista que hay que tener para que aparezca esta acción; null si siempre aparece.</summary>
        public string? Requiere { get; }
        public int Horas { get; }
        /// <summary>Necesaria para ganar: Lion la señala cuando se le pide una pista.</summary>
        public bool Clave { get; }
        /// <summary>Aceptar un atajo indebido (suma al expediente disciplinario oculto del estudio).</summary>
        public bool Atajo { get; }

        public Hallazgo(string id, string accion, LineaDeGuion[] lineas, string? da = null, string? requiere = null, int horas = 1, bool clave = false, bool atajo = false)
        {
            if (horas < 0) throw new ArgumentOutOfRangeException(nameof(horas));
            Id = id; Accion = accion; Lineas = lineas; Da = da; Requiere = requiere; Horas = horas; Clave = clave; Atajo = atajo;
        }
    }

    public sealed class Lugar
    {
        public string Id { get; }
        public string Nombre { get; }
        public string Descripcion { get; }
        /// <summary>Clave del dibujo de fondo.</summary>
        public string Escena { get; }
        /// <summary>Pista o prueba que abre este lugar; null si está abierto desde el inicio.</summary>
        public string? Requiere { get; }
        public IReadOnlyList<Hallazgo> Hallazgos { get; }
        public Lugar(string id, string nombre, string descripcion, string escena, Hallazgo[] hallazgos, string? requiere = null)
        { Id = id; Nombre = nombre; Descripcion = descripcion; Escena = escena; Hallazgos = hallazgos; Requiere = requiere; }
    }

    /// <summary>Una frase del testimonio. Si <see cref="Contradice"/> no es null, esa prueba la desmiente.</summary>
    public sealed class LineaDeTestimonio
    {
        public string Texto { get; }
        /// <summary>Lo que pasa al repreguntar (suele dar una pista de por dónde va la mentira).</summary>
        public IReadOnlyList<LineaDeGuion> Repregunta { get; }
        public string? Contradice { get; }
        /// <summary>Lo que se dice al presentar la prueba correcta.</summary>
        public IReadOnlyList<LineaDeGuion> AlContradecir { get; }
        public LineaDeTestimonio(string texto, LineaDeGuion[] repregunta, string? contradice = null, LineaDeGuion[]? alContradecir = null)
        { Texto = texto; Repregunta = repregunta; Contradice = contradice; AlContradecir = alContradecir ?? Array.Empty<LineaDeGuion>(); }
    }

    /// <summary>Una declaración de un testigo; se rompe con la prueba correcta en la frase correcta.</summary>
    public sealed class Testimonio
    {
        public string Testigo { get; }
        public string Titulo { get; }
        public IReadOnlyList<LineaDeGuion> Antes { get; }
        public IReadOnlyList<LineaDeTestimonio> Lineas { get; }
        public Testimonio(string testigo, string titulo, LineaDeGuion[] antes, LineaDeTestimonio[] lineas)
        {
            if (lineas.Count(l => l.Contradice != null) != 1) throw new ArgumentException("Cada testimonio tiene exactamente una contradicción.");
            Testigo = testigo; Titulo = titulo; Antes = antes; Lineas = lineas;
        }
    }

    /// <summary>Lo que el escrito le pide al juzgado. Solo una es la correcta para el caso.</summary>
    public sealed class Pretension
    {
        public string Texto { get; }
        public bool Correcta { get; }
        /// <summary>Lo que dicen al elegirla (Gonzabio celebra o advierte).</summary>
        public IReadOnlyList<LineaDeGuion> Comentario { get; }
        public Pretension(string texto, bool correcta, LineaDeGuion[] comentario) { Texto = texto; Correcta = correcta; Comentario = comentario; }
    }

    /// <summary>Un caso del modo investigación: lugares por recorrer con un plazo en horas y una audiencia para litigarlo.</summary>
    public sealed class CasoDeInvestigacion
    {
        public string Id { get; }
        public int Numero { get; }
        public string Titulo { get; }
        public string Premisa { get; }
        public int HorasDePlazo { get; }
        public IReadOnlyList<LineaDeGuion> Introduccion { get; }
        public IReadOnlyList<Prueba> Pruebas { get; }
        public IReadOnlyList<string> PruebasIniciales { get; }
        public IReadOnlyList<Lugar> Lugares { get; }
        public IReadOnlyList<Pretension> Pretensiones { get; }
        public IReadOnlyList<LineaDeGuion> AntesDeLaAudiencia { get; }
        public IReadOnlyList<Testimonio> Testimonios { get; }
        public IReadOnlyList<LineaDeGuion> Victoria { get; }
        public IReadOnlyList<LineaDeGuion> Derrota { get; }

        public CasoDeInvestigacion(string id, int numero, string titulo, string premisa, int horas, LineaDeGuion[] introduccion,
            Prueba[] pruebas, string[] pruebasIniciales, Lugar[] lugares, Pretension[] pretensiones, LineaDeGuion[] antesDeLaAudiencia, Testimonio[] testimonios,
            LineaDeGuion[] victoria, LineaDeGuion[] derrota)
        {
            Id = id; Numero = numero; Titulo = titulo; Premisa = premisa; HorasDePlazo = horas; Introduccion = introduccion;
            Pruebas = pruebas; PruebasIniciales = pruebasIniciales; Lugares = lugares; Pretensiones = pretensiones; AntesDeLaAudiencia = antesDeLaAudiencia;
            Testimonios = testimonios; Victoria = victoria; Derrota = derrota;
            Validar();
        }

        public Prueba Prueba(string id) => Pruebas.First(p => p.Id == id);

        /// <summary>El caso tiene que poder ganarse: todo lo que se pide existe y las pruebas clave caben en el plazo.</summary>
        private void Validar()
        {
            if (Pretensiones.Count(p => p.Correcta) != 1) throw new ArgumentException("El escrito tiene exactamente una pretensión correcta.");
            var ids = new HashSet<string>(Pruebas.Select(p => p.Id));
            if (ids.Count != Pruebas.Count) throw new ArgumentException("Hay pruebas repetidas.");
            var hallazgos = Lugares.SelectMany(l => l.Hallazgos).ToList();
            if (hallazgos.Select(h => h.Id).Distinct().Count() != hallazgos.Count) throw new ArgumentException("Hay hallazgos repetidos.");
            foreach (var r in PruebasIniciales.Concat(Lugares.Select(l => l.Requiere)).Concat(hallazgos.SelectMany(h => new[] { h.Da, h.Requiere }))
                         .Concat(Testimonios.SelectMany(t => t.Lineas.Select(l => l.Contradice))))
                if (r != null && !ids.Contains(r)) throw new ArgumentException($"Falta la prueba «{r}».");
            var horasClave = hallazgos.Where(h => h.Clave).Sum(h => h.Horas);
            if (horasClave > HorasDePlazo) throw new ArgumentException("Las pruebas clave no caben en el plazo.");
        }
    }
}
