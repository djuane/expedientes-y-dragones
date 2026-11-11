# Marco ornamental de página (A4 a 4 unidades por mm: 840 x 1188) y textura de pergamino.
import math
from base import *

W, H = 840, 1188
B = 44  # ancho de la banda


def esquina(x, y, rot):
    s = f'<g transform="translate({x} {y}) rotate({rot})">'
    s += f'<circle r="40" fill="url(#cuero)" stroke="#e2b04a" stroke-width="4"/><circle r="31" fill="none" stroke="#e2b04a" stroke-width="1.5"/>'
    for k in range(8):
        a = k * math.pi / 4
        s += f'<path d="M0 0 L{24 * math.cos(a - .22):.1f} {24 * math.sin(a - .22):.1f} L{30 * math.cos(a):.1f} {30 * math.sin(a):.1f} L{24 * math.cos(a + .22):.1f} {24 * math.sin(a + .22):.1f}Z" fill="#e2b04a" opacity=".9"/>'
    s += '<circle r="9" fill="#7a1420" stroke="#ffe08a" stroke-width="2.5"/>'
    # voluta hacia las bandas
    s += '<path d="M40 0 C70 -6 84 18 108 6 C96 22 70 18 56 12" fill="none" stroke="#e2b04a" stroke-width="3"/>'
    s += '<path d="M0 40 C-6 70 18 84 6 108 C22 96 18 70 12 56" fill="none" stroke="#e2b04a" stroke-width="3"/>'
    return s + '</g>'


def marco():
    d = (lg('cuero', [(0, '#8e1d26'), (.5, '#5e0f18'), (1, '#3a0810')], 0, 0, 1, 1)
         + '<filter id="piel"><feTurbulence type="fractalNoise" baseFrequency="0.9" numOctaves="3" seed="2"/><feColorMatrix values="0 0 0 0 0  0 0 0 0 0  0 0 0 0 0  0 0 0 .5 0"/><feComposite in2="SourceGraphic" operator="in"/></filter>')
    marco_path = f'M0 0 H{W} V{H} H0Z M{B} {B + 26} Q{B} {B} {B + 26} {B} H{W - B - 26} Q{W - B} {B} {W - B} {B + 26} V{H - B - 26} Q{W - B} {H - B} {W - B - 26} {H - B} H{B + 26} Q{B} {H - B} {B} {H - B - 26}Z'
    s = f'<path d="{marco_path}" fill="url(#cuero)" fill-rule="evenodd"/>'
    s += f'<path d="{marco_path}" fill="#000" fill-rule="evenodd" filter="url(#piel)" opacity=".7"/>'
    # filetes dorados
    s += f'<rect x="{B - 6}" y="{B - 6}" width="{W - 2 * B + 12}" height="{H - 2 * B + 12}" rx="30" fill="none" stroke="#e2b04a" stroke-width="3"/>'
    s += f'<rect x="12" y="12" width="{W - 24}" height="{H - 24}" fill="none" stroke="#e2b04a" stroke-width="1.6" opacity=".8"/>'
    s += f'<rect x="{B}" y="{B}" width="{W - 2 * B}" height="{H - 2 * B}" rx="26" fill="none" stroke="#2a0408" stroke-width="3"/>'
    # greca escalonada en las bandas laterales (motivo andino)
    for x0 in (12, W - B + 4):
        y = 120
        while y < H - 140:
            s += f'<path d="M{x0 + 8} {y} h8 v-8 h8 v-8 h8 v8 h-8 v8 h8 v8 h-8 v8 h-8 v-8 h-8Z" fill="#e2b04a" opacity=".55"/>'
            y += 44
    for y0 in (14,):
        x = 140
        while x < W - 150:
            s += f'<path d="M{x} {y0 + 16} h8 v-8 h8 v8 h8 v8 h-8 v8 h-8 v-8 h-8Z" fill="#e2b04a" opacity=".55"/>'
            x += 40
    for c in [(B, B, 0), (W - B, B, 90), (W - B, H - B, 180), (B, H - B, 270)]:
        s += esquina(*c)
    # sello inferior para el número de página
    s += f'<circle cx="{W / 2}" cy="{H - 30}" r="30" fill="#7a1420" stroke="#e2b04a" stroke-width="4"/>'
    s += f'<circle cx="{W / 2}" cy="{H - 30}" r="23" fill="none" stroke="#e2b04a" stroke-width="1.5" stroke-dasharray="3 3"/>'
    guardar('marco', W, H, s, d)


def pergamino():
    d = (rg('pg', [(0, '#f7ecd2'), (.65, '#efdfba'), (1, '#d8bf8c')], .5, .45, .8)
         + '<filter id="manchas"><feTurbulence type="fractalNoise" baseFrequency="0.006" numOctaves="4" seed="9"/><feColorMatrix values="0 0 0 0 .55  0 0 0 0 .38  0 0 0 0 .16  0 0 0 .55 -.12"/></filter>'
         + '<filter id="fibra"><feTurbulence type="fractalNoise" baseFrequency="0.7" numOctaves="2" seed="4"/><feColorMatrix values="0 0 0 0 .4  0 0 0 0 .3  0 0 0 0 .15  0 0 0 .22 0"/></filter>')
    s = f'<rect width="{W}" height="{H}" fill="url(#pg)"/><rect width="{W}" height="{H}" filter="url(#manchas)"/><rect width="{W}" height="{H}" filter="url(#fibra)"/>'
    guardar('pergamino', W, H, s, d)


if __name__ == '__main__':
    marco(); pergamino()
