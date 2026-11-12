using System.Collections.Generic;
using System.Linq;

namespace Expedientia.Logica.Expedientes
{
    public sealed class Caso
    {
        public string Id { get; }
        public int Numero { get; }
        public string Titulo { get; }
        public string Premisa { get; }
        public string Calabozo { get; }
        public string Jefe { get; }
        /// <summary>Minutos disponibles antes de que el caso sea "Improcedente por extemporáneo"; null = sin reloj.</summary>
        public int? MinutosDePlazo { get; }
        public bool EnPrototipo { get; }

        public Caso(string id, int numero, string titulo, string premisa, string calabozo, string jefe, int? minutos, bool enPrototipo)
        {
            Id = id; Numero = numero; Titulo = titulo; Premisa = premisa; Calabozo = calabozo; Jefe = jefe;
            MinutosDePlazo = minutos; EnPrototipo = enPrototipo;
        }
    }

    /// <summary>Casos del GDD. Los textos son de juego, no asesoría legal.</summary>
    public static class CatalogoCasos
    {
        public const string MesaDePartes = "Mesa de Partes";
        public const string SalaDeContrataciones = "Sala de Contrataciones";
        public const string JefeColaInfinita = "La Cola Infinita";
        public const string JefeRequisitoImposible = "El Requisito Imposible";

        public static readonly IReadOnlyList<Caso> Todos = new[]
        {
            new Caso("escrito-fantasma", 1, "El escrito fantasma",
                "El cliente jura que presentó su escrito, pero no aparece en el sistema.", MesaDePartes, JefeColaInfinita, null, true),
            new Caso("plazo-en-llamas", 2, "Plazo en llamas",
                "Quedan 2 horas para presentar una apelación y la ventanilla está por cerrar.", MesaDePartes, JefeColaInfinita, 120, true),
            new Caso("buena-pro-imposible", 3, "La buena pro imposible",
                "Una empresa pequeña quiere ganar un proceso de selección con requisitos que nadie cumple.", SalaDeContrataciones, JefeRequisitoImposible, null, true),
        };

        public static Caso Buscar(string id) => Todos.First(c => c.Id == id);
    }
}
