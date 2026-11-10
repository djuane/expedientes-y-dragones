#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace Expedientia.Cliente
{
    /// <summary>
    /// Arte original del juego (Assets/Resources/arte): fichas, retratos, escenas y portada.
    /// Si falta un dibujo, devuelve null y la interfaz sigue con colores planos.
    /// </summary>
    public static class Arte
    {
        private static readonly Dictionary<string, Sprite?> _cache = new Dictionary<string, Sprite?>();
        private static Sprite? _redondeado;

        public static Sprite? Sprite(string nombre)
        {
            if (_cache.TryGetValue(nombre, out var s)) return s;
            s = Resources.Load<Sprite>("arte/" + nombre);
            if (s == null)
            {
                var t = Resources.Load<Texture2D>("arte/" + nombre);
                if (t != null) s = UnityEngine.Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100);
            }
            _cache[nombre] = s;
            return s;
        }

        /// <summary>Ficha del combatiente según su clave (la da la lógica); los bots del estudio usan la de su especialidad.</summary>
        public static Sprite? Ficha(string clave, string especialidad)
        {
            var s = string.IsNullOrEmpty(clave) ? null : Sprite(clave);
            if (s != null) return s;
            return Sprite(especialidad == "Administrativista" ? "administrativista" : especialidad == "Civilista" ? "civilista" : "formalismo");
        }

        /// <summary>Retrato para los diálogos de la trama.</summary>
        public static Sprite? Retrato(string personaje) => Sprite(personaje switch
        {
            "Don Eusebio" => "eusebio",
            "El Cliente" => "cliente",
            "Rosita" => "rosita",
            "Doctora Valdivia" => "valdivia",
            "Especialista Mendoza" => "mendoza",
            "Suboficial Quispe" => "quispe",
            "El Coimero" => "coimero",
            _ => "narrador",
        });

        /// <summary>Escena ilustrada de cada sala (las de combate usan el piso de su calabozo).</summary>
        public static Sprite? Escena(string? sala, string casoId) => Sprite(sala switch
        {
            "Tramite" => "escena-tramite",
            "Audiencia" => "escena-audiencia",
            "Cafeteria" => "escena-cafeteria",
            "Libreria" => "escena-libreria",
            "Coimero" => "escena-pasillo",
            _ => casoId == "buena-pro-imposible" ? "fondo-contrataciones" : "fondo-mesa",
        });

        /// <summary>Rectángulo redondeado blanco de 9 cortes, generado por código, para botones y paneles.</summary>
        public static Sprite Redondeado
        {
            get
            {
                if (_redondeado != null) return _redondeado;
                const int n = 48, r = 16;
                var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
                var px = new Color32[n * n];
                for (var y = 0; y < n; y++)
                    for (var x = 0; x < n; x++)
                    {
                        var dx = Mathf.Max(0, Mathf.Max(r - x - 0.5f, x + 0.5f - (n - r)));
                        var dy = Mathf.Max(0, Mathf.Max(r - y - 0.5f, y + 0.5f - (n - r)));
                        var d = Mathf.Sqrt(dx * dx + dy * dy);
                        var a = Mathf.Clamp01(r - d + 0.5f);
                        px[y * n + x] = new Color32(255, 255, 255, (byte)(a * 255));
                    }
                t.SetPixels32(px);
                t.Apply();
                _redondeado = UnityEngine.Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
                return _redondeado;
            }
        }
    }
}
