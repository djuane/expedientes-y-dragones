#nullable enable
using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Expedientia.Cliente
{
    /// <summary>Paleta del documento de identidad visual y utilidades para armar la interfaz por código.</summary>
    public static class Ui
    {
        public static readonly Color Noche = Hex("#0E0C1A");
        public static readonly Color Dorado = Hex("#E2B04A");
        public static readonly Color Pergamino = Hex("#EDE3CC");
        public static readonly Color Carmesi = Hex("#C8442F");
        public static readonly Color Tinta = Hex("#2B2540");
        public static readonly Color Casilla = Hex("#3A3355");
        public static readonly Color CasillaValida = Hex("#5E7D4F");
        public static readonly Color Aliado = Hex("#3F6FB0");

        // Tipografía (licencia OFL, ver Resources/fuentes/LICENCIAS.txt): Alegreya para leer, Cinzel para títulos.
        private static Font? _fuente, _negrita, _cursiva, _titulo;
        private static Font Cargar(string nombre) => Resources.Load<Font>("fuentes/" + nombre) ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        public static Font Fuente => _fuente ??= Cargar("Alegreya-Regular");
        public static Font Negrita => _negrita ??= Cargar("Alegreya-Bold");
        public static Font Cursiva => _cursiva ??= Cargar("Alegreya-Italic");
        public static Font Titulo => _titulo ??= Cargar("Cinzel-Black");

        /// <summary>A partir de este tamaño, un texto en negrita se compone como título (Cinzel, con contorno).</summary>
        public const int TamanoDeTitulo = 56;

        public static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }

        public static RectTransform Panel(Transform padre, string nombre, Color fondo)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(padre, false);
            go.GetComponent<Image>().color = fondo;
            return (RectTransform)go.transform;
        }

        /// <summary>Panel con esquinas redondeadas.</summary>
        public static RectTransform PanelRedondeado(Transform padre, string nombre, Color fondo)
        {
            var r = Panel(padre, nombre, fondo);
            var img = r.GetComponent<Image>();
            img.sprite = Arte.Redondeado;
            img.type = Image.Type.Sliced;
            return r;
        }

        /// <summary>Imagen del arte del juego; si falta el dibujo, queda transparente.</summary>
        public static Image Imagen(Transform padre, string nombre, Sprite? sprite, bool conservarProporcion = true)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(padre, false);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = sprite == null ? Color.clear : Color.white;
            img.preserveAspect = conservarProporcion;
            img.raycastTarget = false;
            ((RectTransform)go.transform).Anclar(0, 0, 1, 1);
            return img;
        }

        /// <summary>Sombra suave para que el texto se lea sobre las ilustraciones.</summary>
        public static Text ConSombra(this Text t, float distancia = 3)
        {
            var s = t.gameObject.AddComponent<Shadow>();
            s.effectColor = new Color(0, 0, 0, 0.75f);
            s.effectDistance = new Vector2(distancia, -distancia);
            return t;
        }

        public static RectTransform Vacio(Transform padre, string nombre)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            go.transform.SetParent(padre, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Ancla un rectángulo por fracciones de su padre (0 a 1).</summary>
        public static RectTransform Anclar(this RectTransform r, float x0, float y0, float x1, float y1, float margen = 0)
        {
            r.anchorMin = new Vector2(x0, y0);
            r.anchorMax = new Vector2(x1, y1);
            r.offsetMin = new Vector2(margen, margen);
            r.offsetMax = new Vector2(-margen, -margen);
            return r;
        }

        public static Text Texto(Transform padre, string texto, int tamano, Color color, TextAnchor alineacion = TextAnchor.MiddleCenter, FontStyle estilo = FontStyle.Normal)
        {
            var go = new GameObject("Texto", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(padre, false);
            var t = go.GetComponent<Text>();
            t.text = texto; t.fontSize = tamano; t.color = color; t.alignment = alineacion;
            // Se usa la variante real de la fuente (no la negrita o cursiva simuladas por Unity).
            var esTitulo = estilo == FontStyle.Bold && tamano >= TamanoDeTitulo;
            t.font = esTitulo ? Titulo : estilo == FontStyle.Bold ? Negrita : estilo == FontStyle.Italic ? Cursiva : Fuente;
            t.fontStyle = FontStyle.Normal;
            t.lineSpacing = esTitulo ? 0.9f : 1f;
            if (esTitulo)
            {
                var borde = go.AddComponent<Outline>();
                borde.effectColor = new Color(0.08f, 0.05f, 0.1f, 0.9f);
                borde.effectDistance = new Vector2(2.5f, -2.5f);
            }
            t.horizontalOverflow = HorizontalWrapMode.Wrap; t.verticalOverflow = VerticalWrapMode.Truncate;
            t.resizeTextForBestFit = true; t.resizeTextMinSize = Math.Max(10, tamano / 2); t.resizeTextMaxSize = tamano;
            t.raycastTarget = false;
            ((RectTransform)go.transform).Anclar(0, 0, 1, 1, 8);
            return t;
        }

        public static Button Boton(Transform padre, string texto, UnityAction alPulsar, Color? fondo = null, Color? color = null, int tamano = 40)
        {
            var go = new GameObject("Boton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(padre, false);
            var img = go.GetComponent<Image>();
            img.color = fondo ?? Noche;
            img.sprite = Arte.Redondeado;
            img.type = Image.Type.Sliced;
            var b = go.GetComponent<Button>();
            b.targetGraphic = img;
            b.onClick.AddListener(() => Sonido.Tocar("paso", 1.4f, 0.4f));
            b.onClick.AddListener(alPulsar);
            Texto(go.transform, texto, tamano, color ?? Dorado, TextAnchor.MiddleCenter, FontStyle.Bold);
            return b;
        }

        public static void CambiarTexto(this Button b, string texto) => b.GetComponentInChildren<Text>().text = texto;

        public static InputField Campo(Transform padre, string valor, string ayuda)
        {
            var fondo = Panel(padre, "Campo", Color.white);
            var campo = fondo.gameObject.AddComponent<InputField>();
            var texto = Texto(fondo, valor, 38, Noche, TextAnchor.MiddleLeft);
            texto.supportRichText = false;
            texto.resizeTextForBestFit = false;
            var marcador = Texto(fondo, ayuda, 38, new Color(0, 0, 0, 0.35f), TextAnchor.MiddleLeft, FontStyle.Italic);
            marcador.resizeTextForBestFit = false;
            campo.textComponent = texto;
            campo.placeholder = marcador;
            campo.text = valor;
            return campo;
        }

        public static void Limpiar(Transform t)
        {
            for (var i = t.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }
    }
}
