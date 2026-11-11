# Arte del prototipo

Arte original de Expedientes y Dragones, dibujado desde cero con figuras geométricas (caricatura plana con contorno oscuro y la paleta del juego). No usa imágenes, personajes ni estilos de otros juegos. Autor: Leonel Díaz.

| Archivo | Qué es |
| --- | --- |
| `generar.py` | Dibuja todo como SVG en `svg/` (fichas, retratos, escenas, portada, dado y sello) |
| `render.js` | Convierte los SVG a PNG en `cliente-unity/Assets/Resources/arte` con Chromium (Playwright) |

Para cambiar un dibujo: editar `generar.py`, luego `python3 arte/generar.py && node arte/render.js`.

- Fichas y retratos: 256 × 256. Escenas: 1024 × 846 (proporción del tablero). Portada: 1024 × 1024.
- En Unity, `Assets/Scripts/Editor/ImportarArte.cs` importa esta carpeta como sprites sin mipmaps.
- La portada evoca un volcán nevado y arcos de sillar, como pide la identidad visual del GDD, sin reproducir fotografías ni marcas.
- Es arte de prototipo: en producción se reemplaza por el del ilustrador contratado (con cesión de derechos), sin tocar las reglas.
- Técnicas recursivas (v0.9.0): las crestas de la cordillera y de los volcanes usan desplazamiento del punto medio (`cresta`), los muros de sillar se parten en bloques por subdivisión (`_bloques`) y el molle de la portada es un árbol fractal (`rama`). Usan semillas fijas, así que cada corrida produce el mismo dibujo.
- Acabado (v0.10.0): `sombrear` da a cada figura un sombreado de dibujo animado en tres tonos de corte nítido y `_filtro_pintura` agrega brillo en los bordes. Para que una figura quede plana, se le pone `data-plano`. La portada y el logotipo están en `portada.py`. `render.js` carga las tipografías del juego desde `cliente-unity/Assets/Resources/fuentes`.
- Arte generado con IA (provisional, v0.10.1): `gonzabio.png` y `lion.png` (retratos de Gonzabio y Lion) se generó en Canva y no sale de `generar.py`. Por ahora es una vista previa de 200 px ampliada a 256. Se reemplaza por el PNG completo desde `arte/ia/` y, antes del lanzamiento, por el arte de un ilustrador con cesión de derechos.
