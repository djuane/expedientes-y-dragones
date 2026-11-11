# Bestiario: criaturas del sistema (500 x 500) y el gran dragón de la Carga Procesal.
import math, random
from base import *

W = H = 500


def lienzo(nombre, cuerpo, c1, c2, defs='', piso=True):
    d, s = fondo_aura(W, H, c1, c2, '#07040a', 'aura')
    rnd = random.Random(nombre)
    s += chispas(rnd, 30, 10, 10, 490, 420, claro(c1, .3), 1, 2.6, (.2, .6))
    if piso:
        s += f'<ellipse cx="250" cy="462" rx="170" ry="24" fill="#000" opacity=".55" filter="url(#difuso)"/>'
    s += cuerpo
    dv, sv = vineta(W, H, 'vin', .7)
    s += sv + f'<rect width="{W}" height="{H}" fill="#fff" filter="url(#grano)" opacity=".16"/>'
    guardar(nombre, W, H, s, d + dv + defs)


def ojos_malos(x1, x2, y, r=13, iris='#ff3b2f', ceja=True):
    s = ''
    for x in (x1, x2):
        s += (f'<circle cx="{x}" cy="{y}" r="{r * 1.6}" fill="{iris}" opacity=".45" filter="url(#difuso)"/>'
              f'<ellipse cx="{x}" cy="{y}" rx="{r}" ry="{r * .8}" fill="#fff8e0" {T(3.5)}/>'
              f'<circle cx="{x}" cy="{y + 1}" r="{r * .55}" fill="{iris}"/><circle cx="{x}" cy="{y + 1}" r="{r * .25}" fill="#120808"/>'
              f'<circle cx="{x - r * .25}" cy="{y - r * .25}" r="{r * .15}" fill="#fff"/>')
    if ceja:
        s += (f'<path d="M{x1 - r * 1.3} {y - r * 1.4} L{x1 + r * 1.1} {y - r * .6}" {T(6)}/>'
              f'<path d="M{x2 + r * 1.3} {y - r * 1.4} L{x2 - r * 1.1} {y - r * .6}" {T(6)}/>')
    return s


def boca_dientes(x, y, w, h, n=7):
    s = f'<path d="M{x - w / 2} {y} Q{x} {y + h * 2} {x + w / 2} {y} Q{x} {y + h * .6} {x - w / 2} {y}Z" fill="#5a0f18" {T(4)}/>'
    for i in range(n):
        xx = x - w / 2 + w * (i + .5) / n
        s += f'<path d="M{xx - w / n / 2.4} {y + 3} L{xx} {y + h * .75} L{xx + w / n / 2.4} {y + 3}Z" fill="#fff8e6" {T(1.8)}/>'
    return s


def formalismo():
    d = volumen('papel', '#efe4c6') + lg('sombraP', [(0, '#000', 0), (1, '#000', .35)], 0, 0, 1, 0)
    s = ''
    # brazos de papel enrollado
    s += f'<path d="M150 230 Q80 250 70 330" fill="none" {T(36)}/><path d="M150 230 Q80 250 70 330" fill="none" stroke="#e8dbb8" stroke-width="28" stroke-linecap="round"/>'
    s += f'<path d="M350 230 Q430 220 440 140" fill="none" {T(36)}/><path d="M350 230 Q430 220 440 140" fill="none" stroke="#d8caa4" stroke-width="28" stroke-linecap="round"/>'
    # garras de clip
    s += f'<path d="M58 330 q-10 30 10 40 M70 336 q0 34 18 36 M84 332 q8 30 26 28" fill="none" stroke="#9aa4b0" stroke-width="7" stroke-linecap="round"/>'
    s += f'<path d="M430 136 q-6 -34 16 -40 M442 140 q6 -34 28 -30 M450 150 q22 -18 36 0" fill="none" stroke="#9aa4b0" stroke-width="7" stroke-linecap="round"/>'
    # cuerpo: formulario gigante
    s += f'<path d="M150 90 L330 80 L362 112 L356 440 L144 446 Z" fill="url(#papel)" {T(6)} filter="url(#sombra)"/>'
    s += f'<path d="M330 80 L326 116 L362 112Z" fill="#cdbf98" {T(4)}/>'
    s += f'<path d="M356 112 L356 440 L300 442 Q336 300 330 116Z" fill="url(#sombraP)"/>'
    for i, y in enumerate(range(300, 430, 22)):
        s += f'<line x1="176" y1="{y}" x2="{330 - (i % 2) * 40}" y2="{y}" stroke="#8a7a62" stroke-width="4"/>'
    for i, y in enumerate((300, 344, 388)):
        s += f'<rect x="300" y="{y - 12}" width="18" height="18" fill="none" stroke="#5a4a3a" stroke-width="3"/>'
    s += f'<path d="M303 298 l6 7 l12 -16" stroke="#b02a2a" stroke-width="5" fill="none"/>'
    s += ojos_malos(206, 288, 170, 18)
    s += boca_dientes(248, 226, 130, 30, 8)
    # sello de "REQUISITO" en la frente, torcido
    s += (f'<g transform="translate(250 112) rotate(-8)"><rect x="-70" y="-17" width="140" height="34" rx="4" fill="none" stroke="#c0282a" stroke-width="5"/>'
          f'<text x="0" y="9" text-anchor="middle" font-family="Georgia,serif" font-weight="700" font-size="22" fill="#c0282a" letter-spacing="3">REQUISITO</text></g>')
    # piernas
    s += f'<rect x="178" y="440" width="34" height="26" rx="8" fill="#5a4a3a" {T(4)}/><rect x="288" y="440" width="34" height="26" rx="8" fill="#5a4a3a" {T(4)}/>'
    lienzo('cr-formalismo', s, '#7a2a24', '#2a0f12', d)


def plazo():
    d = lg('vidrio', [(0, '#dff4ff', .7), (1, '#7fb0d0', .35)], 0, 0, 1, 1) + rg('fuego', [(0, '#fff6b0'), (.4, '#ffb030'), (1, '#e0301a')], .5, .8, .7)
    s = ''
    # llamas
    s += f'<ellipse cx="250" cy="110" rx="120" ry="90" fill="#ff7a2a" opacity=".5" filter="url(#brilloG)"/>'
    s += (f'<path d="M160 140 Q150 90 186 60 Q184 96 206 102 Q196 50 240 20 Q236 70 262 84 Q270 40 300 30 Q296 80 318 96 Q340 70 336 40 Q372 80 344 140Z" fill="url(#fuego)" {T(4)}/>'
          f'<path d="M200 140 Q206 106 226 96 Q230 116 246 118 Q248 90 270 74 Q272 110 290 116 Q306 104 304 90 Q320 120 304 140Z" fill="#fff2a0" opacity=".9"/>')
    # marco de madera
    s += f'<rect x="140" y="132" width="220" height="30" rx="8" fill="#6b3f22" {T(5)}/><rect x="140" y="400" width="220" height="30" rx="8" fill="#6b3f22" {T(5)}/>'
    s += f'<rect x="150" y="160" width="16" height="242" fill="#5a341c" {T(4)}/><rect x="334" y="160" width="16" height="242" fill="#5a341c" {T(4)}/>'
    # vidrio
    s += f'<path d="M180 162 L320 162 Q320 240 262 282 Q320 322 320 400 L180 400 Q180 322 238 282 Q180 240 180 162Z" fill="url(#vidrio)" {T(5)}/>'
    s += f'<path d="M206 216 Q250 236 294 216 Q284 254 256 276 L244 276 Q216 254 206 216Z" fill="#e8b85a" {T(2)}/>'
    s += f'<path d="M248 280 L248 360" stroke="#e8b85a" stroke-width="4" stroke-dasharray="4 6"/>'
    s += f'<path d="M196 398 Q250 330 304 398Z" fill="#e8b85a" {T(3)}/>'
    s += f'<path d="M194 176 Q196 226 228 258" stroke="#fff" stroke-width="6" fill="none" opacity=".7"/>'
    s += ojos_malos(220, 280, 192, 13)
    s += f'<path d="M226 238 Q250 226 274 238" fill="none" {T(5)}/>'
    # brazos y piernas
    s += f'<path d="M150 260 Q100 250 90 200" fill="none" {T(14)}/><path d="M350 260 Q404 270 420 220" fill="none" {T(14)}/>'
    s += f'<path d="M200 430 L190 466 M300 430 L310 466" {T(14)}/>'
    # números de días que se consumen
    for i, (x, y, n) in enumerate([(84, 160, '3'), (420, 180, '2'), (430, 330, '1'), (70, 320, '0')]):
        s += f'<text x="{x}" y="{y}" font-family="Georgia,serif" font-size="{44 - i * 4}" font-weight="700" fill="#ffb84a" opacity="{.9 - i * .15}" filter="url(#brillo)" text-anchor="middle">{n}</text>'
    lienzo('cr-plazo', s, '#a04a14', '#2a1006', d)


def notificacion():
    d = volumen('sobre', '#efe2c2') + lg('ala', [(0, '#6a4a7a'), (1, '#24142e')], 0, 0, 1, 1)
    s = ''
    for lado in (-1, 1):
        sx = 250 + lado * 90
        s += (f'<path d="M{sx} 220 Q{250 + lado * 200} 120 {250 + lado * 236} 170 Q{250 + lado * 210} 200 {250 + lado * 214} 236 '
              f'Q{250 + lado * 180} 228 {250 + lado * 176} 262 Q{250 + lado * 146} 250 {250 + lado * 130} 280 Z" fill="url(#ala)" {T(5)}/>')
        s += f'<path d="M{sx} 222 L{250 + lado * 236} 170 M{sx} 230 L{250 + lado * 214} 236 M{sx} 240 L{250 + lado * 176} 262" stroke="#8a6a9a" stroke-width="3"/>'
    s += f'<rect x="150" y="170" width="200" height="140" rx="8" fill="url(#sobre)" {T(6)} filter="url(#sombra)"/>'
    s += f'<path d="M150 178 L250 254 L350 178" fill="none" {T(5)}/><path d="M150 304 L222 238 M350 304 L278 238" stroke="#b8a882" stroke-width="4"/>'
    s += ojos_malos(212, 288, 214, 13, '#ffcf3a')
    s += f'<circle cx="250" cy="262" r="22" fill="#b0202a" {T(4)}/><path d="M240 262 Q250 254 260 262 Q250 272 240 262" fill="#5a0a10"/>'
    s += f'<text x="250" y="292" text-anchor="middle" font-family="Georgia,serif" font-size="13" fill="#5a4a3a" font-style="italic">Sr(a). ¿a quién?</text>'
    # flechas de dirección equivocada
    rnd = random.Random(3)
    for i in range(5):
        x, y, a = rnd.uniform(60, 440), rnd.uniform(340, 430), rnd.uniform(0, 360)
        s += f'<g transform="translate({x:.0f} {y:.0f}) rotate({a:.0f})"><path d="M-22 0 L14 0 M6 -9 L16 0 L6 9" fill="none" stroke="#ffcf3a" stroke-width="5" opacity=".75"/></g>'
    lienzo('cr-notificacion', s, '#5a3a7a', '#140a20', d, piso=False)


def usuario():
    d = lg('fant', [(0, '#c8cfe0', .95), (1, '#6a7290', .3)])
    s = (f'<path d="M180 470 Q170 300 190 230 Q200 170 250 168 Q300 170 310 230 Q330 300 320 470 Q300 450 285 470 Q268 452 250 470 Q232 452 215 470 Q198 452 180 470Z" fill="url(#fant)" {T(4)}/>'
         f'<circle cx="250" cy="150" r="52" fill="#c8cfe0" {T(4)}/>'
         f'<path d="M226 150 q8 6 16 0 M258 150 q8 6 16 0" fill="none" {T(3.5)}/><path d="M232 182 q18 -8 36 0" fill="none" {T(3.5)}/>'
         f'<path d="M222 166 q4 4 0 8 M278 166 q-4 4 0 8" stroke="#6a7290" stroke-width="2" fill="none"/>')
    s += (f'<g transform="translate(300 290) rotate(12)"><rect x="-34" y="-46" width="68" height="92" rx="4" fill="#f4ecd6" {T(4)}/>'
          f'<text x="0" y="-18" text-anchor="middle" font-family="Georgia,serif" font-size="14" fill="#5a4a3a">TURNO</text>'
          f'<text x="0" y="22" text-anchor="middle" font-family="Georgia,serif" font-size="34" font-weight="700" fill="#b02a2a">947</text></g>')
    s += f'<text x="140" y="110" font-family="Georgia,serif" font-size="30" fill="#c8cfe0" opacity=".7">z</text><text x="112" y="80" font-family="Georgia,serif" font-size="22" fill="#c8cfe0" opacity=".5">z</text>'
    lienzo('cr-usuario', s, '#3a4060', '#0e1020', d)


def cola():
    """La Cola Infinita: serpiente hecha de gente en fila con boca de dispensador de turnos."""
    d = volumen('disp', '#c0392b') + lg('cuerpoC', [(0, '#6a7290'), (1, '#2a2f48')])
    s = ''
    pts = []
    for i in range(60):
        t = i / 59
        x = 250 + (40 + 190 * t) * math.cos(5.2 * t + 1.2)
        y = 300 - 210 * t + (30 + 80 * t) * math.sin(5.2 * t + 1.2) * .5
        pts.append((x, y))
    for i, (x, y) in enumerate(pts[::-1]):
        r = 14 + (len(pts) - i) * .55
        r = 14 + i * .55
    for i in range(len(pts) - 1, -1, -1):
        x, y = pts[i]
        r = 44 - i * .55
        s += f'<g><ellipse cx="{x:.0f}" cy="{y:.0f}" rx="{r:.0f}" ry="{r * 1.2:.0f}" fill="url(#cuerpoC)" {T(3)}/>'
        if i % 3 == 0:
            s += f'<circle cx="{x:.0f}" cy="{y - r * 1.1:.0f}" r="{r * .45:.0f}" fill="#8a92b0" {T(2.5)}/>'
        s += '</g>'
    # cabeza: dispensador de turnos con fauces
    hx, hy = pts[0]
    s += f'<g transform="translate(250 330) scale(1.25)">'
    s += f'<rect x="-70" y="-90" width="140" height="150" rx="24" fill="url(#disp)" {T(6)} filter="url(#sombra)"/>'
    s += f'<rect x="-46" y="-70" width="92" height="40" rx="6" fill="#1a1a24" {T(4)}/><text x="0" y="-40" text-anchor="middle" font-family="monospace" font-size="30" fill="#ff5a3a" filter="url(#brillo)">∞</text>'
    s += ojos_malos(-30, 30, -2, 12, '#ffd23a')
    s += boca_dientes(0, 28, 110, 26, 9)
    s += f'<path d="M-20 74 L-20 120 L20 120 L20 74" fill="#f4ecd6" {T(3)}/><text x="0" y="106" text-anchor="middle" font-family="Georgia,serif" font-size="18" font-weight="700" fill="#b02a2a">1998</text>'
    s += '</g>'
    lienzo('cr-cola', s, '#6a3a2a', '#140a10', d)


def vuelva():
    d = volumen('cab', '#4a6a8a') + lg('vidrioV', [(0, '#bfe0ff', .6), (1, '#5a7a9a', .4)], 0, 0, 1, 1)
    s = ''
    # brazos que empujan
    s += f'<path d="M140 280 Q70 290 46 250" fill="none" {T(30)}/><path d="M140 280 Q70 290 46 250" fill="none" stroke="#4a6a8a" stroke-width="22" stroke-linecap="round"/>'
    s += f'<path d="M360 280 Q440 290 456 240" fill="none" {T(30)}/><path d="M360 280 Q440 290 456 240" fill="none" stroke="#3a5a7a" stroke-width="22" stroke-linecap="round"/>'
    s += f'<rect x="20" y="210" width="48" height="56" rx="12" fill="#f0d0a8" {T(4)}/><rect x="432" y="200" width="48" height="56" rx="12" fill="#f0d0a8" {T(4)}/>'
    # cuerpo: caseta de ventanilla
    s += f'<path d="M130 120 L370 120 L384 450 L116 450Z" fill="url(#cab)" {T(6)} filter="url(#sombra)"/>'
    s += f'<path d="M120 100 L380 100 L392 130 L108 130Z" fill="#2a3a5a" {T(5)}/>'
    s += f'<text x="250" y="122" text-anchor="middle" font-family="Georgia,serif" font-size="20" font-weight="700" fill="#e2b04a" letter-spacing="2">VENTANILLA 3</text>'
    s += f'<rect x="160" y="150" width="180" height="150" rx="8" fill="url(#vidrioV)" {T(5)}/>'
    # persiana a medio cerrar = párpado
    s += f'<rect x="160" y="150" width="180" height="70" fill="#8a9aaa" {T(4)}/>'
    for y in range(160, 220, 12):
        s += f'<line x1="162" y1="{y}" x2="338" y2="{y}" stroke="#5a6a7a" stroke-width="3"/>'
    s += ojos_malos(212, 288, 238, 14, '#ff8a2a', ceja=False)
    s += f'<path d="M210 278 Q250 268 290 278" fill="none" {T(5)}/>'
    # cartel
    s += (f'<g transform="translate(250 360) rotate(-5)"><rect x="-110" y="-30" width="220" height="60" rx="6" fill="#f6ecd2" {T(5)}/>'
          f'<text x="0" y="10" text-anchor="middle" font-family="Georgia,serif" font-size="26" font-weight="700" fill="#b02a2a">VUELVA MAÑANA</text></g>')
    s += f'<path d="M230 330 L250 316 L270 330" fill="none" {T(3)}/>'
    lienzo('cr-vuelva', s, '#2a4a6a', '#0a1420', d)


def baba(x, y, r, color, ojos=True):
    s = (f'<path d="M{x - r} {y} Q{x - r} {y - r * 1.3} {x} {y - r * 1.3} Q{x + r} {y - r * 1.3} {x + r} {y} '
         f'Q{x + r * .7} {y + r * .3} {x + r * .4} {y + r * .05} Q{x + r * .2} {y + r * .4} {x} {y + r * .1} Q{x - r * .3} {y + r * .4} {x - r * .5} {y + r * .05} Q{x - r * .8} {y + r * .3} {x - r} {y}Z" '
         f'fill="{color}" fill-opacity=".85" {T(4)}/>'
         f'<ellipse cx="{x - r * .4}" cy="{y - r * .9}" rx="{r * .25}" ry="{r * .14}" fill="#fff" opacity=".7"/>')
    if ojos:
        s += ojos_malos(x - r * .3, x + r * .3, y - r * .6, r * .16, '#fff04a', ceja=False)
    return s


def fraccionador():
    rnd = random.Random(8)
    s = ''
    s += baba(250, 360, 130, '#5ac08a')
    # recibos dentro
    for i in range(7):
        s += hoja(200 + rnd.uniform(-50, 100), 300 + rnd.uniform(-30, 50), 30, 40, rnd.uniform(-50, 50), '#f6eed8', sello='#b02a2a')
    s += boca_dientes(250, 330, 90, 18, 6)
    s += baba(90, 430, 50, '#6ad09a') + baba(410, 440, 42, '#6ad09a')
    s += f'<path d="M140 380 Q120 400 112 410 M360 390 Q380 410 392 420" stroke="#5ac08a" stroke-width="10" stroke-linecap="round" opacity=".7"/>'
    s += f'<text x="250" y="120" text-anchor="middle" font-family="Georgia,serif" font-size="30" font-weight="700" fill="#bfffd8" opacity=".8" filter="url(#brillo)">÷ ÷ ÷</text>'
    lienzo('cr-fraccionador', s, '#1e5a3a', '#06140c')


def requisito():
    d = volumen('perg', '#f2e6c4') + volumen('capa', '#8a1a2a') + volumen('oro', '#e2b04a')
    s = ''
    s += f'<path d="M120 170 Q60 330 90 470 L410 470 Q440 330 380 170Z" fill="url(#capa)" {T(6)}/>'
    s += f'<path d="M150 180 Q110 320 130 460" stroke="#e2b04a" stroke-width="6" fill="none"/><path d="M350 180 Q390 320 370 460" stroke="#e2b04a" stroke-width="6" fill="none"/>'
    s += f'<path d="M170 150 L330 150 L338 420 Q300 400 270 430 Q240 400 210 430 Q186 404 162 420Z" fill="url(#perg)" {T(6)} filter="url(#sombra)"/>'
    s += f'<rect x="150" y="128" width="200" height="34" rx="16" fill="#d8c08a" {T(5)}/>'
    s += f'<path d="M168 128 L176 72 L208 106 L250 58 L292 106 L324 72 L332 128Z" fill="url(#oro)" {T(5)}/>'
    for x, c in ((208, '#3a9a5a'), (250, '#b0202a'), (292, '#3a6ab0')):
        s += f'<circle cx="{x}" cy="{116 if x != 250 else 100}" r="8" fill="{c}" {T(2.5)}/>'
    s += ojos_malos(220, 280, 196, 14, '#ffcf3a')
    s += f'<path d="M222 234 Q250 222 278 234" fill="none" {T(5)}/>'
    lineas = ['Copia legalizada', 'del original', 'de la copia', 'Firma del finado']
    for i, t in enumerate(lineas):
        y = 278 + i * 32
        s += f'<rect x="184" y="{y - 14}" width="16" height="16" fill="none" stroke="#5a4a3a" stroke-width="3"/>'
        s += f'<text x="208" y="{y}" font-family="Georgia,serif" font-size="15" fill="#4a3a2a" font-style="italic">{t}</text>'
        s += f'<path d="M184 {y - 14} l16 16 M200 {y - 14} l-16 16" stroke="#b02a2a" stroke-width="3"/>'
    # cetro de cinta roja
    s += f'<rect x="384" y="150" width="12" height="300" rx="5" fill="#6b4524" {T(4)} transform="rotate(10 390 300)"/>'
    s += f'<path d="M420 140 C380 80 470 60 440 120 C430 150 380 170 360 130" fill="none" stroke="#c0282a" stroke-width="12" stroke-linecap="round"/>'
    s += f'<path d="M420 140 C380 80 470 60 440 120 C430 150 380 170 360 130" fill="none" {T(2)}/>'
    s += f'<circle cx="420" cy="140" r="60" fill="#ff4a3a" opacity=".3" filter="url(#brilloG)"/>'
    lienzo('cr-requisito', s, '#6a1a2a', '#14060a', d)


def figura_sombria(saco, sombrero, extra, sonrisa_oro=False, anteojos=False):
    s = f'<path d="M140 470 Q150 290 200 250 L300 250 Q350 290 360 470Z" fill="url(#gab)" {T(6)} filter="url(#sombra)"/>'
    s += f'<path d="M210 252 L250 330 L290 252" fill="#d8d0c0" {T(4)}/><path d="M244 266 L256 266 L262 330 L250 344 L238 330Z" fill="#7a1a1a" {T(3)}/>'
    s += f'<path d="M196 256 L240 360 L206 320Z M304 256 L260 360 L294 320Z" fill="{oscuro(saco, .2)}" {T(4)}/>'
    s += f'<rect x="232" y="214" width="36" height="44" fill="#8a5a3e" {T(4)}/>'
    s += f'<ellipse cx="250" cy="180" rx="52" ry="60" fill="#a8704e" {T(5)}/>'
    s += f'<path d="M200 186 Q250 214 300 186 Q300 240 250 240 Q200 240 200 186Z" fill="#000" opacity=".25"/>'
    if anteojos:
        s += f'<path d="M206 172 L294 172 L290 196 Q270 204 256 186 L244 186 Q230 204 210 196Z" fill="#141018" {T(3)}/><path d="M214 178 l14 0" stroke="#fff" stroke-width="3" opacity=".5"/>'
    else:
        s += f'<path d="M214 178 Q226 170 238 178 M262 178 Q274 170 286 178" fill="none" {T(4)}/><circle cx="226" cy="182" r="4" fill="#fff4b0" filter="url(#brillo)"/><circle cx="274" cy="182" r="4" fill="#fff4b0" filter="url(#brillo)"/>'
    s += f'<path d="M222 210 Q250 232 280 208" fill="#fff" {T(4)}/>'
    if sonrisa_oro:
        s += f'<rect x="258" y="212" width="10" height="10" fill="#f2c04a" {T(1.5)}/>'
    s += f'<path d="M180 140 Q182 100 250 98 Q318 100 320 140Z" fill="{sombrero}" {T(5)}/><path d="M156 142 Q250 120 344 142 Q250 160 156 142Z" fill="{oscuro(sombrero, .2)}" {T(5)}/>'
    s += f'<rect x="184" y="124" width="132" height="12" fill="#7a1a1a" opacity=".8"/>'
    return s + extra


def tramitador():
    d = volumen('gab', '#5a4a3a')
    rnd = random.Random(10)
    extra = ''
    for i in range(5):
        extra += f'<g transform="translate({120 + i * 8} {330 - i * 10}) rotate({-20 + i * 6})"><rect x="-40" y="-26" width="80" height="54" rx="3" fill="#c8963a" {T(3.5)}/><rect x="-36" y="-34" width="28" height="10" rx="2" fill="#c8963a" {T(2.5)}/></g>'
    extra += f'<circle cx="120" cy="320" r="22" fill="#a8704e" {T(4)}/>'
    extra += f'<text x="390" y="200" font-family="Georgia,serif" font-size="22" font-style="italic" fill="#e8d8b0" opacity=".85">psst…</text>'
    lienzo('cr-tramitador', figura_sombria('#5a4a3a', '#3a2e26', extra), '#4a3a2a', '#100a06', d)


def coimero():
    d = volumen('gab', '#2e5a3e')
    rnd = random.Random(11)
    extra = f'<ellipse cx="250" cy="300" rx="190" ry="160" fill="#4aff9a" opacity=".12" filter="url(#brilloG)"/>'
    for i in range(6):
        a = -50 + i * 20
        extra += f'<g transform="translate(360 330) rotate({a})"><rect x="-8" y="-90" width="56" height="36" rx="3" fill="#f0e6cc" {T(3)}/><path d="M-8 -90 L20 -70 L48 -90" fill="none" {T(2.5)}/></g>'
    extra += f'<circle cx="362" cy="336" r="22" fill="#a8704e" {T(4)}/>'
    for i in range(8):
        x, y = rnd.uniform(60, 440), rnd.uniform(60, 440)
        extra += f'<text x="{x:.0f}" y="{y:.0f}" font-family="Georgia,serif" font-size="{rnd.randint(18, 30)}" font-weight="700" fill="#7affb0" opacity=".5">S/</text>'
    lienzo('cr-coimero', figura_sombria('#2e5a3e', '#1e2a22', extra, sonrisa_oro=True, anteojos=True), '#1e4a2e', '#050e08', d)


def expediente_vivo():
    d = volumen('carp', '#d2a04a')
    s = f'<path d="M180 440 L170 470 M320 440 L330 470" {T(14)}/>'
    s += f'<path d="M150 400 Q110 360 120 300" fill="none" {T(14)}/><path d="M350 400 Q400 380 410 330" fill="none" {T(14)}/>'
    s += f'<path d="M140 160 L210 160 L226 140 L360 140 L360 440 L140 440Z" fill="url(#carp)" {T(6)} filter="url(#sombra)"/>'
    rnd = random.Random(12)
    for i in range(6):
        s += f'<path d="M{150 + i * 4} {150 - i * 6} L{350 - i * 3} {150 - i * 6 + rnd.uniform(-4, 4):.0f}" stroke="#f4ecd6" stroke-width="10"/>'
    s += f'<path d="M140 190 L360 190" stroke="#a07028" stroke-width="5"/>'
    s += ojos_malos(212, 288, 268, 16, '#ff6a2a')
    s += boca_dientes(250, 330, 110, 24, 7)
    s += (f'<g transform="translate(250 400)"><rect x="-80" y="-20" width="160" height="40" rx="4" fill="#f6ecd2" {T(3)}/>'
          f'<text x="0" y="8" text-anchor="middle" font-family="monospace" font-size="18" fill="#3a2a1a">EXP. 00113-1998</text></g>')
    s += f'<path d="M120 300 l-14 -12 M120 300 l-2 -18 M120 300 l12 -14 M410 330 l14 -12 M410 330 l2 -18 M410 330 l-12 -14" {T(6)}/>'
    lienzo('cr-expediente', s, '#7a4a1a', '#140a04', d)


def dragon_carga():
    """La Carga Procesal: dragón hecho de expedientes apilados sobre la ciudad (escena 1000 x 700)."""
    Wd, Hd = 1000, 700
    rnd = random.Random(99)
    d = (lg('cielo', [(0, '#1a0c24'), (.55, '#5a1a2a'), (1, '#d0602a')]) + rg('sol', [(0, '#ffe2a0'), (.4, '#ff9a4a', .6), (1, '#ff6a2a', 0)])
         + lg('monte', [(0, '#3a2440'), (1, '#120a18')]) + lg('nieve', [(0, '#fff6ea'), (1, '#d8c8d0')]) + volumen('esc', '#5a2a34')
         + lg('ala2', [(0, '#7a2a34', .95), (1, '#2a0a14', .95)], 0, 0, 1, 1) + lg('ciudad', [(0, '#2a1a2a'), (1, '#0a0610')]))
    s = f'<rect width="{Wd}" height="{Hd}" fill="url(#cielo)"/>'
    s += f'<circle cx="760" cy="430" r="260" fill="url(#sol)"/>'
    s += f'<g filter="url(#nube)" opacity=".5"><ellipse cx="300" cy="200" rx="300" ry="50" fill="#a04a5a"/><ellipse cx="800" cy="140" rx="260" ry="40" fill="#8a3a5a"/></g>'
    # volcán nevado
    pts = [(200, 560)] + cresta(rnd, 200, 560, 480, 250, 30, 5) + cresta(rnd, 480, 250, 560, 252, 6, 3) + cresta(rnd, 560, 252, 860, 560, 30, 5)
    s += '<path d="M' + ' L'.join(f'{x:.0f} {y:.0f}' for x, y in pts) + 'Z" fill="url(#monte)"/>'
    s += f'<path d="M480 250 L560 252 L600 300 L580 296 L566 318 L548 300 L530 322 L512 300 L494 316 L470 296 L446 302Z" fill="url(#nieve)"/>'
    # ciudad de sillar con arcos y cúpulas
    s += f'<rect x="0" y="560" width="{Wd}" height="140" fill="url(#ciudad)"/>'
    x = 0
    while x < Wd:
        w = rnd.randint(50, 110)
        h = rnd.randint(40, 110)
        s += f'<rect x="{x}" y="{600 - h}" width="{w}" height="{h + 100}" fill="#2a1a2a"/>'
        for k in range(w // 26):
            s += f'<path d="M{x + 6 + k * 26} {640 - h * .3:.0f} l0 -14 q8 -12 16 0 l0 14z" fill="#ffb05a" opacity=".55"/>'
        x += w
    s += f'<path d="M420 470 L420 600 L580 600 L580 470 Q500 420 420 470Z" fill="#2a1a2a"/><path d="M440 470 Q500 380 560 470" fill="#2a1a2a"/>'
    s += f'<rect x="494" y="350" width="12" height="60" fill="#2a1a2a"/><path d="M488 352 L512 352 L500 330Z" fill="#2a1a2a"/>'
    # dragón: cuerpo de expedientes apilados (segmentos de carpetas)
    cuerpo = []
    for i in range(40):
        t = i / 39
        cx = 110 + t * 500 + 40 * math.sin(t * 6)
        cy = 490 - 230 * math.sin(t * math.pi * .9) + 30 * math.cos(t * 9)
        cuerpo.append((cx, cy, 18 + 26 * math.sin(t * math.pi * .9 + .2)))
    # alas
    s += (f'<path d="M480 270 Q420 60 220 40 Q300 110 260 150 Q200 130 150 150 Q230 190 200 240 Q260 230 330 300Z" fill="url(#ala2)" {T(5)}/>'
          f'<path d="M560 270 Q640 40 860 20 Q780 100 820 140 Q890 130 940 150 Q850 190 880 240 Q800 230 700 300Z" fill="url(#ala2)" {T(5)}/>')
    for (a, b) in [((480, 270), (220, 40)), ((440, 280), (150, 150)), ((420, 290), (200, 240)), ((560, 270), (860, 20)), ((600, 280), (940, 150)), ((620, 290), (880, 240))]:
        s += f'<path d="M{a[0]} {a[1]} L{b[0]} {b[1]}" stroke="#3a0a14" stroke-width="5"/>'
    # hojas sueltas en el borde del ala
    for i in range(18):
        s += hoja(rnd.uniform(160, 900), rnd.uniform(30, 260), 18, 24, rnd.uniform(-60, 60), '#e8dcc0', lineas=False)
    for i, (cx, cy, r) in enumerate(cuerpo):
        col = ['#c8963a', '#d8a84a', '#b8863a', '#e2c070'][i % 4]
        s += (f'<g transform="translate({cx:.0f} {cy:.0f}) rotate({math.degrees(math.atan2(math.cos(i / 6), 1)) * 1.4:.0f})">'
              f'<rect x="{-r:.0f}" y="{-r * .7:.0f}" width="{r * 2:.0f}" height="{r * 1.4:.0f}" rx="4" fill="{col}" {T(3.5)}/>'
              f'<line x1="{-r * .8:.0f}" y1="{-r * .3:.0f}" x2="{r * .8:.0f}" y2="{-r * .3:.0f}" stroke="#f6ecd2" stroke-width="{max(2, r * .12):.0f}"/>'
              f'<rect x="{-r * .4:.0f}" y="{r * .05:.0f}" width="{r * .8:.0f}" height="{r * .3:.0f}" fill="#f6ecd2" opacity=".8"/></g>')
    # cabeza
    hx, hy = cuerpo[-1][0] + 50, cuerpo[-1][1] - 40
    s += f'<g transform="translate({hx:.0f} {hy:.0f})">'
    s += f'<circle cx="40" cy="20" r="120" fill="#ff7a2a" opacity=".35" filter="url(#brilloG)"/>'
    s += f'<path d="M-70 -20 Q-60 -80 20 -80 Q100 -70 140 -10 L150 20 Q100 30 60 20 Q90 50 120 70 Q40 90 -20 50 Q-80 30 -70 -20Z" fill="url(#esc)" {T(6)}/>'
    s += f'<path d="M-30 -70 L-60 -140 L0 -84 M20 -80 L30 -160 L60 -78" fill="#e2c070" {T(5)}/>'
    s += f'<path d="M60 20 L70 32 L80 20 L90 32 L100 20 L110 32 L120 20" fill="#fff6e0" {T(2)}/>'
    s += f'<ellipse cx="40" cy="-34" rx="18" ry="12" fill="#ffe24a" {T(3)} filter="url(#brillo)"/><ellipse cx="42" cy="-34" rx="4" ry="10" fill="#1a0a0a"/>'
    s += f'<path d="M10 -54 L66 -44" {T(7)}/>'
    s += '</g>'
    # aliento de sellos ardientes
    for i in range(16):
        t = i / 15
        x, y = hx + 150 + t * 120, hy + 40 + t * 120 + rnd.uniform(-15, 15)
        s += f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{10 + t * 26:.0f}" fill="#ff8a2a" opacity="{.7 - t * .35:.2f}" filter="url(#difuso)"/>'
    for i in range(6):
        x, y = hx + 160 + i * 20, hy + 50 + i * 20
        s += f'<g transform="translate({x:.0f} {y:.0f}) rotate({rnd.uniform(-30, 30):.0f})"><circle r="16" fill="none" stroke="#ffe0a0" stroke-width="4" filter="url(#brillo)"/><text y="5" text-anchor="middle" font-size="12" font-family="Georgia" fill="#ffe0a0" font-weight="700">NO</text></g>'
    # cola que termina en un tomo
    tx, ty, _ = cuerpo[0]
    s += f'<path d="M{tx:.0f} {ty:.0f} Q{tx - 80:.0f} {ty + 40:.0f} {tx - 100:.0f} {ty - 30:.0f}" fill="none" stroke="#c8963a" stroke-width="18" stroke-linecap="round"/>'
    # aventureros pequeños en un tejado
    for k, (x, c) in enumerate([(820, '#7c1f2b'), (850, '#24365e'), (880, '#3a3a36')]):
        s += f'<g transform="translate({x} 560)"><rect x="-8" y="-34" width="16" height="30" rx="4" fill="{c}" {T(2)}/><circle cx="0" cy="-42" r="8" fill="#c98d66" {T(2)}/><rect x="-6" y="-6" width="5" height="12" fill="#222"/><rect x="1" y="-6" width="5" height="12" fill="#222"/></g>'
    s += '<circle cx="850" cy="500" r="30" fill="#ffe08a" opacity=".5" filter="url(#brilloG)"/>'
    s += chispas(rnd, 80, 0, 0, Wd, 600, '#ffd08a', 1, 3, (.2, .8))
    dv, sv = vineta(Wd, Hd, 'vin', .6)
    s += sv + f'<rect width="{Wd}" height="{Hd}" fill="#fff" filter="url(#grano)" opacity=".15"/>'
    guardar('esc-dragon', Wd, Hd, s, d + dv)


def todo():
    formalismo(); plazo(); notificacion(); usuario(); cola(); vuelva(); fraccionador(); requisito(); tramitador(); coimero(); expediente_vivo(); dragon_carga()


if __name__ == '__main__':
    todo()
