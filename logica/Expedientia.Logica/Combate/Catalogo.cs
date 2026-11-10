using Expedientia.Logica.Personajes;

namespace Expedientia.Logica.Combate
{
    /// <summary>
    /// Fichas del prototipo. Los números son de partida y se ajustarán con las pruebas de los colegas.
    /// </summary>
    public static class Catalogo
    {
        public static FichaCombatiente Civilista(string id, string nombre, int nivel = 1) =>
            new FichaCombatiente(id, nombre, Bando.Estudio, Especialidad.Civilista,
                new Atributos(firmeza: 10, agilidadProcesal: 12, aguante: 14, doctrina: 16, criterio: 13, oratoria: 10),
                nivel, paciencia: 12 + (nivel - 1) * 6, defensa: 13, movimiento: 3, Atributo.Doctrina, 1, 8, alcance: 2);

        public static FichaCombatiente Administrativista(string id, string nombre, int nivel = 1) =>
            new FichaCombatiente(id, nombre, Bando.Estudio, Especialidad.Administrativista,
                new Atributos(firmeza: 10, agilidadProcesal: 13, aguante: 13, doctrina: 12, criterio: 16, oratoria: 12),
                nivel, paciencia: 11 + (nivel - 1) * 6, defensa: 12, movimiento: 3, Atributo.Criterio, 1, 6, alcance: 3);

        public static FichaCombatiente Formalismo(string id) =>
            new FichaCombatiente(id, "Formalismo", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(10, 10, 10, 8, 8, 6), 1, paciencia: 7, defensa: 11, movimiento: 2, Atributo.Firmeza, 1, 4);

        public static FichaCombatiente PlazoQueVence(string id) =>
            new FichaCombatiente(id, "Plazo que Vence", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(8, 12, 8, 8, 10, 6), 1, paciencia: 5, defensa: 12, movimiento: 3, Atributo.AgilidadProcesal, 1, 4);

        public static FichaCombatiente NotificacionDefectuosa(string id) =>
            new FichaCombatiente(id, "Notificación Defectuosa", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(8, 10, 10, 8, 10, 8), 1, paciencia: 6, defensa: 11, movimiento: 3, Atributo.AgilidadProcesal, 1, 4, alcance: 2);

        public static FichaCombatiente ColaInfinita(string id) =>
            new FichaCombatiente(id, "La Cola Infinita", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(14, 8, 16, 8, 10, 6), 2, paciencia: 30, defensa: 12, movimiento: 1, Atributo.Firmeza, 1, 8);

        public static FichaCombatiente EsbirroDeLaCola(int numero) =>
            new FichaCombatiente($"cola-{numero}", "Usuario en cola", Bando.Sistema, Especialidad.Ninguna,
                new Atributos(10, 8, 8, 6, 6, 6), 1, paciencia: 4, defensa: 9, movimiento: 2, Atributo.Firmeza, 1, 3);
    }
}
