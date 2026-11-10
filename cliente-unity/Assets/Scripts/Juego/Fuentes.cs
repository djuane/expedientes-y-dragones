#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Expedientia.Logica.Combate;
using Expedientia.Logica.Expedientes;
using Expedientia.Logica.Juridico;
using Expedientia.Logica.Sesion;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;

namespace Expedientia.Cliente
{
    /// <summary>De dónde sale la partida: el propio celular (sin conexión) o el servidor (en línea).</summary>
    public interface IFuentePartida
    {
        string JugadorId { get; }
        Task<RespuestaAccion> CrearAsync(string casoId, string especialidad, int bots);
        Task<RespuestaAccion> EjecutarAsync(AccionJuego accion);
        /// <summary>Estado actual (en línea trae lo que hicieron los demás jugadores y el reloj de 30 s).</summary>
        Task<EstadoPartida?> SondearAsync();
    }

    public static class Json
    {
        public static readonly JsonSerializerSettings Opciones = new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver { NamingStrategy = new SnakeCaseNamingStrategy() },
            Converters = { new StringEnumConverter(new CamelCaseNamingStrategy()) },
            NullValueHandling = NullValueHandling.Include,
        };
        public static readonly JsonSerializer Serializador = JsonSerializer.Create(Opciones);

        /// <summary>Los eventos llegan como objetos (sin conexión) o como JSON (en línea); se unifican en JObject.</summary>
        public static JObject Datos(EventoJuego e) =>
            e.Datos is JObject j ? j : e.Datos == null ? new JObject() : JObject.FromObject(e.Datos, Serializador);
    }

    /// <summary>Partida sin conexión: el mismo motor que usa el servidor, corriendo en el celular.</summary>
    public sealed class FuenteLocal : IFuentePartida
    {
        private const string ClaveEstudio = "eyd.estudio";
        private MotorDePartida? _motor;
        private readonly List<Pregunta> _preguntas;
        private readonly System.Random _azar = new System.Random();
        private bool _guardado;

        public string JugadorId => "local";
        public string Alias { get; }

        public FuenteLocal(string alias, bool incluirBorradores)
        {
            Alias = alias;
            _preguntas = CargarPreguntas(incluirBorradores);
        }

        public static List<Pregunta> CargarPreguntas(bool incluirBorradores)
        {
            var lista = new List<Pregunta>();
            foreach (var t in Resources.LoadAll<TextAsset>("preguntas"))
            {
                try
                {
                    var j = JObject.Parse(t.text);
                    var estado = (string?)j["estado"] ?? "borrador";
                    var opciones = j["opciones"]?.ToObject<List<string>>() ?? new List<string>();
                    if (opciones.Count != 4 || estado == "desactualizada") continue;
                    if (estado == "verificada" && DateTime.TryParse((string?)j["fecha_verificacion"], out var fecha))
                        lista.Add(new Pregunta((string)j["id"]!, (string?)j["materia"] ?? "", (string?)j["calabozo"] ?? "", (string?)j["pregunta"] ?? "",
                            opciones, (int)j["indice_correcto"]!, (string?)j["base_legal"] ?? "", (string?)j["enlace_oficial"] ?? "",
                            EstadoPregunta.Verificada, (string?)j["verificado_por"], fecha));
                    else if (incluirBorradores && estado == "borrador")
                        lista.Add(new Pregunta((string)j["id"]!, (string?)j["materia"] ?? "", (string?)j["calabozo"] ?? "", (string?)j["pregunta"] ?? "",
                            opciones, (int)j["indice_correcto"]!, (string?)j["base_legal"] ?? "", "pendiente",
                            EstadoPregunta.Verificada, MotorDePartida.MarcaBorrador, DateTime.UnixEpoch));
                }
                catch (Exception ex) { Debug.LogWarning($"Pregunta {t.name} ignorada: {ex.Message}"); }
            }
            return lista;
        }

        public Task<RespuestaAccion> CrearAsync(string casoId, string especialidad, int bots)
        {
            var estudio = LeerEstudio();
            var esp = especialidad == "Administrativista" ? Especialidad.Administrativista : Especialidad.Civilista;
            var miembros = new List<MiembroDelEquipo> { new MiembroDelEquipo(Ficha(esp, "p1", Alias, estudio.Nivel), JugadorId) };
            for (var i = 0; i < bots; i++)
            {
                var otra = (i % 2 == 0) == (esp == Especialidad.Civilista) ? Especialidad.Administrativista : Especialidad.Civilista;
                miembros.Add(new MiembroDelEquipo(Ficha(otra, $"p{i + 2}", otra == Especialidad.Civilista ? "Bot Civilista" : "Bot Administrativista", estudio.Nivel), null));
            }
            _motor = new MotorDePartida(Guid.NewGuid(), new Expediente(CatalogoCasos.Buscar(casoId), _azar.Next(), miembros, estudio), DateTime.UtcNow);
            _guardado = false;
            return Task.FromResult(Guardar(_motor.Iniciar(DateTime.UtcNow)));
        }

        public Task<RespuestaAccion> EjecutarAsync(AccionJuego accion)
        {
            if (_motor == null) throw new InvalidOperationException("No hay partida.");
            var r = _motor.Ejecutar(JugadorId, accion, DateTime.UtcNow, calabozo =>
            {
                var c = _preguntas.Where(p => p.Calabozo == calabozo).ToList();
                return c.Count == 0 ? null : c[_azar.Next(c.Count)];
            });
            return Task.FromResult(Guardar(r));
        }

        public Task<EstadoPartida?> SondearAsync()
        {
            if (_motor == null) return Task.FromResult<EstadoPartida?>(null);
            var r = _motor.RevisarTiempo(DateTime.UtcNow);
            return Task.FromResult<EstadoPartida?>(r != null ? Guardar(r).Estado : null);
        }

        private RespuestaAccion Guardar(RespuestaAccion r)
        {
            if (_motor != null && _motor.Expediente.Terminado && !_guardado)
            {
                _guardado = true;
                var e = _motor.Expediente.Estudio;
                PlayerPrefs.SetString(ClaveEstudio, JsonConvert.SerializeObject(new[] { e.Honorarios, e.Experiencia, e.Reputacion, e.ExpedienteDisciplinario, e.ExpedientesCerrados }));
                PlayerPrefs.Save();
            }
            return r;
        }

        public static EstadoEstudio LeerEstudio()
        {
            try
            {
                var v = JsonConvert.DeserializeObject<int[]>(PlayerPrefs.GetString(ClaveEstudio, ""));
                if (v != null && v.Length == 5) return EstadoEstudio.Restaurar(v[0], v[1], v[2], v[3], v[4]);
            }
            catch (JsonException) { }
            return new EstadoEstudio();
        }

        private static FichaCombatiente Ficha(Especialidad e, string id, string nombre, int nivel) =>
            e == Especialidad.Civilista ? Catalogo.Civilista(id, nombre, nivel) : Catalogo.Administrativista(id, nombre, nivel);
    }

    /// <summary>Partida en línea contra el servidor de partidas (el servidor decide cada turno).</summary>
    public sealed class FuenteRemota : IFuentePartida
    {
        private readonly string _url;
        private string _token = "";
        private Guid _partida;
        public string JugadorId { get; private set; } = "";

        public FuenteRemota(string url) { _url = url.TrimEnd('/'); }

        public async Task ConectarAsync(string alias)
        {
            var guardado = PlayerPrefs.GetString("eyd.token." + _url, "");
            if (guardado.Length > 0)
            {
                _token = guardado;
                var yo = await Pedir("GET", "/api/yo", null, permitirError: true);
                if (yo != null) { JugadorId = (string)yo["jugador_id"]!; return; }
            }
            var j = await Pedir("POST", "/api/jugadores", new JObject { ["alias"] = alias });
            _token = (string)j!["token"]!;
            JugadorId = (string)j["jugador_id"]!;
            PlayerPrefs.SetString("eyd.token." + _url, _token);
            PlayerPrefs.Save();
        }

        public async Task<RespuestaAccion> CrearAsync(string casoId, string especialidad, int bots)
        {
            var j = await Pedir("POST", "/api/partidas", new JObject { ["caso_id"] = casoId, ["especialidad"] = especialidad, ["bots"] = bots });
            var r = j!.ToObject<RespuestaAccion>(Json.Serializador)!;
            _partida = r.Estado.Id;
            return r;
        }

        public async Task<RespuestaAccion> EjecutarAsync(AccionJuego accion)
        {
            var j = await Pedir("POST", $"/api/partidas/{_partida}/acciones", JObject.FromObject(accion, Json.Serializador), permitirError: true, devolverCuerpoDeError: true);
            return j!.ToObject<RespuestaAccion>(Json.Serializador)!;
        }

        public async Task<EstadoPartida?> SondearAsync()
        {
            if (_partida == Guid.Empty) return null;
            var j = await Pedir("GET", $"/api/partidas/{_partida}", null, permitirError: true);
            return j?.ToObject<EstadoPartida>(Json.Serializador);
        }

        private async Task<JObject?> Pedir(string metodo, string ruta, JObject? cuerpo, bool permitirError = false, bool devolverCuerpoDeError = false)
        {
            using var req = new UnityWebRequest(_url + ruta, metodo);
            req.downloadHandler = new DownloadHandlerBuffer();
            if (cuerpo != null)
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(cuerpo.ToString(Formatting.None)));
                req.SetRequestHeader("Content-Type", "application/json");
            }
            if (_token.Length > 0) req.SetRequestHeader("X-Token", _token);
            req.timeout = 15;
            var tcs = new TaskCompletionSource<bool>();
            req.SendWebRequest().completed += _ => tcs.TrySetResult(true);
            await tcs.Task;
            var texto = req.downloadHandler.text;
            if (req.result == UnityWebRequest.Result.Success) return string.IsNullOrEmpty(texto) ? new JObject() : JObject.Parse(texto);
            if (devolverCuerpoDeError && req.responseCode == 400 && !string.IsNullOrEmpty(texto)) return JObject.Parse(texto);
            if (permitirError) return null;
            throw new Exception($"El servidor respondió {req.responseCode}: {req.error}");
        }
    }
}
