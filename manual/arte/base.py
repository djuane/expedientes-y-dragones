# Utilidades de dibujo para las ilustraciones del manual de Expedientes y Dragones.
# Estilo: ilustración pintada con luz desde arriba a la izquierda, contorno de tinta y brillo de contorno mágico.
# Autor del arte: Leonel Díaz. Todo se dibuja desde cero.
import math, os, random

TINTA = '#1d1320'
OUT = os.path.join(os.path.dirname(__file__), 'svg')
os.makedirs(OUT, exist_ok=True)


def guardar(nombre, w, h, cuerpo, defs=''):
    svg = (f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">'
           f'<defs>{FILTROS}{defs}</defs>{cuerpo}</svg>')
    with open(os.path.join(OUT, nombre + '.svg'), 'w', encoding='utf-8') as f:
        f.write(svg)


FILTROS = '''
<filter id="brillo" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="10" result="b"/><feMerge><feMergeNode in="b"/><feMergeNode in="b"/><feMergeNode in="SourceGraphic"/></feMerge></filter>
<filter id="brilloG" x="-80%" y="-80%" width="260%" height="260%"><feGaussianBlur stdDeviation="26"/></filter>
<filter id="difuso" x="-30%" y="-30%" width="160%" height="160%"><feGaussianBlur stdDeviation="6"/></filter>
<filter id="difuso2" x="-30%" y="-30%" width="160%" height="160%"><feGaussianBlur stdDeviation="2.2"/></filter>
<filter id="sombra" x="-30%" y="-30%" width="160%" height="160%"><feGaussianBlur in="SourceAlpha" stdDeviation="9"/><feOffset dx="6" dy="10"/><feComponentTransfer><feFuncA type="linear" slope="0.45"/></feComponentTransfer><feMerge><feMergeNode/><feMergeNode in="SourceGraphic"/></feMerge></filter>
<filter id="grano" x="0" y="0" width="100%" height="100%"><feTurbulence type="fractalNoise" baseFrequency="0.85" numOctaves="2" seed="7"/><feColorMatrix values="0 0 0 0 0.12  0 0 0 0 0.08  0 0 0 0 0.05  0 0 0 0.55 0"/><feComposite in2="SourceGraphic" operator="in"/></filter>
<filter id="pincel" x="-5%" y="-5%" width="110%" height="110%"><feTurbulence type="fractalNoise" baseFrequency="0.035" numOctaves="3" seed="3" result="t"/><feDisplacementMap in="SourceGraphic" in2="t" scale="7"/></filter>
<filter id="nube" x="-20%" y="-20%" width="140%" height="140%"><feTurbulence type="fractalNoise" baseFrequency="0.012" numOctaves="4" seed="11" result="t"/><feDisplacementMap in="SourceGraphic" in2="t" scale="60"/><feGaussianBlur stdDeviation="4"/></filter>
'''


def lg(id_, stops, x1=0, y1=0, x2=0, y2=1):
    s = ''.join(f'<stop offset="{o}" stop-color="{c}"' + (f' stop-opacity="{a}"' if a is not None else '') + '/>'
                for o, c, a in [(t + (None,))[:3] if len(t) == 2 else t for t in stops])
    return f'<linearGradient id="{id_}" x1="{x1}" y1="{y1}" x2="{x2}" y2="{y2}">{s}</linearGradient>'


def rg(id_, stops, cx=.5, cy=.5, r=.5, fx=None, fy=None):
    s = ''.join(f'<stop offset="{o}" stop-color="{c}"' + (f' stop-opacity="{a}"' if a is not None else '') + '/>'
                for o, c, a in [(t + (None,))[:3] if len(t) == 2 else t for t in stops])
    f = f' fx="{fx}" fy="{fy}"' if fx is not None else ''
    return f'<radialGradient id="{id_}" cx="{cx}" cy="{cy}" r="{r}"{f}>{s}</radialGradient>'


def mezclar(c1, c2, t):
    a = [int(c1[i:i + 2], 16) for i in (1, 3, 5)]
    b = [int(c2[i:i + 2], 16) for i in (1, 3, 5)]
    return '#' + ''.join(f'{round(x + (y - x) * t):02x}' for x, y in zip(a, b))


def claro(c, t=.35):
    return mezclar(c, '#fff4dc', t)


def oscuro(c, t=.45):
    return mezclar(c, '#120a18', t)


def volumen(id_, c):
    """Degradado de volumen: luz arriba a la izquierda, sombra abajo a la derecha."""
    return lg(id_, [(0, claro(c, .3)), (.45, c), (1, oscuro(c, .5))], 0, 0, 1, 1)


def esfera(id_, c):
    return rg(id_, [(0, claro(c, .45)), (.55, c), (1, oscuro(c, .55))], .5, .5, .6, .32, .3)


def T(w=5):
    return f'stroke="{TINTA}" stroke-width="{w}" stroke-linejoin="round" stroke-linecap="round"'


def chispas(rnd, n, x0, y0, x1, y1, color, rmin=1.5, rmax=5, op=(.4, 1)):
    out = []
    for _ in range(n):
        x, y = rnd.uniform(x0, x1), rnd.uniform(y0, y1)
        r = rnd.uniform(rmin, rmax)
        o = rnd.uniform(*op)
        out.append(f'<circle cx="{x:.1f}" cy="{y:.1f}" r="{r:.1f}" fill="{color}" opacity="{o:.2f}"/>')
    return ''.join(out)


def estrella4(x, y, r, color, op=1):
    return (f'<path d="M{x} {y - r} Q{x + r * .18} {y - r * .18} {x + r} {y} Q{x + r * .18} {y + r * .18} {x} {y + r} '
            f'Q{x - r * .18} {y + r * .18} {x - r} {y} Q{x - r * .18} {y - r * .18} {x} {y - r}Z" fill="{color}" opacity="{op}"/>')


def cresta(rnd, x0, y0, x1, y1, rug, prof):
    """Cresta de montaña por desplazamiento del punto medio (recursivo)."""
    if prof == 0:
        return [(x1, y1)]
    mx, my = (x0 + x1) / 2, (y0 + y1) / 2 + rnd.uniform(-rug, rug)
    return cresta(rnd, x0, y0, mx, my, rug / 2, prof - 1) + cresta(rnd, mx, my, x1, y1, rug / 2, prof - 1)


def hoja(x, y, w, h, rot, fondo='#efe3c4', lineas=True, sello=None):
    """Hoja de papel suelta con renglones."""
    s = f'<g transform="translate({x} {y}) rotate({rot})">'
    s += f'<rect x="{-w / 2}" y="{-h / 2}" width="{w}" height="{h}" rx="2" fill="{fondo}" {T(2.5)}/>'
    if lineas:
        for i in range(1, 6):
            yy = -h / 2 + h * i / 7
            s += f'<line x1="{-w / 2 + w * .14}" y1="{yy}" x2="{w / 2 - w * .14}" y2="{yy}" stroke="#7a6a58" stroke-width="{max(1, w / 60)}" opacity=".7"/>'
    if sello:
        s += f'<circle cx="{w * .22}" cy="{h * .25}" r="{w * .16}" fill="none" stroke="{sello}" stroke-width="{max(1.5, w / 40)}" opacity=".85"/>'
    return s + '</g>'


def fondo_aura(w, h, c1, c2, c3, idp='fa'):
    """Fondo de retrato: halo de luz detrás de la figura y viñeta oscura."""
    d = rg(idp, [(0, c1), (.45, c2), (1, c3)], .5, .42, .75)
    return d, f'<rect width="{w}" height="{h}" fill="url(#{idp})"/>'


def vineta(w, h, idv='vin', fuerza=.75):
    d = rg(idv, [(.55, '#000000', 0), (1, '#0a0510', fuerza)], .5, .5, .72)
    return d, f'<rect width="{w}" height="{h}" fill="url(#{idv})"/>'
