#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Expedientia.Logica.Expedientes;
using Expedientia.Logica.Sesion;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Expedientia.Cliente
{
    /// <summary>
    /// Arma toda la interfaz por código (vertical, para un pulgar) y conversa con la fuente de la partida.
    /// La capa visual solo interpreta eventos; las reglas están en Expedientia.Logica.
    /// </summary>
    public sealed class Controlador : MonoBehaviour
    {
        private const int Columnas = 6, Filas = 8;

        private RectTransform _raiz = null!;
        private IFuentePartida? _fuente;
        private EstadoPartida? _estado;
        private string? _objetivo;
        private bool _ocupado;
        private float _proximoSondeo;
        private readonly List<string> _bitacora = new List<string>();

        // Partida
        private Text _cabecera = null!, _subcabecera = null!, _textoBitacora = null!, _textoSala = null!;
        private RectTransform _tablero = null!, _barra = null!, _capa = null!;
        private readonly Button[,] _casillas = new Button[Columnas, Filas];

        // Preferencias del menú
        private string _alias = "Doctor(a)";
        private string _especialidad = "Civilista";
        private string _caso = "escrito-fantasma";
        private bool _enLinea;
        private string _url = "http://192.168.1.252:8080";
        private bool _borradores;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Arrancar()
        {
            if (FindFirstObjectByType<Controlador>() != null) return;
            new GameObject("Expedientes y Dragones").AddComponent<Controlador>();
        }

        private void Awake()
        {
            Application.targetFrameRate = 60;
            Screen.orientation = ScreenOrientation.Portrait;
            if (Camera.main == null)
            {
                var cam = new GameObject("Camara", typeof(Camera)).GetComponent<Camera>();
                cam.tag = "MainCamera"; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Ui.Noche;
            }
            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            var lienzo = new GameObject("Lienzo", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            lienzo.transform.SetParent(transform, false);
            lienzo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var escala = lienzo.GetComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1080, 1920);
            escala.matchWidthOrHeight = 0.5f;
            _raiz = Ui.Panel(lienzo.transform, "Raiz", Ui.Noche).Anclar(0, 0, 1, 1);

            _alias = PlayerPrefs.GetString("eyd.alias", _alias);
            _url = PlayerPrefs.GetString("eyd.url", _url);
            _enLinea = PlayerPrefs.GetInt("eyd.enlinea", 0) == 1;
            MostrarMenu();
        }

        // ───────────────────────────── Menú ─────────────────────────────

        private void MostrarMenu(string? aviso = null)
        {
            Ui.Limpiar(_raiz);
            _estado = null; _fuente = null; _bitacora.Clear();
            var t = Ui.Texto(Ui.Vacio(_raiz, "Titulo").Anclar(0.05f, 0.86f, 0.95f, 0.97f), "EXPEDIENTES\nY DRAGONES", 96, Ui.Dorado, TextAnchor.MiddleCenter, FontStyle.Bold);
            Ui.Texto(Ui.Vacio(_raiz, "Frase").Anclar(0.05f, 0.81f, 0.95f, 0.86f), "Litiga, investiga y vence al dragón de la burocracia.", 36, Ui.Pergamino, TextAnchor.MiddleCenter, FontStyle.Italic);

            var est = FuenteLocal.LeerEstudio();
            Ui.Texto(Ui.Vacio(_raiz, "Estudio").Anclar(0.05f, 0.76f, 0.95f, 0.81f),
                $"Tu estudio: nivel {est.Nivel} · reputación {est.Reputacion} · S/ {est.Honorarios} en honorarios del juego · {est.ExpedientesCerrados} expedientes cerrados",
                30, Ui.Pergamino);

            Ui.Texto(Ui.Vacio(_raiz, "EtiquetaAlias").Anclar(0.05f, 0.71f, 0.95f, 0.75f), "Tu nombre en el juego", 32, Ui.Dorado, TextAnchor.LowerLeft);
            var alias = Ui.Campo(Ui.Vacio(_raiz, "Alias").Anclar(0.05f, 0.65f, 0.95f, 0.71f), _alias, "Doctor(a)...");
            alias.onEndEdit.AddListener(v => { _alias = string.IsNullOrWhiteSpace(v) ? "Doctor(a)" : v.Trim(); PlayerPrefs.SetString("eyd.alias", _alias); });

            Ui.Texto(Ui.Vacio(_raiz, "EtiquetaEsp").Anclar(0.05f, 0.60f, 0.95f, 0.64f), "Especialidad", 32, Ui.Dorado, TextAnchor.LowerLeft);
            Selector(Ui.Vacio(_raiz, "Esp").Anclar(0.05f, 0.53f, 0.95f, 0.60f), new[] { ("Civilista", "Civilista\n(control)"), ("Administrativista", "Administrativista\n(apoyo)") },
                () => _especialidad, v => _especialidad = v);

            Ui.Texto(Ui.Vacio(_raiz, "EtiquetaCaso").Anclar(0.05f, 0.48f, 0.95f, 0.52f), "Caso", 32, Ui.Dorado, TextAnchor.LowerLeft);
            Selector(Ui.Vacio(_raiz, "Caso").Anclar(0.05f, 0.41f, 0.95f, 0.48f), CatalogoCasos.Todos.Select(c => (c.Id, $"{c.Numero}. {c.Titulo}")).ToArray(),
                () => _caso, v => _caso = v);
            var premisa = CatalogoCasos.Buscar(_caso).Premisa;
            Ui.Texto(Ui.Vacio(_raiz, "Premisa").Anclar(0.05f, 0.36f, 0.95f, 0.41f), premisa, 30, Ui.Pergamino, TextAnchor.MiddleLeft, FontStyle.Italic);

            Selector(Ui.Vacio(_raiz, "Modo").Anclar(0.05f, 0.29f, 0.95f, 0.35f), new[] { ("local", "Sin conexión"), ("linea", "En línea") },
                () => _enLinea ? "linea" : "local", v => { _enLinea = v == "linea"; PlayerPrefs.SetInt("eyd.enlinea", _enLinea ? 1 : 0); MostrarMenu(); });
            if (_enLinea)
            {
                var url = Ui.Campo(Ui.Vacio(_raiz, "Url").Anclar(0.05f, 0.23f, 0.95f, 0.28f), _url, "http://servidor:8080");
                url.onEndEdit.AddListener(v => { _url = v.Trim(); PlayerPrefs.SetString("eyd.url", _url); });
            }
            else
            {
                Selector(Ui.Vacio(_raiz, "Borr").Anclar(0.05f, 0.23f, 0.95f, 0.28f), new[] { ("no", "Solo preguntas verificadas"), ("si", "Incluir borradores (pruebas)") },
                    () => _borradores ? "si" : "no", v => { _borradores = v == "si"; MostrarMenu(); });
            }

            Ui.Boton(Ui.Vacio(_raiz, "Jugar").Anclar(0.15f, 0.12f, 0.85f, 0.20f), "ABRIR EXPEDIENTE", () => _ = Empezar(), Ui.Carmesi, Ui.Pergamino, 52);
            if (aviso != null) Ui.Texto(Ui.Vacio(_raiz, "Aviso").Anclar(0.05f, 0.06f, 0.95f, 0.115f), aviso, 30, Ui.Carmesi);
            Ui.Texto(Ui.Vacio(_raiz, "Pie").Anclar(0.05f, 0.0f, 0.95f, 0.05f),
                "Juego de entretenimiento y aprendizaje; no constituye asesoría legal. Incluye material del SRD 5.2 (CC-BY-4.0).", 22, new Color(1, 1, 1, 0.5f));
            _ = t;
        }

        private void Selector(RectTransform zona, (string valor, string texto)[] opciones, Func<string> actual, Action<string> elegir)
        {
            for (var i = 0; i < opciones.Length; i++)
            {
                var (valor, texto) = opciones[i];
                var activo = actual() == valor;
                var b = Ui.Boton(Ui.Vacio(zona, valor).Anclar((float)i / opciones.Length, 0, (float)(i + 1) / opciones.Length, 1, 6), texto,
                    () => { elegir(valor); MostrarMenu(); }, activo ? Ui.Dorado : Ui.Tinta, activo ? Ui.Noche : Ui.Pergamino, 34);
                _ = b;
            }
        }

        private async Task Empezar()
        {
            if (_ocupado) return;
            _ocupado = true;
            try
            {
                if (_enLinea)
                {
                    var remota = new FuenteRemota(_url);
                    await remota.ConectarAsync(_alias);
                    _fuente = remota;
                }
                else _fuente = new FuenteLocal(_alias, _borradores);
                var r = await _fuente.CreateAsyncSafe(_caso, _especialidad, 1);
                ConstruirPartida();
                Anotar($"Don Eusebio: \"{r.Estado.Premisa}\"");
                await Aplicar(r);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                MostrarMenu(_enLinea ? $"No se pudo conectar con {_url}. Revisa la red o juega sin conexión." : ex.Message);
            }
            finally { _ocupado = false; }
        }

        // ───────────────────────────── Partida ─────────────────────────────

        private void ConstruirPartida()
        {
            Ui.Limpiar(_raiz);
            var cab = Ui.Panel(_raiz, "Cabecera", Ui.Tinta).Anclar(0, 0.92f, 1, 1);
            _cabecera = Ui.Texto(Ui.Vacio(cab, "T").Anclar(0, 0.45f, 1, 1), "", 42, Ui.Dorado, TextAnchor.MiddleLeft, FontStyle.Bold);
            _subcabecera = Ui.Texto(Ui.Vacio(cab, "S").Anclar(0, 0, 1, 0.5f), "", 30, Ui.Pergamino, TextAnchor.MiddleLeft);

            _tablero = Ui.Panel(_raiz, "Tablero", Ui.Noche).Anclar(0.02f, 0.47f, 0.98f, 0.915f);
            for (var x = 0; x < Columnas; x++)
                for (var y = 0; y < Filas; y++)
                {
                    int cx = x, cy = y;
                    var celda = Ui.Vacio(_tablero, $"C{x}{y}").Anclar((float)x / Columnas, 1f - (float)(y + 1) / Filas, (float)(x + 1) / Columnas, 1f - (float)y / Filas, 3);
                    _casillas[x, y] = Ui.Boton(celda, "", () => TocarCasilla(cx, cy), Ui.Casilla, Ui.Pergamino, 26);
                }

            var sala = Ui.Panel(_raiz, "Sala", Ui.Tinta).Anclar(0.02f, 0.37f, 0.98f, 0.465f);
            _textoSala = Ui.Texto(sala, "", 34, Ui.Pergamino, TextAnchor.MiddleLeft);
            var bit = Ui.Panel(_raiz, "Bitacora", new Color(0, 0, 0, 0.35f)).Anclar(0.02f, 0.235f, 0.98f, 0.365f);
            _textoBitacora = Ui.Texto(bit, "", 26, Ui.Pergamino, TextAnchor.LowerLeft);
            _textoBitacora.resizeTextForBestFit = false;
            _barra = Ui.Vacio(_raiz, "Barra").Anclar(0.02f, 0.01f, 0.98f, 0.23f);
            _capa = Ui.Vacio(_raiz, "Capa").Anclar(0, 0, 1, 1);
        }

        private bool EsMiTurno =>
            _estado?.Combate is { Terminado: false } c && _fuente != null &&
            _estado.Equipo.Any(m => m.Id == c.TurnoDe && m.JugadorId == _fuente.JugadorId);

        private CombatienteEstado? Yo =>
            _estado == null || _fuente == null ? null : _estado.Equipo.FirstOrDefault(m => m.JugadorId == _fuente.JugadorId);

        private void Pintar()
        {
            if (_estado == null) return;
            var e = _estado;
            _cabecera.text = $"{e.Titulo}";
            var reloj = e.MinutosRestantes.HasValue ? $" · reloj {e.MinutosRestantes / 60}:{e.MinutosRestantes % 60:00}" : "";
            _subcabecera.text = $"Sala {Math.Max(1, e.IndiceSala + 1)} de {e.Salas.Count}: {NombreSala(e.SalaActual)}{reloj} · reputación {e.Estudio.Reputacion} · S/ {e.HonorariosDisponibles} · pruebas jurídicas {e.PruebasJuridicasRestantes}";

            var porCasilla = new Dictionary<(int, int), CombatienteEstado>();
            if (e.Combate != null) foreach (var c in e.Combate.Combatientes.Where(c => !c.Caido)) porCasilla[(c.Columna, c.Fila)] = c;
            var validas = new HashSet<(int, int)>(EsMiTurno && e.Combate != null ? e.Combate.CasillasValidas.Select(v => (v[0], v[1])) : Enumerable.Empty<(int, int)>());
            for (var x = 0; x < Columnas; x++)
                for (var y = 0; y < Filas; y++)
                {
                    var b = _casillas[x, y];
                    var img = (Image)b.targetGraphic;
                    if (porCasilla.TryGetValue((x, y), out var c))
                    {
                        var esTurno = e.Combate!.TurnoDe == c.Id;
                        img.color = c.Bando == "Estudio" ? (c.JugadorId == _fuente?.JugadorId ? Ui.Dorado : Ui.Aliado) : (c.Id == _objetivo ? Ui.Carmesi : Ui.Hex("#6B2B3A"));
                        var estado = c.TurnosInmovilizado > 0 ? " ⛓" : "";
                        b.CambiarTexto($"{(esTurno ? "▶ " : "")}{Corto(c.Nombre)}\n{c.Paciencia}/{c.PacienciaMaxima}{estado}");
                        b.GetComponentInChildren<Text>().color = c.Bando == "Estudio" && c.JugadorId == _fuente?.JugadorId ? Ui.Noche : Ui.Pergamino;
                    }
                    else
                    {
                        img.color = validas.Contains((x, y)) ? Ui.CasillaValida : (e.Combate == null ? Ui.Tinta : Ui.Casilla);
                        b.CambiarTexto("");
                    }
                }

            _textoSala.text = DescripcionSala(e);
            _textoBitacora.text = string.Join("\n", _bitacora.Skip(Math.Max(0, _bitacora.Count - 7)));
            PintarBarra();
            if (e.Sentencia != null) MostrarSentencia(e);
        }

        private static string Corto(string n) => n.Length <= 12 ? n : n.Substring(0, 11) + "…";

        private static string NombreSala(string? s) => s switch
        {
            "Combate" => "combate", "Tramite" => "trámite", "Audiencia" => "audiencia", "Libreria" => "librería jurídica",
            "Cafeteria" => "cafetería del juzgado", "Coimero" => "pasillo", "RedDelCoimero" => "la red del Coimero", "Jefe" => "jefe: La Cola Infinita",
            _ => "—",
        };

        private string DescripcionSala(EstadoPartida e)
        {
            if (e.SalaResuelta) return "Sala superada. Don Eusebio: \"Sigan, doctores, que el expediente no se mueve solo.\"";
            switch (e.SalaActual)
            {
                case "Combate":
                case "RedDelCoimero":
                case "Jefe":
                    if (e.Combate == null) return "";
                    var turno = e.Combate.Combatientes.FirstOrDefault(c => c.Id == e.Combate.TurnoDe);
                    return EsMiTurno
                        ? $"Tu turno (ronda {e.Combate.Ronda}). Toca una casilla verde para moverte ({e.Combate.MovimientoRestante} casillas) o un enemigo para elegirlo."
                        : $"Turno de {turno?.Nombre}…";
                case "Tramite": return "Una puerta pide \"el requisito correcto\". Una prueba jurídica acertada antes te da ventaja.";
                case "Audiencia": return "El Cliente cambia su versión de los hechos. ¿Lo confrontan o revisan el expediente?";
                case "Cafeteria": return "La cafetería del juzgado. Un café y el equipo recupera paciencia.";
                case "Libreria": return "La librería jurídica (y el Ekeko). Se paga con honorarios del juego, nunca con dinero real.";
                case "Coimero": return "Un señor muy amable ofrece \"agilizar\" el trámite por una propina. ¿Qué hace el estudio?";
                default: return "";
            }
        }

        private void PintarBarra()
        {
            Ui.Limpiar(_barra);
            var e = _estado!;
            var botones = new List<(string texto, Action accion, Color fondo)>();
            void B(string t, Action a, Color? f = null) => botones.Add((t, a, f ?? Ui.Noche));
            AccionJuego A(string tipo) => new AccionJuego { Tipo = tipo };

            if (e.Sentencia != null) { }
            else if (e.SalaResuelta) B("SIGUIENTE SALA ▶", () => Enviar(A("avanzar")), Ui.Carmesi);
            else switch (e.SalaActual)
            {
                case "Combate":
                case "RedDelCoimero":
                case "Jefe":
                    if (EsMiTurno)
                    {
                        var yo = e.Combate!.Combatientes.First(c => c.Id == e.Combate.TurnoDe);
                        B(_objetivo == null ? "Atacar\n(elige enemigo)" : "ATACAR", () => { if (_objetivo != null) Enviar(new AccionJuego { Tipo = "atacar", Objetivo = _objetivo }); else Anotar("Toca primero a un enemigo."); }, Ui.Carmesi);
                        var hab = yo.Especialidad == "Civilista" ? ("MedidaCautelar", "Medida\ncautelar") : ("Queja", "Queja");
                        B(hab.Item2, () => { if (_objetivo != null) Enviar(new AccionJuego { Tipo = "habilidad", Habilidad = hab.Item1, Objetivo = _objetivo }); else Anotar("Toca primero a un enemigo."); });
                        B($"Prueba\njurídica ({e.PruebasJuridicasRestantes})", () => Enviar(A("pedirPregunta")));
                        if (e.TieneEscritoPreFirmado && _objetivo != null) B("Atacar con escrito\npre-firmado", () => Enviar(new AccionJuego { Tipo = "atacar", Objetivo = _objetivo, Opcion = "escrito" }));
                        if (!e.Combate.CasacionUsada) B("Casación", () => Enviar(new AccionJuego { Tipo = "casacion" }));
                        B("Terminar\nturno", () => Enviar(A("terminarTurno")), Ui.Tinta);
                        B("Piloto\nautomático", () => Enviar(A("piloto")), Ui.Tinta);
                    }
                    if (Yo?.Especialidad == "Administrativista")
                    {
                        var aliado = e.Equipo.FirstOrDefault(m => m.Id != Yo.Id && !m.Caido);
                        if (aliado != null) B($"Apelar por\n{Corto(aliado.Nombre)}", () => Enviar(new AccionJuego { Tipo = "apelar", Objetivo = aliado.Id }), Ui.Tinta);
                    }
                    if (!EsMiTurno && botones.Count == 0) B("Esperando…", () => { }, Ui.Tinta);
                    break;
                case "Tramite":
                    B("Presentar el escrito\n(Criterio)", () => Enviar(A("tramite")), Ui.Carmesi);
                    if (e.TieneEscritoPreFirmado) B("Usar escrito\npre-firmado", () => Enviar(new AccionJuego { Tipo = "tramite", Opcion = "escrito" }));
                    B($"Prueba\njurídica ({e.PruebasJuridicasRestantes})", () => Enviar(A("pedirPregunta")));
                    break;
                case "Audiencia":
                    B("Confrontar\n(Oratoria)", () => Enviar(new AccionJuego { Tipo = "audiencia", Opcion = "confrontar" }), Ui.Carmesi);
                    B("Revisar el expediente\n(Criterio)", () => Enviar(new AccionJuego { Tipo = "audiencia", Opcion = "revisar" }), Ui.Carmesi);
                    B($"Prueba\njurídica ({e.PruebasJuridicasRestantes})", () => Enviar(A("pedirPregunta")));
                    break;
                case "Cafeteria":
                    B("Tomar un café", () => Enviar(A("descansar")), Ui.Carmesi);
                    break;
                case "Libreria":
                    B($"Escrito pre-firmado\nS/ {Expediente.CostoEscritoPreFirmado}", () => Enviar(new AccionJuego { Tipo = "comprar", Objeto = "EscritoPreFirmado" }));
                    B($"Café doble\nS/ {Expediente.CostoCafeDoble}", () => Enviar(new AccionJuego { Tipo = "comprar", Objeto = "CafeDoble" }));
                    B($"Código anotado\nS/ {Expediente.CostoCodigoAnotado}", () => Enviar(new AccionJuego { Tipo = "comprar", Objeto = "CodigoAnotado" }));
                    B("Salir", () => Enviar(A("salirTienda")), Ui.Carmesi);
                    break;
                case "Coimero":
                    B("Aceptar", () => Enviar(new AccionJuego { Tipo = "coimero", Decision = "Aceptar" }), Ui.Tinta);
                    B("Rechazar", () => Enviar(new AccionJuego { Tipo = "coimero", Decision = "Rechazar" }));
                    B("Denunciar", () => Enviar(new AccionJuego { Tipo = "coimero", Decision = "Denunciar" }), Ui.Carmesi);
                    break;
            }

            var filas = botones.Count > 4 ? 2 : 1;
            var porFila = (int)Math.Ceiling(botones.Count / (float)filas);
            for (var i = 0; i < botones.Count; i++)
            {
                var (texto, accion, fondo) = botones[i];
                int f = i / Math.Max(1, porFila), c = i % Math.Max(1, porFila);
                var zona = Ui.Vacio(_barra, "B" + i).Anclar((float)c / porFila, 1f - (float)(f + 1) / filas, (float)(c + 1) / porFila, 1f - (float)f / filas, 6);
                Ui.Boton(zona, texto, () => accion(), fondo, Ui.Pergamino, 34);
            }
        }

        private void TocarCasilla(int x, int y)
        {
            if (_estado?.Combate == null || _ocupado) return;
            var c = _estado.Combate.Combatientes.FirstOrDefault(k => !k.Caido && k.Columna == x && k.Fila == y);
            if (c != null)
            {
                if (c.Bando == "Sistema") { _objetivo = c.Id; Anotar($"Objetivo: {c.Nombre} (paciencia {c.Paciencia}, defensa {c.Defensa})."); Pintar(); }
                else Anotar($"{c.Nombre}: paciencia {c.Paciencia}/{c.PacienciaMaxima}.");
                return;
            }
            if (EsMiTurno && _estado.Combate.CasillasValidas.Any(v => v[0] == x && v[1] == y))
                Enviar(new AccionJuego { Tipo = "mover", Columna = x, Fila = y });
        }

        private async void Enviar(AccionJuego a)
        {
            if (_fuente == null || _ocupado) return;
            _ocupado = true;
            try
            {
                var r = await _fuente.EjecutarAsync(a);
                if (!r.Valida) { Anotar($"✗ {r.Motivo}"); _estado = r.Estado; Pintar(); return; }
                if (r.Extra?.Pregunta != null) { _estado = r.Estado; Pintar(); MostrarPregunta(r.Extra.Pregunta); return; }
                await Aplicar(r);
            }
            catch (Exception ex) { Debug.LogException(ex); Anotar("✗ Sin conexión con el servidor. Intenta de nuevo."); }
            finally { _ocupado = false; }
        }

        private async Task Aplicar(RespuestaAccion r)
        {
            if (_estado?.SalaActual != r.Estado.SalaActual || _estado?.IndiceSala != r.Estado.IndiceSala) _objetivo = null;
            foreach (var ev in r.Eventos) await Narrar(ev);
            _estado = r.Estado;
            if (_objetivo != null && (_estado.Combate == null || _estado.Combate.Combatientes.All(c => c.Id != _objetivo || c.Caido))) _objetivo = null;
            if (r.Extra?.Correccion != null) MostrarCorreccion(r.Extra.Correccion);
            Pintar();
        }

        private string Nombre(string id) =>
            _estado?.Combate?.Combatientes.FirstOrDefault(c => c.Id == id)?.Nombre
            ?? _estado?.Equipo.FirstOrDefault(c => c.Id == id)?.Nombre ?? id;

        private async Task Narrar(EventoJuego ev)
        {
            var d = Json.Datos(ev);
            string S(string k) => (string?)d[k] ?? "";
            switch (ev.Tipo)
            {
                case "Ataco":
                    var momento = S("momento");
                    var linea = $"{Nombre(S("atacante"))} → {Nombre(S("objetivo"))}: d20 = {(int)d["natural"]!} (total {(int)d["total"]!}) {((bool)d["acierto"]! ? "acierta" : "falla")}";
                    Anotar(linea);
                    var mio = _estado?.Equipo.Any(m => m.Id == S("atacante") && m.JugadorId == _fuente?.JugadorId) == true;
                    if (mio || momento != "ninguno") await Dado((int)d["natural"]!, momento);
                    break;
                case "RecibioDanio": Anotar($"   {Nombre(S("id"))} pierde {(int)d["cantidad"]!} de paciencia."); break;
                case "Cayo": Anotar($"   {Nombre(S("id"))} cae."); break;
                case "Movio": break;
                case "UsoHabilidad": Anotar($"{Nombre(S("id"))} usa {S("habilidad")}."); break;
                case "Inmovilizado": Anotar($"   {Nombre(S("id"))} queda inmovilizado {(int)d["turnos"]!} turnos."); break;
                case "Aparecio": Anotar($"¡Se suma a la cola: {S("nombre")}!"); break;
                case "FinDeCombate": Anotar($"— {S("motivo")} —"); break;
                case "SalaIniciada": Anotar($"Entran a la sala {(int)d["indice"]! + 1}: {NombreSala(Capital(S("tipo")))}."); break;
                case "SalaSuperada": Anotar($"✓ {S("detalle")}"); break;
                case "RelojAvanzo": break;
                case "ComentarioDeDonEusebio": Anotar($"Don Eusebio: \"{S("texto")}\""); break;
                case "SentenciaDictada": break;
                case "InicioDeTurno": break;
            }
        }

        private static string Capital(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

        private void Anotar(string s)
        {
            _bitacora.Add(s);
            if (_bitacora.Count > 60) _bitacora.RemoveRange(0, _bitacora.Count - 60);
            if (_textoBitacora != null) _textoBitacora.text = string.Join("\n", _bitacora.Skip(Math.Max(0, _bitacora.Count - 7)));
        }

        // ───────────────────────────── El dado como espectáculo ─────────────────────────────

        private Task Dado(int natural, string momento)
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(AnimarDado(natural, momento, tcs));
            return tcs.Task;
        }

        private IEnumerator AnimarDado(int natural, string momento, TaskCompletionSource<bool> fin)
        {
            var fondo = Ui.Panel(_capa, "Dado", new Color(0, 0, 0, 0.55f)).Anclar(0, 0, 1, 1);
            var caja = Ui.Panel(fondo, "Caja", Ui.Pergamino).Anclar(0.3f, 0.42f, 0.7f, 0.62f);
            var num = Ui.Texto(caja, "", 150, Ui.Noche, TextAnchor.MiddleCenter, FontStyle.Bold);
            var rnd = new System.Random();
            for (var i = 0; i < 10; i++)
            {
                num.text = rnd.Next(1, 21).ToString();
                caja.localRotation = Quaternion.Euler(0, 0, rnd.Next(-25, 25));
                yield return new WaitForSeconds(0.04f + i * 0.01f);
            }
            caja.localRotation = Quaternion.identity;
            num.text = natural.ToString();
            if (momento == "fundado" || momento == "improcedente")
            {
                var sello = Ui.Texto(Ui.Vacio(fondo, "Sello").Anclar(0.05f, 0.64f, 0.95f, 0.76f),
                    momento == "fundado" ? "¡FUNDADO!" : "IMPROCEDENTE", 110, momento == "fundado" ? Ui.Dorado : Ui.Carmesi, TextAnchor.MiddleCenter, FontStyle.Bold);
                for (var t = 0f; t < 0.35f; t += Time.deltaTime)
                {
                    sello.transform.localScale = Vector3.one * Mathf.Lerp(2.2f, 1f, t / 0.35f);
                    yield return null;
                }
                sello.transform.localScale = Vector3.one;
                if (momento == "improcedente")
                    Ui.Texto(Ui.Vacio(fondo, "Pifia").Anclar(0.05f, 0.34f, 0.95f, 0.41f), new[] { "Se le cayó el expediente.", "Se acabó la tinta.", "Firmó en la línea equivocada." }[rnd.Next(3)], 40, Ui.Pergamino);
                yield return new WaitForSeconds(0.9f);
            }
            yield return new WaitForSeconds(0.45f);
            Destroy(fondo.gameObject);
            fin.TrySetResult(true);
        }

        // ───────────────────────────── Prueba jurídica ─────────────────────────────

        private void MostrarPregunta(PreguntaVisible p)
        {
            var fondo = Ui.Panel(_capa, "Pregunta", new Color(0, 0, 0, 0.7f)).Anclar(0, 0, 1, 1);
            var carta = Ui.Panel(fondo, "Carta", Ui.Pergamino).Anclar(0.04f, 0.12f, 0.96f, 0.88f);
            Ui.Texto(Ui.Vacio(carta, "Mat").Anclar(0, 0.9f, 1, 0.98f), $"PRUEBA JURÍDICA · {p.Materia}", 34, Ui.Carmesi, TextAnchor.MiddleCenter, FontStyle.Bold);
            if (p.Borrador)
                Ui.Texto(Ui.Panel(carta, "Borr", Ui.Hex("#FFF3B0")).Anclar(0.04f, 0.84f, 0.96f, 0.9f), "BORRADOR SIN VERIFICAR: solo para pruebas", 28, Ui.Noche);
            Ui.Texto(Ui.Vacio(carta, "Q").Anclar(0.04f, 0.6f, 0.96f, 0.84f), p.Texto, 44, Ui.Noche);
            var barra = Ui.Panel(carta, "Tiempo", Ui.Carmesi).Anclar(0.04f, 0.56f, 0.96f, 0.58f);
            var respondido = false;
            for (var i = 0; i < 4; i++)
            {
                var indice = i;
                Ui.Boton(Ui.Vacio(carta, "O" + i).Anclar(0.04f, 0.42f - i * 0.135f, 0.96f, 0.54f - i * 0.135f, 4), $"{(char)('A' + i)}) {p.Opciones[i]}",
                    () => { if (respondido) return; respondido = true; Destroy(fondo.gameObject); Responder(indice); }, Ui.Noche, Ui.Pergamino, 38);
            }
            StartCoroutine(Cuenta(barra, p.Segundos, () => { if (respondido) return; respondido = true; Destroy(fondo.gameObject); Responder(null); }));
        }

        private IEnumerator Cuenta(RectTransform barra, int segundos, Action alTerminar)
        {
            for (var t = 0f; t < segundos; t += Time.deltaTime)
            {
                if (barra == null) yield break;
                barra.anchorMax = new Vector2(Mathf.Lerp(0.96f, 0.04f, t / segundos), barra.anchorMax.y);
                yield return null;
            }
            alTerminar();
        }

        private void Responder(int? indice) => Enviar(new AccionJuego { Tipo = "responder", Indice = indice });

        private void MostrarCorreccion(CorreccionPregunta c)
        {
            var fondo = Ui.Panel(_capa, "Correccion", new Color(0, 0, 0, 0.7f)).Anclar(0, 0, 1, 1);
            var carta = Ui.Panel(fondo, "Carta", Ui.Pergamino).Anclar(0.04f, 0.2f, 0.96f, 0.8f);
            var titulo = c.Resultado == "Acierto" ? "¡Correcto! Tu próxima tirada va con ventaja." : c.Resultado == "Fallo" ? "No era esa. Tiras normal." : "Se acabó el tiempo. Tiras normal.";
            Ui.Texto(Ui.Vacio(carta, "R").Anclar(0.04f, 0.82f, 0.96f, 0.96f), titulo, 40, c.Resultado == "Acierto" ? Ui.Hex("#2F6B2F") : Ui.Carmesi, TextAnchor.MiddleCenter, FontStyle.Bold);
            Ui.Texto(Ui.Vacio(carta, "A").Anclar(0.04f, 0.66f, 0.96f, 0.82f), $"Respuesta: {c.Respuesta}", 38, Ui.Noche);
            Ui.Texto(Ui.Vacio(carta, "B").Anclar(0.04f, 0.48f, 0.96f, 0.66f), $"Base legal: {c.BaseLegal}", 32, Ui.Noche);
            var verif = c.FechaVerificacion == null ? "Pregunta en borrador: aún no verificada en la fuente oficial." : $"Verificada por {c.VerificadoPor} el {c.FechaVerificacion}.";
            Ui.Texto(c.FechaVerificacion == null ? Ui.Panel(carta, "V", Ui.Hex("#FFF3B0")).Anclar(0.04f, 0.36f, 0.96f, 0.47f) : Ui.Vacio(carta, "V").Anclar(0.04f, 0.36f, 0.96f, 0.47f), verif, 28, Ui.Noche);
            if (Uri.TryCreate(c.EnlaceOficial, UriKind.Absolute, out var u) && u.Scheme == "https")
                Ui.Boton(Ui.Vacio(carta, "E").Anclar(0.04f, 0.2f, 0.96f, 0.33f, 4), "Ver la norma en la fuente oficial", () => Application.OpenURL(u.ToString()), Ui.Tinta, Ui.Pergamino, 32);
            Ui.Boton(Ui.Vacio(carta, "C").Anclar(0.25f, 0.03f, 0.75f, 0.17f, 4), "Seguir", () => Destroy(fondo.gameObject), Ui.Carmesi, Ui.Pergamino, 40);
        }

        // ───────────────────────────── Sentencia ─────────────────────────────

        private void MostrarSentencia(EstadoPartida e)
        {
            if (_capa.Find("Sentencia") != null) return;
            var s = e.Sentencia!;
            var fondo = Ui.Panel(_capa, "Sentencia", new Color(0, 0, 0, 0.8f)).Anclar(0, 0, 1, 1);
            var carta = Ui.Panel(fondo, "Carta", Ui.Pergamino).Anclar(0.06f, 0.2f, 0.94f, 0.8f);
            Ui.Texto(Ui.Vacio(carta, "T").Anclar(0.04f, 0.78f, 0.96f, 0.96f), "SENTENCIA", 70, Ui.Noche, TextAnchor.MiddleCenter, FontStyle.Bold);
            Ui.Texto(Ui.Vacio(carta, "M").Anclar(0.04f, 0.6f, 0.96f, 0.78f), s.Motivo, 50, s.Fundada ? Ui.Hex("#2F6B2F") : Ui.Carmesi, TextAnchor.MiddleCenter, FontStyle.Bold);
            Ui.Texto(Ui.Vacio(carta, "H").Anclar(0.04f, 0.42f, 0.96f, 0.6f), $"Honorarios: S/ {s.Honorarios} · Experiencia: {s.Experiencia}\nTu estudio: nivel {e.Estudio.Nivel} · reputación {e.Estudio.Reputacion}", 36, Ui.Noche);
            Ui.Texto(Ui.Vacio(carta, "L").Anclar(0.04f, 0.22f, 0.96f, 0.42f), "Lo que aprendiste: la ficha jurídica de este caso se publicará cuando un abogado la verifique en la fuente oficial.", 28, Ui.Tinta, TextAnchor.MiddleCenter, FontStyle.Italic);
            Ui.Boton(Ui.Vacio(carta, "V").Anclar(0.2f, 0.04f, 0.8f, 0.18f, 4), "Volver al estudio", () => MostrarMenu(), Ui.Carmesi, Ui.Pergamino, 40);
        }

        // ───────────────────────────── En línea: cambios de los demás ─────────────────────────────

        private async void Update()
        {
            if (!(_fuente is FuenteRemota) || _estado == null || _estado.Sentencia != null || _ocupado || Time.time < _proximoSondeo) return;
            _proximoSondeo = Time.time + 2f;
            if (EsMiTurno && _estado.Combate != null) return;
            _ocupado = true;
            try
            {
                var nuevo = await _fuente.SondearAsync();
                if (nuevo != null && nuevo.Version != _estado.Version) { _estado = nuevo; Pintar(); }
            }
            catch (Exception ex) { Debug.LogWarning(ex.Message); }
            finally { _ocupado = false; }
        }
    }

    internal static class ExtensionesFuente
    {
        public static Task<RespuestaAccion> CreateAsyncSafe(this IFuentePartida f, string caso, string esp, int bots) => f.CrearAsync(caso, esp, bots);
    }
}
