# Especialidades jugables (figura completa) para el manual.
import random
from base import *

W, H = 600, 880


def _cabello_atras(p):
    c, e = f'url(#pelo)', p['pelo_estilo']
    if e == 'largo':
        return f'<path d="M238 190 Q226 104 300 100 Q374 104 362 190 L384 352 Q344 372 300 346 Q256 372 216 352 Z" fill="{c}" {T()}/>'
    if e == 'melena':
        return f'<path d="M236 190 Q226 104 300 100 Q374 104 364 190 L374 282 Q336 296 300 280 Q264 296 226 282 Z" fill="{c}" {T()}/>'
    if e == 'mono':
        return f'<circle cx="300" cy="112" r="34" fill="{c}" {T()}/>'
    return ''


def _cabello_frente(p):
    c, e = 'url(#pelo)', p['pelo_estilo']
    if e in ('largo', 'melena'):
        return (f'<path d="M240 196 Q236 116 300 112 Q366 114 360 196 Q350 150 318 140 Q296 168 254 172 Q244 182 240 196Z" fill="{c}" {T(4)}/>'
                f'<path d="M262 132 Q300 118 336 136" fill="none" stroke="{claro(p["pelo"], .35)}" stroke-width="6" opacity=".55"/>')
    if e == 'mono':
        return (f'<path d="M244 190 Q238 120 300 116 Q362 120 356 190 Q346 150 300 146 Q256 150 244 190Z" fill="{c}" {T(4)}/>'
                f'<path d="M262 136 Q300 124 336 138" fill="none" stroke="{claro(p["pelo"], .35)}" stroke-width="5" opacity=".5"/>')
    if e == 'corto':
        return (f'<path d="M244 190 Q232 116 300 108 Q370 112 358 190 Q354 152 334 144 Q304 160 266 148 Q250 160 244 190Z" fill="{c}" {T(4)}/>'
                f'<path d="M270 126 Q304 114 338 130" fill="none" stroke="{claro(p["pelo"], .35)}" stroke-width="5" opacity=".5"/>')
    if e == 'canoso':
        return (f'<path d="M244 206 Q238 160 256 140 Q262 170 254 206Z M356 206 Q362 160 344 140 Q338 170 346 206Z" fill="{c}" {T(3)}/>'
                f'<path d="M262 132 Q300 118 338 132 Q320 124 300 124 Q280 124 262 132Z" fill="{c}" {T(3)}/>')
    if e == 'rizado':
        s = ''
        rnd = random.Random(4)
        for i in range(16):
            import math
            a = math.pi * (1.05 + .9 * i / 15)
            x, y = 300 + 66 * math.cos(a), 178 + 70 * math.sin(a)
            s += f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{rnd.randint(18, 24)}" fill="{c}" {T(3.5)}/>'
        return s
    return ''


def cara(p):
    piel = p['piel']
    s = ''
    # orejas
    s += f'<ellipse cx="245" cy="202" rx="11" ry="17" fill="{oscuro(piel, .12)}" {T(4)}/><ellipse cx="355" cy="202" rx="11" ry="17" fill="{oscuro(piel, .2)}" {T(4)}/>'
    s += f'<path d="M244 182 Q246 256 300 270 Q354 256 356 182 Q356 122 300 122 Q244 122 244 182Z" fill="url(#piel)" {T()}/>'
    # sombra del lado derecho y bajo el mentón
    s += f'<path d="M322 126 Q356 136 356 182 Q354 256 300 270 Q338 236 336 182 Q334 146 322 126Z" fill="{oscuro(piel, .3)}" opacity=".35"/>'
    # ojos
    for x in (278, 322):
        s += (f'<ellipse cx="{x}" cy="203" rx="11" ry="7.5" fill="#fbf6ee"/>'
              f'<circle cx="{x + 1}" cy="203" r="6" fill="{p["ojos"]}"/><circle cx="{x + 1}" cy="203" r="3" fill="#120a10"/>'
              f'<circle cx="{x - 1.5}" cy="200.5" r="1.8" fill="#fff"/>'
              f'<path d="M{x - 12} 201 Q{x} 192 {x + 12} 201" fill="none" stroke="{TINTA}" stroke-width="3.2"/>')
    ce = p.get('cejas', 0)
    s += (f'<path d="M264 {186 + ce} Q278 {178 - ce} 290 {185}" fill="none" stroke="{oscuro(p["pelo"], .2)}" stroke-width="4.5"/>'
          f'<path d="M310 185 Q322 {178 - ce} 336 {186 + ce}" fill="none" stroke="{oscuro(p["pelo"], .2)}" stroke-width="4.5"/>')
    s += f'<path d="M300 208 Q296 226 291 231 Q300 236 308 231" fill="none" stroke="{oscuro(piel, .45)}" stroke-width="2.6"/>'
    s += f'<circle cx="266" cy="226" r="9" fill="#e0706a" opacity=".22"/><circle cx="334" cy="226" r="9" fill="#e0706a" opacity=".22"/>'
    b = p.get('boca', 'firme')
    if b == 'sonrisa':
        s += f'<path d="M282 246 Q300 260 318 246" fill="#7a2a2a" stroke="{TINTA}" stroke-width="3"/><path d="M286 248 Q300 252 314 248" fill="none" stroke="#fff" stroke-width="2.5"/>'
    elif b == 'confiada':
        s += f'<path d="M284 248 Q302 256 318 244" fill="none" stroke="{TINTA}" stroke-width="3.2"/>'
    else:
        s += f'<path d="M286 248 Q300 252 314 248" fill="none" stroke="{TINTA}" stroke-width="3.2"/>'
    if p.get('arrugas'):
        s += f'<path d="M262 214 q-4 6 0 12 M338 214 q4 6 0 12 M286 168 q14 -4 28 0" fill="none" stroke="{oscuro(piel, .4)}" stroke-width="2" opacity=".6"/>'
    if p.get('lentes'):
        s += (f'<g fill="#bfe3ff" fill-opacity=".18" stroke="{p["lentes"]}" stroke-width="4"><circle cx="278" cy="203" r="17"/><circle cx="322" cy="203" r="17"/></g>'
              f'<path d="M295 202 Q300 198 305 202 M261 200 L246 196 M339 200 L354 196" fill="none" stroke="{p["lentes"]}" stroke-width="4"/>'
              f'<path d="M268 194 l8 -4 M312 194 l8 -4" stroke="#fff" stroke-width="3" opacity=".7"/>')
    if p.get('bigote'):
        s += f'<path d="M282 240 Q292 232 300 238 Q308 232 318 240 Q308 246 300 242 Q292 246 282 240Z" fill="{p["bigote"]}" {T(2)}/>'
    return s


def brazo(puntos, color, ancho=50, puno='#f3ead8'):
    d = 'M' + ' L'.join(f'{x} {y}' for x, y in puntos)
    (xa, ya), (xb, yb) = puntos[-2], puntos[-1]
    import math
    L = math.hypot(xb - xa, yb - ya) or 1
    cx, cy = xb - (xb - xa) / L * 6, yb - (yb - ya) / L * 6
    ang = math.degrees(math.atan2(yb - ya, xb - xa))
    cuff = f'<rect x="{-9}" y="{-ancho * .5}" width="16" height="{ancho}" rx="4" fill="{puno}" {T(3.5)} transform="translate({cx:.0f} {cy:.0f}) rotate({ang:.0f})"/>'
    return cuff + (f'<path d="{d}" fill="none" stroke="{TINTA}" stroke-width="{ancho + 9}" stroke-linecap="round" stroke-linejoin="round"/>'
            f'<path d="{d}" fill="none" stroke="{color}" stroke-width="{ancho}" stroke-linecap="round" stroke-linejoin="round"/>'
            f'<path d="{d}" fill="none" stroke="{claro(color, .25)}" stroke-width="{ancho * .28}" stroke-linecap="round" stroke-linejoin="round" opacity=".45" transform="translate(-9 -4)"/>' + cuff)


def mano(x, y, piel, r=21):
    return (f'<circle cx="{x}" cy="{y}" r="{r}" fill="{piel}" {T(4.5)}/>'
            f'<path d="M{x - r * .6} {y - r * .2} q{r * .3} {-r * .5} {r * .7} {-r * .35}" fill="none" stroke="{claro(piel, .4)}" stroke-width="3" opacity=".7"/>')


def cuerpo(p):
    s = ''
    sac, pan = p['saco'], p.get('pantalon', '#2a2633')
    # piernas / falda
    if p.get('falda'):
        f = p['falda']
        s += f'<rect x="246" y="690" width="40" height="150" rx="14" fill="{p["medias"]}" {T(4.5)}/><rect x="314" y="690" width="40" height="150" rx="14" fill="{oscuro(p["medias"], .15)}" {T(4.5)}/>'
        s += f'<path d="M236 500 L364 500 L392 712 Q300 730 208 712 Z" fill="url(#falda)" {T()}/>'
        s += f'<path d="M300 520 L300 718" stroke="{oscuro(f, .35)}" stroke-width="3"/>'
    else:
        s += f'<path d="M234 500 L366 500 L390 836 L326 836 L300 610 L274 836 L210 836 Z" fill="url(#pant)" {T()}/>'
        s += f'<path d="M262 560 L246 820 M340 560 L356 820" stroke="{oscuro(pan, .4)}" stroke-width="3" opacity=".7"/>'
    s += (f'<path d="M204 846 Q206 822 236 818 L282 818 Q292 830 288 850 Z" fill="url(#zap)" {T(4.5)}/>'
          f'<path d="M396 846 Q394 822 364 818 L318 818 Q308 830 312 850 Z" fill="url(#zap)" {T(4.5)}/>'
          f'<path d="M220 828 Q240 822 262 824" stroke="#fff" stroke-width="3" opacity=".35" fill="none"/>')
    # saco (largo si es abrigo)
    largo = p.get('abrigo', 0)
    s += (f'<path d="M210 322 Q218 296 262 286 L338 286 Q382 296 390 322 L384 {528 + largo} Q300 {546 + largo} 216 {528 + largo} Z" fill="url(#saco)" {T()}/>')
    s += f'<path d="M270 286 L300 392 L330 286 Z" fill="{p["camisa"]}" {T(4)}/>'
    if p.get('corbata'):
        s += f'<path d="M293 300 L307 300 L314 392 L300 414 L286 392 Z" fill="url(#corb)" {T(3.5)}/>'
    if p.get('panuelo'):
        s += f'<path d="M276 290 Q300 330 324 290 Q312 318 300 340 Q288 318 276 290Z" fill="{p["panuelo"]}" {T(3.5)}/>'
    s += (f'<path d="M268 287 L296 400 L258 344 L272 326 L250 312 Z" fill="{claro(sac, .12)}" {T(4)}/>'
          f'<path d="M332 287 L304 400 L342 344 L328 326 L350 312 Z" fill="{oscuro(sac, .15)}" {T(4)}/>')
    s += f'<circle cx="300" cy="446" r="5" fill="{p.get("botones", "#d9b25a")}" {T(2)}/><circle cx="300" cy="482" r="5" fill="{p.get("botones", "#d9b25a")}" {T(2)}/>'
    s += f'<path d="M338 300 Q378 316 380 400 L372 {520 + largo} Q350 {530 + largo} 336 {530 + largo} Q356 420 338 300Z" fill="{oscuro(sac, .5)}" opacity=".35"/>'
    if p.get('cinturon'):
        s += f'<rect x="226" y="{500 + largo * 0}" width="148" height="18" fill="{p["cinturon"]}" {T(3)}/>'
    if p.get('manta'):
        m1, m2 = p['manta']
        s += f'<path d="M206 330 Q300 400 394 330 L388 386 Q300 452 212 386Z" fill="{m1}" {T(4)}/>'
        for i, yy in enumerate((350, 366, 380)):
            s += f'<path d="M{210 + i * 2} {yy + 4} Q300 {yy + 70 - i * 4} {390 - i * 2} {yy + 4}" fill="none" stroke="{m2 if i % 2 == 0 else "#f2e2b0"}" stroke-width="5"/>'
        for k in range(9):
            x = 236 + k * 16
            s += f'<path d="M{x} {404 + abs(4 - k) * -5} l6 -7 l6 7 l-6 7z" fill="#f2e2b0" opacity=".9"/>'
    # cuello
    s += f'<rect x="282" y="244" width="36" height="52" fill="{oscuro(p["piel"], .18)}" {T(4)}/>'
    return s


def defs_figura(p):
    d = (lg('piel', [(0, claro(p['piel'], .18)), (.6, p['piel']), (1, oscuro(p['piel'], .22))], 0, 0, 1, .3)
         + volumen('saco', p['saco']) + volumen('pant', p.get('pantalon', '#2a2633'))
         + lg('pelo', [(0, claro(p['pelo'], .2)), (1, oscuro(p['pelo'], .35))], 0, 0, 1, 1)
         + lg('zap', [(0, '#4a3a38'), (1, '#120b10')]) + volumen('corb', p.get('corbata', '#bb3333')))
    if p.get('falda'):
        d += volumen('falda', p['falda'])
    return d


def escenario(fondo1, fondo2, luz, rnd, piso='#3a2d3a'):
    """Arco de sillar con luz de fondo y piso de baldosas."""
    d, s = fondo_aura(W, H, luz, fondo1, fondo2, 'aura')
    d += lg('sillar', [(0, '#fbf6ea'), (1, '#c9bba3')], 0, 0, 1, 1) + lg('piso', [(0, piso), (1, oscuro(piso, .6))])
    # arco
    s += (f'<path d="M80 880 L80 330 Q80 120 300 110 Q520 120 520 330 L520 880 L470 880 L470 340 Q470 168 300 162 Q130 168 130 340 L130 880Z" fill="url(#sillar)" {T(5)} opacity=".92"/>')
    for i in range(9):
        import math
        a = math.pi * (1 + i / 8)
        x1, y1 = 300 + 195 * math.cos(a), 335 + 200 * math.sin(a)
        x2, y2 = 300 + 172 * math.cos(a), 335 + 176 * math.sin(a)
        s += f'<line x1="{x1:.0f}" y1="{y1:.0f}" x2="{x2:.0f}" y2="{y2:.0f}" stroke="#a89880" stroke-width="3"/>'
    for yy in range(400, 880, 70):
        s += f'<line x1="80" y1="{yy}" x2="130" y2="{yy}" stroke="#a89880" stroke-width="3"/><line x1="470" y1="{yy + 35}" x2="520" y2="{yy + 35}" stroke="#a89880" stroke-width="3"/>'
    # rosetón tallado en la clave del arco
    s += f'<circle cx="300" cy="136" r="16" fill="#efe5cf" {T(3)}/><circle cx="300" cy="136" r="7" fill="#c9bba3"/>'
    s += f'<rect x="0" y="790" width="{W}" height="90" fill="url(#piso)"/>'
    for x in range(-200, 800, 70):
        s += f'<line x1="{300 + (x - 300) * .55}" y1="790" x2="{x}" y2="880" stroke="#000" stroke-opacity=".25" stroke-width="2"/>'
    s += '<line x1="0" y1="820" x2="600" y2="820" stroke="#000" stroke-opacity=".2" stroke-width="2"/>'
    s += f'<ellipse cx="300" cy="846" rx="170" ry="22" fill="#000" opacity=".45" filter="url(#difuso)"/>'
    s += chispas(rnd, 40, 20, 40, 580, 780, claro(luz, .4), 1, 3, (.2, .7))
    return d, s


def luz_magica(cx, cy, color, r=260):
    """Luz del objeto mágico sobre la figura (máscara con la silueta)."""
    d = (rg('lm', [(0, color, .55), (.5, color, .15), (1, color, 0)]) +
         '<filter id="blanco"><feColorMatrix values="0 0 0 0 1  0 0 0 0 1  0 0 0 0 1  0 0 0 1 0"/></filter>'
         '<mask id="silueta"><g filter="url(#blanco)"><use href="#fig"/></g></mask>')
    s = (f'<g mask="url(#silueta)" style="mix-blend-mode:screen"><circle cx="{cx}" cy="{cy}" r="{r}" fill="url(#lm)"/></g>')
    return d, s


def armar(nombre, p, fondo, brazos_atras, brazos_frente, props_atras='', props_frente='', luz=None, extra_defs=''):
    rnd = random.Random(hash(nombre) % 1000)
    d0, esc = escenario(*fondo, rnd)
    fig = (props_atras + _cabello_atras(p) + brazos_atras + cuerpo(p) + cara(p) + _cabello_frente(p) + brazos_frente)
    d = d0 + defs_figura(p) + extra_defs
    d += ('<filter id="contorno"><feColorMatrix values="0 0 0 0 1  0 0 0 0 .93  0 0 0 0 .75  0 0 0 .9 0"/><feGaussianBlur stdDeviation="1.5"/></filter>'
          + lg('somb', [(0, '#000', 0), (.55, '#000', 0), (1, '#1a0820', .55)], 0, 0, 1, .25)
          + '<filter id="blanco2"><feColorMatrix values="0 0 0 0 1  0 0 0 0 1  0 0 0 0 1  0 0 0 1 0"/></filter>'
          + '<mask id="silueta2"><g filter="url(#blanco2)"><use href="#fig"/></g></mask>')
    s = esc + '<use href="#fig" filter="url(#contorno)" transform="translate(-5 -4)"/>' + f'<g id="fig">{fig}</g>'
    s += '<g mask="url(#silueta2)" style="mix-blend-mode:multiply"><rect width="600" height="880" fill="url(#somb)"/></g>'
    if luz:
        d1, s1 = luz_magica(*luz)
        d += d1
        s += s1
    s += props_frente
    dv, sv = vineta(W, H, 'vin', .6)
    d += dv
    s += sv + f'<rect width="{W}" height="{H}" fill="#fff" filter="url(#grano)" opacity=".18"/>'
    guardar(nombre, W, H, s, d)


def libro_abierto(x, y, rnd, brillo='#ffd56a'):
    s = f'<ellipse cx="{x}" cy="{y - 30}" rx="90" ry="70" fill="{brillo}" opacity=".55" filter="url(#brilloG)"/>'
    s += (f'<g transform="translate({x} {y}) rotate(-8)">'
          f'<path d="M-78 -6 L0 6 L78 -6 L78 22 L0 34 L-78 22Z" fill="#6b1d22" {T(4)}/>'
          f'<path d="M-72 -10 Q-36 -30 0 -4 Q36 -30 72 -10 L72 14 Q36 -6 0 20 Q-36 -6 -72 14Z" fill="#f6ecd2" {T(3.5)}/>'
          f'<path d="M0 -4 L0 20" stroke="{TINTA}" stroke-width="2.5"/>')
    for i in range(4):
        s += f'<path d="M{-62 + i * 2} {-6 + i * 4} Q-34 {-20 + i * 4} -8 {-2 + i * 4}" fill="none" stroke="#8a7a62" stroke-width="2"/>'
        s += f'<path d="M{62 - i * 2} {-6 + i * 4} Q34 {-20 + i * 4} 8 {-2 + i * 4}" fill="none" stroke="#8a7a62" stroke-width="2"/>'
    s += '</g>'
    # páginas y letras que vuelan
    for i in range(7):
        s += hoja(x - 80 - i * 18 + rnd.uniform(-10, 10), y - 40 - i * 40 + rnd.uniform(-10, 10), 26, 34, rnd.uniform(-40, 40), '#fbf2d8')
    for i in range(16):
        s += estrella4(x + rnd.uniform(-170, 60), y - rnd.uniform(20, 120), rnd.uniform(4, 10), '#fff1b0', rnd.uniform(.5, 1))
    return s


def cadenas_luz(x0, y0, x1, y1, color='#ffd56a'):
    import math
    n = 12
    s = f'<path d="M{x0} {y0} Q{(x0 + x1) / 2} {min(y0, y1) - 80} {x1} {y1}" fill="none" stroke="{color}" stroke-width="20" opacity=".35" filter="url(#difuso)"/>'
    for i in range(n + 1):
        t = i / n
        mx, my = (x0 + x1) / 2, min(y0, y1) - 80
        x = (1 - t) ** 2 * x0 + 2 * (1 - t) * t * mx + t * t * x1
        y = (1 - t) ** 2 * y0 + 2 * (1 - t) * t * my + t * t * y1
        rot = 90 * (i % 2)
        s += f'<ellipse cx="{x:.0f}" cy="{y:.0f}" rx="13" ry="8" transform="rotate({rot + math.degrees(math.atan2(y1 - y0, x1 - x0)):.0f} {x:.0f} {y:.0f})" fill="none" stroke="#fff6c8" stroke-width="5" filter="url(#brillo)"/>'
    return s


def civilista():
    p = dict(piel='#d9a07c', pelo='#2b1a14', pelo_estilo='largo', ojos='#5a3a22', saco='#7c1f2b', falda='#3a1a24',
             medias='#3b2a2e', camisa='#f3ead8', boca='confiada', cejas=2, botones='#e2b04a')
    rnd = random.Random(1)
    atras = brazo([(220, 322), (196, 400), (232, 470)], '#7c1f2b')
    frente = brazo([(382, 322), (440, 268), (468, 186)], '#8a2532') + mano(470, 172, '#d9a07c') + mano(246, 478, '#d9a07c')
    props = libro_abierto(250, 470, rnd)
    props += cadenas_luz(476, 160, 575, 90)
    props += '<g filter="url(#brillo)">' + ''.join(estrella4(470 + rnd.uniform(-40, 40), 170 + rnd.uniform(-40, 40), rnd.uniform(6, 13), '#fff5c0') for _ in range(6)) + '</g>'
    armar('esp-civilista', p, ('#5a2030', '#1a0d17', '#ffcf7a'), atras, frente, props_frente=props, luz=(260, 420, '#ffc95a', 300))


def sello_gigante(x, y, color='#7fc8ff'):
    return (f'<ellipse cx="{x}" cy="{y + 70}" rx="80" ry="60" fill="{color}" opacity=".5" filter="url(#brilloG)"/>'
            f'<g transform="translate({x} {y}) rotate(14)">'
            f'<ellipse cx="0" cy="-58" rx="26" ry="22" fill="#5a3a22" {T(4)}/><rect x="-10" y="-46" width="20" height="44" fill="#6e4a2c" {T(4)}/>'
            f'<path d="M-46 -4 L46 -4 L54 30 L-54 30Z" fill="#8a5a34" {T(4)}/>'
            f'<rect x="-58" y="30" width="116" height="20" rx="4" fill="#2a2a3a" {T(4)}/>'
            f'<path d="M-46 0 L40 0" stroke="#c89060" stroke-width="5" opacity=".7"/></g>'
            f'<ellipse cx="{x + 10}" cy="{y + 74}" rx="60" ry="10" fill="{color}" opacity=".9" filter="url(#brillo)"/>')


def pergamino_cayendo(x, y, rnd):
    s = (f'<path d="M{x} {y} C{x - 30} {y + 120} {x + 40} {y + 200} {x - 10} {y + 330} L{x + 50} {y + 340} C{x + 100} {y + 210} {x + 30} {y + 120} {x + 60} {y}Z" fill="#f4e8c8" {T(4)}/>')
    for i in range(9):
        yy = y + 30 + i * 33
        s += f'<line x1="{x + 4 - (i % 3) * 6}" y1="{yy}" x2="{x + 42 - (i % 3) * 6}" y2="{yy + 2}" stroke="#7a6a58" stroke-width="2.5"/>'
    s += f'<rect x="{x - 6}" y="{y - 12}" width="76" height="22" rx="10" fill="#c8a25a" {T(4)}/>'
    return s


def administrativista():
    p = dict(piel='#c98d66', pelo='#1c1412', pelo_estilo='corto', ojos='#3a2416', saco='#24365e', pantalon='#1f2a44',
             camisa='#f4f1ea', corbata='#b02a2a', lentes='#2a2228', boca='sonrisa', cejas=-1)
    rnd = random.Random(2)
    atras = brazo([(218, 322), (186, 410), (168, 490)], '#24365e')
    frente = brazo([(382, 322), (452, 286), (470, 210)], '#2c4170') + mano(470, 200, '#c98d66')
    props_a = pergamino_cayendo(138, 470, rnd)
    props_f = mano(170, 500, '#c98d66') + sello_gigante(476, 120)
    for i in range(14):
        props_f += estrella4(480 + rnd.uniform(-90, 90), 200 + rnd.uniform(-20, 80), rnd.uniform(4, 9), '#cfefff', rnd.uniform(.5, 1))
    armar('esp-administrativista', p, ('#203054', '#0b0d1c', '#9fd6ff'), atras, frente, props_atras=props_a, props_frente=props_f,
          luz=(470, 210, '#8fd0ff', 280))


def lupa(x, y, rot=-30, color='#9cffd9'):
    return (f'<circle cx="{x}" cy="{y}" r="70" fill="{color}" opacity=".45" filter="url(#brilloG)"/>'
            f'<g transform="translate({x} {y}) rotate({rot})">'
            f'<rect x="-9" y="44" width="18" height="78" rx="7" fill="#4a2c1c" {T(4)}/>'
            f'<circle r="48" fill="{color}" fill-opacity=".35" stroke="#d6b25e" stroke-width="10"/>'
            f'<circle r="48" fill="none" {T(3)}/><path d="M-26 -20 Q-14 -34 4 -36" stroke="#fff" stroke-width="6" fill="none" opacity=".8"/>'
            f'<path d="M-18 6 l10 -10 l10 10 l10 -10 l10 10" stroke="#1e5a4a" stroke-width="3" fill="none"/></g>')


def penalista():
    p = dict(piel='#b97a55', pelo='#120c10', pelo_estilo='mono', ojos='#2b1a10', saco='#3a3a36', pantalon='#22201f',
             camisa='#e8e0d0', boca='firme', cejas=4, abrigo=110, cinturon='#2a2420', botones='#999')
    rnd = random.Random(3)
    atras = brazo([(218, 322), (180, 400), (150, 330)], '#3a3a36')
    frente = brazo([(382, 322), (404, 420), (380, 500)], '#44443f') + mano(378, 506, '#b97a55')
    carpeta = (f'<g transform="translate(372 470) rotate(8)"><rect x="-10" y="0" width="74" height="96" rx="4" fill="#c8963a" {T(4)}/>'
               f'<rect x="-4" y="-8" width="30" height="12" rx="3" fill="#c8963a" {T(3)}/><text x="27" y="56" font-family="serif" font-size="18" text-anchor="middle" fill="#5a3010" font-weight="700">EXP.</text></g>')
    props_f = carpeta + mano(372, 506, '#b97a55') + lupa(150, 270) + mano(152, 330, '#b97a55')
    # huellas brillantes en el piso
    for i, (x, y) in enumerate([(70, 860), (120, 830), (170, 860), (440, 830), (500, 860)]):
        props_f += f'<ellipse cx="{x}" cy="{y}" rx="12" ry="7" fill="#9cffd9" opacity=".6" filter="url(#brillo)"/>'
    armar('esp-penalista', p, ('#25302c', '#0b100f', '#a8ffe0'), atras, frente, props_frente=props_f, luz=(150, 270, '#9cffd9', 260))


def farol(x, y, color='#ffb347'):
    return (f'<circle cx="{x}" cy="{y + 30}" r="110" fill="{color}" opacity=".45" filter="url(#brilloG)"/>'
            f'<g transform="translate({x} {y})"><path d="M-6 -30 Q0 -52 6 -30" fill="none" {T(4)}/>'
            f'<path d="M-26 -26 L26 -26 L20 -14 L-20 -14Z" fill="#3a3030" {T(4)}/>'
            f'<rect x="-22" y="-14" width="44" height="60" rx="6" fill="#ffe6a0" {T(4)}/>'
            f'<path d="M0 0 Q-10 18 0 34 Q10 18 0 0" fill="#ff9a3a"/>'
            f'<path d="M-22 6 L22 6 M-22 26 L22 26 M0 -14 L0 46" stroke="#3a3030" stroke-width="3"/>'
            f'<path d="M-26 46 L26 46 L20 56 L-20 56Z" fill="#3a3030" {T(4)}/></g>')


def laboralista():
    p = dict(piel='#a86e4c', pelo='#1a120e', pelo_estilo='corto', ojos='#2a1a10', saco='#5b6a3a', pantalon='#2c3a4e',
             camisa='#efe6d2', boca='sonrisa', cejas=1, bigote='#1a120e', botones='#c8a050')
    rnd = random.Random(5)
    atras = brazo([(218, 322), (170, 300), (150, 230)], '#5b6a3a')
    frente = brazo([(382, 322), (420, 410), (436, 486)], '#66763f') + mano(438, 494, '#a86e4c')
    rollo = (f'<g transform="translate(440 520) rotate(-60)"><rect x="-90" y="-16" width="180" height="32" rx="16" fill="#efe2c0" {T(4)}/>'
             f'<ellipse cx="90" cy="0" rx="9" ry="16" fill="#d8c8a0" {T(3)}/><rect x="-20" y="-17" width="18" height="34" fill="#b02a2a"/></g>')
    props_f = rollo + mano(438, 494, '#a86e4c') + farol(146, 196) + mano(150, 226, '#a86e4c')
    # casco de seguridad sobre la cabeza
    casco = (f'<path d="M238 156 Q240 92 300 88 Q360 92 362 156Z" fill="#f2b630" {T(5)}/><path d="M226 158 L374 158 L370 172 L230 172Z" fill="#e0a020" {T(4)}/>'
             f'<path d="M296 90 L304 90 L306 156 L294 156Z" fill="#ffd25a" {T(2)}/><path d="M256 120 Q270 100 290 98" stroke="#fff" stroke-width="5" opacity=".6" fill="none"/>')
    armar('esp-laboralista', p, ('#3a2e1c', '#120d08', '#ffbb5c'), atras, frente, props_frente=casco + props_f, luz=(150, 220, '#ffb347', 300))


def baston_balanza(x, y, color='#ffe28a'):
    s = f'<rect x="{x - 7}" y="{y}" width="14" height="{846 - y}" rx="6" fill="#6b4524" {T(4)}/>'
    s += f'<circle cx="{x}" cy="{y - 40}" r="90" fill="{color}" opacity=".5" filter="url(#brilloG)"/>'
    s += (f'<g transform="translate({x} {y})"><circle cx="0" cy="-10" r="12" fill="#e2b04a" {T(3.5)}/>'
          f'<path d="M-70 -48 L70 -48" stroke="#e2b04a" stroke-width="7"/><path d="M-70 -48 L70 -48" {T(2)}/>'
          f'<path d="M0 -22 L0 -70" stroke="#e2b04a" stroke-width="7"/><circle cx="0" cy="-74" r="8" fill="#fff4c0" filter="url(#brillo)"/>'
          f'<path d="M-70 -48 L-92 0 M-70 -48 L-48 0 M70 -48 L48 0 M70 -48 L92 0" stroke="#d9b25e" stroke-width="2.5"/>'
          f'<path d="M-96 0 Q-70 22 -44 0Z M44 0 Q70 22 96 0Z" fill="#e2b04a" {T(3)}/></g>')
    return s


def constitucionalista():
    p = dict(piel='#c58a62', pelo='#d8d2cc', pelo_estilo='melena', ojos='#3a2a1a', saco='#4a2a5a', falda='#2a1a33',
             medias='#3a2a3a', camisa='#efe6d6', boca='confiada', arrugas=True, manta=('#b0302a', '#2a6a5a'), botones='#e2b04a')
    rnd = random.Random(6)
    atras = brazo([(218, 322), (196, 410), (170, 470)], '#4a2a5a')
    frente = brazo([(382, 322), (430, 380), (462, 330)], '#563266') + mano(462, 322, '#c58a62')
    props_a = baston_balanza(462, 250)
    libro = (f'<g transform="translate(160 476) rotate(-12)"><rect x="-40" y="-52" width="80" height="104" rx="5" fill="#1f3f5a" {T(4)}/>'
             f'<rect x="-32" y="-44" width="64" height="88" fill="none" stroke="#e2b04a" stroke-width="3"/>'
             f'<circle cx="0" cy="-6" r="14" fill="none" stroke="#e2b04a" stroke-width="3"/><path d="M-10 -6 L10 -6 M0 -16 L0 4" stroke="#e2b04a" stroke-width="3"/></g>')
    props_f = libro + mano(174, 478, '#c58a62') + mano(462, 322, '#c58a62')
    props_f += '<g>' + ''.join(estrella4(462 + rnd.uniform(-110, 110), 200 + rnd.uniform(-80, 60), rnd.uniform(4, 10), '#fff2b0', rnd.uniform(.5, 1)) for _ in range(14)) + '</g>'
    armar('esp-constitucionalista', p, ('#3a2050', '#120a1a', '#ffe08a'), atras, frente, props_atras=props_a, props_frente=props_f,
          luz=(462, 210, '#ffe08a', 320))


def tributarista():
    p = dict(piel='#e0ac86', pelo='#3a2416', pelo_estilo='rizado', ojos='#4a3018', saco='#2f5a4a', pantalon='#2a2a30',
             camisa='#f6f0e2', panuelo='#e2b04a', boca='sonrisa', cejas=0, lentes='#7a5a2a', botones='#e2b04a')
    rnd = random.Random(7)
    atras = brazo([(218, 322), (180, 400), (160, 462)], '#2f5a4a')
    frente = brazo([(382, 322), (436, 340), (476, 290)], '#376a57') + mano(476, 282, '#e0ac86')
    escudo = (f'<g transform="translate(232 520)"><path d="M-80 -90 L80 -90 L80 10 Q80 80 0 110 Q-80 80 -80 10Z" fill="#e8dcc0" {T(5)}/>'
              f'<path d="M-66 -76 L66 -76 L66 8 Q66 66 0 92 Q-66 66 -66 8Z" fill="none" stroke="#2f5a4a" stroke-width="5"/>')
    for i in range(5):
        y = -56 + i * 26
        escudo += f'<line x1="-50" y1="{y}" x2="50" y2="{y}" stroke="#7a6a58" stroke-width="3"/>'
        for k in range(4):
            escudo += f'<circle cx="{-40 + k * 26 + (i % 2) * 8}" cy="{y}" r="7" fill="#c8963a" {T(2)}/>'
    escudo += '</g>'
    monedas = ''
    for i in range(10):
        x, y = 476 + rnd.uniform(-90, 80), 240 - i * 22 + rnd.uniform(-10, 10)
        monedas += f'<g transform="translate({x:.0f} {y:.0f}) rotate({rnd.uniform(-40, 40):.0f})"><ellipse rx="16" ry="9" fill="#f2c04a" {T(3)}/><ellipse rx="8" ry="4" fill="none" stroke="#a07020" stroke-width="2"/></g>'
    props_f = escudo + mano(158, 470, '#e0ac86') + f'<circle cx="476" cy="200" r="100" fill="#ffd060" opacity=".45" filter="url(#brilloG)"/>' + monedas + mano(476, 282, '#e0ac86')
    armar('esp-tributarista', p, ('#1e3a32', '#0a1210', '#ffe08a'), atras, frente, props_frente=props_f, luz=(476, 220, '#ffd060', 280))


def todo():
    civilista(); administrativista(); penalista(); laboralista(); constitucionalista(); tributarista()


if __name__ == '__main__':
    todo()
