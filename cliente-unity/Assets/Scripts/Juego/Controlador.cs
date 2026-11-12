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
        private const string ClavePrologo = "eyd.prologo-visto";

        private RectTransform _raiz = null!;
        private IFuentePartida? _fuente;
        private EstadoPartida? _estado;
        private string? _objetivo;
        private bool _ocupado;
        private float _proximoSondeo;
        private readonly List<string> _bitacora = new List<string>();

        // Partida
        private Text _cabecera = null!, _subcabecera = null!, _textoBitacora = null!, _textoSala = null!;
        private RectTransform _tablero = null!, _barra = null!, _capa = null!, _rejilla = null!, _capaFichas = null!;
        private Image _fondoTablero = null!;
        private readonly Image[,] _casillas = new Image[Columnas, Filas];
        private readonly Dictionary<string, FichaVisual> _fichas = new Dictionary<string, FichaVisual>();
        private bool _posicionesViejas;
        private bool _saltarDialogos;

        // Preferencias del menú
        private string _alias = "Doctor(a)";
        private string _especialidad = "Civilista";
        private string _caso = "escrito-fantasma";
        private bool _enLinea;
        private string _url = "http://192.168.1.252:8080";
        private bool _borradores;

        private sealed class FichaVisual
        {
            public RectTransform Raiz = null!;
            public Image Dibujo = null!;
            public RectTransform Vida = null!;
            public Image VidaColor = null!;
            public Text Insignia = null!;
            public GameObject InsigniaFondo = null!;
            public int Maxima;
            public int Columna, Fila;
        }

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
            if (FindFirstObjectByType<AudioListener>() == null) Camera.main!.gameObject.AddComponent<AudioListener>();
            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Sonido.Preparar(gameObject);

            var lienzo = new GameObject("Lienzo", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            lienzo.transform.SetParent(transform, false);
            lienzo.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var escala = lienzo.GetComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1080, 1920);
            escala.matchWidthOrHeight = 0.5f;
            _raiz = Ui.Panel(lienzo.transform, "Raiz", Ui.Noche).Anclar(0, 0, 1, 1);
            Efectos.Preparar(lienzo.transform);

            _alias = PlayerPrefs.GetString("eyd.alias", _alias);
            _url = PlayerPrefs.GetString("eyd.url", _url);
            _enLinea = PlayerPrefs.GetInt("eyd.enlinea", 0) == 1;
            // Primera vez: directo a un combate, sin menús. El prólogo queda en el menú para quien lo quiera.
            if (PlayerPrefs.GetInt(ClavePrologo, 0) == 0)
            {
                PlayerPrefs.SetInt(ClavePrologo, 1);
                MostrarMenu();
                _ = JugarYa();
            }
            else MostrarMenu();
        }

        /// <summary>Partida rápida: sin conexión, con la última especialidad y el caso más difícil que ya desbloqueó el estudio.</summary>
        private Task JugarYa()
        {
            _enLinea = false;
            var cerrados = FuenteLocal.LeerEstudio().ExpedientesCerrados;
            var casos = CatalogoCasos.Todos;
            _caso = casos[Math.Min(casos.Count - 1, cerrados / 2)].Id;
            return Empezar();
        }

        // ───────────────────────────── Portada y menú ─────────────────────────────

        private void MostrarMenu(string? aviso = null)
        {
            Ui.Limpiar(_raiz);
            _estado = null; _fuente = null; _bitacora.Clear(); _fichas.Clear();
            Sonido.Musica(true);
            Ui.Imagen(Ui.Vacio(_raiz, "Portada").Anclar(0, 0.22f, 1, 1), "Arte", Arte.Sprite("portada"), false);
            Ui.Panel(_raiz, "Base", new Color(0.055f, 0.047f, 0.1f, 0.92f)).Anclar(0, 0, 1, 0.29f);
            var zonaTitulo = Ui.Vacio(_raiz, "Titulo").Anclar(0.03f, 0.82f, 0.97f, 0.985f);
            var logo = Arte.Sprite("logo");
            if (logo != null) Ui.Imagen(zonaTitulo, "Logo", logo);
            else Ui.Texto(zonaTitulo, "EXPEDIENTES\nY DRAGONES", 104, Ui.Dorado, TextAnchor.MiddleCenter, FontStyle.Bold).ConSombra(5);
            Ui.Texto(Ui.Vacio(_raiz, "Frase").Anclar(0.06f, 0.79f, 0.94f, 0.84f), "Litiga, investiga y vence a los monstruos de la burocracia.", 36, Ui.Pergamino, TextAnchor.MiddleCenter, FontStyle.Italic).ConSombra();

            var est = FuenteLocal.LeerEstudio();
            var acto = est.ExpedientesCerrados < 3 ? "Acto 1 · El primer caso" : "Acto 1 · Algo despierta en el Archivo Central";
            Ui.Texto(Ui.Vacio(_raiz, "Acto").Anclar(0.05f, 0.245f, 0.95f, 0.285f), acto, 40, Ui.Dorado, TextAnchor.MiddleCenter, FontStyle.Bold);
            var proximo = est.Proximo;
            var meta = proximo == null ? "Todos los desbloqueos ganados."
                : $"Próximo desbloqueo: {proximo.Nombre} (faltan {proximo.ExpedientesNecesarios - est.ExpedientesCerrados} expedientes)";
            Ui.Texto(Ui.Vacio(_raiz, "Estudio").Anclar(0.05f, 0.205f, 0.95f, 0.245f),
                $"Tu estudio: nivel {est.Nivel} · reputación {est.Reputacion} · S/ {est.Honorarios} · {est.ExpedientesCerrados} cerrados\n{meta}", 28, Ui.Pergamino);

            Ui.Boton(Ui.Vacio(_raiz, "Jugar").Anclar(0.12f, 0.115f, 0.88f, 0.195f), "¡JUGAR YA!", () => _ = JugarYa(), Ui.Carmesi, Ui.Pergamino, 56);
            Ui.Boton(Ui.Vacio(_raiz, "Preparar").Anclar(0.04f, 0.05f, 0.36f, 0.105f), "Elegir caso", () => MostrarPreparacion(), Ui.Tinta, Ui.Pergamino, 30);
            Ui.Boton(Ui.Vacio(_raiz, "Prologo").Anclar(0.37f, 0.05f, 0.65f, 0.105f), "Prólogo", () => _ = VerPrologo(), Ui.Tinta, Ui.Pergamino, 30);
            Ui.Boton(Ui.Vacio(_raiz, "Sonido").Anclar(0.66f, 0.05f, 0.96f, 0.105f), Sonido.Silenciado ? "Sonido: no" : "Sonido: sí",
                () => { Sonido.Silenciado = !Sonido.Silenciado; MostrarMenu(); }, Ui.Tinta, Ui.Pergamino, 30);
            if (aviso != null) Ui.Texto(Ui.PanelRedondeado(_raiz, "Aviso", Ui.Carmesi).Anclar(0.05f, 0.30f, 0.95f, 0.36f), aviso, 30, Ui.Pergamino);
            Ui.Texto(Ui.Vacio(_raiz, "Pie").Anclar(0.04f, 0.0f, 0.96f, 0.045f),
                "Juego de entretenimiento y aprendizaje; no constituye asesoría legal. Incluye material del SRD 5.2 (CC-BY-4.0).", 22, new Color(1, 1, 1, 0.55f));
        }

        private async Task VerPrologo()
        {
            PlayerPrefs.SetInt(ClavePrologo, 1);
            foreach (var l in Guion.Prologo) await Dialogo(l.Personaje, l.Texto);
            _saltarDialogos = false;
        }

        private void MostrarPreparacion(string? aviso = null)
        {
            Ui.Limpiar(_raiz);
            Ui.Imagen(Ui.Vacio(_raiz, "Fondo").Anclar(0, 0, 1, 1), "Arte", Arte.Sprite("fondo-mesa"), false).color = new Color(1, 1, 1, 0.18f);
            Ui.Texto(Ui.Vacio(_raiz, "Titulo").Anclar(0.05f, 0.92f, 0.95f, 0.98f), "NUEVO EXPEDIENTE", 60, Ui.Dorado, TextAnchor.MiddleCenter, FontStyle.Bold).ConSombra();

            Ui.Texto(Ui.Vacio(_raiz, "EtiquetaAlias").Anclar(0.05f, 0.875f, 0.95f, 0.91f), "Tu nombre en el juego", 32, Ui.Dorado, TextAnchor.LowerLeft);
            var alias = Ui.Campo(Ui.Vacio(_raiz, "Alias").Anclar(0.05f, 0.82f, 0.95f, 0.87f), _alias, "Doctor(a)...");
            alias.onEndEdit.AddListener(v => { _alias = string.IsNullOrWhiteSpace(v) ? "Doctor(a)" : v.Trim(); PlayerPrefs.SetString("eyd.alias", _alias); });

            Ui.Texto(Ui.Vacio(_raiz, "EtiquetaEsp").Anclar(0.05f, 0.775f, 0.95f, 0.81f), "Especialidad (la otra la lleva tu colega bot)", 32, Ui.Dorado, TextAnchor.LowerLeft);
            var esp = Ui.Vacio(_raiz, "Esp").Anclar(0.05f, 0.60f, 0.95f, 0.77f);
            TarjetaEspecialidad(esp, 0, "Civilista", "civilista", "Control: Medida cautelar\ny Nulidad");
            TarjetaEspecialidad(esp, 1, "Administrativista", "administrativista", "Apoyo: Queja, Apelación\ny Silencio administrativo");

            Ui.Texto(Ui.Vacio(_raiz, "EtiquetaCaso").Anclar(0.05f, 0.555f, 0.95f, 0.59f), "Caso", 32, Ui.Dorado, TextAnchor.LowerLeft);
            var casos = CatalogoCasos.Todos.ToList();
            var zonaCasos = Ui.Vacio(_raiz, "Casos").Anclar(0.05f, 0.45f, 0.95f, 0.55f);
            for (var i = 0; i < casos.Count; i++)
            {
                var c = casos[i];
                var activo = _caso == c.Id;
                Ui.Boton(Ui.Vacio(zonaCasos, c.Id).Anclar((float)i / casos.Count, 0, (float)(i + 1) / casos.Count, 1, 6), $"{c.Numero}. {c.Titulo}",
                    () => { _caso = c.Id; MostrarPreparacion(); }, activo ? Ui.Dorado : Ui.Tinta, activo ? Ui.Noche : Ui.Pergamino, 32);
            }
            var caso = CatalogoCasos.Buscar(_caso);
            var detalle = Ui.PanelRedondeado(_raiz, "Premisa", new Color(0, 0, 0, 0.45f)).Anclar(0.05f, 0.355f, 0.95f, 0.44f);
            Ui.Texto(detalle, $"{caso.Premisa}\nCalabozo: {caso.Calabozo} · Jefe: {caso.Jefe}{(caso.MinutosDePlazo.HasValue ? $" · Reloj: {caso.MinutosDePlazo / 60} h" : "")}",
                30, Ui.Pergamino, TextAnchor.MiddleLeft, FontStyle.Italic);

            Selector(Ui.Vacio(_raiz, "Modo").Anclar(0.05f, 0.285f, 0.95f, 0.34f), new[] { ("local", "Sin conexión"), ("linea", "En línea") },
                () => _enLinea ? "linea" : "local", v => { _enLinea = v == "linea"; PlayerPrefs.SetInt("eyd.enlinea", _enLinea ? 1 : 0); });
            if (_enLinea)
            {
                var url = Ui.Campo(Ui.Vacio(_raiz, "Url").Anclar(0.05f, 0.225f, 0.95f, 0.275f), _url, "http://servidor:8080");
                url.onEndEdit.AddListener(v => { _url = v.Trim(); PlayerPrefs.SetString("eyd.url", _url); });
            }
            else
                Selector(Ui.Vacio(_raiz, "Borr").Anclar(0.05f, 0.225f, 0.95f, 0.275f), new[] { ("no", "Solo preguntas verificadas"), ("si", "Incluir borradores (pruebas)") },
                    () => _borradores ? "si" : "no", v => _borradores = v == "si");

            Ui.Boton(Ui.Vacio(_raiz, "Jugar").Anclar(0.12f, 0.115f, 0.88f, 0.205f), "ABRIR EXPEDIENTE", () => _ = Empezar(), Ui.Carmesi, Ui.Pergamino, 52);
            Ui.Boton(Ui.Vacio(_raiz, "Volver").Anclar(0.3f, 0.04f, 0.7f, 0.095f), "Volver", () => MostrarMenu(), Ui.Tinta, Ui.Pergamino, 32);
            if (aviso != null) Ui.Texto(Ui.PanelRedondeado(_raiz, "Aviso", Ui.Carmesi).Anclar(0.05f, 0.0f, 0.95f, 0.04f), aviso, 26, Ui.Pergamino);
        }

        private void TarjetaEspecialidad(RectTransform zona, int i, string valor, string dibujo, string texto)
        {
            var activo = _especialidad == valor;
            var b = Ui.Boton(Ui.Vacio(zona, valor).Anclar(i * 0.5f, 0, i * 0.5f + 0.5f, 1, 6), "", () => { _especialidad = valor; MostrarPreparacion(); },
                activo ? Ui.Dorado : Ui.Tinta, Ui.Pergamino, 30);
            Ui.Imagen(Ui.Vacio(b.transform, "Ficha").Anclar(0.03f, 0.08f, 0.42f, 0.92f), "Dibujo", Arte.Sprite(dibujo));
            Ui.Texto(Ui.Vacio(b.transform, "Nombre").Anclar(0.42f, 0.55f, 1f, 0.95f), valor, 34, activo ? Ui.Noche : Ui.Dorado, TextAnchor.MiddleLeft, FontStyle.Bold);
            Ui.Texto(Ui.Vacio(b.transform, "Rol").Anclar(0.42f, 0.05f, 1f, 0.55f), texto, 24, activo ? Ui.Noche : Ui.Pergamino, TextAnchor.UpperLeft);
        }

        private void Selector(RectTransform zona, (string valor, string texto)[] opciones, Func<string> actual, Action<string> elegir)
        {
            for (var i = 0; i < opciones.Length; i++)
            {
                var (valor, texto) = opciones[i];
                var activo = actual() == valor;
                Ui.Boton(Ui.Vacio(zona, valor).Anclar((float)i / opciones.Length, 0, (float)(i + 1) / opciones.Length, 1, 6), texto,
                    () => { elegir(valor); MostrarPreparacion(); }, activo ? Ui.Dorado : Ui.Tinta, activo ? Ui.Noche : Ui.Pergamino, 32);
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
                Sonido.Musica(false);
                ConstruirPartida();
                Anotar($"Caso: {r.Estado.Premisa}");
                await Aplicar(r);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                if (_estado == null && !_enLinea) { MostrarMenu(ex.Message); return; }
                MostrarPreparacion(_enLinea ? $"No se pudo conectar con {_url}. Revisa la red o juega sin conexión." : ex.Message);
            }
            finally { _ocupado = false; }
        }

        // ───────────────────────────── Partida ─────────────────────────────

        private void ConstruirPartida()
        {
            Ui.Limpiar(_raiz);
            _fichas.Clear();
            _ofertaPintada = "";
            var cab = Ui.Panel(_raiz, "Cabecera", Ui.Tinta).Anclar(0, 0.92f, 1, 1);
            _cabecera = Ui.Texto(Ui.Vacio(cab, "T").Anclar(0.02f, 0.45f, 0.98f, 1), "", 42, Ui.Dorado, TextAnchor.MiddleLeft, FontStyle.Bold);
            _subcabecera = Ui.Texto(Ui.Vacio(cab, "S").Anclar(0.02f, 0, 0.98f, 0.5f), "", 28, Ui.Pergamino, TextAnchor.MiddleLeft);

            _tablero = Ui.PanelRedondeado(_raiz, "Tablero", Ui.Tinta).Anclar(0.02f, 0.47f, 0.98f, 0.915f);
            _tablero.gameObject.AddComponent<RectMask2D>();
            _fondoTablero = Ui.Imagen(_tablero, "Fondo", null, false);
            _rejilla = Ui.Vacio(_tablero, "Rejilla").Anclar(0, 0, 1, 1);
            for (var x = 0; x < Columnas; x++)
                for (var y = 0; y < Filas; y++)
                {
                    int cx = x, cy = y;
                    var celda = Ui.Vacio(_rejilla, $"C{x}{y}");
                    AnclarCasilla(celda, x, y, 3);
                    var b = Ui.Boton(celda, "", () => TocarCasilla(cx, cy), new Color(1, 1, 1, 0.06f), Ui.Pergamino, 20);
                    _casillas[x, y] = (Image)b.targetGraphic;
                }
            _capaFichas = Ui.Vacio(_tablero, "Fichas").Anclar(0, 0, 1, 1);

            var sala = Ui.PanelRedondeado(_raiz, "Sala", Ui.Tinta).Anclar(0.02f, 0.37f, 0.98f, 0.465f);
            _textoSala = Ui.Texto(sala, "", 32, Ui.Pergamino, TextAnchor.MiddleLeft);
            var bit = Ui.PanelRedondeado(_raiz, "Bitacora", new Color(0, 0, 0, 0.35f)).Anclar(0.02f, 0.235f, 0.98f, 0.365f);
            _textoBitacora = Ui.Texto(bit, "", 26, Ui.Pergamino, TextAnchor.LowerLeft);
            _textoBitacora.resizeTextForBestFit = false;
            _barra = Ui.Vacio(_raiz, "Barra").Anclar(0.02f, 0.01f, 0.98f, 0.23f);
            _capa = Ui.Vacio(_raiz, "Capa").Anclar(0, 0, 1, 1);
        }

        private static void AnclarCasilla(RectTransform r, int x, int y, float margen = 0) =>
            r.Anclar((float)x / Columnas, 1f - (float)(y + 1) / Filas, (float)(x + 1) / Columnas, 1f - (float)y / Filas, margen);

        private bool EsMiTurno =>
            _estado?.Combate is { Terminado: false } c && _fuente != null &&
            _estado.Equipo.Any(m => m.Id == c.TurnoDe && m.JugadorId == _fuente.JugadorId);

        private CombatienteEstado? Yo =>
            _estado == null || _fuente == null ? null : _estado.Equipo.FirstOrDefault(m => m.JugadorId == _fuente.JugadorId);

        private CombatienteEstado? YoEnCombate =>
            Yo == null ? null : _estado?.Combate?.Combatientes.FirstOrDefault(c => c.Id == Yo.Id) ?? Yo;

        private void Pintar()
        {
            if (_estado == null) return;
            var e = _estado;
            _cabecera.text = e.Titulo;
            var reloj = e.MinutosRestantes.HasValue ? $" · reloj {e.MinutosRestantes / 60}:{e.MinutosRestantes % 60:00}" : "";
            _subcabecera.text = $"Sala {Math.Max(1, e.IndiceSala + 1)} de {e.Salas.Count}: {NombreSala(e.SalaActual)}{reloj} · reputación {e.Estudio.Reputacion} · S/ {e.HonorariosDisponibles} · pruebas {e.PruebasJuridicasRestantes}";

            var enCombate = e.Combate != null;
            _fondoTablero.sprite = Arte.Escena(enCombate ? "Combate" : e.SalaActual, e.CasoId);
            _fondoTablero.color = _fondoTablero.sprite == null ? Color.clear
                : e.SalaActual == "Jefe" ? new Color(1f, 0.78f, 0.78f) : e.SalaActual == "RedDelCoimero" ? new Color(0.75f, 0.7f, 0.9f) : Color.white;
            _rejilla.gameObject.SetActive(enCombate);

            var porCasilla = new Dictionary<(int, int), CombatienteEstado>();
            if (e.Combate != null) foreach (var c in e.Combate.Combatientes.Where(c => !c.Caido)) porCasilla[(c.Columna, c.Fila)] = c;
            var validas = new HashSet<(int, int)>(EsMiTurno && e.Combate != null ? e.Combate.CasillasValidas.Select(v => (v[0], v[1])) : Enumerable.Empty<(int, int)>());
            var yo = YoEnCombate;
            for (var x = 0; x < Columnas; x++)
                for (var y = 0; y < Filas; y++)
                {
                    var img = _casillas[x, y];
                    if (porCasilla.TryGetValue((x, y), out var c))
                    {
                        var esTurno = e.Combate!.TurnoDe == c.Id;
                        var alAlcance = EsMiTurno && yo != null && c.Bando == "Sistema" && Math.Max(Math.Abs(c.Columna - yo.Columna), Math.Abs(c.Fila - yo.Fila)) <= AlcanceDe(yo);
                        img.color = c.Id == _objetivo ? new Color(0.78f, 0.27f, 0.18f, 0.75f)
                            : esTurno ? new Color(0.89f, 0.69f, 0.29f, 0.6f)
                            : alAlcance ? new Color(0.78f, 0.27f, 0.18f, 0.3f)
                            : new Color(1, 1, 1, 0.06f);
                    }
                    else img.color = validas.Contains((x, y)) ? new Color(0.37f, 0.62f, 0.31f, 0.55f) : new Color(1, 1, 1, 0.06f);
                }
            SincronizarFichas(e);

            _textoSala.text = DescripcionSala(e);
            _textoBitacora.text = string.Join("\n", _bitacora.Skip(Math.Max(0, _bitacora.Count - 7)));
            PintarBarra();
            PintarMejoras(e);
            if (e.Sentencia != null) MostrarSentencia(e);
        }

        private static int AlcanceDe(CombatienteEstado c) => c.Alcance > 0 ? c.Alcance : c.Especialidad == "Administrativista" ? 3 : c.Especialidad == "Civilista" ? 2 : 1;

        // ── Mejoras: al ganar cada combate el equipo elige 1 de 3 (el gancho de cada partida) ──

        private string _ofertaPintada = "";

        private void PintarMejoras(EstadoPartida e)
        {
            var clave = string.Join(",", e.MejorasOfrecidas.Select(m => m.Id));
            var actual = _capa.Find("Mejoras");
            if (clave == _ofertaPintada && (actual != null) == (clave.Length > 0)) return;
            if (actual != null) Destroy(actual.gameObject);
            _ofertaPintada = clave;
            if (clave.Length == 0 || e.Sentencia != null) return;

            Sonido.Tocar("fundado", 1.15f, 0.6f);
            var fondo = Ui.Panel(_capa, "Mejoras", new Color(0, 0, 0, 0.78f)).Anclar(0, 0, 1, 1);
            var titulo = e.IndiceSala <= 0 && !e.SalaResuelta ? "MALETÍN DEL ESTUDIO" : "¡SALA GANADA!";
            Ui.Texto(Ui.Vacio(fondo, "T").Anclar(0.04f, 0.83f, 0.96f, 0.92f), titulo, 72, Ui.Dorado, TextAnchor.MiddleCenter, FontStyle.Bold).ConSombra(4);
            Ui.Texto(Ui.Vacio(fondo, "S").Anclar(0.04f, 0.77f, 0.96f, 0.83f), "Elige una mejora para todo el equipo", 40, Ui.Pergamino, TextAnchor.MiddleCenter, FontStyle.Italic);
            for (var i = 0; i < e.MejorasOfrecidas.Count; i++)
            {
                var m = e.MejorasOfrecidas[i];
                var indice = i;
                var arriba = 0.74f - i * 0.19f;
                var carta = Ui.Boton(Ui.Vacio(fondo, "M" + i).Anclar(0.06f, arriba - 0.17f, 0.94f, arriba, 4), "",
                    () => Enviar(new AccionJuego { Tipo = "mejora", Indice = indice }), m.Rara ? Ui.Dorado : Ui.Pergamino, Ui.Noche, 30);
                var tinta = m.Rara ? Ui.Noche : Ui.Carmesi;
                Ui.Texto(Ui.Vacio(carta.transform, "N").Anclar(0.05f, 0.52f, 0.95f, 0.95f), (m.Rara ? "★ " : "") + m.Nombre.ToUpperInvariant(), 46, tinta, TextAnchor.MiddleLeft, FontStyle.Bold);
                Ui.Texto(Ui.Vacio(carta.transform, "D").Anclar(0.05f, 0.06f, 0.95f, 0.52f), m.Descripcion, 32, Ui.Noche, TextAnchor.UpperLeft);
                StartCoroutine(Aparecer((RectTransform)carta.transform));
            }
            if (e.PuedeVolverASortear)
                Ui.Boton(Ui.Vacio(fondo, "R").Anclar(0.25f, 0.06f, 0.75f, 0.13f, 4), "Segunda opinión (resortear)", () => Enviar(new AccionJuego { Tipo = "resortear" }), Ui.Tinta, Ui.Pergamino, 32);
            if (e.MejorasTomadas.Count > 0)
                Ui.Texto(Ui.Vacio(fondo, "Ya").Anclar(0.04f, 0.0f, 0.96f, 0.06f), "Ya tienen: " + string.Join(" · ", e.MejorasTomadas.Select(t => t.Nombre)), 26, Ui.Pergamino);
        }

        // ── Fichas del tablero: cada combatiente es un dibujo que se anima ──

        private void SincronizarFichas(EstadoPartida e)
        {
            var vivos = e.Combate?.Combatientes.Where(c => !c.Caido).ToList() ?? new List<CombatienteEstado>();
            foreach (var id in _fichas.Keys.Where(k => vivos.All(v => v.Id != k)).ToList())
            {
                if (_fichas[id].Raiz != null) Destroy(_fichas[id].Raiz.gameObject);
                _fichas.Remove(id);
            }
            foreach (var c in vivos)
            {
                var nueva = !_fichas.TryGetValue(c.Id, out var f);
                if (nueva) { f = CrearFicha(c); _fichas[c.Id] = f; }
                f!.Columna = c.Columna; f.Fila = c.Fila; f.Maxima = c.PacienciaMaxima;
                AnclarCasilla(f.Raiz, c.Columna, c.Fila, 4);
                f.Raiz.localScale = Vector3.one;
                f.Raiz.localRotation = Quaternion.identity;
                f.Dibujo.color = c.TurnosInmovilizado > 0 ? new Color(0.65f, 0.75f, 1f) : Color.white;
                PonerVida(f, c.Paciencia);
                var insignia = c.TurnosParaVencer.HasValue ? $"Vence en {c.TurnosParaVencer}" : c.TurnosInmovilizado > 0 ? $"Inmóvil {c.TurnosInmovilizado}" : "";
                if (c.Id == "jefe" && e.Combate?.Requisito != null) insignia = c.TurnosParaVencer.HasValue ? insignia : NombreRequisito(e.Combate.Requisito);
                f.Insignia.text = insignia;
                f.InsigniaFondo.SetActive(insignia.Length > 0);
                if (nueva) StartCoroutine(Aparecer(f.Raiz));
            }
        }

        private static string NombreRequisito(string r) => r switch { "EnPersona" => "En persona", "Virtual" => "Virtual", _ => "Con constancia" };

        private FichaVisual CrearFicha(CombatienteEstado c)
        {
            var f = new FichaVisual { Raiz = Ui.Vacio(_capaFichas, "F-" + c.Id) };
            f.Dibujo = Ui.Imagen(Ui.Vacio(f.Raiz, "Dibujo").Anclar(0.04f, 0.14f, 0.96f, 1f), "Img", Arte.Ficha(c.Clave, c.Especialidad));
            var barra = Ui.PanelRedondeado(f.Raiz, "VidaFondo", new Color(0, 0, 0, 0.7f)).Anclar(0.1f, 0.02f, 0.9f, 0.12f);
            barra.GetComponent<Image>().raycastTarget = false;
            f.Vida = Ui.PanelRedondeado(barra, "Vida", Color.green).Anclar(0, 0, 1, 1, 2);
            f.VidaColor = f.Vida.GetComponent<Image>();
            f.VidaColor.raycastTarget = false;
            var ins = Ui.PanelRedondeado(f.Raiz, "Insignia", new Color(0.05f, 0.05f, 0.1f, 0.85f)).Anclar(0f, 0.82f, 1f, 1f);
            ins.GetComponent<Image>().raycastTarget = false;
            f.InsigniaFondo = ins.gameObject;
            f.Insignia = Ui.Texto(ins, "", 20, Ui.Dorado, TextAnchor.MiddleCenter, FontStyle.Bold);
            return f;
        }

        private static void PonerVida(FichaVisual f, int paciencia)
        {
            var p = f.Maxima <= 0 ? 0 : Mathf.Clamp01(paciencia / (float)f.Maxima);
            f.Vida.anchorMax = new Vector2(Mathf.Max(0.02f, p), 1);
            f.VidaColor.color = p > 0.5f ? new Color(0.4f, 0.75f, 0.35f) : p > 0.25f ? Ui.Dorado : Ui.Carmesi;
        }

        private static IEnumerator Aparecer(RectTransform r)
        {
            for (var t = 0f; t < 0.25f && r != null; t += Time.deltaTime)
            {
                r.localScale = Vector3.one * Mathf.SmoothStep(0.2f, 1f, t / 0.25f);
                yield return null;
            }
            if (r != null) r.localScale = Vector3.one;
        }

        private static string NombreSala(string? s) => s switch
        {
            "Combate" => "combate", "Tramite" => "trámite", "Audiencia" => "audiencia", "Libreria" => "librería jurídica",
            "Cafeteria" => "cafetería del juzgado", "Coimero" => "pasillo", "RedDelCoimero" => "la red del Coimero", "Jefe" => "jefe",
            _ => "—",
        };

        private string DescripcionSala(EstadoPartida e)
        {
            if (e.SalaResuelta) return "Sala superada. Don Eusebio: «Sigan, doctores, que el expediente no se mueve solo.»";
            var trama = string.IsNullOrEmpty(e.TextoSala) ? "" : e.TextoSala;
            if (e.Combate == null) return trama;
            var turno = e.Combate.Combatientes.FirstOrDefault(c => c.Id == e.Combate.TurnoDe);
            var requisito = e.Combate.RequisitoDescripcion != null ? $"\nRequisito del jefe: {e.Combate.RequisitoDescripcion}" : "";
            var instruccion = EsMiTurno
                ? $"Tu turno (ronda {e.Combate.Ronda}): verde = moverte ({e.Combate.MovimientoRestante}); toca un enemigo para elegirlo."
                : $"Turno de {turno?.Nombre}…";
            return instruccion + requisito;
        }

        private void PintarBarra()
        {
            Ui.Limpiar(_barra);
            var e = _estado!;
            var botones = new List<(string texto, Action accion, Color fondo)>();
            void B(string t, Action a, Color? f = null) => botones.Add((t, a, f ?? Ui.Noche));
            AccionJuego A(string tipo) => new AccionJuego { Tipo = tipo };

            if (e.Sentencia != null) { }
            else if (e.SalaResuelta && e.MejorasOfrecidas.Count > 0) B("ELEGIR MEJORA", () => { _ofertaPintada = ""; PintarMejoras(e); }, Ui.Dorado);
            else if (e.SalaResuelta) B("SIGUIENTE SALA ▶", () => Enviar(A("avanzar")), Ui.Carmesi);
            else switch (e.SalaActual)
            {
                case "Combate":
                case "RedDelCoimero":
                case "Jefe":
                    var yo = YoEnCombate;
                    if (EsMiTurno)
                    {
                        var actual = e.Combate!.Combatientes.First(c => c.Id == e.Combate.TurnoDe);
                        if (e.Combate.AccionDisponible)
                        {
                            B(_objetivo == null ? "Atacar\n(elige enemigo)" : "ATACAR", () => { if (_objetivo != null) Enviar(new AccionJuego { Tipo = "atacar", Objetivo = _objetivo }); else Anotar("Toca primero a un enemigo."); }, Ui.Carmesi);
                            var hab = actual.Especialidad == "Civilista" ? ("MedidaCautelar", "Medida\ncautelar") : ("Queja", "Queja");
                            var recarga = actual.RecargaHabilidad > 0 ? $"\n(en {actual.RecargaHabilidad})" : "";
                            B(hab.Item2 + recarga, () => { if (_objetivo != null) Enviar(new AccionJuego { Tipo = "habilidad", Habilidad = hab.Item1, Objetivo = _objetivo }); else Anotar("Toca primero a un enemigo."); },
                                actual.RecargaHabilidad > 0 ? Ui.Tinta : Ui.Noche);
                            if (e.TieneEscritoPreFirmado && _objetivo != null) B("Atacar con escrito\npre-firmado", () => Enviar(new AccionJuego { Tipo = "atacar", Objetivo = _objetivo, Opcion = "escrito" }));
                        }
                        B($"Prueba\njurídica ({e.PruebasJuridicasRestantes})", () => Enviar(A("pedirPregunta")));
                        if (!e.Combate.CasacionUsada) B("Casación", () => Enviar(new AccionJuego { Tipo = "casacion" }));
                        B("Terminar\nturno", () => Enviar(A("terminarTurno")), Ui.Tinta);
                        B("Piloto\nautomático", () => Enviar(A("piloto")), Ui.Tinta);
                    }
                    if (yo != null && yo.Especialidad == "Civilista" && e.Combate!.NulidadPosible && yo.ReaccionDisponible && yo.RecargaNulidad == 0)
                        B("Nulidad\n(anula el golpe)", () => Enviar(A("nulidad")), Ui.Hex("#2F6B4A"));
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
                    B($"Escrito pre-firmado\nS/ {Expediente.CostoEscritoPreFirmado}", () => Comprar("EscritoPreFirmado"));
                    B($"Café doble\nS/ {Expediente.CostoCafeDoble}", () => Comprar("CafeDoble"));
                    B($"Código anotado\nS/ {Expediente.CostoCodigoAnotado}", () => Comprar("CodigoAnotado"));
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
                Ui.Boton(zona, texto, () => accion(), fondo, Ui.Pergamino, 32);
            }
        }

        private void Comprar(string objeto)
        {
            Sonido.Tocar("moneda");
            Enviar(new AccionJuego { Tipo = "comprar", Objeto = objeto });
        }

        private static string Corto(string n) => n.Length <= 12 ? n : n.Substring(0, 11) + "…";

        private void TocarCasilla(int x, int y)
        {
            if (_estado?.Combate == null || _ocupado) return;
            var c = _estado.Combate.Combatientes.FirstOrDefault(k => !k.Caido && k.Columna == x && k.Fila == y);
            if (c != null)
            {
                if (c.Bando == "Sistema") { _objetivo = c.Id; Anotar($"Objetivo: {c.Nombre} (paciencia {c.Paciencia}, defensa {c.Defensa}){Rasgo(c.Rasgo)}."); Pintar(); }
                else Anotar($"{c.Nombre}: paciencia {c.Paciencia}/{c.PacienciaMaxima}.");
                return;
            }
            if (EsMiTurno && _estado.Combate.CasillasValidas.Any(v => v[0] == x && v[1] == y))
                Enviar(new AccionJuego { Tipo = "mover", Columna = x, Fila = y });
        }

        private static string Rasgo(string r) => r switch
        {
            "Vence" => ". Si no cae en 3 turnos, vence y golpea a quien esté cerca; inmovilizado, su plazo se suspende",
            "NotificaAlAzar" => ". Ataca a quien no corresponde",
            "ExigeRequisito" => ". Sin el requisito correcto (ventaja) recibe la mitad; inmovilízalo y golpéalo con ventaja",
            "Empuja" => ". Te devuelve una casilla atrás: «vuelva mañana»",
            "SeFracciona" => ". Cada golpe que aguanta lo parte en una copia",
            _ => "",
        };

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
            // Comodidad: si ya no queda acción ni movimiento, el turno se termina solo.
            if (EsMiTurno && _estado?.Combate is { AccionDisponible: false, MovimientoRestante: 0 } && a.Tipo != "terminarTurno")
                Enviar(new AccionJuego { Tipo = "terminarTurno" });
        }

        private async Task Aplicar(RespuestaAccion r)
        {
            if (_estado?.SalaActual != r.Estado.SalaActual || _estado?.IndiceSala != r.Estado.IndiceSala) _objetivo = null;
            _posicionesViejas = false;
            foreach (var ev in r.Eventos) await Narrar(ev);
            _saltarDialogos = false;
            _estado = r.Estado;
            if (_objetivo != null && (_estado.Combate == null || _estado.Combate.Combatientes.All(c => c.Id != _objetivo || c.Caido))) _objetivo = null;
            if (r.Extra?.Correccion != null) MostrarCorreccion(r.Extra.Correccion);
            Pintar();
        }

        private string Nombre(string id) =>
            _estado?.Combate?.Combatientes.FirstOrDefault(c => c.Id == id)?.Nombre
            ?? _estado?.Equipo.FirstOrDefault(c => c.Id == id)?.Nombre ?? id;

        private FichaVisual? Ficha(string id) => !_posicionesViejas && _fichas.TryGetValue(id, out var f) && f.Raiz != null ? f : null;

        private async Task Narrar(EventoJuego ev)
        {
            var d = Json.Datos(ev);
            string S(string k) => (string?)d[k] ?? "";
            int N(string k) => (int?)d[k] ?? 0;
            switch (ev.Tipo)
            {
                case "Ataco":
                {
                    var momento = S("momento");
                    Anotar($"{Nombre(S("atacante"))} → {Nombre(S("objetivo"))}: d20 = {N("natural")} (total {N("total")}) {((bool?)d["acierto"] == true ? "acierta" : "falla")}");
                    var mio = _estado?.Equipo.Any(m => m.Id == S("atacante") && m.JugadorId == _fuente?.JugadorId) == true;
                    await Embestir(S("atacante"), S("objetivo"));
                    if (mio || momento != "ninguno") await Dado(N("natural"), momento);
                    break;
                }
                case "RecibioDanio":
                    Anotar($"   {Nombre(S("id"))} pierde {N("cantidad")} de paciencia.");
                    await Golpe(S("id"), N("cantidad"), N("paciencia_restante"));
                    break;
                case "Recupero":
                    Anotar($"   {Nombre(S("id"))} recupera {N("cantidad")} de paciencia.");
                    Sonido.Tocar("sanar");
                    Flotar(S("id"), $"+{N("cantidad")}", new Color(0.5f, 0.9f, 0.5f));
                    if (Ficha(S("id")) is { } fr) PonerVida(fr, N("paciencia_restante"));
                    await Esperar(0.3f);
                    break;
                case "Cayo":
                    Anotar($"   {Nombre(S("id"))} cae.");
                    await Desvanecer(S("id"));
                    break;
                case "Movio":
                    await Deslizar(S("id"), d["hasta"]);
                    break;
                case "Empujado":
                    Anotar($"   «Vuelva mañana»: {Nombre(S("id"))} retrocede una casilla.");
                    Flotar(S("id"), "¡Vuelva mañana!", Ui.Pergamino);
                    await Deslizar(S("id"), d["hasta"]);
                    break;
                case "UsoHabilidad":
                {
                    var h = S("habilidad");
                    var nombreHab = h switch { "medidaCautelar" => "Medida cautelar", "recursoDeApelacion" => "Recurso de apelación", "queja" => "Queja", "casacion" => "Casación", "nulidad" => "Nulidad", _ => h };
                    Anotar($"{Nombre(S("id"))} usa {nombreHab}.");
                    Sonido.Tocar(h == "nulidad" ? "campana" : "sello", 1.2f, 0.6f);
                    Flotar(S("id"), nombreHab, Ui.Dorado);
                    await Esperar(0.35f);
                    break;
                }
                case "Inmovilizado":
                    Anotar($"   {Nombre(S("id"))} queda inmovilizado {N("turnos")} turnos.");
                    Flotar(S("id"), "Inmovilizado", new Color(0.65f, 0.75f, 1f));
                    break;
                case "Observado":
                    Anotar($"   {S("motivo")}");
                    Sonido.Tocar("sello", 0.8f, 0.5f);
                    Flotar(S("id"), "OBSERVADO", Ui.Carmesi);
                    await Esperar(0.4f);
                    break;
                case "Vencio":
                    Anotar($"¡{Nombre(S("id"))} venció! Golpea a todos los que estaban cerca.");
                    Sonido.Tocar("explosion");
                    Flotar(S("id"), "¡VENCIÓ!", new Color(1f, 0.6f, 0.2f));
                    await Explotar(S("id"));
                    break;
                case "RequisitoCambio":
                    Anotar($"El Requisito Imposible cambia: {S("descripcion")}");
                    Sonido.Tocar("campana", 0.7f, 0.6f);
                    break;
                case "Aparecio": Anotar($"Aparece: {S("nombre")}."); break;
                case "FinDeCombate": Anotar($"— {S("motivo")} —"); break;
                case "SalaIniciada":
                    _posicionesViejas = true;
                    Sonido.Tocar("campana", 1f, 0.4f);
                    Anotar($"Entran a la sala {N("indice") + 1}: {NombreSala(Capital(S("tipo")))}.");
                    break;
                case "SalaSuperada": Anotar($"✓ {S("detalle")}"); break;
                case "ComentarioDeDonEusebio": await Dialogo("Don Eusebio", S("texto")); break;
                case "Dialogo": await Dialogo(S("personaje"), S("texto")); break;
                case "MejorasOfrecidas":
                    Anotar("¡Combate ganado! Elijan una mejora.");
                    break;
                case "MejoraElegida":
                    Anotar($"★ Mejora: {S("nombre")}. {S("descripcion")}");
                    Sonido.Tocar("moneda", 1.1f, 0.7f);
                    Efectos.Chispas(Vector2.zero, Ui.Dorado, 24);
                    Efectos.Texto(Vector2.zero, S("nombre"), Ui.Dorado);
                    await Esperar(0.4f);
                    break;
                case "DesbloqueoGanado":
                    Efectos.Confeti(90);
                    await Dialogo(Guion.Gonzabio, $"¡El estudio crece! Desbloqueaste «{S("nombre")}»: {S("descripcion")}");
                    break;
                case "SentenciaDictada":
                    Sonido.Tocar((bool?)d["sentencia"]?["fundada"] == true ? "victoria" : "derrota");
                    break;
            }
        }

        private static string Capital(string s) => s.Length == 0 ? s : char.ToUpperInvariant(s[0]) + s.Substring(1);

        private void Anotar(string s)
        {
            _bitacora.Add(s);
            if (_bitacora.Count > 60) _bitacora.RemoveRange(0, _bitacora.Count - 60);
            if (_textoBitacora != null) _textoBitacora.text = string.Join("\n", _bitacora.Skip(Math.Max(0, _bitacora.Count - 7)));
        }

        // ───────────────────────────── Animaciones ─────────────────────────────

        private Task Correr(IEnumerator rutina)
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(Envolver(rutina, tcs));
            return tcs.Task;
        }

        private static IEnumerator Envolver(IEnumerator rutina, TaskCompletionSource<bool> tcs)
        {
            yield return rutina;
            tcs.TrySetResult(true);
        }

        private Task Esperar(float segundos) => Correr(EsperarRutina(segundos));
        private static IEnumerator EsperarRutina(float s) { yield return new WaitForSeconds(s); }

        private Task Deslizar(string id, JToken? hasta)
        {
            var f = Ficha(id);
            if (f == null || hasta == null) return Task.CompletedTask;
            int x = (int?)hasta["columna"] ?? f.Columna, y = (int?)hasta["fila"] ?? f.Fila;
            return Correr(DeslizarRutina(f, x, y));
        }

        private static IEnumerator DeslizarRutina(FichaVisual f, int x, int y)
        {
            Sonido.Tocar("paso", 0.9f, 0.5f);
            Vector2 min0 = f.Raiz.anchorMin, max0 = f.Raiz.anchorMax;
            var min1 = new Vector2((float)x / Columnas, 1f - (float)(y + 1) / Filas);
            var max1 = new Vector2((float)(x + 1) / Columnas, 1f - (float)y / Filas);
            for (var t = 0f; t < 0.22f && f.Raiz != null; t += Time.deltaTime)
            {
                var k = Mathf.SmoothStep(0, 1, t / 0.22f);
                f.Raiz.anchorMin = Vector2.Lerp(min0, min1, k);
                f.Raiz.anchorMax = Vector2.Lerp(max0, max1, k);
                yield return null;
            }
            if (f.Raiz == null) yield break;
            f.Raiz.anchorMin = min1; f.Raiz.anchorMax = max1;
            f.Columna = x; f.Fila = y;
        }

        private Task Embestir(string atacante, string objetivo)
        {
            var a = Ficha(atacante);
            var o = Ficha(objetivo);
            if (a == null || o == null) return Task.CompletedTask;
            return Correr(EmbestirRutina(a, o));
        }

        private static IEnumerator EmbestirRutina(FichaVisual a, FichaVisual o)
        {
            var dir = new Vector2(o.Columna - a.Columna, a.Fila - o.Fila).normalized * 26f;
            for (var t = 0f; t < 0.16f && a.Raiz != null; t += Time.deltaTime)
            {
                a.Raiz.anchoredPosition = dir * Mathf.Sin(Mathf.PI * t / 0.16f);
                yield return null;
            }
            if (a.Raiz != null) a.Raiz.anchoredPosition = Vector2.zero;
        }

        private Task Golpe(string id, int cantidad, int restante)
        {
            Sonido.Tocar("golpe", UnityEngine.Random.Range(0.9f, 1.1f));
            Flotar(id, $"-{cantidad}", new Color(1f, 0.45f, 0.4f));
            var f = Ficha(id);
            if (f == null) return Esperar(0.15f);
            PonerVida(f, restante);
            return Correr(SacudirRutina(f));
        }

        private static IEnumerator SacudirRutina(FichaVisual f)
        {
            var original = f.Dibujo.color;
            for (var t = 0f; t < 0.3f && f.Raiz != null; t += Time.deltaTime)
            {
                f.Raiz.anchoredPosition = new Vector2(Mathf.Sin(t * 70) * 10 * (1 - t / 0.3f), 0);
                f.Dibujo.color = Color.Lerp(new Color(1, 0.35f, 0.35f), original, t / 0.3f);
                yield return null;
            }
            if (f.Raiz == null) yield break;
            f.Raiz.anchoredPosition = Vector2.zero;
            f.Dibujo.color = original;
        }

        private Task Desvanecer(string id)
        {
            var f = Ficha(id);
            if (f == null) return Task.CompletedTask;
            _fichas.Remove(id);
            return Correr(DesvanecerRutina(f));
        }

        private IEnumerator DesvanecerRutina(FichaVisual f)
        {
            if (f.Raiz == null) yield break;
            Efectos.Papelitos(Efectos.Centro(f.Raiz));
            var grupo = f.Raiz.gameObject.AddComponent<CanvasGroup>();
            for (var t = 0f; t < 0.3f && f.Raiz != null; t += Time.deltaTime)
            {
                grupo.alpha = 1 - t / 0.3f;
                f.Raiz.localScale = Vector3.one * (1 - 0.3f * t / 0.3f);
                f.Raiz.localRotation = Quaternion.Euler(0, 0, 40 * t / 0.3f);
                yield return null;
            }
            if (f.Raiz != null) Destroy(f.Raiz.gameObject);
        }

        private Task Explotar(string id)
        {
            var f = Ficha(id);
            if (f == null) return Esperar(0.3f);
            return Correr(ExplotarRutina(f));
        }

        private static IEnumerator ExplotarRutina(FichaVisual f)
        {
            if (f.Raiz != null) Efectos.Chispas(Efectos.Centro(f.Raiz), new Color(1f, 0.6f, 0.15f), 24);
            for (var t = 0f; t < 0.35f && f.Raiz != null; t += Time.deltaTime)
            {
                f.Raiz.localScale = Vector3.one * (1 + 0.8f * t / 0.35f);
                f.Dibujo.color = Color.Lerp(Color.white, new Color(1f, 0.55f, 0.1f, 0.2f), t / 0.35f);
                yield return null;
            }
        }

        /// <summary>Texto que sube y se desvanece sobre una ficha (daño, curación, «Observado»…). No detiene la narración.</summary>
        private void Flotar(string id, string texto, Color color)
        {
            var f = Ficha(id);
            if (f == null || f.Raiz == null) return;
            Efectos.Texto(Efectos.Centro(f.Raiz), texto, color);
        }

        // ───────────────────────────── Diálogos de la trama ─────────────────────────────

        private Task Dialogo(string personaje, string texto)
        {
            Anotar($"{personaje}: «{texto}»");
            if (_saltarDialogos) return Task.CompletedTask;
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(DialogoRutina(personaje, texto, tcs));
            return tcs.Task;
        }

        private IEnumerator DialogoRutina(string personaje, string texto, TaskCompletionSource<bool> fin)
        {
            var padre = _capa != null && _capa.gameObject.activeInHierarchy && _estado != null ? _capa : _raiz;
            var fondo = Ui.Panel(padre, "Dialogo", new Color(0, 0, 0, 0.45f)).Anclar(0, 0, 1, 1);
            var tocado = false;
            var boton = fondo.gameObject.AddComponent<Button>();
            boton.targetGraphic = fondo.GetComponent<Image>();
            boton.onClick.AddListener(() => tocado = true);
            var caja = Ui.PanelRedondeado(fondo, "Caja", Ui.Pergamino).Anclar(0.03f, 0.03f, 0.97f, 0.27f);
            var retrato = Ui.PanelRedondeado(fondo, "Retrato", Ui.Tinta).Anclar(0.04f, 0.2f, 0.3f, 0.345f);
            Ui.Imagen(Ui.Vacio(retrato, "Img").Anclar(0.05f, 0.05f, 0.95f, 0.95f), "Img", Arte.Retrato(personaje));
            var placa = Ui.PanelRedondeado(fondo, "Nombre", Ui.Carmesi).Anclar(0.31f, 0.235f, 0.75f, 0.285f);
            Ui.Texto(placa, personaje == Guion.Narrador ? "Expedientia" : personaje, 34, Ui.Pergamino, TextAnchor.MiddleCenter, FontStyle.Bold);
            var cuerpo = Ui.Texto(Ui.Vacio(caja, "Texto").Anclar(0.04f, 0.14f, 0.96f, 0.8f), "", 38, Ui.Noche, TextAnchor.UpperLeft);
            cuerpo.resizeTextForBestFit = false;
            Ui.Texto(Ui.Vacio(caja, "Seguir").Anclar(0.5f, 0.02f, 0.97f, 0.14f), "Toca para seguir ▸", 24, Ui.Carmesi, TextAnchor.MiddleRight, FontStyle.Italic);
            Ui.Boton(Ui.Vacio(fondo, "Saltar").Anclar(0.74f, 0.29f, 0.97f, 0.34f), "Saltar ▸▸", () => { _saltarDialogos = true; tocado = true; }, Ui.Tinta, Ui.Pergamino, 28);
            var tono = personaje switch { "Don Eusebio" => 0.75f, "El Cliente" => 1.1f, "Rosita" => 1.35f, "Doctora Valdivia" => 1.2f, "El Coimero" => 0.85f, "Suboficial Quispe" => 0.9f, "Especialista Mendoza" => 1.3f, _ => 1f };
            for (var i = 1; i <= texto.Length && !tocado; i++)
            {
                cuerpo.text = texto.Substring(0, i);
                if (i % 3 == 0 && personaje != Guion.Narrador) Sonido.Tocar("blip", tono, 0.35f);
                yield return new WaitForSeconds(0.022f);
            }
            cuerpo.text = texto;
            if (!_saltarDialogos)
            {
                tocado = false;
                while (!tocado) yield return null;
            }
            Destroy(fondo.gameObject);
            fin.TrySetResult(true);
        }

        // ───────────────────────────── El dado como espectáculo ─────────────────────────────

        private Task Dado(int natural, string momento) => Correr(AnimarDado(natural, momento));

        private IEnumerator AnimarDado(int natural, string momento)
        {
            var fondo = Ui.Panel(_capa, "Dado", new Color(0, 0, 0, 0.55f)).Anclar(0, 0, 1, 1);
            var caja = Ui.Vacio(fondo, "Caja").Anclar(0.28f, 0.4f, 0.72f, 0.64f);
            Ui.Imagen(caja, "D20", Arte.Sprite("d20"));
            var num = Ui.Texto(Ui.Vacio(caja, "N").Anclar(0.2f, 0.25f, 0.8f, 0.7f), "", 120, Ui.Noche, TextAnchor.MiddleCenter, FontStyle.Bold);
            var rnd = new System.Random();
            Sonido.Tocar("dado");
            for (var i = 0; i < 10; i++)
            {
                num.text = rnd.Next(1, 21).ToString();
                caja.localRotation = Quaternion.Euler(0, 0, rnd.Next(-35, 35));
                yield return new WaitForSeconds(0.04f + i * 0.01f);
            }
            caja.localRotation = Quaternion.identity;
            num.text = natural.ToString();
            if (momento == "fundado" || momento == "improcedente")
            {
                var fundado = momento == "fundado";
                Sonido.Tocar(fundado ? "fundado" : "improcedente");
                var zonaSello = Ui.Vacio(fondo, "Sello").Anclar(0.08f, 0.6f, 0.92f, 0.8f);
                var sello = Ui.Imagen(Ui.Vacio(zonaSello, "Aro").Anclar(0.25f, -0.1f, 0.75f, 1.1f), "Aro", Arte.Sprite("sello"));
                sello.color = fundado ? Ui.Dorado : Ui.Carmesi;
                Ui.Texto(zonaSello, fundado ? "¡FUNDADO!" : "IMPROCEDENTE", 100, fundado ? Ui.Dorado : Ui.Carmesi, TextAnchor.MiddleCenter, FontStyle.Bold).ConSombra(5);
                zonaSello.localRotation = Quaternion.Euler(0, 0, fundado ? -8 : 6);
                for (var t = 0f; t < 0.3f; t += Time.deltaTime)
                {
                    zonaSello.localScale = Vector3.one * Mathf.Lerp(2.4f, 1f, t / 0.3f);
                    yield return null;
                }
                zonaSello.localScale = Vector3.one;
                Sonido.Tocar("sello");
                if (fundado) Efectos.Confeti(70);
                if (!fundado)
                    Ui.Texto(Ui.Vacio(fondo, "Pifia").Anclar(0.05f, 0.32f, 0.95f, 0.39f), new[] { "Se le cayó el expediente.", "Se acabó la tinta.", "Firmó en la línea equivocada." }[rnd.Next(3)], 40, Ui.Pergamino).ConSombra();
                yield return new WaitForSeconds(0.9f);
            }
            yield return new WaitForSeconds(0.4f);
            Destroy(fondo.gameObject);
        }

        // ───────────────────────────── Prueba jurídica ─────────────────────────────

        private void MostrarPregunta(PreguntaVisible p)
        {
            Sonido.Tocar("campana", 1.2f, 0.5f);
            var fondo = Ui.Panel(_capa, "Pregunta", new Color(0, 0, 0, 0.7f)).Anclar(0, 0, 1, 1);
            var carta = Ui.PanelRedondeado(fondo, "Carta", Ui.Pergamino).Anclar(0.04f, 0.12f, 0.96f, 0.88f);
            Ui.Texto(Ui.Vacio(carta, "Mat").Anclar(0, 0.9f, 1, 0.98f), $"PRUEBA JURÍDICA · {p.Materia}", 34, Ui.Carmesi, TextAnchor.MiddleCenter, FontStyle.Bold);
            if (p.Borrador)
                Ui.Texto(Ui.Panel(carta, "Borr", Ui.Hex("#FFF3B0")).Anclar(0.04f, 0.84f, 0.96f, 0.9f), "BORRADOR SIN VERIFICAR: solo para pruebas", 28, Ui.Noche);
            Ui.Texto(Ui.Vacio(carta, "Q").Anclar(0.04f, 0.6f, 0.96f, 0.84f), p.Texto, 44, Ui.Noche);
            var barra = Ui.Panel(carta, "Tiempo", Ui.Carmesi).Anclar(0.04f, 0.56f, 0.96f, 0.58f);
            var respondido = false;
            for (var i = 0; i < p.Opciones.Count && i < 4; i++)
            {
                var indice = i;
                Ui.Boton(Ui.Vacio(carta, "O" + i).Anclar(0.04f, 0.42f - i * 0.135f, 0.96f, 0.54f - i * 0.135f, 4), $"{(char)('A' + i)}) {p.Opciones[i]}",
                    () => { if (respondido) return; respondido = true; Destroy(fondo.gameObject); Responder(indice); }, Ui.Tinta, Ui.Pergamino, 38);
            }
            StartCoroutine(Cuenta(barra, p.Segundos, () => { if (respondido) return; respondido = true; Destroy(fondo.gameObject); Responder(null); }));
        }

        private IEnumerator Cuenta(RectTransform barra, int segundos, Action alTerminar)
        {
            var ultimo = -1;
            for (var t = 0f; t < segundos; t += Time.deltaTime)
            {
                if (barra == null) yield break;
                barra.anchorMax = new Vector2(Mathf.Lerp(0.96f, 0.04f, t / segundos), barra.anchorMax.y);
                var restante = segundos - (int)t;
                if (restante <= 5 && restante != ultimo) { ultimo = restante; Sonido.Tocar("paso", 1.6f, 0.6f); }
                yield return null;
            }
            alTerminar();
        }

        private void Responder(int? indice) => Enviar(new AccionJuego { Tipo = "responder", Indice = indice });

        private void MostrarCorreccion(CorreccionPregunta c)
        {
            Sonido.Tocar(c.Resultado == "Acierto" ? "fundado" : "improcedente", 1f, 0.6f);
            var fondo = Ui.Panel(_capa, "Correccion", new Color(0, 0, 0, 0.7f)).Anclar(0, 0, 1, 1);
            var carta = Ui.PanelRedondeado(fondo, "Carta", Ui.Pergamino).Anclar(0.04f, 0.2f, 0.96f, 0.8f);
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
            var carta = Ui.PanelRedondeado(fondo, "Carta", Ui.Pergamino).Anclar(0.06f, 0.16f, 0.94f, 0.84f);
            Ui.Texto(Ui.Vacio(carta, "T").Anclar(0.04f, 0.84f, 0.96f, 0.97f), "SENTENCIA", 70, Ui.Noche, TextAnchor.MiddleCenter, FontStyle.Bold);
            var color = s.Fundada ? Ui.Hex("#2F6B2F") : Ui.Carmesi;
            var zona = Ui.Vacio(carta, "Sello").Anclar(0.1f, 0.56f, 0.9f, 0.84f);
            var aro = Ui.Imagen(Ui.Vacio(zona, "Aro").Anclar(0.22f, 0f, 0.78f, 1f), "Aro", Arte.Sprite("sello"));
            aro.color = color;
            Ui.Texto(zona, s.Fundada ? "FUNDADO" : "ARCHIVADO", 70, color, TextAnchor.MiddleCenter, FontStyle.Bold);
            zona.localRotation = Quaternion.Euler(0, 0, -7);
            Ui.Texto(Ui.Vacio(carta, "M").Anclar(0.04f, 0.47f, 0.96f, 0.56f), s.Motivo, 40, color, TextAnchor.MiddleCenter, FontStyle.Bold);
            Ui.Texto(Ui.Vacio(carta, "H").Anclar(0.04f, 0.33f, 0.96f, 0.47f), $"Honorarios: S/ {s.Honorarios} · Experiencia: {s.Experiencia}\nTu estudio: nivel {e.Estudio.Nivel} · reputación {e.Estudio.Reputacion}", 34, Ui.Noche);
            var meta = e.Estudio.ProximoDesbloqueo == null ? "Tu estudio ya tiene todos los desbloqueos."
                : $"Próximo desbloqueo: {e.Estudio.ProximoDesbloqueo} (faltan {e.Estudio.FaltanParaDesbloqueo} expedientes Fundados)\n{e.Estudio.ProximoDesbloqueoDescripcion}";
            var armado = e.MejorasTomadas.Count == 0 ? "" : "Tu armado: " + string.Join(" · ", e.MejorasTomadas.Select(t => t.Nombre)) + "\n";
            Ui.Texto(Ui.Vacio(carta, "L").Anclar(0.04f, 0.17f, 0.96f, 0.33f), armado + meta, 28, Ui.Tinta, TextAnchor.MiddleCenter, FontStyle.Italic);
            Ui.Boton(Ui.Vacio(carta, "V").Anclar(0.04f, 0.03f, 0.48f, 0.15f, 4), "Al estudio", () => MostrarMenu(), Ui.Tinta, Ui.Pergamino, 36);
            Ui.Boton(Ui.Vacio(carta, "Otra").Anclar(0.52f, 0.03f, 0.96f, 0.15f, 4), "¡OTRA PARTIDA!", () => { MostrarMenu(); _ = JugarYa(); }, Ui.Carmesi, Ui.Pergamino, 40);
            StartCoroutine(Aparecer(zona));
            if (s.Fundada) Efectos.Confeti(160);
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
