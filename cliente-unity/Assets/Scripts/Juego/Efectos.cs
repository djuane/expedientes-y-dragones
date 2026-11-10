#nullable enable
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Expedientia.Cliente
{
    /// <summary>
    /// Efectos de partículas y textos flotantes del prototipo, sobre una capa propia encima de toda la interfaz.
    /// Técnicas: reciclaje de objetos (las piezas no se destruyen: vuelven a una pila y se reutilizan),
    /// datos en un arreglo de structs y una sola actualización por cuadro para todas las piezas (no una rutina por pieza).
    /// Así un festejo de 120 papelitos no genera basura para el recolector ni tirones en el celular.
    /// </summary>
    public sealed class Efectos : MonoBehaviour
    {
        private const int Capacidad = 320;
        private static Efectos? _instancia;

        private struct Pieza
        {
            public Image Imagen;
            public Vector2 Posicion, Velocidad;
            public float Giro, VelocidadGiro, Vida, Duracion, Vaiven, Fase, Gravedad;
            public Color Color;
        }

        private struct Flotante
        {
            public RectTransform Zona;
            public Text Texto;
            public Vector2 Inicio;
            public float Vida;
            public Color Color;
        }

        private RectTransform _capa = null!;
        private readonly Pieza[] _piezas = new Pieza[Capacidad];
        private int _vivas;
        private readonly Stack<Image> _libres = new Stack<Image>();
        private readonly List<Flotante> _flotantes = new List<Flotante>();
        private readonly Stack<Flotante> _flotantesLibres = new Stack<Flotante>();

        private static readonly Color[] Confites =
        {
            Ui.Dorado, Ui.Carmesi, Ui.Aliado, Ui.Hex("#5E9E4F"), Ui.Pergamino, Ui.Hex("#B06FC0"),
        };

        /// <summary>Crea la capa de efectos como último hijo del lienzo (queda encima de todo y no se borra al cambiar de pantalla).</summary>
        public static void Preparar(Transform lienzo)
        {
            if (_instancia != null) return;
            var capa = Ui.Vacio(lienzo, "Efectos").Anclar(0, 0, 1, 1);
            _instancia = capa.gameObject.AddComponent<Efectos>();
            _instancia._capa = capa;
        }

        /// <summary>Punto de la capa de efectos que corresponde al centro de <paramref name="r"/>.</summary>
        public static Vector2 Centro(RectTransform r)
        {
            if (_instancia == null) return Vector2.zero;
            var esquinas = new Vector3[4];
            r.GetWorldCorners(esquinas);
            return _instancia._capa.InverseTransformPoint((esquinas[0] + esquinas[2]) * 0.5f);
        }

        /// <summary>Lluvia de confeti desde arriba de la pantalla (victoria, ¡Fundado!).</summary>
        public static void Confeti(int cantidad = 120)
        {
            if (_instancia == null) return;
            var r = _instancia._capa.rect;
            for (var i = 0; i < cantidad; i++)
            {
                var p = new Vector2(Random.Range(r.xMin, r.xMax), r.yMax + Random.Range(0f, 300f));
                var v = new Vector2(Random.Range(-80f, 80f), Random.Range(-500f, -250f));
                _instancia.Emitir(p, v, Confites[Random.Range(0, Confites.Length)], new Vector2(Random.Range(14f, 22f), Random.Range(8f, 12f)),
                    Random.Range(2.2f, 3.4f), 380f, Random.Range(40f, 110f));
            }
        }

        /// <summary>Estallido de papelitos desde un punto (un enemigo que cae, un expediente que se archiva).</summary>
        public static void Papelitos(Vector2 origen, int cantidad = 18, Color? color = null)
        {
            if (_instancia == null) return;
            for (var i = 0; i < cantidad; i++)
            {
                var angulo = Random.Range(0f, Mathf.PI * 2);
                var rapidez = Random.Range(220f, 520f);
                var v = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * rapidez + Vector2.up * 200f;
                _instancia.Emitir(origen, v, color ?? Ui.Pergamino, new Vector2(Random.Range(16f, 26f), Random.Range(20f, 30f)),
                    Random.Range(0.9f, 1.4f), 900f, Random.Range(20f, 60f));
            }
        }

        /// <summary>Chispas cortas (un golpe crítico, el Plazo que Vence que explota).</summary>
        public static void Chispas(Vector2 origen, Color color, int cantidad = 14)
        {
            if (_instancia == null) return;
            for (var i = 0; i < cantidad; i++)
            {
                var angulo = Random.Range(0f, Mathf.PI * 2);
                var v = new Vector2(Mathf.Cos(angulo), Mathf.Sin(angulo)) * Random.Range(300f, 700f);
                _instancia.Emitir(origen, v, color, new Vector2(10, 10), Random.Range(0.35f, 0.6f), 300f, 0f);
            }
        }

        /// <summary>Texto que sube y se desvanece (daño, curación, «Observado»…). Reutiliza los textos ya creados.</summary>
        public static void Texto(Vector2 posicion, string texto, Color color)
        {
            if (_instancia == null) return;
            var f = _instancia._flotantesLibres.Count > 0 ? _instancia._flotantesLibres.Pop() : _instancia.NuevoFlotante();
            f.Inicio = posicion;
            f.Vida = 0;
            f.Color = color;
            f.Texto.text = texto;
            f.Texto.color = color;
            f.Zona.anchoredPosition = posicion;
            f.Zona.gameObject.SetActive(true);
            f.Zona.SetAsLastSibling();
            _instancia._flotantes.Add(f);
        }

        private Flotante NuevoFlotante()
        {
            var zona = Ui.Vacio(_capa, "Flotante");
            zona.anchorMin = zona.anchorMax = new Vector2(0.5f, 0.5f);
            zona.sizeDelta = new Vector2(360, 70);
            var t = Ui.Texto(zona, "", 40, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold).ConSombra(3);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.raycastTarget = false;
            return new Flotante { Zona = zona, Texto = t };
        }

        private void Emitir(Vector2 posicion, Vector2 velocidad, Color color, Vector2 tamano, float duracion, float gravedad, float vaiven)
        {
            if (_vivas >= Capacidad) return;
            Image img;
            if (_libres.Count > 0) img = _libres.Pop();
            else
            {
                var go = new GameObject("Pieza", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(_capa, false);
                img = go.GetComponent<Image>();
                img.raycastTarget = false;
                img.rectTransform.anchorMin = img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            }
            img.rectTransform.sizeDelta = tamano;
            img.color = color;
            img.enabled = true;
            _piezas[_vivas++] = new Pieza
            {
                Imagen = img, Posicion = posicion, Velocidad = velocidad, Color = color,
                Giro = Random.Range(0f, 360f), VelocidadGiro = Random.Range(-540f, 540f),
                Duracion = duracion, Gravedad = gravedad, Vaiven = vaiven, Fase = Random.Range(0f, 6.3f),
            };
        }

        private void Update()
        {
            var dt = Time.deltaTime;
            for (var i = _vivas - 1; i >= 0; i--)
            {
                ref var p = ref _piezas[i];
                p.Vida += dt;
                if (p.Vida >= p.Duracion || p.Imagen == null)
                {
                    if (p.Imagen != null) { p.Imagen.enabled = false; _libres.Push(p.Imagen); }
                    _piezas[i] = _piezas[--_vivas]; // quitar sin desplazar: se trae la última al hueco
                    continue;
                }
                p.Velocidad.y -= p.Gravedad * dt;
                p.Velocidad *= 1 - 0.8f * dt; // resistencia del aire: el papel planea
                p.Posicion += p.Velocidad * dt;
                p.Giro += p.VelocidadGiro * dt;
                var vaiven = Mathf.Sin(p.Vida * 6f + p.Fase) * p.Vaiven;
                var rt = p.Imagen.rectTransform;
                rt.anchoredPosition = new Vector2(p.Posicion.x + vaiven, p.Posicion.y);
                // El giro en X simula el papel que voltea; el de Z, que da vueltas.
                rt.localRotation = Quaternion.Euler(Mathf.Sin(p.Vida * 9f + p.Fase) * 70f, 0, p.Giro);
                var restante = p.Duracion - p.Vida;
                if (restante < 0.4f) p.Imagen.color = new Color(p.Color.r, p.Color.g, p.Color.b, restante / 0.4f);
            }

            for (var i = _flotantes.Count - 1; i >= 0; i--)
            {
                var f = _flotantes[i];
                f.Vida += dt;
                if (f.Vida >= 0.9f || f.Zona == null)
                {
                    if (f.Zona != null) { f.Zona.gameObject.SetActive(false); _flotantesLibres.Push(f); }
                    _flotantes[i] = _flotantes[_flotantes.Count - 1];
                    _flotantes.RemoveAt(_flotantes.Count - 1);
                    continue;
                }
                f.Zona.anchoredPosition = f.Inicio + new Vector2(0, 30 + 90 * f.Vida);
                f.Texto.color = new Color(f.Color.r, f.Color.g, f.Color.b, 1 - Mathf.Max(0, f.Vida - 0.5f) / 0.4f);
                _flotantes[i] = f;
            }
        }
    }
}
