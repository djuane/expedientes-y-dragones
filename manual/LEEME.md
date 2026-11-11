# Manual del juego

Manual de *Expedientes y Dragones* en formato de libro de rol (A4, 34 páginas): reglas, especialidades, bestiario, zonas, aventura y plantillas. Autor: Leonel Díaz.

**PDF listo:** `salida/Expedientes-y-Dragones-Manual.pdf`

## Cómo se arma

| Archivo | Qué hace |
| --- | --- |
| `arte/personajes.py` | Las seis especialidades de cuerpo entero |
| `arte/criaturas.py` | El bestiario y la escena del dragón de la Carga Procesal |
| `arte/escenas.py` | Portada, Mesa de Partes, Archivo Central, mapa, retratos y dado |
| `arte/marco.py` | Marco de página y textura de pergamino |
| `arte/rasterizar.js` | Pasa los SVG a imágenes de alta resolución en `img/` |
| `construir.py` | Texto y maquetación: genera `manual.html` |
| `estilo.css` | Diseño de página, tipografía y fichas |
| `pdf.js` | Imprime el PDF y avisa si algún texto se sale de su página |
| `metadatos.py` | Pone título y autor al PDF |

Para regenerar todo (necesita Python 3 y Node con Playwright/Chromium):

```bash
cd manual/arte
python3 personajes.py && python3 criaturas.py && python3 escenas.py && python3 marco.py && node rasterizar.js
cd ..
python3 construir.py && node pdf.js && python3 metadatos.py
```

Para cambiar solo el texto basta con editar `construir.py` y correr la última línea.

## Notas

- Todo el arte es original y se dibuja desde código; no usa imágenes, logotipos ni personajes de otros juegos.
- Las fuentes (Cinzel, Cinzel Decorative, EB Garamond) son de Google Fonts con licencia SIL Open Font License y están en `fuentes/`.
- Civilista y Administrativista, el bestiario de Mesa de Partes y de la Sala de Contrataciones, los objetos de la librería y la progresión usan los números del código (`logica/`). Las otras cuatro especialidades, la Carga Procesal y las reglas marcadas como opcionales son propuestas para el juego en mesa.
- El texto resaltado en amarillo es un dato por verificar (el enlace a la edición oficial del TUO de la Ley 27444 en gob.pe).
