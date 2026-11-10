using System.Collections.Generic;

namespace Expedientia.Logica.Expedientes
{
    /// <summary>Una línea de diálogo. Los personajes son ficticios (GDD: humor de oficio, sin personas reales).</summary>
    public sealed class LineaDeGuion
    {
        public string Personaje { get; }
        public string Texto { get; }
        public LineaDeGuion(string personaje, string texto) { Personaje = personaje; Texto = texto; }
    }

    /// <summary>Lo que dicen los personajes en un caso: llegada, jefe y cierre, y cómo se describe cada sala.</summary>
    public sealed class GuionDeCaso
    {
        public IReadOnlyList<LineaDeGuion> Llegada { get; }
        public IReadOnlyList<LineaDeGuion> Jefe { get; }
        public IReadOnlyList<LineaDeGuion> Victoria { get; }
        public IReadOnlyList<LineaDeGuion> Derrota { get; }
        /// <summary>Descripciones propias del caso; si una sala no tiene, se usa la general del juego.</summary>
        public IReadOnlyDictionary<TipoSala, string[]> Salas { get; }

        public GuionDeCaso(LineaDeGuion[] llegada, LineaDeGuion[] jefe, LineaDeGuion[] victoria, LineaDeGuion[] derrota, Dictionary<TipoSala, string[]>? salas = null)
        {
            Llegada = llegada; Jefe = jefe; Victoria = victoria; Derrota = derrota;
            Salas = salas ?? new Dictionary<TipoSala, string[]>();
        }
    }

    /// <summary>
    /// Trama del Acto 1 ("El primer caso") del GDD: el estudio recién fundado descubre que los expedientes olvidados
    /// cobran vida y que detrás está la Carga Procesal. Son textos de juego; no explican normas ni son asesoría legal.
    /// </summary>
    public static class Guion
    {
        public const string Narrador = "Narrador";
        public const string Eusebio = "Don Eusebio";
        public const string Cliente = "El Cliente";
        public const string Rosita = "Rosita";
        public const string Valdivia = "Doctora Valdivia";
        public const string Mendoza = "Especialista Mendoza";
        public const string Quispe = "Suboficial Quispe";
        public const string Coimero = "El Coimero";

        private static LineaDeGuion L(string p, string t) => new LineaDeGuion(p, t);

        /// <summary>Prólogo que se ve la primera vez que se abre el juego.</summary>
        public static readonly IReadOnlyList<LineaDeGuion> Prologo = new[]
        {
            L(Narrador, "República de Expedientia. Un país inventado, con instituciones sospechosamente familiares."),
            L(Narrador, "Una noche, en el sótano del Archivo Central, los expedientes olvidados empezaron a moverse."),
            L(Eusebio, "Treinta años en este juzgado, doctor, y nunca había visto caminar a un expediente. Hasta el martes."),
            L(Eusebio, "Dicen que es la Carga Procesal. Crece con cada trámite que nadie atiende."),
            L(Valdivia, "Usted abrió su estudio en un cuarto alquilado frente a la Corte. Buen lugar para empezar; pésimo para dormir."),
            L(Valdivia, "Cierre casos, gane reputación y no acepte atajos. Los atajos siempre cobran después."),
            L(Eusebio, "Ahí viene su primer cliente. Trae cara de ventanilla cerrada."),
        };

        /// <summary>Descripciones generales de cada tipo de sala (se elige una según la semilla de la partida).</summary>
        public static readonly IReadOnlyDictionary<TipoSala, string[]> SalasGenerales = new Dictionary<TipoSala, string[]>
        {
            [TipoSala.Combate] = new[]
            {
                "Tres formularios con colmillos bloquean el pasillo de ventanillas.",
                "Un enjambre de notificaciones mal dirigidas revolotea entre las ventanillas.",
                "Los plazos se escaparon del calendario y quieren vencer encima de ustedes.",
            },
            [TipoSala.Tramite] = new[]
            {
                "Una puerta pide «el requisito correcto». Nadie sabe cuál es, pero todos saben que no es el que traen.",
                "Ventanilla 3: «Falta copia legalizada del original de la copia». Toca presentar el escrito.",
            },
            [TipoSala.Audiencia] = new[]
            {
                "El Cliente cambia su versión de los hechos. ¿Lo confrontan o revisan el expediente?",
                "El Cliente jura que la fecha era otra. Su cuñado, que vino de testigo, dice una tercera.",
            },
            [TipoSala.Cafeteria] = new[]
            {
                "La cafetería del juzgado. El café es fuerte y el pan con chicharrón, más.",
                "Un café de pie. Don Eusebio asegura que aquí se resuelven más casos que en las salas.",
            },
            [TipoSala.Libreria] = new[]
            {
                "La librería jurídica. El Ekeko de los Honorarios cobra en honorarios del juego, nunca «a cuenta».",
            },
            [TipoSala.Coimero] = new[]
            {
                "Un señor muy amable ofrece «agilizar» el trámite por una propina. ¿Qué hace el estudio?",
                "Un «abogado de pasillo» susurra que conoce a alguien que conoce a alguien. Por un aporte, el expediente aparece.",
            },
            [TipoSala.RedDelCoimero] = new[]
            {
                "La red del Coimero se esconde en la trastienda. Esta vez no hay atajos.",
            },
        };

        public static readonly IReadOnlyDictionary<string, GuionDeCaso> Casos = new Dictionary<string, GuionDeCaso>
        {
            ["escrito-fantasma"] = new GuionDeCaso(
                new[]
                {
                    L(Cliente, "Doctor, yo presenté el escrito. Tenía el cargo… bueno, tenía el cargo hasta el lunes."),
                    L(Eusebio, "En Mesa de Partes los escritos no se pierden, doctor. Se independizan."),
                    L(Rosita, "Si lo presentó, está en la cola. Todo termina en la cola."),
                },
                new[]
                {
                    L(Rosita, "Al fondo de la cola está su escrito… y algo bastante más grande."),
                    L(Eusebio, "Doctores, esa cola no avanza desde 1998. Suerte."),
                },
                new[]
                {
                    L(Cliente, "¡Apareció! Con sello y todo."),
                    L(Eusebio, "Estaba al final de la cola, doctor. Como todo en la vida."),
                    L(Valdivia, "Buen primer caso. Pero ese escrito no se perdió solo: algo en el Archivo Central se está comiendo los expedientes."),
                },
                new[]
                {
                    L(Eusebio, "Archivado, doctor. No se preocupe: aquí lo archivado también resucita."),
                }),

            ["plazo-en-llamas"] = new GuionDeCaso(
                new[]
                {
                    L(Cliente, "Doctores, me notificaron la sentencia hace casi diez días. ¿Todavía se puede hacer algo?"),
                    L(Eusebio, "La ventanilla cierra en dos horas. El reloj ya corre y no espera a nadie."),
                },
                new[]
                {
                    L(Eusebio, "La última cola del día. Si llegan a la ventanilla, llegan."),
                },
                new[]
                {
                    L(Eusebio, "Presentado a tiempo, con fecha y hora. Enmárquenlo."),
                    L(Valdivia, "Cada semana hay más plazos imposibles. La Carga Procesal despertó y alguien la alimenta. Dicen que es el Dragón del Formalismo."),
                },
                new[]
                {
                    L(Eusebio, "Para la próxima, doctor, el cargo de recepción se pide el mismo día."),
                },
                new Dictionary<TipoSala, string[]>
                {
                    [TipoSala.Combate] = new[]
                    {
                        "Los plazos se escaparon del calendario y corren hacia la ventanilla antes que ustedes.",
                        "Tres Formalismos bloquean el pasillo. El reloj de la pared avanza.",
                    },
                }),

            ["buena-pro-imposible"] = new GuionDeCaso(
                new[]
                {
                    L(Cliente, "Somos una empresa chica. Las bases piden diez años de experiencia en una tecnología que salió el año pasado."),
                    L(Eusebio, "En la Sala de Contrataciones los requisitos cambian más rápido que los funcionarios."),
                },
                new[]
                {
                    L(Eusebio, "Ese es El Requisito Imposible. Cada dos rondas pide algo distinto. Lean bien antes de golpear."),
                },
                new[]
                {
                    L(Cliente, "¡Nos dejaron competir en igualdad de condiciones!"),
                    L(Valdivia, "Detrás de esas bases hay alguien que escribe requisitos a la medida. Esto recién empieza."),
                },
                new[]
                {
                    L(Eusebio, "Proceso desierto. Igual que mi paciencia."),
                },
                new Dictionary<TipoSala, string[]>
                {
                    [TipoSala.Combate] = new[]
                    {
                        "Bases integradas, sobres sellados y un «Vuelva mañana» que no se mueve de la puerta.",
                        "Una compra grande se partió en pedacitos para pasar desapercibida. Los pedacitos muerden.",
                    },
                    [TipoSala.Tramite] = new[]
                    {
                        "Mesa de partes de la entidad: hay que presentar la consulta a las bases antes de que cierre la etapa.",
                    },
                }),
        };

        /// <summary>Líneas de aliados honestos (GDD: cada región tiene aliados que hacen bien su trabajo).</summary>
        public static readonly LineaDeGuion MendozaRevela = L(Mendoza, "Yo les aviso qué está exigiendo en cada momento. Las bases se pueden observar, doctores.");
        public static readonly LineaDeGuion QuispeLlega = L(Quispe, "Vengo con ustedes, doctores. La corrupción existe, pero no es la institución.");

        public static GuionDeCaso De(string casoId) => Casos.TryGetValue(casoId, out var g) ? g : Casos["escrito-fantasma"];

        /// <summary>Elige la descripción de una sala sin gastar el azar de la partida (así no cambia el combate).</summary>
        public static string DescribirSala(string casoId, TipoSala tipo, int semilla, int indice)
        {
            var g = De(casoId);
            if (!g.Salas.TryGetValue(tipo, out var opciones) && !SalasGenerales.TryGetValue(tipo, out opciones)) return "";
            var i = (int)((uint)(semilla * 31 + indice * 17) % (uint)opciones.Length);
            return opciones[i];
        }
    }
}
