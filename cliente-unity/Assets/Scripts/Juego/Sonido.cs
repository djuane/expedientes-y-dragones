#nullable enable
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Expedientia.Cliente
{
    /// <summary>
    /// Audio del prototipo, sintetizado por código al abrir el juego: no usa archivos ni música de terceros.
    /// Efectos (dado, sello, golpe, ventanilla, monedas, voces de diálogo) y un tema de menú original en escala pentatónica,
    /// con una "quena" (seno con vibrato) y un "charango" (cuerda pulsada). En producción se reemplaza por audio encargado.
    /// </summary>
    public sealed class Sonido : MonoBehaviour
    {
        private const int Frecuencia = 22050;
        private const string ClaveSilencio = "eyd.silencio";
        private static Sonido? _instancia;
        private readonly Dictionary<string, AudioClip> _clips = new Dictionary<string, AudioClip>();
        private AudioSource _efectos = null!, _musica = null!;

        public static bool Silenciado
        {
            get => PlayerPrefs.GetInt(ClaveSilencio, 0) == 1;
            set
            {
                PlayerPrefs.SetInt(ClaveSilencio, value ? 1 : 0);
                if (_instancia != null) AudioListener.volume = value ? 0 : 1;
            }
        }

        public static void Preparar(GameObject dueno)
        {
            if (_instancia != null) return;
            _instancia = dueno.AddComponent<Sonido>();
        }

        private void Awake()
        {
            _efectos = gameObject.AddComponent<AudioSource>();
            _musica = gameObject.AddComponent<AudioSource>();
            _musica.loop = true;
            _musica.volume = 0.35f;
            AudioListener.volume = Silenciado ? 0 : 1;
            Generar();
        }

        /// <summary>Toca un efecto. <paramref name="tono"/> cambia la altura (por ejemplo, la voz de cada personaje).</summary>
        public static void Tocar(string nombre, float tono = 1f, float volumen = 0.8f)
        {
            if (_instancia == null || !_instancia._clips.TryGetValue(nombre, out var clip)) return;
            _instancia._efectos.pitch = tono;
            _instancia._efectos.PlayOneShot(clip, volumen);
        }

        public static void Musica(bool sonar)
        {
            if (_instancia == null) return;
            var m = _instancia._musica;
            if (sonar && !m.isPlaying) { m.clip = _instancia._clips["tema"]; m.Play(); }
            else if (!sonar && m.isPlaying) m.Stop();
        }

        // ───────────────────────── Síntesis ─────────────────────────

        private void Generar()
        {
            var azar = new System.Random(20261110);
            _clips["dado"] = Clip("dado", 0.55f, (t, i) =>
            {
                // 8 golpecitos de madera, cada vez más espaciados
                var s = 0f;
                for (var k = 0; k < 8; k++)
                {
                    var t0 = 0.06f * k + 0.004f * k * k;
                    var dt = t - t0;
                    if (dt >= 0 && dt < 0.03f) s += (float)(azar.NextDouble() * 2 - 1) * Mathf.Exp(-dt * 160) * 0.7f + Mathf.Sin(2 * Mathf.PI * 1800 * dt) * Mathf.Exp(-dt * 200) * 0.3f;
                }
                return s;
            });
            _clips["sello"] = Clip("sello", 0.35f, (t, i) =>
                Mathf.Sin(2 * Mathf.PI * (90 - 40 * t) * t) * Mathf.Exp(-t * 14) + (float)(azar.NextDouble() * 2 - 1) * Mathf.Exp(-t * 60) * 0.5f);
            _clips["golpe"] = Clip("golpe", 0.25f, (t, i) =>
                Mathf.Sin(2 * Mathf.PI * (220 - 500 * t) * t) * Mathf.Exp(-t * 18) * 0.8f + (float)(azar.NextDouble() * 2 - 1) * Mathf.Exp(-t * 40) * 0.5f);
            _clips["paso"] = Clip("paso", 0.08f, (t, i) => (float)(azar.NextDouble() * 2 - 1) * Mathf.Exp(-t * 90) * 0.5f);
            _clips["moneda"] = Clip("moneda", 0.35f, (t, i) => Cuadrada(t < 0.08f ? 988 : 1319, t) * Mathf.Exp(-t * 8) * 0.35f);
            _clips["campana"] = Clip("campana", 0.9f, (t, i) =>
                (Mathf.Sin(2 * Mathf.PI * 1046 * t) + 0.5f * Mathf.Sin(2 * Mathf.PI * 2093 * t) + 0.25f * Mathf.Sin(2 * Mathf.PI * 3140 * t)) * Mathf.Exp(-t * 5) * 0.4f);
            _clips["blip"] = Clip("blip", 0.05f, (t, i) => Cuadrada(420, t) * Mathf.Exp(-t * 40) * 0.25f);
            _clips["fundado"] = Clip("fundado", 0.9f, (t, i) =>
            {
                float[] notas = { 523, 659, 784, 1046 };
                var k = Mathf.Min(3, (int)(t / 0.11f));
                var dt = t - k * 0.11f;
                return Cuadrada(notas[k], t) * 0.25f * Mathf.Exp(-dt * (k == 3 ? 3 : 10));
            });
            _clips["improcedente"] = Clip("improcedente", 0.9f, (t, i) =>
                Sierra(330 * Mathf.Pow(0.5f, t * 1.1f), t) * 0.25f * (1 - t / 0.9f) * (0.6f + 0.4f * Mathf.Sin(2 * Mathf.PI * 6 * t)));
            _clips["explosion"] = Clip("explosion", 0.7f, (t, i) =>
                (float)(azar.NextDouble() * 2 - 1) * Mathf.Exp(-t * 6) * 0.8f + Mathf.Sin(2 * Mathf.PI * 55 * t) * Mathf.Exp(-t * 5) * 0.5f);
            _clips["sanar"] = Clip("sanar", 0.5f, (t, i) => Mathf.Sin(2 * Mathf.PI * (660 + 600 * t) * t) * Mathf.Exp(-t * 5) * 0.3f);
            _clips["victoria"] = Melodia("victoria", new[] { (69, 0.15f), (72, 0.15f), (74, 0.15f), (76, 0.3f), (74, 0.15f), (76, 0.15f), (81, 0.6f) }, 0.3f);
            _clips["derrota"] = Melodia("derrota", new[] { (76, 0.3f), (74, 0.3f), (72, 0.3f), (69, 0.8f) }, 0.3f);
            _clips["tema"] = Tema();
        }

        private static float Cuadrada(float f, float t) => Mathf.Sin(2 * Mathf.PI * f * t) >= 0 ? 1 : -1;
        private static float Sierra(float f, float t) => 2 * (t * f - Mathf.Floor(t * f + 0.5f));
        private static float Hz(int midi) => 440f * Mathf.Pow(2, (midi - 69) / 12f);

        private static AudioClip Clip(string nombre, float segundos, Func<float, int, float> f)
        {
            var n = (int)(segundos * Frecuencia);
            var datos = new float[n];
            for (var i = 0; i < n; i++)
            {
                var t = i / (float)Frecuencia;
                var fundido = Mathf.Min(1, (n - i) / (Frecuencia * 0.01f));
                datos[i] = Mathf.Clamp(f(t, i) * fundido, -1, 1);
            }
            var c = AudioClip.Create(nombre, n, 1, Frecuencia, false);
            c.SetData(datos, 0);
            return c;
        }

        private static AudioClip Melodia(string nombre, (int midi, float dur)[] notas, float volumen)
        {
            var total = 0f;
            foreach (var (_, d) in notas) total += d;
            var datos = new float[(int)((total + 0.3f) * Frecuencia)];
            var t0 = 0f;
            foreach (var (m, d) in notas) { Quena(datos, t0, d + 0.1f, Hz(m), volumen); t0 += d; }
            var c = AudioClip.Create(nombre, datos.Length, 1, Frecuencia, false);
            c.SetData(datos, 0);
            return c;
        }

        /// <summary>Voz de "quena": seno con armónico suave, vibrato y ataque de soplido.</summary>
        private static void Quena(float[] buf, float inicio, float dur, float f, float vol)
        {
            var i0 = (int)(inicio * Frecuencia);
            var n = (int)(dur * Frecuencia);
            var fase = 0f;
            for (var i = 0; i < n && i0 + i < buf.Length; i++)
            {
                var t = i / (float)Frecuencia;
                var vib = 1 + 0.006f * Mathf.Sin(2 * Mathf.PI * 5.5f * t) * Mathf.Min(1, t * 4);
                fase += 2 * Mathf.PI * f * vib / Frecuencia;
                var env = Mathf.Min(1, t / 0.04f) * Mathf.Min(1, (dur - t) / 0.06f);
                buf[i0 + i] += (Mathf.Sin(fase) + 0.18f * Mathf.Sin(2 * fase)) * env * vol;
            }
        }

        /// <summary>Cuerda pulsada (Karplus-Strong) para el "charango".</summary>
        private static void Charango(float[] buf, float inicio, float f, float vol, System.Random azar)
        {
            var i0 = (int)(inicio * Frecuencia);
            var periodo = Mathf.Max(2, (int)(Frecuencia / f));
            var linea = new float[periodo];
            for (var i = 0; i < periodo; i++) linea[i] = (float)(azar.NextDouble() * 2 - 1);
            var n = (int)(0.9f * Frecuencia);
            for (var i = 0; i < n && i0 + i < buf.Length; i++)
            {
                var k = i % periodo;
                var v = linea[k];
                linea[k] = 0.497f * (v + linea[(k + 1) % periodo]);
                buf[i0 + i] += v * vol;
            }
        }

        /// <summary>Tema del estudio: 8 compases en la menor pentatónica, compuesto para el juego.</summary>
        private static AudioClip Tema()
        {
            const float negra = 60f / 96f;
            // Melodía (nota MIDI, duración en negras); 0 = silencio.
            var melodia = new (int, float)[]
            {
                (69, 1), (72, 0.5f), (74, 0.5f), (76, 1), (74, 0.5f), (72, 0.5f),
                (74, 1.5f), (72, 0.5f), (69, 2),
                (76, 1), (79, 0.5f), (76, 0.5f), (74, 1), (72, 1),
                (74, 0.5f), (76, 0.5f), (74, 0.5f), (72, 0.5f), (69, 2),
                (72, 1), (74, 0.5f), (76, 0.5f), (79, 1), (81, 1),
                (79, 0.5f), (76, 0.5f), (74, 1), (76, 2),
                (74, 1), (72, 0.5f), (69, 0.5f), (67, 1), (69, 1),
                (72, 0.5f), (69, 0.5f), (67, 1), (69, 2),
            };
            var total = 0f;
            foreach (var (_, d) in melodia) total += d;
            var segundos = total * negra;
            var buf = new float[(int)(segundos * Frecuencia)];
            var t = 0f;
            foreach (var (m, d) in melodia)
            {
                if (m > 0) Quena(buf, t, d * negra * 0.95f, Hz(m), 0.22f);
                t += d * negra;
            }
            // Acompañamiento: rasgueo de charango en corcheas (acordes de la menor y do mayor alternados).
            var azar = new System.Random(7);
            int[][] acordes = { new[] { 57, 64, 69, 72 }, new[] { 55, 62, 67, 71 }, new[] { 53, 60, 65, 69 }, new[] { 57, 64, 69, 72 } };
            var compases = (int)(total / 4);
            for (var c = 0; c < compases; c++)
            {
                var acorde = acordes[c % acordes.Length];
                for (var k = 0; k < 8; k++)
                {
                    var inicio = (c * 4 + k * 0.5f) * negra;
                    var nota = acorde[(k * 3 + c) % acorde.Length];
                    Charango(buf, inicio, Hz(nota), k % 4 == 0 ? 0.16f : 0.09f, azar);
                }
            }
            for (var i = 0; i < buf.Length; i++) buf[i] = Mathf.Clamp(buf[i], -1, 1);
            var clip = AudioClip.Create("tema", buf.Length, 1, Frecuencia, false);
            clip.SetData(buf, 0);
            return clip;
        }
    }
}
