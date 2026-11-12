#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Expedientia.Logica.Expedientes;
using Expedientia.Logica.Investigacion;
using Expedientia.Logica.Juridico;
using UnityEngine;
using UnityEngine.UI;

namespace Expedientia.Cliente
{
    /// <summary>
    /// Modo principal desde v0.13: investigar el caso contra un plazo en horas, armar el escrito y litigar la audiencia
    /// desmintiendo al testigo con la prueba justa. Se juega sin conexión con la misma lógica que tiene pruebas automáticas.
    /// </summary>
    public sealed partial class Controlador
    {
        private PartidaDeInvestigacion? _inv;
        private string _lugar = "estudio";
        private int _linea;
        private readonly HashSet<string> _lugaresVistos = new HashSet<string>();
        private List<Pregunta>? _banco;

        // ───────────────────────────── Inicio ─────────────────────────────

        private async Task EmpezarInvestigacion(string casoId = "escrito-fantasma")
        {
            if (_ocupado) return;
            _ocupado = true;
            try
            {
                _estado = null; _fuente = null;
                _inv = new PartidaDeInvestigacion(CasosDeInvestigacion.Buscar(casoId), FuenteLocal.LeerEstudio());
                _lugar = "estudio";
                _linea = 0;
                _lugaresVistos.Clear();
                foreach (var l in _inv.LugaresAbiertos) _lugaresVistos.Add(l.Id);
                Sonido.Musica(false);
                PintarInvestigacion();
                await Decir(_inv.Caso.Introduccion);
                PintarInvestigacion();
            }
            finally { _ocupado = false; }
        }

        private async Task Decir(IEnumerable<LineaDeGuion> lineas)
        {
            foreach (var l in lineas) await Dialogo(l.Personaje, l.Texto);
            _saltarDialogos = false;
        }

        // ───────────────────────────── Investigación ─────────────────────────────

        private void PintarInvestigacion()
        {
            var p = _inv!;
            Ui.Limpiar(_raiz);
            var lugar = p.Caso.Lugares.First(l => l.Id == _lugar);

            // Escena del lugar, arriba.
            var escena = Ui.Vacio(_raiz, "Escena").Anclar(0, 0.56f, 1, 1);
            Ui.Imagen(escena, "Fondo", Arte.Sprite(lugar.Escena), false);
            Ui.Panel(escena, "Velo", new Color(0, 0, 0, 0.35f)).Anclar(0, 0, 1, 1);
            var cab = Ui.Panel(_raiz, "Cabecera", new Color(0.06f, 0.05f, 0.1f, 0.85f)).Anclar(0, 0.925f, 1, 1);
            Ui.Texto(Ui.Vacio(cab, "T").Anclar(0.03f, 0.45f, 0.7f, 1), $"Caso {p.Caso.Numero}: {p.Caso.Titulo}", 38, Ui.Dorado, TextAnchor.MiddleLeft, FontStyle.Bold);
            Ui.Texto(Ui.Vacio(cab, "S").Anclar(0.03f, 0, 0.7f, 0.5f), "Investigación · la audiencia es a las 5 p. m.", 26, Ui.Pergamino, TextAnchor.MiddleLeft, FontStyle.Italic);
            Ui.Texto(Ui.Vacio(cab, "H").Anclar(0.7f, 0.1f, 0.97f, 0.9f), $"⏱ {p.HorasRestantes} h\n{Relojes(p.HorasRestantes, p.Caso.HorasDePlazo)}", 30,
                p.HorasRestantes <= 2 ? Ui.Carmesi : Ui.Dorado, TextAnchor.MiddleRight, FontStyle.Bold);

            var placa = Ui.PanelRedondeado(escena, "Lugar", new Color(0.06f, 0.05f, 0.1f, 0.8f)).Anclar(0.03f, 0.03f, 0.97f, 0.3f);
            Ui.Texto(Ui.Vacio(placa, "N").Anclar(0.03f, 0.55f, 0.97f, 0.97f), lugar.Nombre.ToUpperInvariant(), 44, Ui.Dorado, TextAnchor.MiddleLeft, FontStyle.Bold);
            Ui.Texto(Ui.Vacio(placa, "D").Anclar(0.03f, 0.03f, 0.97f, 0.55f), lugar.Descripcion, 28, Ui.Pergamino, TextAnchor.UpperLeft, FontStyle.Italic);

            // Mapa: los lugares abiertos, con un punto si queda algo por hacer.
            var abiertos = p.LugaresAbiertos;
            var mapa = Ui.Vacio(_raiz, "Mapa").Anclar(0.02f, 0.425f, 0.98f, 0.555f);
            var porFila = 4;
            var filas = (int)Math.Ceiling(abiertos.Count / (float)porFila);
            for (var i = 0; i < abiertos.Count; i++)
            {
                var l = abiertos[i];
                int f = i / porFila, c = i % porFila;
                var nuevo = !_lugaresVistos.Contains(l.Id);
                var activo = l.Id == _lugar;
                var marca = nuevo ? "¡Nuevo! " : p.QuedaAlgo(l.Id) ? "• " : "";
                var id = l.Id;
                Ui.Boton(Ui.Vacio(mapa, l.Id).Anclar((float)c / porFila, 1f - (float)(f + 1) / filas, (float)(c + 1) / porFila, 1f - (float)f / filas, 4),
                    marca + l.Nombre, () => { _lugar = id; _lugaresVistos.Add(id); Sonido.Tocar("paso", 1f, 0.5f); PintarInvestigacion(); },
                    activo ? Ui.Dorado : nuevo ? Ui.Carmesi : Ui.Tinta, activo ? Ui.Noche : Ui.Pergamino, 26);
            }
            _lugaresVistos.Add(_lugar);

            // Acciones del lugar.
            var acciones = p.Acciones(_lugar);
            var zona = Ui.Vacio(_raiz, "Acciones").Anclar(0.04f, 0.15f, 0.96f, 0.415f);
            if (acciones.Count == 0)
                Ui.Texto(zona, "Aquí ya no queda nada por hacer. Prueben en otro lugar.", 32, Ui.Pergamino, TextAnchor.MiddleCenter, FontStyle.Italic);
            for (var i = 0; i < acciones.Count && i < 4; i++)
            {
                var h = acciones[i];
                var alcanza = h.Horas <= p.HorasRestantes;
                var costo = h.Horas == 0 ? "gratis" : $"{h.Horas} h";
                Ui.Boton(Ui.Vacio(zona, "A" + i).Anclar(0, 1f - (i + 1) * 0.25f, 1, 1f - i * 0.25f, 5), $"{h.Accion}  ({costo})",
                    () => { if (alcanza) _ = Investigar(h.Id); else Anotar("No alcanza el tiempo."); }, alcanza ? Ui.Noche : Ui.Tinta, alcanza ? Ui.Pergamino : new Color(1, 1, 1, 0.4f), 32);
            }

            // Barra inferior.
            var barra = Ui.Vacio(_raiz, "Barra").Anclar(0.02f, 0.015f, 0.98f, 0.14f);
            Ui.Boton(Ui.Vacio(barra, "Exp").Anclar(0, 0, 0.32f, 1, 5), $"Expediente\n({p.Obtenidas.Count})", () => MostrarExpediente(null), Ui.Tinta, Ui.Pergamino, 30);
            Ui.Boton(Ui.Vacio(barra, "Lion").Anclar(0.34f, 0, 0.64f, 1, 5), $"Pista de Lion\n({p.PistasDeLionRestantes})", () => _ = PistaDeLion(), Ui.Tinta, Ui.Pergamino, 30);
            Ui.Boton(Ui.Vacio(barra, "Aud").Anclar(0.66f, 0, 1, 1, 5), "Armar escrito\ny AUDIENCIA ▶", () => MostrarEscrito(), Ui.Carmesi, Ui.Pergamino, 30);
        }

        private static string Relojes(int quedan, int total) => new string('●', Math.Max(0, quedan)) + new string('○', Math.Max(0, total - quedan));

        private async Task Investigar(string hallazgo)
        {
            if (_ocupado || _inv == null) return;
            _ocupado = true;
            try
            {
                var r = _inv.Investigar(hallazgo);
                Sonido.Tocar("paso", 0.9f, 0.5f);
                await Decir(r.Lineas);
                if (r.Nueva != null) await TarjetaDePrueba(r.Nueva);
                PintarInvestigacion();
                if (r.SeAcaboElPlazo)
                {
                    await Dialogo(Guion.Gonzabio, "¡Son las cinco! Se acabó el tiempo. Armemos el escrito con lo que tenemos y a la audiencia.");
                    _saltarDialogos = false;
                    MostrarEscrito();
                }
            }
            catch (InvalidOperationException ex) { Anotar(ex.Message); }
            finally { _ocupado = false; }
        }

        private async Task PistaDeLion()
        {
            if (_ocupado || _inv == null) return;
            _ocupado = true;
            try { await Decir(_inv.PedirPistaALion().Lineas); PintarInvestigacion(); }
            finally { _ocupado = false; }
        }

        /// <summary>Una prueba nueva aparece como una carta que gira y brilla.</summary>
        private Task TarjetaDePrueba(Prueba prueba)
        {
            var tcs = new TaskCompletionSource<bool>();
            StartCoroutine(TarjetaRutina(prueba, tcs));
            return tcs.Task;
        }

        private IEnumerator TarjetaRutina(Prueba prueba, TaskCompletionSource<bool> fin)
        {
            Sonido.Tocar(prueba.Viciada ? "improcedente" : "fundado", 1.1f, 0.7f);
            var fondo = Ui.Panel(_raiz, "Nueva", new Color(0, 0, 0, 0.75f)).Anclar(0, 0, 1, 1);
            var tocado = false;
            var boton = fondo.gameObject.AddComponent<Button>();
            boton.targetGraphic = fondo.GetComponent<Image>();
            boton.onClick.AddListener(() => tocado = true);
            var titulo = prueba.Viciada ? "PAPEL DUDOSO" : prueba.Presentable ? "¡PRUEBA NUEVA!" : "PISTA NUEVA";
            Ui.Texto(Ui.Vacio(fondo, "T").Anclar(0.04f, 0.72f, 0.96f, 0.82f), titulo, 70, prueba.Viciada ? Ui.Carmesi : Ui.Dorado, TextAnchor.MiddleCenter, FontStyle.Bold).ConSombra(4);
            var carta = Ui.PanelRedondeado(fondo, "Carta", prueba.Viciada ? Ui.Hex("#B9A98A") : Ui.Pergamino).Anclar(0.1f, 0.36f, 0.9f, 0.7f);
            Ui.Texto(Ui.Vacio(carta, "N").Anclar(0.05f, 0.62f, 0.95f, 0.95f), prueba.Nombre, 46, Ui.Carmesi, TextAnchor.MiddleCenter, FontStyle.Bold);
            Ui.Texto(Ui.Vacio(carta, "D").Anclar(0.06f, 0.06f, 0.94f, 0.62f), prueba.Descripcion, 34, Ui.Noche, TextAnchor.UpperCenter);
            Ui.Texto(Ui.Vacio(fondo, "S").Anclar(0.1f, 0.28f, 0.9f, 0.34f), "Toca para guardarla en el expediente", 28, Ui.Pergamino, TextAnchor.MiddleCenter, FontStyle.Italic);
            if (!prueba.Viciada) Efectos.Chispas(Vector2.zero, Ui.Dorado, 26);
            for (var t = 0f; t < 0.35f; t += Time.deltaTime)
            {
                var k = Mathf.SmoothStep(0, 1, t / 0.35f);
                carta.localScale = new Vector3(k, 1, 1);
                carta.localRotation = Quaternion.Euler(0, 0, (1 - k) * 12);
                yield return null;
            }
            carta.localScale = Vector3.one; carta.localRotation = Quaternion.identity;
            while (!tocado) yield return null;
            Destroy(fondo.gameObject);
            fin.TrySetResult(true);
        }

        /// <summary>
        /// Lista de pruebas y pistas. Si <paramref name="alElegir"/> no es null, sirve para elegir qué presentar en la audiencia.
        /// </summary>
        private void MostrarExpediente(Action<Prueba>? alElegir)
        {
            var p = _inv!;
            var fondo = Ui.Panel(_raiz, "Expediente", new Color(0, 0, 0, 0.85f)).Anclar(0, 0, 1, 1);
            Ui.Texto(Ui.Vacio(fondo, "T").Anclar(0.04f, 0.9f, 0.96f, 0.97f), alElegir == null ? "EXPEDIENTE DEL CASO" : "¿QUÉ PRUEBA PRESENTAS?", 52, Ui.Dorado, TextAnchor.MiddleCenter, FontStyle.Bold);
            var lista = alElegir == null ? p.Obtenidas : p.Presentables;
            var alto = Math.Min(0.11f, 0.78f / Math.Max(1, lista.Count));
            for (var i = 0; i < lista.Count; i++)
            {
                var pr = lista[i];
                var arriba = 0.89f - i * alto;
                var zona = Ui.Vacio(fondo, "P" + i).Anclar(0.04f, arriba - alto, 0.96f, arriba, 4);
                var color = pr.Viciada ? Ui.Hex("#B9A98A") : pr.Presentable ? Ui.Pergamino : Ui.Hex("#C9D6E8");
                var b = Ui.Boton(zona, "", () =>
                {
                    if (alElegir == null) return;
                    Destroy(fondo.gameObject);
                    alElegir(pr);
                }, color, Ui.Noche, 28);
                Ui.Texto(Ui.Vacio(b.transform, "N").Anclar(0.03f, 0.5f, 0.97f, 0.97f), (pr.Presentable ? "" : "Pista: ") + pr.Nombre, 32, Ui.Carmesi, TextAnchor.MiddleLeft, FontStyle.Bold);
                Ui.Texto(Ui.Vacio(b.transform, "D").Anclar(0.03f, 0.03f, 0.97f, 0.5f), pr.Descripcion, 22, Ui.Noche, TextAnchor.UpperLeft);
            }
            Ui.Boton(Ui.Vacio(fondo, "C").Anclar(0.3f, 0.015f, 0.7f, 0.075f, 4), alElegir == null ? "Cerrar" : "Mejor no", () => Destroy(fondo.gameObject), Ui.Tinta, Ui.Pergamino, 34);
        }

        // ───────────────────────────── Escrito ─────────────────────────────

        private void MostrarEscrito()
        {
            var p = _inv!;
            if (p.Fase != Fase.Investigacion) return;
            var fondo = Ui.Panel(_raiz, "Escrito", new Color(0, 0, 0, 0.88f)).Anclar(0, 0, 1, 1);
            Ui.Texto(Ui.Vacio(fondo, "T").Anclar(0.04f, 0.86f, 0.96f, 0.95f), "ARMAR EL ESCRITO", 62, Ui.Dorado, TextAnchor.MiddleCenter, FontStyle.Bold).ConSombra(4);
            var faltan = p.HorasRestantes > 0 ? $"Les quedan {p.HorasRestantes} h para seguir investigando. " : "";
            Ui.Texto(Ui.Vacio(fondo, "S").Anclar(0.06f, 0.78f, 0.94f, 0.86f), faltan + "¿Qué le pide el escrito al juzgado?", 34, Ui.Pergamino, TextAnchor.MiddleCenter, FontStyle.Italic);
            for (var i = 0; i < p.Caso.Pretensiones.Count; i++)
            {
                var indice = i;
                var arriba = 0.76f - i * 0.19f;
                Ui.Boton(Ui.Vacio(fondo, "P" + i).Anclar(0.06f, arriba - 0.17f, 0.94f, arriba, 4), p.Caso.Pretensiones[i].Texto,
                    () => { Destroy(fondo.gameObject); _ = IrALaAudiencia(indice); }, Ui.Pergamino, Ui.Noche, 34);
            }
            if (p.HorasRestantes > 0)
                Ui.Boton(Ui.Vacio(fondo, "V").Anclar(0.25f, 0.06f, 0.75f, 0.14f, 4), "Seguir investigando", () => Destroy(fondo.gameObject), Ui.Tinta, Ui.Pergamino, 34);
        }

        private async Task IrALaAudiencia(int pretension)
        {
            if (_ocupado || _inv == null) return;
            _ocupado = true;
            try
            {
                var r = _inv.PresentarEscrito(pretension);
                Sonido.Tocar("sello", 1f, 0.7f);
                _linea = 0;
                PintarAudiencia();
                await Decir(r.Lineas);
                PintarAudiencia();
            }
            finally { _ocupado = false; }
        }

        // ───────────────────────────── Audiencia ─────────────────────────────

        private void PintarAudiencia()
        {
            var p = _inv!;
            if (p.Fase == Fase.Sentencia) { MostrarSentenciaInvestigacion(); return; }
            var t = p.TestimonioActual!;
            _linea = Mathf.Clamp(_linea, 0, t.Lineas.Count - 1);
            Ui.Limpiar(_raiz);
            Ui.Imagen(Ui.Vacio(_raiz, "Sala").Anclar(0, 0.4f, 1, 1), "Fondo", Arte.Sprite("escena-audiencia"), false);
            Ui.Panel(_raiz, "Velo", new Color(0, 0, 0, 0.3f)).Anclar(0, 0.4f, 1, 1);

            var cab = Ui.Panel(_raiz, "Cabecera", new Color(0.06f, 0.05f, 0.1f, 0.85f)).Anclar(0, 0.925f, 1, 1);
            Ui.Texto(Ui.Vacio(cab, "T").Anclar(0.03f, 0.45f, 0.65f, 1), $"Testimonio {p.IndiceTestimonio + 1} de {p.Caso.Testimonios.Count}", 36, Ui.Dorado, TextAnchor.MiddleLeft, FontStyle.Bold);
            Ui.Texto(Ui.Vacio(cab, "S").Anclar(0.03f, 0, 0.65f, 0.5f), t.Titulo, 28, Ui.Pergamino, TextAnchor.MiddleLeft, FontStyle.Italic);
            Ui.Texto(Ui.Vacio(cab, "C").Anclar(0.62f, 0.1f, 0.97f, 0.9f), $"Credibilidad\n{new string('♦', p.Credibilidad)}{new string('◇', PartidaDeInvestigacion.CredibilidadMaxima - p.Credibilidad)}",
                30, p.Credibilidad <= 2 ? Ui.Carmesi : Ui.Dorado, TextAnchor.MiddleRight, FontStyle.Bold);

            var retrato = Ui.PanelRedondeado(_raiz, "Testigo", Ui.Tinta).Anclar(0.28f, 0.5f, 0.72f, 0.88f);
            Ui.Imagen(Ui.Vacio(retrato, "Img").Anclar(0.05f, 0.05f, 0.95f, 0.95f), "Img", Arte.Retrato(t.Testigo));
            var placa = Ui.PanelRedondeado(_raiz, "Nombre", Ui.Carmesi).Anclar(0.25f, 0.455f, 0.75f, 0.505f);
            Ui.Texto(placa, t.Testigo, 34, Ui.Pergamino, TextAnchor.MiddleCenter, FontStyle.Bold);

            var caja = Ui.PanelRedondeado(_raiz, "Frase", Ui.Pergamino).Anclar(0.03f, 0.25f, 0.97f, 0.445f);
            var marca = p.Repreguntada(_linea) ? "  (repreguntada)" : "";
            Ui.Texto(Ui.Vacio(caja, "N").Anclar(0.04f, 0.78f, 0.96f, 0.97f), $"Frase {_linea + 1} de {t.Lineas.Count}{marca}", 24, Ui.Carmesi, TextAnchor.MiddleLeft, FontStyle.Bold);
            var frase = Ui.Texto(Ui.Vacio(caja, "F").Anclar(0.04f, 0.04f, 0.96f, 0.78f), $"«{t.Lineas[_linea].Texto}»", 40, Ui.Noche, TextAnchor.MiddleLeft, FontStyle.Italic);
            frase.resizeTextMinSize = 24;

            var nav = Ui.Vacio(_raiz, "Nav").Anclar(0.03f, 0.165f, 0.97f, 0.24f);
            Ui.Boton(Ui.Vacio(nav, "Ant").Anclar(0, 0, 0.3f, 1, 4), "◀ Anterior", () => { _linea = (_linea - 1 + t.Lineas.Count) % t.Lineas.Count; PintarAudiencia(); }, Ui.Tinta, Ui.Pergamino, 32);
            Ui.Boton(Ui.Vacio(nav, "Rep").Anclar(0.32f, 0, 0.68f, 1, 4), "Repreguntar", () => _ = Repreguntar(), Ui.Noche, Ui.Pergamino, 32);
            Ui.Boton(Ui.Vacio(nav, "Sig").Anclar(0.7f, 0, 1, 1, 4), "Siguiente ▶", () => { _linea = (_linea + 1) % t.Lineas.Count; PintarAudiencia(); }, Ui.Tinta, Ui.Pergamino, 32);
            Ui.Boton(Ui.Vacio(_raiz, "Presentar").Anclar(0.08f, 0.03f, 0.92f, 0.15f, 4), "¡PRESENTAR PRUEBA!", () => MostrarExpediente(pr => _ = Presentar(pr)), Ui.Carmesi, Ui.Pergamino, 50);
        }

        private async Task Repreguntar()
        {
            if (_ocupado || _inv == null) return;
            _ocupado = true;
            try { await Decir(_inv.Repreguntar(_linea).Lineas); PintarAudiencia(); }
            finally { _ocupado = false; }
        }

        private async Task Presentar(Prueba prueba)
        {
            if (_ocupado || _inv == null) return;
            _ocupado = true;
            try
            {
                var r = _inv.Presentar(_linea, prueba.Id);
                if (r.Contradiccion)
                {
                    await Sello("¡CONTRADICCIÓN!", Ui.Dorado, "fundado");
                    await Decir(r.Lineas);
                    _linea = 0;
                    if (r.PideFundamento && _inv.Fase == Fase.Audiencia) await Fundamento();
                }
                else
                {
                    await Sello("¡NO HA LUGAR!", Ui.Carmesi, "improcedente");
                    await Decir(r.Lineas);
                }
                PintarAudiencia();
            }
            catch (InvalidOperationException ex) { Anotar(ex.Message); }
            finally { _ocupado = false; }
        }

        /// <summary>Sello grande que cae sobre la pantalla con sacudida y chispas.</summary>
        private Task Sello(string texto, Color color, string sonido) => Correr(SelloRutina(texto, color, sonido));

        private IEnumerator SelloRutina(string texto, Color color, string sonido)
        {
            Sonido.Tocar(sonido, 1f, 0.9f);
            var fondo = Ui.Panel(_raiz, "Sello", new Color(0, 0, 0, 0.4f)).Anclar(0, 0, 1, 1);
            var zona = Ui.Vacio(fondo, "Z").Anclar(0.02f, 0.45f, 0.98f, 0.6f);
            Ui.Texto(zona, texto, 96, color, TextAnchor.MiddleCenter, FontStyle.Bold).ConSombra(6);
            for (var t = 0f; t < 0.22f; t += Time.deltaTime)
            {
                var k = t / 0.22f;
                zona.localScale = Vector3.one * Mathf.Lerp(2.4f, 1f, k * k);
                zona.localRotation = Quaternion.Euler(0, 0, -6 * k);
                yield return null;
            }
            zona.localScale = Vector3.one;
            Efectos.Chispas(Vector2.zero, color, 30);
            for (var t = 0f; t < 0.3f; t += Time.deltaTime)
            {
                _raiz.anchoredPosition = UnityEngine.Random.insideUnitCircle * 14f * (1 - t / 0.3f);
                yield return null;
            }
            _raiz.anchoredPosition = Vector2.zero;
            yield return new WaitForSeconds(0.55f);
            Destroy(fondo.gameObject);
        }

        /// <summary>La jueza pide el fundamento: una pregunta jurídica verificada del banco (si hay). Acertar devuelve credibilidad.</summary>
        private async Task Fundamento()
        {
            _banco ??= FuenteLocal.CargarPreguntas(_borradores);
            if (_banco.Count == 0) return;
            var q = _banco[UnityEngine.Random.Range(0, _banco.Count)];
            await Dialogo(Guion.Valdivia, "Antes de seguir, doctor: fundamente. Le haré una pregunta. Si acierta, recupera credibilidad.");
            var tcs = new TaskCompletionSource<int?>();
            var fondo = Ui.Panel(_raiz, "Pregunta", new Color(0, 0, 0, 0.8f)).Anclar(0, 0, 1, 1);
            var carta = Ui.PanelRedondeado(fondo, "Carta", Ui.Pergamino).Anclar(0.04f, 0.12f, 0.96f, 0.88f);
            Ui.Texto(Ui.Vacio(carta, "Mat").Anclar(0, 0.9f, 1, 0.98f), $"FUNDAMENTO · {q.Materia}", 34, Ui.Carmesi, TextAnchor.MiddleCenter, FontStyle.Bold);
            if (q.VerificadoPor == Expedientia.Logica.Sesion.MotorDePartida.MarcaBorrador)
                Ui.Texto(Ui.Panel(carta, "Borr", Ui.Hex("#FFF3B0")).Anclar(0.04f, 0.84f, 0.96f, 0.9f), "BORRADOR SIN VERIFICAR: solo para pruebas", 28, Ui.Noche);
            Ui.Texto(Ui.Vacio(carta, "Q").Anclar(0.04f, 0.6f, 0.96f, 0.84f), q.Texto, 42, Ui.Noche);
            for (var i = 0; i < q.Opciones.Count && i < 4; i++)
            {
                var indice = i;
                Ui.Boton(Ui.Vacio(carta, "O" + i).Anclar(0.04f, 0.42f - i * 0.135f, 0.96f, 0.54f - i * 0.135f, 4), $"{(char)('A' + i)}) {q.Opciones[i]}",
                    () => tcs.TrySetResult(indice), Ui.Tinta, Ui.Pergamino, 36);
            }
            var elegida = await tcs.Task;
            Destroy(fondo.gameObject);
            var acierto = elegida == q.IndiceCorrecto;
            _inv!.FundamentoRespondido(acierto);
            await Dialogo(Guion.Valdivia, acierto ? "Correcto. Se le devuelve un punto de credibilidad." : $"No. La respuesta era: {q.Opciones[q.IndiceCorrecto]}. Base legal: {q.BaseLegal}.");
            _saltarDialogos = false;
        }

        // ───────────────────────────── Sentencia ─────────────────────────────

        private void MostrarSentenciaInvestigacion()
        {
            var p = _inv!;
            FuenteLocal.GuardarEstudio(p.Estudio);
            Ui.Limpiar(_raiz);
            Ui.Imagen(Ui.Vacio(_raiz, "Sala").Anclar(0, 0, 1, 1), "Fondo", Arte.Sprite("escena-audiencia"), false).color = new Color(1, 1, 1, 0.3f);
            var fundado = p.Fundado == true;
            var carta = Ui.PanelRedondeado(_raiz, "Carta", Ui.Pergamino).Anclar(0.06f, 0.14f, 0.94f, 0.86f);
            Ui.Texto(Ui.Vacio(carta, "T").Anclar(0.04f, 0.86f, 0.96f, 0.97f), "SENTENCIA", 70, Ui.Noche, TextAnchor.MiddleCenter, FontStyle.Bold);
            var color = fundado ? Ui.Hex("#2F6B2F") : Ui.Carmesi;
            var zona = Ui.Vacio(carta, "Sello").Anclar(0.1f, 0.58f, 0.9f, 0.86f);
            Ui.Imagen(Ui.Vacio(zona, "Aro").Anclar(0.22f, 0f, 0.78f, 1f), "Aro", Arte.Sprite("sello")).color = color;
            Ui.Texto(zona, fundado ? "FUNDADO" : "IMPROCEDENTE", 66, color, TextAnchor.MiddleCenter, FontStyle.Bold);
            zona.localRotation = Quaternion.Euler(0, 0, -7);
            Ui.Texto(Ui.Vacio(carta, "E").Anclar(0.04f, 0.47f, 0.96f, 0.58f), fundado ? new string('★', p.Estrellas) + new string('☆', 3 - p.Estrellas) : "Sin estrellas", 64, Ui.Dorado, TextAnchor.MiddleCenter, FontStyle.Bold);
            var est = p.Estudio;
            Ui.Texto(Ui.Vacio(carta, "H").Anclar(0.04f, 0.3f, 0.96f, 0.47f),
                $"Errores: {p.Errores} · horas que sobraron: {p.HorasRestantes}\nHonorarios: S/ {p.Honorarios} · Experiencia: {p.Experiencia}\nTu estudio: nivel {est.Nivel} · {est.ExpedientesCerrados} casos ganados", 32, Ui.Noche);
            var consejo = fundado ? (p.Estrellas < 3 ? "Para 3 estrellas: gana sin presentar pruebas equivocadas y pidiendo lo correcto en el escrito." : "Caso perfecto. Lion ya está leyendo el siguiente expediente…")
                : "Consejo: escucha las repreguntas y pide pistas a Lion. Cada testimonio se rompe con una prueba del expediente.";
            Ui.Texto(Ui.Vacio(carta, "L").Anclar(0.04f, 0.16f, 0.96f, 0.3f), consejo, 28, Ui.Tinta, TextAnchor.MiddleCenter, FontStyle.Italic);
            Ui.Boton(Ui.Vacio(carta, "V").Anclar(0.04f, 0.03f, 0.48f, 0.14f, 4), "Al estudio", () => { _inv = null; MostrarMenu(); }, Ui.Tinta, Ui.Pergamino, 36);
            Ui.Boton(Ui.Vacio(carta, "Otra").Anclar(0.52f, 0.03f, 0.96f, 0.14f, 4), fundado ? "JUGAR DE NUEVO" : "REINTENTAR", () => _ = EmpezarInvestigacion(p.Caso.Id), Ui.Carmesi, Ui.Pergamino, 36);
            StartCoroutine(Aparecer(zona));
            Sonido.Tocar(fundado ? "victoria" : "derrota");
            if (fundado) Efectos.Confeti(160);
        }
    }
}
