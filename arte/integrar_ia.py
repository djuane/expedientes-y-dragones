# Prepara el arte generado con IA (arte/ia/, descargado de Canva) para el juego:
# recorta la portada a la proporción de la pantalla de inicio y reduce los retratos.
# A Gonzabio se le dibujan los lentes que pidió Leonel (el original de Canva no los tiene).
import os
from PIL import Image, ImageDraw

BASE = os.path.dirname(os.path.abspath(__file__))
IA = os.path.join(BASE, 'ia')
SALIDA = os.path.join(BASE, '..', 'cliente-unity', 'Assets', 'Resources', 'arte')


def portada():
    im = Image.open(os.path.join(IA, 'portada.jpg')).convert('RGB')
    w, h = im.size
    alto = round(w * 1498 / 1080)  # la portada ocupa 1080 × 1498 en la pantalla de 1080 × 1920
    y0 = (h - alto) // 2
    im.crop((0, y0, w, y0 + alto)).save(os.path.join(SALIDA, 'portada.jpg'), quality=90)


def lentes(im):
    """Lentes rectangulares de marco negro; coordenadas medidas sobre el retrato a 256 px."""
    S = 4
    k = im.width / 256
    capa = Image.new('RGBA', (im.width * S, im.height * S), (0, 0, 0, 0))
    d = ImageDraw.Draw(capa)
    f = k * S

    def r(x0, y0, x1, y1):
        return [x0 * f, y0 * f, x1 * f, y1 * f]
    negro = (15, 10, 15, 255)
    for cx, cy in ((109, 68), (140, 67.5)):
        d.rounded_rectangle(r(cx - 12, cy - 7, cx + 12, cy + 7), radius=3 * f, fill=(200, 225, 255, 40), outline=negro, width=round(2.2 * f))
        d.line(r(cx - 9, cy - 4, cx - 4, cy - 6), fill=(255, 255, 255, 140), width=round(f))
    d.line(r(121, 67, 128, 66.5), fill=negro, width=round(2 * f))
    d.line(r(97, 66, 90, 64), fill=negro, width=round(2 * f))
    d.line(r(152, 65.5, 159, 63.5), fill=negro, width=round(2 * f))
    im = im.convert('RGBA')
    im.alpha_composite(capa.resize(im.size, Image.LANCZOS))
    return im.convert('RGB')


def retrato(nombre, con_lentes=False):
    im = Image.open(os.path.join(IA, nombre + '.jpg')).convert('RGB').resize((512, 512), Image.LANCZOS)
    if con_lentes:
        im = lentes(im)
    im.save(os.path.join(SALIDA, nombre + '.png'))


portada()
retrato('gonzabio', con_lentes=True)
retrato('lion')
print('Arte IA integrado.')
