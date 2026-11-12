#nullable enable
#if CAPTURAS
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Expedientia.Logica.Expedientes;
using Expedientia.Logica.Investigacion;
using UnityEngine;

namespace Expedientia.Cliente
{
    /// <summary>
    /// Solo en la compilación de Windows con la marca CAPTURAS (nunca en el APK): recorre el caso 1 con la solución
    /// y guarda capturas a 1080×1920 de la investigación, el escrito, la audiencia y la sentencia.
    /// Uso: ExpedientesYDragones.exe -capturas D:\ruta\de\salida
    /// </summary>
    public sealed partial class Controlador
    {
        private void Start()
        {
            var args = Environment.GetCommandLineArgs();
            var i = Array.IndexOf(args, "-capturas");
            if (i < 0 || i + 1 >= args.Length) return;
            StartCoroutine(Capturar(args[i + 1]));
        }

        private IEnumerator Capturar(string carpeta)
        {
            Directory.CreateDirectory(carpeta);
            const int ancho = 1080, alto = 1920;
            var rt = new RenderTexture(ancho, alto, 24, RenderTextureFormat.ARGB32);
            var cam = Camera.main!;
            cam.targetTexture = rt;
            var lienzo = GetComponentInChildren<Canvas>();
            lienzo.renderMode = RenderMode.ScreenSpaceCamera;
            lienzo.worldCamera = cam;
            lienzo.planeDistance = 1;
            for (var f = 0; f < 10; f++) yield return null;

            var p = new PartidaDeInvestigacion(CasosDeInvestigacion.Buscar("escrito-fantasma"), new EstadoEstudio());
            _inv = p;
            _lugaresVistos.Clear();
            foreach (var l in p.LugaresAbiertos) _lugaresVistos.Add(l.Id);

            // Investigación a medio camino: algunas pruebas clave ya en el expediente.
            for (var n = 0; n < 3; n++)
            {
                var h = p.LugaresAbiertos.SelectMany(l => p.Acciones(l.Id)).FirstOrDefault(x => x.Clave);
                if (h == null) break;
                p.Investigar(h.Id);
            }
            _lugar = p.LugaresAbiertos.FirstOrDefault(l => p.QuedaAlgo(l.Id))?.Id ?? "estudio";
            PintarInvestigacion();
            yield return Foto(rt, carpeta, "1-investigacion.png", 1f);

            MostrarExpediente(null);
            yield return Foto(rt, carpeta, "2-expediente.png", 0.5f);

            // Resto de lo clave y el escrito.
            for (var n = 0; n < 20; n++)
            {
                var h = p.LugaresAbiertos.SelectMany(l => p.Acciones(l.Id)).FirstOrDefault(x => x.Clave);
                if (h == null) break;
                p.Investigar(h.Id);
            }
            PintarInvestigacion();
            MostrarEscrito();
            yield return Foto(rt, carpeta, "3-escrito.png", 0.5f);

            p.PresentarEscrito(p.Caso.Pretensiones.ToList().FindIndex(x => x.Correcta));
            _linea = 0;
            PintarAudiencia();
            yield return Foto(rt, carpeta, "4-audiencia.png", 1f);

            var t = p.TestimonioActual!;
            _linea = t.Lineas.ToList().FindIndex(l => l.Contradice != null);
            PintarAudiencia();
            var sello = Sello("¡CONTRADICCIÓN!", Ui.Dorado, "fundado");
            yield return Foto(rt, carpeta, "5-contradiccion.png", 0.45f);
            while (!sello.IsCompleted) yield return null;

            while (p.Fase == Fase.Audiencia)
            {
                var ti = p.TestimonioActual!;
                var li = ti.Lineas.ToList().FindIndex(l => l.Contradice != null);
                p.Presentar(li, ti.Lineas[li].Contradice!);
            }
            MostrarSentenciaInvestigacion();
            yield return Foto(rt, carpeta, "6-sentencia.png", 1.5f);

            Debug.Log("Capturas listas en " + carpeta);
            Application.Quit(0);
        }

        private IEnumerator Foto(RenderTexture rt, string carpeta, string nombre, float espera)
        {
            yield return new WaitForSeconds(espera);
            yield return new WaitForEndOfFrame();
            var cam = Camera.main!;
            cam.Render();
            var previa = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            tex.Apply();
            RenderTexture.active = previa;
            File.WriteAllBytes(Path.Combine(carpeta, nombre), tex.EncodeToPNG());
            Destroy(tex);
            Debug.Log("Captura: " + nombre);
        }
    }
}
#endif
