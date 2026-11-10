using Expedientia.Logica.Personajes;

namespace Expedientia.Logica.Combate
{
    /// <summary>
    /// Fichas del prototipo. Los números son de partida y se ajustan con la simulación de partidas y las pruebas de los colegas.
    /// La clave de cada ficha es la que usa la capa visual para elegir su dibujo.
    /// </summary>
    public static class Catalogo
    {
        // ── Estudio ──
        public static FichaCombatiente Civilista(string id, string nombre, int nivel = 1) =>
            new FichaCombatiente(id, nombre, Bando.Estudio, Especialidad.Civilista,
                new Atributos(firmeza: 10, agilidadProcesal: 12, aguante: 14, doctrina: 16, criterio: 13, oratoria: 10),
                nivel, paciencia: 12 + (nivel - 1) * 6, defensa: 13, movimiento: 3, Atributo.Doctrina, 1, 8, alcance: 2);

        public static FichaCombatiente Administrativista(string id, string nombre, int nivel = 1) =>
            new FichaCombatiente(id, nombre, Bando.Estudio, Especialidad.Administrativista,
                new Atributos(firmeza: 10, agilidadProcesal: 13, aguante: 13, doctrina: 12, criterio: 16, oratoria: 12),
                nivel, paciencia: 11 + (nivel - 1) * 6, defensa: 12, movimiento: 3, Atributo.Criterio, 1, 6, alcance: 3);

        /// <summary>Aliado honesto que se suma a la sala de la red del Coimero si la Reputación es alta (GDD).</summary>
        public static FichaCombatiente SuboficialQuispe(string id) =>
            new FichaCombatiente(id, "Suboficial Quispe", Bando.Estudio, Especialidad.Ninguna,
                new Atributos(14, 12, 14, 10, 12, 10), 2, paciencia: 16, defensa: 14, movimiento: 3, Atributo.Firmeza, 1, 8,
                clave: "quispe");

        // ── Mesa de Partes ──
        public static FichaCombatiente Formalismo(string id) =>
            new FichaCombatiente(id, "Formalismo", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(10, 10, 10, 8, 8, 6), 1, paciencia: 8, defensa: 11, movimiento: 2, Atributo.Firmeza, 1, 4,
                rasgo: Rasgo.ExigeRequisito, clave: "formalismo");

        public static FichaCombatiente PlazoQueVence(string id) =>
            new FichaCombatiente(id, "Plazo que Vence", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(8, 12, 8, 8, 10, 6), 1, paciencia: 5, defensa: 12, movimiento: 3, Atributo.AgilidadProcesal, 1, 4,
                rasgo: Rasgo.Vence, clave: "plazo");

        public static FichaCombatiente NotificacionDefectuosa(string id) =>
            new FichaCombatiente(id, "Notificación Defectuosa", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(8, 10, 10, 8, 10, 8), 1, paciencia: 6, defensa: 11, movimiento: 3, Atributo.AgilidadProcesal, 1, 4, alcance: 2,
                rasgo: Rasgo.NotificaAlAzar, clave: "notificacion");

        public static FichaCombatiente ColaInfinita(string id, int paciencia = 30) =>
            new FichaCombatiente(id, "La Cola Infinita", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(14, 8, 16, 8, 10, 6), 2, paciencia: paciencia, defensa: 12, movimiento: 1, Atributo.Firmeza, 1, 8,
                clave: "cola");

        public static FichaCombatiente EsbirroDeLaCola(int numero) =>
            new FichaCombatiente($"cola-{numero}", "Usuario en cola", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(10, 8, 8, 6, 6, 6), 1, paciencia: 4, defensa: 9, movimiento: 2, Atributo.Firmeza, 1, 3,
                clave: "usuario");

        // ── Sala de Contrataciones ──
        public static FichaCombatiente VuelvaManiana(string id) =>
            new FichaCombatiente(id, "Vuelva Mañana", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(10, 10, 12, 8, 10, 8), 1, paciencia: 7, defensa: 11, movimiento: 2, Atributo.Firmeza, 1, 4, alcance: 2,
                rasgo: Rasgo.Empuja, clave: "vuelva");

        public static FichaCombatiente Fraccionador(string id) =>
            new FichaCombatiente(id, "Fraccionador", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(10, 12, 10, 8, 8, 6), 1, paciencia: 9, defensa: 10, movimiento: 3, Atributo.AgilidadProcesal, 1, 4,
                rasgo: Rasgo.SeFracciona, clave: "fraccionador");

        public static FichaCombatiente Fraccion(string id) =>
            new FichaCombatiente(id, "Fracción", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(8, 12, 8, 6, 6, 6), 1, paciencia: 3, defensa: 9, movimiento: 3, Atributo.AgilidadProcesal, 1, 3,
                clave: "fraccion");

        public static FichaCombatiente RequisitoImposible(string id, int paciencia = 32) =>
            new FichaCombatiente(id, "El Requisito Imposible", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(14, 10, 16, 12, 12, 8), 2, paciencia: paciencia, defensa: 12, movimiento: 2, Atributo.Firmeza, 1, 6, alcance: 2,
                clave: "requisito");

        // ── Red del Coimero ──
        public static FichaCombatiente Tramitador(string id) =>
            new FichaCombatiente(id, "Tramitador", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(12, 12, 12, 8, 10, 12), 1, paciencia: 9, defensa: 12, movimiento: 3, Atributo.AgilidadProcesal, 1, 6,
                clave: "tramitador");
    }
}
