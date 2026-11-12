# Portada, escenas, mapa y retratos de personajes no jugadores para el manual.
import math, random
from base import *
import personajes as P


def cielo_atardecer(Wd, Hd, rnd, idp='cielo'):
    d = lg(idp, [(0, '#120a22'), (.45, '#3a1636'), (.75, '#a8402e'), (1, '#f0a050')])
    s = f'<rect width="{Wd}" height="{Hd}" fill="url(#{idp})"/>'
    for _ in range(140):
        x, y = rnd.uniform(0, Wd), rnd.uniform(0, Hd * .4)
        s += f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{rnd.uniform(.6, 2.2):.1f}" fill="#fff6e0" opacity="{rnd.uniform(.3, .9):.2f}"/>'
    return d, s


def volcan(rnd, x0, x1, cima, base, ancho_cima=90, idm='monte', idn='nieve'):
    cx = (x0 + x1) / 2
    a, b = cx - ancho_cima / 2, cx + ancho_cima / 2
    pts = [(x0, base)] + cresta(rnd, x0, base, a, cima, 40, 6) + cresta(rnd, a, cima, b, cima + 4, 8, 3) + cresta(rnd, b, cima + 4, x1, base, 40, 6)
    d = lg(idm, [(0, '#4a2c4c'), (1, '#160c1c')]) + lg(idn, [(0, '#fffaf0'), (1, '#e0cfd6')])
    s = '<path d="M' + ' L'.join(f'{x:.0f} {y:.0f}' for x, y in pts) + f'Z" fill="url(#{idm})"/>'
    # nieve
    h = (base - cima) * .22
    nv = [(a - h * .9, cima + h)]
    for i in range(9):
        t = i / 8
        nv.append((a - h * .9 + (b - a + h * 1.8) * t, cima + h + (14 if i % 2 else -6) + rnd.uniform(-6, 6)))
    s += (f'<path d="M{a} {cima} L{b} {cima + 4} L{b + h * .9} {cima + h} ' + ' '.join(f'L{x:.0f} {y:.0f}' for x, y in reversed(nv)) + f'Z" fill="url(#{idn})"/>')
    # surcos
    for k in range(6):
        xx = a + (b - a) * k / 5
        s += f'<path d="M{xx:.0f} {cima + 10} Q{xx + (xx - cx) * 1.5:.0f} {(cima + base) / 2:.0f} {xx + (xx - cx) * 3:.0f} {base}" stroke="#000" stroke-opacity=".18" stroke-width="3" fill="none"/>'
    return d, s


def arcada_sillar(y, Wd, alto=120, paso=110, idp='sil'):
    d = lg(idp, [(0, '#f6efe0'), (1, '#bfae94')])
    s = f'<rect x="0" y="{y}" width="{Wd}" height="{alto + 300}" fill="url(#{idp})" {T(4)}/>'
    s += f'<rect x="0" y="{y - 18}" width="{Wd}" height="22" fill="#e8dcc6" {T(4)}/>'
    x = 20
    while x < Wd:
        s += f'<path d="M{x} {y + alto + 300} L{x} {y + 50} Q{x + paso * .4} {y + 4} {x + paso * .8} {y + 50} L{x + paso * .8} {y + alto + 300}Z" fill="#241626" {T(4)}/>'
        s += f'<path d="M{x + 6} {y + 52} Q{x + paso * .4} {y + 14} {x + paso * .8 - 6} {y + 52}" stroke="#ffcf8a" stroke-width="3" fill="none" opacity=".35"/>'
        x += paso
    return d, s


def portada():
    Wd, Hd = 1240, 1754
    rnd = random.Random(2026)
    d, s = cielo_atardecer(Wd, Hd, rnd)
    d += rg('luna', [(0, '#fffbe8'), (.7, '#f4e2b0'), (1, '#e8c88a')]) + rg('halo', [(0, '#ffe8b0', .7), (1, '#ffe8b0', 0)])
    s += f'<circle cx="860" cy="700" r="330" fill="url(#halo)"/><circle cx="860" cy="700" r="170" fill="url(#luna)"/>'
    s += f'<circle cx="810" cy="660" r="26" fill="#e2cc98" opacity=".6"/><circle cx="900" cy="750" r="18" fill="#e2cc98" opacity=".6"/>'
    s += f'<g filter="url(#nube)" opacity=".55"><ellipse cx="250" cy="700" rx="360" ry="40" fill="#7a2a4a"/><ellipse cx="1000" cy="820" rx="300" ry="36" fill="#a04050"/></g>'
    d1, s1 = volcan(rnd, -60, 1040, 820, 1320, 110)
    d2, s2 = volcan(random.Random(5), 640, 1400, 980, 1320, 60, 'monte2', 'nieve2')
    d += d1 + d2
    s += s2.replace('url(#monte2)', 'url(#monte2)') + s1
    # torre de expedientes
    tw = ''
    y = 1300
    for i in range(16):
        w = 220 - i * 6 + rnd.uniform(-10, 10)
        h = 46
        x = 300 + rnd.uniform(-12, 12) - w / 2
        col = ['#d8b06a', '#c89a50', '#e6c88a'][i % 3]
        tw += f'<rect x="{x:.0f}" y="{y - h:.0f}" width="{w:.0f}" height="{h}" rx="3" fill="{col}" {T(4)}/>'
        tw += f'<line x1="{x + 8:.0f}" y1="{y - h + 14:.0f}" x2="{x + w - 8:.0f}" y2="{y - h + 14:.0f}" stroke="#b02a2a" stroke-width="5"/>'
        tw += f'<line x1="{x + 8:.0f}" y1="{y - h + 30:.0f}" x2="{x + w * .6:.0f}" y2="{y - h + 30:.0f}" stroke="#6a5a48" stroke-width="3"/>'
        y -= h
    tw += f'<path d="M300 {y} L300 {y - 90}" {T(6)}/><path d="M300 {y - 90} L360 {y - 70} L300 {y - 50}Z" fill="#c0282a" {T(4)}/>'
    tw += f'<path d="M270 1300 L270 1236 Q300 1206 330 1236 L330 1300Z" fill="#1a0e18" {T(4)}/><rect x="290" y="1180" width="22" height="22" fill="#ffcf6a" filter="url(#brillo)"/>'
    s += tw
    # dragón de expedientes enroscado alrededor de la torre y volando a la luna
    seg = []
    for i in range(46):
        t = i / 45
        a = t * math.pi * 3.2
        x = 300 + (150 + 40 * t) * math.cos(a) + t * 420
        y = 1250 - t * 560 + 45 * math.sin(a)
        seg.append((x, y, 20 + 22 * math.sin(t * math.pi)))
    d += lg('ala', [(0, '#8a2434', .95), (1, '#2a0a16', .95)], 0, 0, 1, 1) + volumen('esc', '#6a2232')
    hx, hy, _ = seg[-1]
    ax, ay, _ = seg[38]
    s += (f'<g transform="translate({ax - hx + 60:.0f} {ay - hy + 40:.0f})"><path d="M{hx - 120} {hy + 40} Q{hx - 340} {hy - 260} {hx - 560} {hy - 230} Q{hx - 450} {hy - 160} {hx - 480} {hy - 100} Q{hx - 400} {hy - 110} {hx - 380} {hy - 40} Q{hx - 300} {hy - 60} {hx - 220} {hy + 60}Z" fill="url(#ala)" {T(6)}/>'
          f'<path d="M{hx - 60} {hy + 30} Q{hx + 40} {hy - 300} {hx + 260} {hy - 360} Q{hx + 180} {hy - 270} {hx + 230} {hy - 210} Q{hx + 150} {hy - 200} {hx + 150} {hy - 130} Q{hx + 80} {hy - 120} {hx + 30} {hy + 50}Z" fill="url(#ala)" {T(6)}/></g>')
    detras = [p for i, p in enumerate(seg) if math.sin(i / 45 * math.pi * 3.2) < 0 and i < 30]
    for i, (x, y, r) in enumerate(seg):
        col = ['#c8963a', '#d8a84a', '#b8863a', '#e2c070'][i % 4]
        s += (f'<g transform="translate({x:.0f} {y:.0f}) rotate({(i * 23) % 50 - 25})"><rect x="{-r * 1.2:.0f}" y="{-r * .75:.0f}" width="{r * 2.4:.0f}" height="{r * 1.5:.0f}" rx="4" fill="{col}" {T(4)}/>'
              f'<line x1="{-r:.0f}" y1="{-r * .3:.0f}" x2="{r:.0f}" y2="{-r * .3:.0f}" stroke="#f6ecd2" stroke-width="{max(2, r * .14):.0f}"/></g>')
    s += f'<g transform="translate({hx + 20:.0f} {hy - 30:.0f}) scale(1.1)">'
    s += f'<circle cx="40" cy="10" r="140" fill="#ff8a3a" opacity=".3" filter="url(#brilloG)"/>'
    s += f'<path d="M-70 -20 Q-60 -80 20 -80 Q100 -70 140 -10 L150 20 Q100 30 60 20 Q90 50 120 70 Q40 90 -20 50 Q-80 30 -70 -20Z" fill="url(#esc)" {T(6)}/>'
    s += f'<path d="M-30 -70 L-60 -140 L0 -84 M20 -80 L30 -160 L60 -78" fill="#e2c070" {T(5)}/>'
    s += f'<path d="M60 20 L70 32 L80 20 L90 32 L100 20 L110 32 L120 20" fill="#fff6e0" {T(2)}/>'
    s += f'<ellipse cx="40" cy="-34" rx="18" ry="12" fill="#ffe24a" {T(3)} filter="url(#brillo)"/><ellipse cx="42" cy="-34" rx="4" ry="10" fill="#1a0a0a"/><path d="M10 -54 L66 -44" {T(7)}/>'
    s += '</g>'
    for i in range(40):
        s += hoja(rnd.uniform(100, 1150), rnd.uniform(300, 1200), rnd.uniform(14, 26), rnd.uniform(18, 32), rnd.uniform(-70, 70), '#efe3c4', lineas=False)
    # ciudad de sillar
    d3, s3 = arcada_sillar(1330, Wd, 140, 120)
    d += d3
    s += s3
    s += f'<rect x="0" y="1520" width="{Wd}" height="240" fill="#0e0812"/>'
    # héroes en primer plano (siluetas con brillo)
    colores = ['#ffcf7a', '#9fd6ff', '#a8ffe0', '#ffbb5c', '#ffe08a', '#ffd060']
    for k, c in enumerate(colores):
        x = 220 + k * 160
        s += f'<ellipse cx="{x}" cy="1560" rx="70" ry="100" fill="{c}" opacity=".25" filter="url(#brilloG)"/>'
        s += (f'<g transform="translate({x} 1640)" fill="#0b060e">'
              f'<circle cx="0" cy="-170" r="26"/><path d="M-42 -132 Q0 -150 42 -132 L50 -40 L-50 -40Z"/>'
              f'<path d="M-36 -40 L-40 40 L-10 40 L0 -10 L10 40 L40 40 L36 -40Z"/>'
              f'<path d="M-44 -126 L{-70 + (k % 2) * 20} {-60 - (k % 3) * 40} M44 -126 L{70 - (k % 2) * 10} {-200 + (k % 3) * 30}" stroke="#0b060e" stroke-width="22" stroke-linecap="round"/></g>')
        s += f'<circle cx="{x + 70 - (k % 2) * 10}" cy="{1440 + (k % 3) * 30}" r="10" fill="{c}" filter="url(#brillo)"/>'
    s += chispas(rnd, 120, 0, 200, Wd, 1600, '#ffe0a0', 1, 3.4, (.2, .9))
    dv, sv = vineta(Wd, Hd, 'vin', .55)
    s += sv + f'<rect width="{Wd}" height="{Hd}" fill="#fff" filter="url(#grano)" opacity=".14"/>'
    guardar('portada', Wd, Hd, s, d + dv)


def escena_mesa():
    Wd, Hd = 1000, 640
    rnd = random.Random(31)
    d = lg('pared', [(0, '#3a2a3e'), (1, '#1a1020')]) + lg('suelo', [(0, '#5a4048'), (1, '#1a1018')]) + rg('foco', [(0, '#ffe2a0', .8), (1, '#ffe2a0', 0)])
    s = f'<rect width="{Wd}" height="{Hd}" fill="url(#pared)"/>'
    vx, vy = 500, 260  # punto de fuga
    s += f'<path d="M0 {Hd} L{vx - 60} {vy + 60} L{vx + 60} {vy + 60} L{Wd} {Hd}Z" fill="url(#suelo)"/>'
    for k in range(-8, 9):
        s += f'<line x1="{vx + k * 8}" y1="{vy + 60}" x2="{vx + k * 140}" y2="{Hd}" stroke="#000" stroke-opacity=".25" stroke-width="2"/>'
    for k in range(1, 9):
        yy = vy + 60 + (Hd - vy - 60) * (k / 8) ** 1.8
        s += f'<line x1="0" y1="{yy:.0f}" x2="{Wd}" y2="{yy:.0f}" stroke="#000" stroke-opacity=".2" stroke-width="2"/>'
    # ventanillas a ambos lados con perspectiva
    for lado in (-1, 1):
        for k in range(7):
            t = (k / 7) ** 1.4
            x = vx + lado * (480 - 420 * t)
            w = 150 * (1 - t) + 14
            h = 260 * (1 - t) + 30
            y = vy + 60 - h * .75
            xr = x - w if lado > 0 else x
            s += f'<rect x="{xr:.0f}" y="{y:.0f}" width="{w:.0f}" height="{h:.0f}" fill="#2a1a2a" {T(3)}/>'
            s += f'<rect x="{xr + w * .12:.0f}" y="{y + h * .2:.0f}" width="{w * .76:.0f}" height="{h * .4:.0f}" fill="#ffcf8a" opacity="{.7 - t * .4:.2f}"/>'
            s += f'<text x="{xr + w / 2:.0f}" y="{y + h * .14:.0f}" text-anchor="middle" font-family="Georgia,serif" font-size="{max(6, 22 * (1 - t)):.0f}" fill="#e2b04a">{k * 2 + (1 if lado < 0 else 2)}</text>'
    # pantalla de turnos
    s += f'<rect x="420" y="70" width="160" height="80" rx="8" fill="#120a10" {T(5)}/><text x="500" y="128" text-anchor="middle" font-family="monospace" font-size="46" fill="#ff5a3a" filter="url(#brillo)">0947</text>'
    s += f'<circle cx="500" cy="200" r="220" fill="url(#foco)" opacity=".35"/>'
    # la cola que serpentea hacia el fondo
    for k in range(40):
        t = k / 39
        x = vx + 260 * (1 - t) * math.sin(t * 9)
        y = Hd - 40 - (Hd - 40 - vy - 60) * t ** .7
        sc = 1 - t * .9
        s += (f'<g transform="translate({x:.0f} {y:.0f}) scale({sc:.2f})" opacity="{.95 - t * .3:.2f}"><ellipse cx="0" cy="-60" rx="30" ry="60" fill="#5a6280" {T(3)}/>'
              f'<circle cx="0" cy="-130" r="22" fill="#7a82a0" {T(3)}/></g>')
    # héroes de espaldas en primer plano
    for x, c in ((150, '#7c1f2b'), (850, '#24365e')):
        s += (f'<g transform="translate({x} {Hd + 20})"><path d="M-80 0 Q-80 -160 0 -170 Q80 -160 80 0Z" fill="{c}" {T(5)}/>'
              f'<circle cx="0" cy="-210" r="48" fill="#2a1a14" {T(5)}/></g>')
    s += estrella4(150, 420, 18, '#ffe08a') + estrella4(850, 420, 18, '#bfe6ff')
    s += chispas(rnd, 60, 0, 0, Wd, Hd, '#ffe0a0', .8, 2.4, (.2, .6))
    dv, sv = vineta(Wd, Hd, 'vin', .7)
    s += sv + f'<rect width="{Wd}" height="{Hd}" fill="#fff" filter="url(#grano)" opacity=".15"/>'
    guardar('esc-mesa', Wd, Hd, s, d + dv)


def escena_archivo():
    Wd, Hd = 1000, 640
    rnd = random.Random(41)
    d = lg('fondoA', [(0, '#16101a'), (1, '#060408')]) + rg('farol', [(0, '#ffcf7a', .85), (.5, '#ff9a3a', .25), (1, '#ff9a3a', 0)])
    s = f'<rect width="{Wd}" height="{Hd}" fill="url(#fondoA)"/>'
    vx, vy = 500, 300
    for lado in (-1, 1):
        for k in range(8):
            t = (k / 8) ** 1.3
            x = vx + lado * (520 - 460 * t)
            w = 120 * (1 - t) + 10
            top, bot = vy - 320 * (1 - t) - 20, vy + 300 * (1 - t) + 20
            xr = x - w if lado > 0 else x
            s += f'<rect x="{xr:.0f}" y="{top:.0f}" width="{w:.0f}" height="{bot - top:.0f}" fill="#3a2418" {T(2.5)}/>'
            for j in range(6):
                yy = top + (bot - top) * (j + 1) / 7
                s += f'<rect x="{xr:.0f}" y="{yy:.0f}" width="{w:.0f}" height="{max(2, 6 * (1 - t)):.0f}" fill="#5a3a24"/>'
                n = max(2, int(8 * (1 - t)))
                for q in range(n):
                    hh = (bot - top) / 7 * .8
                    s += f'<rect x="{xr + q * w / n:.0f}" y="{yy - hh:.0f}" width="{w / n * .8:.0f}" height="{hh:.0f}" fill="{rnd.choice(["#c8963a", "#b8863a", "#e2c070", "#a87830"])}" opacity="{1 - t * .6:.2f}"/>'
    # ojos en la oscuridad
    for _ in range(12):
        x, y = rnd.uniform(380, 620), rnd.uniform(200, 380)
        sc = rnd.uniform(.5, 1.2)
        s += f'<g filter="url(#brillo)"><ellipse cx="{x:.0f}" cy="{y:.0f}" rx="{5 * sc:.1f}" ry="{3 * sc:.1f}" fill="#ff4a2a"/><ellipse cx="{x + 14 * sc:.0f}" cy="{y:.0f}" rx="{5 * sc:.1f}" ry="{3 * sc:.1f}" fill="#ff4a2a"/></g>'
    # pilas de expedientes en el piso y el expediente vivo
    for k in range(12):
        x = rnd.uniform(40, 960)
        y = rnd.uniform(520, 620)
        for j in range(rnd.randint(2, 6)):
            s += f'<rect x="{x - 40:.0f}" y="{y - j * 16:.0f}" width="80" height="16" rx="2" fill="{rnd.choice(["#c8963a", "#e2c070", "#b8863a"])}" {T(2.5)} transform="rotate({rnd.uniform(-5, 5):.1f} {x:.0f} {y:.0f})"/>'
    s += f'<circle cx="220" cy="420" r="300" fill="url(#farol)"/>'
    s += P.farol(220, 380)
    s += chispas(rnd, 80, 0, 0, Wd, Hd, '#e0c8a0', .6, 2, (.15, .5))
    dv, sv = vineta(Wd, Hd, 'vin', .8)
    s += sv + f'<rect width="{Wd}" height="{Hd}" fill="#fff" filter="url(#grano)" opacity=".15"/>'
    guardar('esc-archivo', Wd, Hd, s, d + dv)


def mapa():
    Wd, Hd = 1000, 720
    rnd = random.Random(55)
    d = rg('pergM', [(0, '#f4e6c4'), (.7, '#e2cc9a'), (1, '#b8955a')], .5, .5, .75)
    tinta = '#4a2a18'
    s = f'<rect width="{Wd}" height="{Hd}" fill="url(#pergM)"/>'
    s += f'<rect width="{Wd}" height="{Hd}" fill="#7a4a1a" filter="url(#grano)" opacity=".35"/>'
    # río Chili-like inventado: Río Trámite
    s += f'<path d="M-10 150 C200 220 260 120 420 240 S640 300 720 420 S900 520 1010 600" fill="none" stroke="#5a8aa0" stroke-width="18" opacity=".55"/>'
    s += f'<path d="M-10 150 C200 220 260 120 420 240 S640 300 720 420 S900 520 1010 600" fill="none" stroke="{tinta}" stroke-width="2" stroke-dasharray="2 8"/>'
    # montañas pequeñas
    for _ in range(26):
        x, y = rnd.uniform(560, 980), rnd.uniform(40, 220)
        sz = rnd.uniform(18, 40)
        s += f'<path d="M{x - sz} {y} L{x} {y - sz * 1.2} L{x + sz} {y}" fill="#d8bf8a" stroke="{tinta}" stroke-width="2.5"/><path d="M{x} {y - sz * 1.2} L{x + sz * .3} {y}" stroke="{tinta}" stroke-width="1.5"/>'
    s += f'<path d="M700 200 L790 60 L880 200" fill="#e8d8b0" stroke="{tinta}" stroke-width="4"/><path d="M770 92 L790 60 L812 92 L800 100 L790 88 L780 100Z" fill="#fff"/>'
    # arbolitos
    for _ in range(40):
        x, y = rnd.uniform(40, 360), rnd.uniform(420, 680)
        s += f'<circle cx="{x:.0f}" cy="{y:.0f}" r="7" fill="#9aaa6a" stroke="{tinta}" stroke-width="1.8"/><line x1="{x:.0f}" y1="{y + 7:.0f}" x2="{x:.0f}" y2="{y + 13:.0f}" stroke="{tinta}" stroke-width="2"/>'
    lugares = [
        (180, 300, 'Mesa de Partes', 'ventanilla'), (470, 390, 'Palacio de Justicia', 'palacio'), (640, 560, 'Sala de Contrataciones', 'sala'),
        (330, 560, 'Pasaje del Coimero', 'pasaje'), (500, 620, 'Archivo Central', 'archivo'), (820, 320, 'Torre de la Carga', 'torre'),
        (200, 120, 'Cafetería del Juzgado', 'taza'), (450, 130, 'Librería Jurídica', 'libro'), (840, 620, 'Estudio del jugador', 'estudio'),
    ]
    caminos = [(0, 1), (1, 2), (1, 3), (1, 4), (1, 5), (0, 6), (6, 7), (7, 1), (2, 8), (4, 8), (3, 4)]
    for a, b in caminos:
        (x1, y1, _, _), (x2, y2, _, _) = lugares[a], lugares[b]
        mx, my = (x1 + x2) / 2 + rnd.uniform(-40, 40), (y1 + y2) / 2 + rnd.uniform(-40, 40)
        s += f'<path d="M{x1} {y1} Q{mx:.0f} {my:.0f} {x2} {y2}" fill="none" stroke="#8a2a1a" stroke-width="3.5" stroke-dasharray="10 8"/>'
    for x, y, nombre, ic in lugares:
        s += f'<circle cx="{x}" cy="{y}" r="34" fill="#f6ead0" stroke="{tinta}" stroke-width="3.5"/>'
        if ic == 'ventanilla':
            s += f'<rect x="{x - 18}" y="{y - 16}" width="36" height="30" fill="none" stroke="{tinta}" stroke-width="3"/><rect x="{x - 12}" y="{y - 10}" width="24" height="12" fill="#e2b04a"/>'
        elif ic == 'palacio':
            s += f'<path d="M{x - 22} {y - 6} L{x} {y - 22} L{x + 22} {y - 6}Z M{x - 18} {y - 4} L{x - 18} {y + 16} M{x - 6} {y - 4} L{x - 6} {y + 16} M{x + 6} {y - 4} L{x + 6} {y + 16} M{x + 18} {y - 4} L{x + 18} {y + 16} M{x - 24} {y + 18} L{x + 24} {y + 18}" fill="#e2b04a" stroke="{tinta}" stroke-width="3"/>'
        elif ic == 'sala':
            s += f'<path d="M{x - 16} {y - 14} L{x + 16} {y - 14} L{x + 16} {y + 16} L{x - 16} {y + 16}Z M{x - 10} {y - 4} L{x + 10} {y - 4} M{x - 10} {y + 4} L{x + 10} {y + 4}" fill="none" stroke="{tinta}" stroke-width="3"/><circle cx="{x + 10}" cy="{y + 10}" r="6" fill="#c0282a"/>'
        elif ic == 'pasaje':
            s += f'<text x="{x}" y="{y + 10}" text-anchor="middle" font-family="Georgia,serif" font-size="28" font-weight="700" fill="#2e5a3e">S/</text>'
        elif ic == 'archivo':
            s += f'<rect x="{x - 18}" y="{y - 16}" width="36" height="32" fill="#c8963a" stroke="{tinta}" stroke-width="3"/><path d="M{x - 18} {y - 4} L{x + 18} {y - 4} M{x - 18} {y + 6} L{x + 18} {y + 6}" stroke="{tinta}" stroke-width="2"/>'
        elif ic == 'torre':
            s += f'<path d="M{x - 14} {y + 20} L{x - 10} {y - 20} L{x + 10} {y - 20} L{x + 14} {y + 20}Z" fill="#c8963a" stroke="{tinta}" stroke-width="3"/><path d="M{x} {y - 20} L{x} {y - 30} L{x + 12} {y - 26} L{x} {y - 22}" fill="#c0282a" stroke="{tinta}" stroke-width="2"/>'
        elif ic == 'taza':
            s += f'<path d="M{x - 14} {y - 8} L{x + 10} {y - 8} L{x + 8} {y + 14} L{x - 12} {y + 14}Z" fill="#fff" stroke="{tinta}" stroke-width="3"/><path d="M{x + 10} {y - 4} q10 2 0 12" fill="none" stroke="{tinta}" stroke-width="3"/><path d="M{x - 6} {y - 14} q4 -6 0 -10 M{x + 2} {y - 14} q4 -6 0 -10" fill="none" stroke="{tinta}" stroke-width="2"/>'
        elif ic == 'libro':
            s += f'<path d="M{x - 20} {y - 10} Q{x - 10} {y - 16} {x} {y - 8} Q{x + 10} {y - 16} {x + 20} {y - 10} L{x + 20} {y + 14} Q{x + 10} {y + 8} {x} {y + 16} Q{x - 10} {y + 8} {x - 20} {y + 14}Z" fill="#fff" stroke="{tinta}" stroke-width="3"/>'
        else:
            s += f'<path d="M{x - 18} {y + 16} L{x - 18} {y - 4} L{x} {y - 20} L{x + 18} {y - 4} L{x + 18} {y + 16}Z" fill="#f2eee4" stroke="{tinta}" stroke-width="3"/><rect x="{x - 5}" y="{y + 2}" width="10" height="14" fill="{tinta}"/>'
        s += (f'<g transform="translate({x} {y + 58})"><rect x="{-len(nombre) * 6.2 - 10:.0f}" y="-16" width="{len(nombre) * 12.4 + 20:.0f}" height="28" rx="4" fill="#f6ead0" stroke="{tinta}" stroke-width="2"/>'
              f'<text y="5" text-anchor="middle" font-family="Georgia,serif" font-size="17" font-weight="700" fill="{tinta}">{nombre}</text></g>')
    # dragón dibujado en el margen
    s += f'<path d="M880 400 q-30 -40 10 -60 q20 30 50 10 q-10 40 30 50 q-50 20 -90 0Z" fill="#b8452a" opacity=".6" stroke="{tinta}" stroke-width="2"/>'
    s += f'<text x="900" y="460" text-anchor="middle" font-family="Georgia,serif" font-style="italic" font-size="15" fill="{tinta}">Aquí hay dragones</text>'
    # rosa de los vientos
    cx, cy = 90, 90
    s += f'<circle cx="{cx}" cy="{cy}" r="56" fill="none" stroke="{tinta}" stroke-width="2.5"/><circle cx="{cx}" cy="{cy}" r="46" fill="none" stroke="{tinta}" stroke-width="1.2"/>'
    for a in range(8):
        r = 60 if a % 2 == 0 else 34
        ang = a * math.pi / 4
        x, y = cx + r * math.sin(ang), cy - r * math.cos(ang)
        x1, y1 = cx + 9 * math.sin(ang + math.pi / 2), cy - 9 * math.cos(ang + math.pi / 2)
        x2, y2 = cx + 9 * math.sin(ang - math.pi / 2), cy - 9 * math.cos(ang - math.pi / 2)
        s += f'<path d="M{x1:.0f} {y1:.0f} L{x:.0f} {y:.0f} L{x2:.0f} {y2:.0f}Z" fill="{"#8a2a1a" if a % 2 == 0 else "#e2c070"}" stroke="{tinta}" stroke-width="2"/>'
    s += f'<text x="{cx}" y="{cy - 66}" text-anchor="middle" font-family="Georgia,serif" font-size="18" font-weight="700" fill="{tinta}">N</text>'
    s += f'<text x="500" y="44" text-anchor="middle" font-family="Georgia,serif" font-size="34" font-weight="700" font-style="italic" fill="{tinta}">República de Expedientia</text>'
    s += f'<rect x="10" y="10" width="{Wd - 20}" height="{Hd - 20}" fill="none" stroke="{tinta}" stroke-width="4"/><rect x="18" y="18" width="{Wd - 36}" height="{Hd - 36}" fill="none" stroke="{tinta}" stroke-width="1.5"/>'
    dv, sv = vineta(Wd, Hd, 'vin', .35)
    guardar('mapa', Wd, Hd, s + sv, d + dv)


def retrato(nombre, p, luz, fondo, extra=''):
    """Busto para personajes no jugadores (recorte cuadrado de la figura)."""
    rnd = random.Random(nombre)
    d = P.defs_figura(p) + rg('fr', [(0, luz), (.6, fondo), (1, '#0a0610')], .5, .4, .7)
    s = '<rect x="150" y="60" width="300" height="300" fill="url(#fr)"/>'
    s += chispas(rnd, 20, 150, 60, 450, 360, claro(luz, .3), .8, 2.2, (.2, .6))
    fig = P._cabello_atras(p) + P.cuerpo(p) + P.cara(p) + P._cabello_frente(p) + extra
    s += '<use href="#fig" filter="url(#contorno)" transform="translate(-4 -3)"/>' + f'<g id="fig">{fig}</g>'
    d += '<filter id="contorno"><feColorMatrix values="0 0 0 0 1  0 0 0 0 .93  0 0 0 0 .75  0 0 0 .9 0"/><feGaussianBlur stdDeviation="1.5"/></filter>'
    d += rg('vinr', [(.55, '#000', 0), (1, '#0a0510', .7)], .5, .5, .72)
    s += '<rect x="150" y="60" width="300" height="300" fill="url(#vinr)"/>'
    svg = (f'<svg xmlns="http://www.w3.org/2000/svg" width="300" height="300" viewBox="150 60 300 300"><defs>{FILTROS}{d}</defs>{s}</svg>')
    with open(os.path.join(OUT, nombre + '.svg'), 'w', encoding='utf-8') as f:
        f.write(svg)


def pnjs():
    retrato('pnj-eusebio', dict(piel='#c08a64', pelo='#d6d0ca', pelo_estilo='canoso', ojos='#3a2a1a', saco='#6a5a44', camisa='#efe6d2',
                                lentes='#4a3a2a', bigote='#d6d0ca', arrugas=True, boca='sonrisa'), '#d8a85a', '#3a2a1a')
    retrato('pnj-rosita', dict(piel='#d6a07a', pelo='#5a2a1a', pelo_estilo='rizado', ojos='#3a2a1a', saco='#2a6a7a', camisa='#f4ecda',
                               boca='sonrisa', panuelo='#e2b04a'), '#7ad0d8', '#1a3a40')
    retrato('pnj-valdivia', dict(piel='#c8926a', pelo='#2a1a1a', pelo_estilo='mono', ojos='#2a1a10', saco='#1a1a22', camisa='#f4f1ea',
                                 boca='confiada', cejas=2, lentes='#b08a3a', arrugas=True), '#e2b04a', '#2a1a2a')
    retrato('pnj-mendoza', dict(piel='#b9805a', pelo='#1a1210', pelo_estilo='corto', ojos='#2a1a10', saco='#4a4a52', camisa='#e8eef4',
                                corbata='#2a4a7a', boca='firme', cejas=4), '#9ab0d0', '#1a2030')
    retrato('pnj-quispe', dict(piel='#a8704e', pelo='#120c0a', pelo_estilo='corto', ojos='#2a1a10', saco='#3a5a3a', camisa='#d8e0c8',
                               corbata='#2a3a2a', boca='firme', bigote='#120c0a', botones='#e2b04a'), '#b0d890', '#142a14',
            extra=f'<path d="M232 150 Q240 96 300 92 Q360 96 368 150Z" fill="#2e4a2e" {T(5)}/><path d="M220 152 L380 152 L374 168 L226 168Z" fill="#1a2a1a" {T(4)}/><circle cx="300" cy="124" r="10" fill="#e2b04a" {T(3)}/>')
    retrato('pnj-cliente', dict(piel='#d0986e', pelo='#3a2a20', pelo_estilo='corto', ojos='#3a2a1a', saco='#8a6a4a', camisa='#e8dcc8',
                                boca='firme', cejas=-4), '#e0b080', '#3a2418',
            extra=f'<path d="M354 160 q8 14 0 22 q-8 -8 0 -22Z" fill="#9fd6ff" {T(2)}/>')


def adornos():
    # dado de veinte caras
    d = volumen('dado', '#b02a2a')
    s = '<g transform="translate(100 100)">'
    pts = [(0, -90), (78, -45), (78, 45), (0, 90), (-78, 45), (-78, -45)]
    s += '<path d="M' + ' L'.join(f'{x} {y}' for x, y in pts) + f'Z" fill="url(#dado)" {T(5)}/>'
    s += f'<path d="M0 -90 L-46 26 L46 26Z" fill="{claro("#b02a2a", .2)}" {T(4)}/>'
    for a, b in [((-78, -45), (-46, 26)), ((78, -45), (46, 26)), ((-46, 26), (0, 90)), ((46, 26), (0, 90)), ((-78, 45), (-46, 26)), ((78, 45), (46, 26))]:
        s += f'<path d="M{a[0]} {a[1]} L{b[0]} {b[1]}" {T(4)}/>'
    s += '<text x="0" y="12" text-anchor="middle" font-family="Georgia,serif" font-size="40" font-weight="700" fill="#fff4dc">20</text></g>'
    guardar('d20', 200, 200, s, d)


if __name__ == '__main__':
    portada(); escena_mesa(); escena_archivo(); mapa(); pnjs(); adornos()
