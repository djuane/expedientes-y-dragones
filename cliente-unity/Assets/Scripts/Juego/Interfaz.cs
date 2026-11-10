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

        private static Font? _fuente;
        public static Font Fuente => _fuente ??= Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }

        public static RectTransform Panel(Transform padre, string nombre, Color fondo)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(padre, false);
            go.GetComponent<Image>().color = fondo;
            return (RectTransform)go.transform;
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
            t.font = Fuente; t.text = texto; t.fontSize = tamano; t.color = color; t.alignment = alineacion; t.fontStyle = estilo;
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
            var b = go.GetComponent<Button>();
            b.targetGraphic = img;
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
