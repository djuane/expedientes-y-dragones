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
