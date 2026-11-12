# Portada ilustrada (v0.10.0), 1024 × 1420: la proporción con que se muestra en el celular.
# Composición propia: de noche, un dragón hecho de expedientes se enrosca en la Torre de Expedientes
# y lanza un torbellino de papeles; abajo, el equipo del estudio lo enfrenta de espaldas, en la
# escalinata de sillar, con la pluma de la Civilista encendida como antorcha. Al fondo, el volcán.
# Se ejecuta desde generar.py y usa sus funciones (quebrada, trazo, _mezclar, arco, guardar).
PW, PH = 1024, 1420
_p = random.Random(1420)


def _resplandor(cx, cy, r, color, opacidad, gid):
    return (f'<defs><radialGradient id="{gid}"><stop offset="0" stop-color="{color}" stop-opacity="{opacidad}"/>'
            f'<stop offset="1" stop-color="{color}" stop-opacity="0"/></radialGradient></defs>'
            f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="url(#{gid})"/>')


def _cielo():
    s = ('<defs><linearGradient id="pcielo" x1="0" y1="0" x2="0" y2="1">'
         '<stop offset="0" stop-color="#07061a"/><stop offset="0.32" stop-color="#1b1640"/>'
         '<stop offset="0.55" stop-color="#3d2a5c"/><stop offset="0.7" stop-color="#8a4a5a"/><stop offset="0.78" stop-color="#d98a52"/></linearGradient></defs>'
         f'<rect x="-40" y="-40" width="{PW + 80}" height="{PH + 80}" fill="url(#pcielo)"/>')
    for _ in range(160):
        x, y = _p.uniform(0, PW), _p.uniform(14, 640)
        r = _p.choice([0.8, 1, 1.2, 1.6, 2.2])
        s += f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{r}" fill="#fff8e8" opacity="{_p.uniform(0.25, 0.95) * (1 - y / 800):.2f}"/>'
    # luna con halo
    s += _resplandor(250, 330, 260, '#cfd8ff', 0.35, 'halo-luna')
    s += ('<defs><radialGradient id="luna" cx="0.42" cy="0.38" r="0.65"><stop offset="0" stop-color="#fffbea"/>'
          '<stop offset="0.75" stop-color="#e9e2c8"/><stop offset="1" stop-color="#bfb79a"/></radialGradient></defs>'
          '<circle cx="250" cy="330" r="96" fill="url(#luna)"/>')
    for cx, cy, r in ((222, 300, 18), (282, 360, 12), (262, 296, 8), (230, 368, 9)):
        s += f'<circle cx="{cx}" cy="{cy}" r="{r}" fill="#cfc6a6" opacity="0.6"/>'
    return s


def _nubes(y0, color, luz, n, gid):
    s = f'<defs><filter id="{gid}" x="-20%" y="-50%" width="140%" height="200%"><feGaussianBlur stdDeviation="6"/></filter></defs><g filter="url(#{gid})">'
    for _ in range(n):
        x, y = _p.uniform(-100, PW), y0 + _p.uniform(-40, 40)
        w, h = _p.uniform(160, 340), _p.uniform(26, 50)
        s += f'<ellipse cx="{x:.0f}" cy="{y:.0f}" rx="{w / 2:.0f}" ry="{h / 2:.0f}" fill="{color}" opacity="0.75"/>'
        s += f'<ellipse cx="{x - w * 0.08:.0f}" cy="{y - h * 0.25:.0f}" rx="{w * 0.36:.0f}" ry="{h * 0.22:.0f}" fill="{luz}" opacity="0.45"/>'
    return s + '</g>'


def _montanas():
    lejos = quebrada([(-20, 900), (150, 820), (360, 860), (560, 790), (780, 850), (1044, 800)], 48, 6, _p)
    s = f'<path d="{trazo(lejos + [(1044, 1100), (-20, 1100)])}" fill="#4a3b66" opacity="0.9"/>'
    cono = quebrada([(380, 1000), (640, 600), (700, 594), (1060, 1000)], 30, 6, _p)
    s += ('<defs><linearGradient id="pvolcan" x1="0" y1="0" x2="1" y2="0.4"><stop offset="0" stop-color="#6a5a8e"/>'
          '<stop offset="0.55" stop-color="#3f3360"/><stop offset="1" stop-color="#2a2244"/></linearGradient>'
          f'<clipPath id="pcono"><path d="{trazo(cono)}"/></clipPath></defs>'
          f'<path d="{trazo(cono)}" fill="url(#pvolcan)"/>')
    nieve = quebrada([(540, 690), (600, 680), (650, 720), (700, 676), (760, 724), (840, 690)], 18, 5, _p)
    s += (f'<g clip-path="url(#pcono)"><path d="{trazo([(500, 560)] + nieve + [(880, 560)])}" fill="#eef0ff" opacity="0.92"/>'
          f'<path d="{trazo([(500, 560)] + nieve + [(880, 560)])}" fill="none" stroke="#9aa4d8" stroke-width="3"/>')
    for x, d in ((600, -90), (650, -30), (700, 40), (750, 120)):
        s += f'<path d="{trazo(quebrada([(x, 720), (x + d, 1000)], 12, 4, _p), False)}" fill="none" stroke="#2a2244" stroke-width="4" opacity="0.6"/>'
    s += '</g>'
    # neblina en la base
    s += ('<defs><linearGradient id="niebla" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#d98a52" stop-opacity="0"/>'
          '<stop offset="1" stop-color="#d98a52" stop-opacity="0.45"/></linearGradient></defs>'
          f'<rect y="860" width="{PW}" height="160" fill="url(#niebla)"/>')
    return s


def _ciudad():
    """Arcos de sillar con ventanas encendidas, en contraluz."""
    s = '<defs><linearGradient id="pmuro" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#5a4a6a"/><stop offset="1" stop-color="#2a2038"/></linearGradient></defs>'
    s += f'<rect x="-10" y="1010" width="{PW + 20}" height="200" fill="url(#pmuro)" stroke="{TINTA}" stroke-width="4"/>'
    for i in range(9):
        x = -40 + i * 125
        s += f'<path d="M{x} 1210 L{x} 1080 A45 45 0 0 1 {x + 90} 1080 L{x + 90} 1210 Z" fill="#160f22"/>'
        if _p.random() < 0.6:
            s += _resplandor(x + 45, 1110, 70, '#ffb860', 0.35, f'vent{i}')
            s += f'<rect x="{x + 33}" y="1090" width="24" height="34" rx="12" fill="#ffcf7a"/>'
    for i in range(14):  # cúpulas y espadañas
        x = _p.uniform(0, PW)
        s += f'<path d="M{x - 30:.0f} 1012 Q{x:.0f} {960 - _p.uniform(0, 30):.0f} {x + 30:.0f} 1012 Z" fill="#3a2e4c"/>'
    return s


def _torre():
    """Torre de expedientes apilados, iluminada desde abajo por el fuego del dragón."""
    s = ''
    y = 1040
    for i in range(16):
        w = 190 - i * 6
        x = 760 - w / 2 + _p.uniform(-10, 10)
        h = 44
        y -= h
        tono = ['#efe3c8', '#e2d1ae', '#f4e9d2', '#d9c49e'][i % 4]
        s += (f'<rect x="{x:.0f}" y="{y}" width="{w}" height="{h}" fill="{tono}" stroke="{TINTA}" stroke-width="4" transform="rotate({_p.uniform(-2.5, 2.5):.1f} {x + w / 2:.0f} {y + h / 2})"/>'
              f'<path d="M{x + 6:.0f} {y + 14} h{w - 12} M{x + 6:.0f} {y + 26} h{w - 20}" stroke="#b9a37c" stroke-width="2"/>'
              f'<path d="M{x:.0f} {y + 34} h{w}" stroke="{CARMESI}" stroke-width="5"/>')
    return s


def _hoja(cx, cy, w, h, ang, tono, borde=TINTA, grosor=3):
    return (f'<g transform="translate({cx:.1f} {cy:.1f}) rotate({ang:.1f})">'
            f'<rect x="{-w / 2:.1f}" y="{-h / 2:.1f}" width="{w:.1f}" height="{h:.1f}" fill="{tono}" stroke="{borde}" stroke-width="{grosor}"/>'
            f'<path d="M{-w / 2 + 4:.1f} {-h / 2 + h * 0.3:.1f} h{w * 0.7:.1f} M{-w / 2 + 4:.1f} {-h / 2 + h * 0.55:.1f} h{w * 0.5:.1f}" stroke="#a08a6a" stroke-width="1.6"/></g>')


def _bezier(p0, p1, p2, p3, t):
    u = 1 - t
    x = u ** 3 * p0[0] + 3 * u * u * t * p1[0] + 3 * u * t * t * p2[0] + t ** 3 * p3[0]
    y = u ** 3 * p0[1] + 3 * u * u * t * p1[1] + 3 * u * t * t * p2[1] + t ** 3 * p3[1]
    return x, y


def _dragon():
    """Cuerpo de hojas superpuestas a lo largo de dos curvas; alas de abanico de papel plegado; cabeza de origami."""
    s = ''
    # alas (detrás del cuerpo): abanicos de hojas plegadas
    for (ox, oy, a0, a1, largo) in ((600, 610, 175, 95, 360), (640, 600, 80, 15, 330)):
        n = 9
        for k in range(n):
            a = math.radians(a0 + (a1 - a0) * k / (n - 1))
            x2, y2 = ox + math.cos(a) * largo, oy - math.sin(a) * largo
            a2 = math.radians(a0 + (a1 - a0) * (k + 1) / (n - 1)) if k < n - 1 else a
            x3, y3 = ox + math.cos(a2) * largo * 0.92, oy - math.sin(a2) * largo * 0.92
            tono = _mezclar('#e8d9b8', '#7a5a6a', 0.15 + 0.5 * (k % 2))
            s += f'<path d="M{ox} {oy} L{x2:.0f} {y2:.0f} L{x3:.0f} {y3:.0f} Z" fill="{tono}" stroke="{TINTA}" stroke-width="3" stroke-linejoin="round"/>'
        s += f'<circle cx="{ox}" cy="{oy}" r="10" fill="{CARMESI}" stroke="{TINTA}" stroke-width="3"/>'
    # cuerpo: cola que baja enroscándose a la torre, y cuello que sube hacia la cabeza
    curvas = [((560, 1020), (980, 990), (980, 760), (700, 700)),
              ((700, 700), (480, 650), (560, 470), (420, 430))]
    puntos = []
    for idx, c in enumerate(curvas):
        for k in range(60):
            t = k / 59
            puntos.append((_bezier(*c, t), idx + t))
    for i, ((x, y), u) in enumerate(puntos):
        (x2, y2), _ = puntos[min(i + 1, len(puntos) - 1)]
        ang = math.degrees(math.atan2(y2 - y, x2 - x))
        grosor = 18 + 34 * min(1, u / 1.4)  # la cola es delgada, el cuello grueso
        tono = _mezclar('#efe2c4', '#b8a07a', (i % 3) / 3) if i % 7 else '#c9b38a'
        s += _hoja(x, y, grosor * 1.3, grosor, ang + _p.uniform(-12, 12), tono)
        if i % 9 == 4:  # lacres rojos a lo largo del lomo
            s += f'<circle cx="{x:.0f}" cy="{y - grosor * 0.35:.0f}" r="{grosor * 0.18:.1f}" fill="{CARMESI}" stroke="{TINTA}" stroke-width="2"/>'
    # cabeza de origami mirando a la izquierda, hacia el equipo
    hx, hy = 400, 430
    s += f'<g transform="translate({hx} {hy}) scale(1.45) translate({-hx} {-hy})">'
    s += (f'<path d="M{hx + 40} {hy - 50} L{hx - 70} {hy - 20} L{hx - 140} {hy + 20} L{hx - 60} {hy + 30} L{hx + 50} {hy + 40} Z" fill="#efe2c4" stroke="{TINTA}" stroke-width="4" stroke-linejoin="round"/>'
          f'<path d="M{hx - 140} {hy + 20} L{hx - 70} {hy + 60} L{hx + 30} {hy + 70} L{hx + 50} {hy + 40} L{hx - 60} {hy + 30} Z" fill="#cdb791" stroke="{TINTA}" stroke-width="4" stroke-linejoin="round"/>'
          f'<path d="M{hx - 70} {hy - 20} L{hx - 10} {hy + 5} L{hx + 40} {hy - 50}" fill="none" stroke="#a08a6a" stroke-width="2"/>'
          f'<path d="M{hx + 20} {hy - 45} L{hx + 70} {hy - 120} L{hx + 50} {hy - 40} Z" fill="#d9c49e" stroke="{TINTA}" stroke-width="4" stroke-linejoin="round"/>'
          f'<path d="M{hx - 10} {hy - 30} L{hx + 10} {hy - 100} L{hx + 15} {hy - 32} Z" fill="#e8d9b8" stroke="{TINTA}" stroke-width="4" stroke-linejoin="round"/>'
          + _resplandor(hx - 40, hy - 2, 26, '#ffb030', 0.9, 'ojo')
          + f'<path d="M{hx - 58} {hy - 6} L{hx - 26} {hy - 12} L{hx - 34} {hy + 2} Z" fill="#ffd34a" stroke="{TINTA}" stroke-width="2.5"/>'
          f'<circle cx="{hx - 10}" cy="{hy - 28}" r="11" fill="{CARMESI}" stroke="{TINTA}" stroke-width="3"/>'
          f'<path d="M{hx - 132} {hy + 26} l10 14 l8 -12 l10 16 l8 -14" fill="none" stroke="{TINTA}" stroke-width="3"/>')
    return s + '</g>'


def _torbellino():
    """El aliento del dragón: un torrente de papeles y chispas que baja en espiral hacia el equipo."""
    s = _resplandor(220, 760, 260, '#ff9a3c', 0.35, 'aliento')
    for k in range(70):
        t = k / 69
        x = 270 - 150 * t + math.sin(t * 9) * 60 * t + _p.uniform(-30, 30)
        y = 470 + 520 * t + _p.uniform(-25, 25)
        ang = _p.uniform(0, 360)
        w = 18 + 26 * t
        tono = _mezclar('#fff2d0', '#ffb055', _p.uniform(0, 0.8))
        s += _hoja(x, y, w, w * 1.3, ang, tono, '#4a2a1a', 2)
    for _ in range(90):
        t = _p.random()
        x = 260 - 150 * t + _p.uniform(-90, 90)
        y = 480 + 520 * t + _p.uniform(-40, 40)
        s += f'<circle cx="{x:.0f}" cy="{y:.0f}" r="{_p.uniform(1.5, 4):.1f}" fill="#ffd27a" opacity="{_p.uniform(0.5, 1):.2f}"/>'
    return s


def _escalinata():
    """Escalinata de sillar en perspectiva, más oscura hacia abajo (la luz viene del fondo)."""
    s = ''
    fuga = (PW / 2, 900)
    for i in range(7):
        y = 1200 + i * 34 + i * i * 2
        h = 34 + i * 4
        tono = _mezclar('#cbbd9f', '#2a2030', i / 7)
        s += f'<rect x="-20" y="{y}" width="{PW + 40}" height="{h}" fill="{tono}" stroke="{TINTA}" stroke-width="3"/>'
        s += f'<rect x="-20" y="{y}" width="{PW + 40}" height="5" fill="#fff0c8" opacity="{0.35 - i * 0.04:.2f}"/>'
        for k in range(-6, 8):
            x0 = PW / 2 + k * 150
            xs = fuga[0] + (x0 - fuga[0]) * (y - fuga[1]) / (1200 - fuga[1])
            xi = fuga[0] + (x0 - fuga[0]) * (y + h - fuga[1]) / (1200 - fuga[1])
            s += f'<path d="M{xs:.0f} {y} L{xi:.0f} {y + h}" stroke="{TINTA}" stroke-width="2" opacity="0.45"/>'
    return s


def _figura(cx, base, alto, capa, pelo, peinado='corto', brazo=None, piel='#d9a27a', maletin=False):
    """Abogado de espaldas, en contraluz: toga con pliegues movida por el viento, hombros, cabello
    y borde iluminado por la pluma encendida (luz a la izquierda)."""
    hombro = alto * 0.26
    cuello_y = base - alto * 0.8
    cabeza_r = alto * 0.095
    s = ''
    # toga: trapecio con vuelo hacia la derecha (viento del torbellino)
    s += (f'<path d="M{cx - hombro:.0f} {cuello_y + alto * 0.06:.0f} Q{cx - hombro * 1.15:.0f} {base - alto * 0.35:.0f} {cx - hombro * 1.05:.0f} {base} '
          f'L{cx + hombro * 1.5:.0f} {base + 4} Q{cx + hombro * 1.25:.0f} {base - alto * 0.4:.0f} {cx + hombro:.0f} {cuello_y + alto * 0.06:.0f} '
          f'Q{cx:.0f} {cuello_y - alto * 0.02:.0f} {cx - hombro:.0f} {cuello_y + alto * 0.06:.0f} Z" fill="{capa}" stroke="{TINTA}" stroke-width="5" stroke-linejoin="round"/>')
    for k in range(4):  # pliegues
        x0 = cx - hombro * 0.6 + k * hombro * 0.45
        s += f'<path d="M{x0:.0f} {cuello_y + alto * 0.2:.0f} Q{x0 + 8:.0f} {base - alto * 0.3:.0f} {x0 + hombro * 0.12 * (k + 1):.0f} {base}" fill="none" stroke="{TINTA}" stroke-width="3" opacity="0.45"/>'
    # borde de luz
    s += (f'<path d="M{cx - hombro:.0f} {cuello_y + alto * 0.06:.0f} Q{cx - hombro * 1.15:.0f} {base - alto * 0.35:.0f} {cx - hombro * 1.05:.0f} {base}" fill="none" stroke="#ffd98a" stroke-width="5" opacity="0.85"/>'
          f'<path d="M{cx - hombro:.0f} {cuello_y + alto * 0.06:.0f} Q{cx:.0f} {cuello_y - alto * 0.02:.0f} {cx + hombro * 0.4:.0f} {cuello_y + alto * 0.02:.0f}" fill="none" stroke="#ffd98a" stroke-width="4" opacity="0.7"/>')
    # cuello y cabeza
    s += f'<rect x="{cx - cabeza_r * 0.45:.0f}" y="{cuello_y - cabeza_r * 0.6:.0f}" width="{cabeza_r * 0.9:.0f}" height="{cabeza_r * 0.8:.0f}" fill="{_mezclar(piel, "#2a1a20", 0.45)}"/>'
    s += f'<ellipse cx="{cx}" cy="{cuello_y - cabeza_r * 1.2:.0f}" rx="{cabeza_r:.0f}" ry="{cabeza_r * 1.12:.0f}" fill="{pelo}" stroke="{TINTA}" stroke-width="4"/>'
    if peinado == 'largo':
        s += (f'<path d="M{cx - cabeza_r:.0f} {cuello_y - cabeza_r * 1.2:.0f} Q{cx - cabeza_r * 1.25:.0f} {cuello_y + alto * 0.1:.0f} {cx - cabeza_r * 0.3:.0f} {cuello_y + alto * 0.16:.0f} '
              f'L{cx + cabeza_r * 0.6:.0f} {cuello_y + alto * 0.15:.0f} Q{cx + cabeza_r * 1.3:.0f} {cuello_y + alto * 0.05:.0f} {cx + cabeza_r:.0f} {cuello_y - cabeza_r * 1.2:.0f} Z" fill="{pelo}" stroke="{TINTA}" stroke-width="4"/>')
    elif peinado == 'canoso':  # coronilla calva y cabello cano a los lados
        s += f'<ellipse cx="{cx}" cy="{cuello_y - cabeza_r * 1.45:.0f}" rx="{cabeza_r * 0.62:.0f}" ry="{cabeza_r * 0.7:.0f}" fill="{piel}" opacity="0.85"/>'
    s += f'<path d="M{cx - cabeza_r * 0.95:.0f} {cuello_y - cabeza_r * 1.3:.0f} A{cabeza_r:.0f} {cabeza_r * 1.1:.0f} 0 0 1 {cx - cabeza_r * 0.1:.0f} {cuello_y - cabeza_r * 2.25:.0f}" fill="none" stroke="#ffd98a" stroke-width="4" opacity="0.85"/>'
    if brazo:
        bx, by = brazo
        sx, sy = cx - hombro * 0.85, cuello_y + alto * 0.1
        s += (f'<path d="M{sx:.0f} {sy:.0f} Q{(sx + bx) / 2 - 20:.0f} {(sy + by) / 2 + 10:.0f} {bx} {by}" fill="none" stroke="{TINTA}" stroke-width="{alto * 0.1 + 8:.0f}" stroke-linecap="round"/>'
              f'<path d="M{sx:.0f} {sy:.0f} Q{(sx + bx) / 2 - 20:.0f} {(sy + by) / 2 + 10:.0f} {bx} {by}" fill="none" stroke="{capa}" stroke-width="{alto * 0.1:.0f}" stroke-linecap="round"/>'
              f'<circle cx="{bx}" cy="{by}" r="{alto * 0.045:.0f}" fill="{piel}" stroke="{TINTA}" stroke-width="3"/>')
    if maletin:
        mx, my = cx + hombro * 1.1, base - alto * 0.32
        s += (f'<rect x="{mx:.0f}" y="{my:.0f}" width="{alto * 0.2:.0f}" height="{alto * 0.15:.0f}" rx="5" fill="#5a3a24" stroke="{TINTA}" stroke-width="4"/>'
              f'<path d="M{mx + alto * 0.06:.0f} {my:.0f} v-10 h{alto * 0.08:.0f} v10" fill="none" stroke="{TINTA}" stroke-width="4"/>')
    return s


def _equipo():
    s = _resplandor(330, 1000, 260, '#ffd77a', 0.5, 'pluma-luz')
    s += _figura(150, 1400, 330, '#24314f', '#1a1410', 'corto', maletin=True, piel='#c98f62')
    s += _figura(560, 1410, 300, '#3a2a22', '#bdb6aa', 'canoso')
    s += _figura(355, 1380, 370, '#1d1a2a', '#2b1d16', 'largo', brazo=(318, 1036), piel='#e0ac84')
    # pluma encendida en alto
    s += (f'<path d="M314 1040 Q296 960 336 890 Q348 960 322 1040 Z" fill="#fff6dc" stroke="{TINTA}" stroke-width="3"/>'
          f'<path d="M318 1038 L333 900" stroke="#c8a24a" stroke-width="2"/>'
          + _resplandor(334, 890, 60, '#fff2b0', 0.95, 'punta'))
    return s


def _rayos():
    s = ('<defs><linearGradient id="rayo" x1="0" y1="1" x2="0" y2="0"><stop offset="0" stop-color="#fff2c0" stop-opacity="0.3"/><stop offset="1" stop-color="#fff2c0" stop-opacity="0"/></linearGradient>'
         '<filter id="difuso" x="-50%" y="-50%" width="200%" height="200%"><feGaussianBlur stdDeviation="14"/></filter></defs><g filter="url(#difuso)">')
    for a in (-2, -1, 0, 1, 2):
        s += f'<path d="M334 890 L{334 + a * 120 - 16} 520 L{334 + a * 120 + 16} 520 Z" fill="url(#rayo)"/>'
    return s + '</g>'



guardar('portada-vectorial', PW, PH,
        _cielo() + _nubes(520, '#4b3a6a', '#c9a0b8', 9, 'nub1') + _montanas() + _nubes(780, '#7a4a62', '#ffb88a', 7, 'nub2')
        + _ciudad() + _torre() + '<g transform="translate(0 60)">' + _dragon() + '</g>' + _torbellino() + _escalinata() + _rayos() + _equipo())


# ───────────── Logotipo (v0.10.0) ─────────────
# Letras Cinzel Decorative (licencia OFL; render.js carga las fuentes) en oro con relieve, filete y un sello de lacre con la balanza.
def _logo():
    LW, LH = 1024, 470
    defs = ('<defs><linearGradient id="oro" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#fff3c4"/>'
            '<stop offset="0.38" stop-color="#f0c45a"/><stop offset="0.62" stop-color="#b9822a"/><stop offset="1" stop-color="#ffe08a"/></linearGradient>'
            '<filter id="relieve" x="-10%" y="-20%" width="120%" height="140%">'
            '<feGaussianBlur in="SourceAlpha" stdDeviation="2" result="b"/>'
            '<feSpecularLighting in="b" surfaceScale="4" specularConstant="0.9" specularExponent="25" lighting-color="#fffbe8" result="s"><feDistantLight azimuth="235" elevation="45"/></feSpecularLighting>'
            '<feComposite in="s" in2="SourceAlpha" operator="in" result="s2"/>'
            '<feComposite in="SourceGraphic" in2="s2" operator="arithmetic" k2="1" k3="0.6"/></filter>'
            '<filter id="sombra" x="-10%" y="-20%" width="120%" height="150%"><feDropShadow dx="0" dy="8" stdDeviation="7" flood-color="#000" flood-opacity="0.75"/></filter>'
            '<radialGradient id="lacre" cx="0.4" cy="0.35" r="0.7"><stop offset="0" stop-color="#e86a50"/><stop offset="0.7" stop-color="#a8301e"/><stop offset="1" stop-color="#6e1a10"/></radialGradient></defs>')

    def palabra(texto, y, tam):
        comun = f'x="{LW / 2}" y="{y}" font-family="Cinzel Decorative" font-size="{tam}" text-anchor="middle" letter-spacing="4"'
        return (f'<text {comun} fill="none" stroke="#1b1020" stroke-width="16" stroke-linejoin="round">{texto}</text>'
                f'<text {comun} fill="url(#oro)" stroke="#6a4214" stroke-width="2.5" filter="url(#relieve)">{texto}</text>')

    filete = ''
    for lado in (-1, 1):
        x0 = LW / 2 + lado * 70
        x1 = LW / 2 + lado * 440
        filete += (f'<path d="M{x0} 232 L{x1} 232" stroke="#1b1020" stroke-width="9" stroke-linecap="round"/>'
                   f'<path d="M{x0} 232 L{x1} 232" stroke="url(#oro)" stroke-width="4" stroke-linecap="round"/>'
                   f'<path d="M{x1} 232 q{lado * 22} -18 {lado * 8} -32 q{-lado * 14} 10 {-lado * 2} 22" fill="none" stroke="#f0c45a" stroke-width="4"/>'
                   f'<circle cx="{x0 + lado * 30}" cy="232" r="6" fill="#f0c45a" stroke="#1b1020" stroke-width="3"/>')
    # sello de lacre con balanza en el centro del filete
    sello = (f'<g transform="translate({LW / 2} 232)">'
             '<path d="M0 -50 C18 -52 30 -40 44 -34 C52 -18 46 0 50 14 C42 30 30 42 12 48 C-6 54 -24 46 -38 36 C-50 22 -48 4 -52 -12 C-44 -30 -28 -46 0 -50 Z" fill="url(#lacre)" stroke="#2a0c08" stroke-width="4"/>'
             '<circle r="34" fill="none" stroke="#f3a088" stroke-width="2" opacity="0.6"/>'
             '<path d="M0 -24 V22 M-16 24 H16 M-26 -14 H26" stroke="#ffd9c8" stroke-width="4" stroke-linecap="round"/>'
             '<path d="M-26 -14 L-34 6 H-18 Z M26 -14 L18 6 H34 Z" fill="none" stroke="#ffd9c8" stroke-width="3" stroke-linejoin="round"/></g>')
    cuerpo = (defs + '<g filter="url(#sombra)">'
              + palabra('EXPEDIENTES', 175, 120) + filete + sello
              + f'<text x="{LW / 2 - 150}" y="250" font-family="Alegreya Italic" font-size="54" text-anchor="middle" fill="#f6e6b8" stroke="#1b1020" stroke-width="7" paint-order="stroke">y</text>'
              + palabra('DRAGONES', 365, 132) + '</g>')
    guardar('logo', LW, LH, cuerpo, pintar=False)


_logo()
