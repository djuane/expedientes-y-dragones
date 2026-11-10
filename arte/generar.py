# Genera el arte original de Expedientes y Dragones como SVG (caricatura plana, contorno oscuro).
# Autor del arte: Leonel Díaz. Todo se dibuja desde cero con figuras geométricas.
import os, math

OUT = os.path.join(os.path.dirname(__file__), 'svg')
os.makedirs(OUT, exist_ok=True)

TINTA = '#1b1626'
NOCHE = '#0E0C1A'
DORADO = '#E2B04A'
PERG = '#EDE3CC'
CARMESI = '#C8442F'
TINTA2 = '#2B2540'
SILLAR = '#F2EEE4'
AZUL = '#3F6FB0'
S = f'stroke="{TINTA}" stroke-width="6" stroke-linejoin="round" stroke-linecap="round"'
S4 = f'stroke="{TINTA}" stroke-width="4" stroke-linejoin="round" stroke-linecap="round"'


def guardar(nombre, w, h, cuerpo, fondo=None):
    bg = f'<rect width="{w}" height="{h}" fill="{fondo}"/>' if fondo else ''
    svg = f'<svg xmlns="http://www.w3.org/2000/svg" width="{w}" height="{h}" viewBox="0 0 {w} {h}">{bg}{cuerpo}</svg>'
    with open(os.path.join(OUT, f'{nombre}.svg'), 'w', encoding='utf-8') as f:
        f.write(svg)


def disco(anillo):
    return (f'<circle cx="128" cy="128" r="118" fill="{TINTA2}" stroke="{anillo}" stroke-width="10"/>'
            f'<circle cx="128" cy="128" r="104" fill="#ffffff" opacity="0.05"/>')


def recorte(cuerpo):
    return ('<defs><clipPath id="c"><circle cx="128" cy="128" r="113"/></clipPath></defs>'
            f'<g clip-path="url(#c)">{cuerpo}</g>')


# ───────────────────────── Personas ─────────────────────────
def persona(piel='#d9a27a', pelo='#2a1f1a', estilo='corto', ropa='#222031', cuello='camisa', boca='sonrisa',
            lentes=False, bigote=False, extra='', cejas='normal', ojos='normal', rubor=True):
    p = []
    # torso
    p.append(f'<path d="M28 262 C34 196 74 172 128 172 C182 172 222 196 228 262 Z" fill="{ropa}" {S}/>')
    if cuello == 'toga':
        p.append(f'<path d="M100 176 L128 230 L156 176 Z" fill="#ffffff" {S4}/>')
        p.append(f'<path d="M116 196 L128 250 L140 196" fill="#ffffff" {S4}/>')
        p.append(f'<path d="M70 190 C86 210 92 236 92 262 M186 190 C170 210 164 236 164 262" fill="none" stroke="{DORADO}" stroke-width="7"/>')
    elif cuello == 'corbata':
        p.append(f'<path d="M104 176 L128 204 L152 176 Z" fill="#f4f1ea" {S4}/>')
        p.append(f'<path d="M128 202 L118 214 L128 262 L138 214 Z" fill="{CARMESI}" {S4}/>')
    elif cuello == 'chompa':
        p.append(f'<path d="M100 174 Q128 206 156 174" fill="none" {S4}/>')
        p.append(f'<path d="M110 176 L128 198 L146 176 Z" fill="#f4f1ea" {S4}/>')
        p.append(f'<circle cx="128" cy="224" r="4" fill="{TINTA}"/><circle cx="128" cy="244" r="4" fill="{TINTA}"/>')
    elif cuello == 'uniforme':
        p.append(f'<path d="M100 176 L128 200 L156 176" fill="none" {S4}/>')
        p.append(f'<rect x="150" y="206" width="34" height="22" rx="4" fill="{DORADO}" {S4}/>')
        p.append(f'<path d="M167 210 l3 6 7 1 -5 5 1 7 -6 -3 -6 3 1 -7 -5 -5 7 -1 z" fill="{TINTA2}"/>')
    elif cuello == 'perlas':
        for i in range(9):
            a = math.pi * (0.15 + 0.7 * i / 8)
            p.append(f'<circle cx="{128 - 34*math.cos(a):.1f}" cy="{176 + 20*math.sin(a):.1f}" r="5" fill="#fffaf0" stroke="{TINTA}" stroke-width="2"/>')
    elif cuello == 'fotocheck':
        p.append(f'<path d="M104 176 L128 214 L152 176" fill="none" stroke="{AZUL}" stroke-width="5"/>')
        p.append(f'<rect x="114" y="212" width="28" height="36" rx="4" fill="#ffffff" {S4}/>')
        p.append(f'<rect x="120" y="220" width="16" height="12" fill="{AZUL}"/>')
    else:
        p.append(f'<path d="M104 176 L128 204 L152 176 Z" fill="#f4f1ea" {S4}/>')
    # cuello y cabeza
    p.append(f'<rect x="110" y="140" width="36" height="40" rx="12" fill="{piel}" {S4}/>')
    # pelo de atrás
    if estilo == 'largo':
        p.append(f'<path d="M74 92 C62 140 64 170 70 184 L96 184 C92 160 92 130 96 110 Z" fill="{pelo}" {S}/>')
        p.append(f'<path d="M182 92 C194 140 192 170 186 184 L160 184 C164 160 164 130 160 110 Z" fill="{pelo}" {S}/>')
    if estilo == 'mono':
        p.append(f'<circle cx="128" cy="40" r="22" fill="{pelo}" {S}/>')
    p.append(f'<ellipse cx="128" cy="100" rx="52" ry="58" fill="{piel}" {S}/>')
    # orejas
    p.append(f'<ellipse cx="76" cy="106" rx="9" ry="13" fill="{piel}" {S4}/><ellipse cx="180" cy="106" rx="9" ry="13" fill="{piel}" {S4}/>')
    # pelo
    if estilo == 'corto':
        p.append(f'<path d="M76 92 C74 52 100 38 128 38 C160 38 184 54 180 92 C168 72 150 64 128 66 C106 64 88 72 76 92 Z" fill="{pelo}" {S}/>')
    elif estilo == 'calvo':
        p.append(f'<path d="M74 110 C70 84 76 70 86 66 C84 80 86 94 90 104 Z" fill="{pelo}" {S4}/>')
        p.append(f'<path d="M182 110 C186 84 180 70 170 66 C172 80 170 94 166 104 Z" fill="{pelo}" {S4}/>')
        p.append(f'<path d="M104 52 Q128 44 152 52" fill="none" stroke="#ffffff" stroke-opacity="0.35" stroke-width="6"/>')
    elif estilo in ('largo', 'mono'):
        p.append(f'<path d="M74 104 C70 56 98 40 128 40 C160 40 186 56 182 104 C170 78 150 66 126 70 C108 72 90 84 74 104 Z" fill="{pelo}" {S}/>')
    elif estilo == 'gorra':
        p.append(f'<path d="M72 78 C74 44 100 30 128 30 C156 30 182 44 184 78 Z" fill="#2f5d5a" {S}/>')
        p.append(f'<path d="M66 80 L190 80 L196 92 L60 92 Z" fill="#1f3f3d" {S4}/>')
        p.append(f'<circle cx="128" cy="56" r="11" fill="{DORADO}" {S4}/>')
    elif estilo == 'sombrero':
        p.append(f'<ellipse cx="128" cy="62" rx="84" ry="16" fill="#3a2f2a" {S}/>')
        p.append(f'<path d="M84 62 C86 24 104 14 128 14 C152 14 170 24 172 62 Z" fill="#4a3d36" {S}/>')
        p.append(f'<rect x="86" y="46" width="84" height="10" fill="{CARMESI}"/>')
    # cejas
    if cejas == 'arriba':
        p.append(f'<path d="M94 80 Q106 72 116 80 M140 80 Q150 72 162 80" fill="none" {S4}/>')
    elif cejas == 'picaras':
        p.append(f'<path d="M94 84 L116 80 M140 78 L162 86" fill="none" {S4}/>')
    elif cejas == 'cansadas':
        p.append(f'<path d="M94 80 L116 84 M140 84 L162 80" fill="none" {S4}/>')
    else:
        p.append(f'<path d="M94 82 Q105 77 116 82 M140 82 Q151 77 162 82" fill="none" {S4}/>')
    # ojos
    if ojos == 'cansados':
        p.append(f'<path d="M98 100 Q105 96 112 100 M144 100 Q151 96 158 100" fill="none" {S4}/>')
    else:
        p.append(f'<ellipse cx="105" cy="100" rx="6" ry="8" fill="{TINTA}"/><ellipse cx="151" cy="100" rx="6" ry="8" fill="{TINTA}"/>')
        p.append('<circle cx="107" cy="97" r="2" fill="#fff"/><circle cx="153" cy="97" r="2" fill="#fff"/>')
    if lentes:
        p.append(f'<circle cx="105" cy="100" r="16" fill="#ffffff" fill-opacity="0.15" {S4}/><circle cx="151" cy="100" r="16" fill="#ffffff" fill-opacity="0.15" {S4}/><path d="M121 100 L135 100" {S4}/>')
    # nariz
    p.append(f'<path d="M128 104 Q122 120 130 124" fill="none" {S4}/>')
    if rubor:
        p.append(f'<ellipse cx="94" cy="122" rx="9" ry="5" fill="{CARMESI}" opacity="0.25"/><ellipse cx="162" cy="122" rx="9" ry="5" fill="{CARMESI}" opacity="0.25"/>')
    if bigote:
        p.append(f'<path d="M104 134 C112 124 124 126 128 132 C132 126 144 124 152 134 C142 136 134 136 128 134 C122 136 114 136 104 134 Z" fill="{bigote if isinstance(bigote, str) else "#3a2a22"}" {S4}/>')
    # boca
    bocas = {
        'sonrisa': 'M110 138 Q128 152 146 138',
        'preocupada': 'M110 144 Q119 138 128 144 Q137 150 146 144',
        'picara': 'M110 140 Q132 154 150 134',
        'seria': 'M114 142 L142 142',
        'abierta': 'M112 138 Q128 160 144 138 Z',
    }
    relleno = '#7a2a2a' if boca == 'abierta' else 'none'
    p.append(f'<path d="{bocas[boca]}" fill="{relleno}" {S4}/>')
    p.append(extra)
    return ''.join(p)


def ficha_persona(nombre, anillo, **kw):
    guardar(nombre, 256, 256, disco(anillo) + recorte(persona(**kw)))


ficha_persona('civilista', DORADO, piel='#e0ac84', pelo='#2b1d16', estilo='largo', ropa='#1d1a24', cuello='toga', boca='sonrisa', cejas='arriba')
ficha_persona('administrativista', DORADO, piel='#c98f63', pelo='#18120f', estilo='corto', ropa='#2c3f63', cuello='corbata', boca='sonrisa', lentes=True)
ficha_persona('quispe', AZUL, piel='#b9805a', pelo='#18120f', estilo='gorra', ropa='#2f5d5a', cuello='uniforme', boca='seria', cejas='normal')
ficha_persona('eusebio', DORADO, piel='#d4a07a', pelo='#d9d4cc', estilo='calvo', ropa='#6b5a44', cuello='chompa', boca='seria', lentes=True, bigote='#d9d4cc', cejas='cansadas', ojos='cansados')
ficha_persona('cliente', PERG, piel='#dcae86', pelo='#3b2a20', estilo='corto', ropa='#7c8a5c', cuello='camisa', boca='preocupada', cejas='arriba',
              extra=f'<g transform="rotate(-8 190 220)"><rect x="160" y="190" width="56" height="70" fill="#fffaf0" {S4}/><path d="M168 206 h40 M168 218 h40 M168 230 h30" stroke="{TINTA}" stroke-width="3"/></g>')
ficha_persona('rosita', PERG, piel='#c48a62', pelo='#1c1412', estilo='mono', ropa='#a4425a', cuello='camisa', boca='sonrisa', lentes=True)
ficha_persona('valdivia', DORADO, piel='#e6b892', pelo='#efeae2', estilo='mono', ropa='#3b2f4f', cuello='perlas', boca='picara', cejas='normal')
ficha_persona('mendoza', AZUL, piel='#cf9a70', pelo='#2a1b14', estilo='largo', ropa='#455a7a', cuello='fotocheck', boca='sonrisa')
ficha_persona('coimero', CARMESI, piel='#d19a72', pelo='#2a1f1a', estilo='sombrero', ropa='#4a3d36', cuello='corbata', boca='picara', bigote='#1c1412', cejas='picaras')
ficha_persona('tramitador', CARMESI, piel='#c69066', pelo='#1c1412', estilo='sombrero', ropa='#3d3a46', cuello='camisa', boca='picara', cejas='picaras', lentes=True,
              extra=f'<rect x="150" y="196" width="60" height="40" fill="#efe2b8" {S4}/><path d="M150 196 L180 220 L210 196" fill="none" {S4}/>')

# Narrador: un libro abierto con pluma
guardar('narrador', 256, 256, disco(PERG) + f'''
<path d="M40 170 C70 150 104 152 128 168 C152 152 186 150 216 170 L216 92 C186 72 152 74 128 90 C104 74 70 72 40 92 Z" fill="{SILLAR}" {S}/>
<path d="M128 90 L128 168" {S}/>
<path d="M58 104 Q90 92 116 104 M58 122 Q90 110 116 122 M140 104 Q166 92 198 104 M140 122 Q166 110 198 122" fill="none" stroke="{TINTA}" stroke-width="3"/>
<path d="M170 40 C196 56 196 84 176 120 L168 116 C178 84 174 62 170 40 Z" fill="{DORADO}" {S4}/>''')


# ───────────────────────── Monstruos del sistema ─────────────────────────
def ojos_monstruo(x1, y1, x2, y2, r=13, enojo=True):
    s = f'<circle cx="{x1}" cy="{y1}" r="{r}" fill="#fff" {S4}/><circle cx="{x2}" cy="{y2}" r="{r}" fill="#fff" {S4}/>'
    s += f'<circle cx="{x1+2}" cy="{y1+2}" r="{r*0.45:.1f}" fill="{TINTA}"/><circle cx="{x2-2}" cy="{y2+2}" r="{r*0.45:.1f}" fill="{TINTA}"/>'
    if enojo:
        s += f'<path d="M{x1-r-2} {y1-r-6} L{x1+r} {y1-r+4} M{x2+r+2} {y2-r-6} L{x2-r} {y2-r+4}" {S}/>'
    return s


def dientes(x, y, w, n=5, h=12):
    paso = w / n
    s = f'<path d="M{x} {y} Q{x+w/2} {y+44} {x+w} {y} Z" fill="#5a1a22" {S4}/>'
    for i in range(n):
        cx = x + paso * i + paso / 2
        s += f'<path d="M{cx-paso/2+2:.1f} {y+1} L{cx:.1f} {y+h} L{cx+paso/2-2:.1f} {y+1} Z" fill="#fff" stroke="{TINTA}" stroke-width="2"/>'
    return s


def ficha_monstruo(nombre, cuerpo, anillo=CARMESI):
    guardar(nombre, 256, 256, disco(anillo) + cuerpo)


# Formalismo: una hoja-formulario con colmillos y un sello en la frente
ficha_monstruo('formalismo', f'''
<path d="M66 54 L170 54 L196 80 L196 212 L66 212 Z" fill="{SILLAR}" {S}/>
<path d="M170 54 L170 80 L196 80" fill="#d8d0bd" {S4}/>
<path d="M84 160 h40 M84 176 h60 M150 160 h30" stroke="{TINTA}" stroke-width="4" opacity="0.5"/>
<rect x="80" y="186" width="14" height="14" fill="none" stroke="{TINTA}" stroke-width="3"/><rect x="104" y="186" width="14" height="14" fill="none" stroke="{TINTA}" stroke-width="3"/>
{ojos_monstruo(104, 96, 158, 96)}
{dientes(96, 124, 70, 5)}
<g transform="rotate(-14 160 64)"><rect x="128" y="44" width="56" height="26" rx="4" fill="none" stroke="{CARMESI}" stroke-width="5"/><text x="156" y="63" font-family="DejaVu Sans" font-weight="bold" font-size="13" text-anchor="middle" fill="{CARMESI}">REQ.</text></g>
<path d="M60 150 L36 132 M202 150 L226 132" {S}/>''')

# Plazo que Vence: reloj de arena en llamas
ficha_monstruo('plazo', f'''
<path d="M128 26 C150 50 136 60 152 76 C164 60 158 46 170 38 C186 70 176 92 128 94 C80 92 70 68 88 40 C96 56 100 62 108 66 C106 50 118 40 128 26 Z" fill="#f08a2c" {S4}/>
<path d="M128 50 C140 64 134 74 144 82 C126 90 112 86 112 74 C112 66 122 60 128 50 Z" fill="#ffd25a"/>
<rect x="72" y="84" width="112" height="16" rx="6" fill="#7a4a2a" {S}/>
<rect x="72" y="212" width="112" height="16" rx="6" fill="#7a4a2a" {S}/>
<path d="M86 100 L170 100 C170 136 136 146 136 156 C136 166 170 176 170 212 L86 212 C86 176 120 166 120 156 C120 146 86 136 86 100 Z" fill="#cfe7f0" fill-opacity="0.85" {S}/>
<path d="M98 112 L158 112 C150 132 132 140 128 150 C124 140 106 132 98 112 Z" fill="#e8c46a"/>
<path d="M128 156 L128 196" stroke="#e8c46a" stroke-width="4"/>
<path d="M100 212 C108 192 148 192 156 212 Z" fill="#e8c46a"/>
{ojos_monstruo(108, 124, 148, 124, r=9)}''')

# Notificación Defectuosa: sobre con alas de murciélago
ficha_monstruo('notificacion', f'''
<path d="M70 120 C40 96 26 104 14 92 C22 126 30 140 44 146 C36 156 40 170 56 170 C58 158 68 150 74 150 Z" fill="#4a4060" {S4}/>
<path d="M186 120 C216 96 230 104 242 92 C234 126 226 140 212 146 C220 156 216 170 200 170 C198 158 188 150 182 150 Z" fill="#4a4060" {S4}/>
<rect x="66" y="82" width="124" height="96" rx="6" fill="#efe2b8" {S}/>
<path d="M66 88 L128 140 L190 88" fill="none" {S}/>
<circle cx="128" cy="140" r="12" fill="{CARMESI}" {S4}/>
{ojos_monstruo(102, 116, 154, 116, r=10)}
<path d="M110 160 Q128 150 146 160" fill="none" {S4}/>
<text x="128" y="206" font-family="DejaVu Sans" font-weight="bold" font-size="18" text-anchor="middle" fill="{PERG}">¿A quién?</text>''')

# Usuario en cola: personita con ticket numerado
ficha_monstruo('usuario', recorte(persona(piel='#c99068', pelo='#2b201a', estilo='corto', ropa='#5d5a6e', cuello='camisa', boca='seria', ojos='cansados', cejas='cansadas',
               extra=f'<rect x="150" y="190" width="56" height="34" rx="4" fill="#ffffff" {S4}/><text x="178" y="214" font-family="DejaVu Sans" font-weight="bold" font-size="18" text-anchor="middle" fill="{CARMESI}">A-847</text>')))

# La Cola Infinita: serpiente hecha de tickets numerados
cola = []
pts = [(60, 200), (90, 214), (122, 206), (150, 186), (164, 156), (154, 126), (128, 112), (104, 100)]
for i, (x, y) in enumerate(pts):
    cola.append(f'<g transform="rotate({(i*13)%40-20} {x} {y})"><rect x="{x-20}" y="{y-14}" width="40" height="28" rx="4" fill="{"#f4efe4" if i%2 else "#e6dcc6"}" {S4}/>'
                f'<text x="{x}" y="{y+6}" font-family="DejaVu Sans" font-weight="bold" font-size="13" text-anchor="middle" fill="{CARMESI}">{900+i*7}</text></g>')
ficha_monstruo('cola', ''.join(cola) + f'''
<path d="M70 44 C110 20 170 24 196 58 C214 84 200 112 168 116 L92 116 C66 110 52 70 70 44 Z" fill="#8a3a4a" {S}/>
{ojos_monstruo(104, 70, 158, 70, r=14)}
{dientes(102, 92, 66, 6, 14)}
<path d="M178 36 L196 18 L200 44 Z M78 36 L60 18 L56 44 Z" fill="{DORADO}" {S4}/>''')

# Vuelva Mañana: ventanilla cerrada con brazos cruzados
ficha_monstruo('vuelva', f'''
<rect x="58" y="48" width="140" height="160" rx="10" fill="#6d5a7a" {S}/>
<rect x="76" y="70" width="104" height="70" rx="6" fill="#bcd6dd" {S4}/>
<path d="M76 104 h104" {S4}/>
<rect x="84" y="148" width="88" height="30" rx="4" fill="{CARMESI}" {S4}/>
<text x="128" y="169" font-family="DejaVu Sans" font-weight="bold" font-size="14" text-anchor="middle" fill="#fff">CERRADO</text>
{ojos_monstruo(106, 92, 150, 92, r=9)}
<path d="M112 124 L144 124" {S4}/>
<path d="M44 196 C70 180 110 186 128 196 C146 186 186 180 212 196" fill="none" stroke="#6d5a7a" stroke-width="22" stroke-linecap="round"/>
<path d="M44 196 C70 180 110 186 128 196 C146 186 186 180 212 196" fill="none" {S}/>''')

# Fraccionador: caja de compra con grietas
ficha_monstruo('fraccionador', f'''
<path d="M60 92 L128 64 L196 92 L196 188 L128 216 L60 188 Z" fill="#c28a52" {S}/>
<path d="M60 92 L128 120 L196 92 M128 120 L128 216" fill="none" {S}/>
<path d="M150 70 L140 100 L158 118 L146 150 L164 176 L156 206" fill="none" stroke="{TINTA}" stroke-width="5"/>
<path d="M106 150 L124 168" stroke="{TINTA}" stroke-width="5"/>
{ojos_monstruo(88, 132, 112, 140, r=9)}
<path d="M84 172 Q100 162 114 174" fill="none" {S4}/>
<text x="176" y="160" font-family="DejaVu Sans" font-weight="bold" font-size="18" text-anchor="middle" fill="{TINTA}">S/</text>''')

ficha_monstruo('fraccion', f'''
<path d="M84 104 L128 86 L172 104 L172 168 L128 186 L84 168 Z" fill="#d49a62" {S}/>
<path d="M84 104 L128 122 L172 104 M128 122 L128 186" fill="none" {S4}/>
{ojos_monstruo(104, 140, 120, 146, r=7)}
<path d="M150 96 L142 120 L156 140" fill="none" stroke="{TINTA}" stroke-width="4"/>''')

# El Requisito Imposible: rollo de requisitos con corona de sellos
items = ''.join(f'<rect x="84" y="{140+i*22}" width="14" height="14" fill="#fff" stroke="{TINTA}" stroke-width="3"/><path d="M106 {147+i*22} h{60 - (i%3)*12}" stroke="{TINTA}" stroke-width="4"/>' for i in range(3))
ficha_monstruo('requisito', f'''
<path d="M70 76 L186 76 L186 208 C186 220 172 222 168 212 C164 202 150 204 146 214 C142 224 128 224 124 214 C120 204 106 204 102 214 C98 224 84 222 80 210 L70 210 Z" fill="{SILLAR}" {S}/>
<ellipse cx="128" cy="76" rx="64" ry="14" fill="#d8d0bd" {S}/>
{items}
<path d="M88 148 l4 4 8 -10" stroke="{CARMESI}" stroke-width="4" fill="none"/>
<path d="M150 162 l14 14 M164 162 l-14 14" stroke="{CARMESI}" stroke-width="5"/>
{ojos_monstruo(108, 110, 148, 110, r=11)}
<path d="M80 66 L92 38 L110 56 L128 32 L146 56 L164 38 L176 66 Z" fill="{DORADO}" {S4}/>
<circle cx="128" cy="48" r="6" fill="{CARMESI}"/>''')

# ───────────────────────── Dado d20 ─────────────────────────
r = 110
hexa = ' '.join(f'{128 + r*math.cos(math.pi/6 + i*math.pi/3):.1f},{128 + r*math.sin(math.pi/6 + i*math.pi/3):.1f}' for i in range(6))
inner = [(128, 52), (62, 168), (194, 168)]
tri = ' '.join(f'{x},{y}' for x, y in inner)
caras = ''
hx = [(128 + r*math.cos(math.pi/6 + i*math.pi/3), 128 + r*math.sin(math.pi/6 + i*math.pi/3)) for i in range(6)]
for (x, y) in inner:
    for (a, b) in hx:
        if math.hypot(a - x, b - y) < 130:
            caras += f'<path d="M{x} {y} L{a:.1f} {b:.1f}" stroke="{TINTA}" stroke-width="4" opacity="0.6"/>'
guardar('d20', 256, 256, f'''
<polygon points="{hexa}" fill="{PERG}" {S}/>
{caras}
<polygon points="{tri}" fill="#fffaf0" {S}/>
<polygon points="{hexa}" fill="none" {S}/>''')

# Sello circular (para «Fundado» / «Improcedente»): se tiñe en Unity
guardar('sello', 256, 256, f'''
<circle cx="128" cy="128" r="116" fill="none" stroke="#ffffff" stroke-width="12"/>
<circle cx="128" cy="128" r="96" fill="none" stroke="#ffffff" stroke-width="5" stroke-dasharray="10 8"/>
<path d="M40 128 L60 118 L60 138 Z M216 128 L196 118 L196 138 Z" fill="#ffffff"/>''')


# ───────────────────────── Escenas (1024 × 846) ─────────────────────────
W, H = 1024, 846


def baldosas(c1, c2, y0=300, tam=64):
    s = ''
    filas = (H - y0) // tam + 1
    for f in range(filas):
        for c in range(W // tam + 1):
            col = c1 if (f + c) % 2 == 0 else c2
            s += f'<rect x="{c*tam}" y="{y0 + f*tam}" width="{tam}" height="{tam}" fill="{col}"/>'
    return s


def pared_sillar(alto=300, color='#e9e2d2', junta='#cfc5b0'):
    s = f'<rect width="{W}" height="{alto}" fill="{color}"/>'
    for f in range(alto // 40 + 1):
        off = 0 if f % 2 == 0 else 60
        for c in range(-1, W // 120 + 2):
            s += f'<rect x="{c*120+off}" y="{f*40}" width="120" height="40" fill="none" stroke="{junta}" stroke-width="3"/>'
    return s


def arco(x, y, w, h, relleno='#2b2540'):
    return (f'<path d="M{x} {y+h} L{x} {y+w/2} A{w/2} {w/2} 0 0 1 {x+w} {y+w/2} L{x+w} {y+h} Z" fill="{relleno}" {S}/>'
            f'<path d="M{x-14} {y+h} L{x-14} {y+w/2} A{w/2+14} {w/2+14} 0 0 1 {x+w+14} {y+w/2} L{x+w+14} {y+h}" fill="none" stroke="#cfc5b0" stroke-width="10"/>')


def ventanilla(x, y, n, cerrada=False):
    letrero = f'<rect x="{x+20}" y="{y+8}" width="120" height="30" rx="4" fill="{TINTA2}"/><text x="{x+80}" y="{y+30}" font-family="DejaVu Sans" font-weight="bold" font-size="18" text-anchor="middle" fill="{DORADO}">VENTANILLA {n}</text>'
    vidrio = '#9fb9c4' if not cerrada else '#6b6f7a'
    s = f'<rect x="{x}" y="{y}" width="160" height="200" fill="#7a5a3e" {S}/>' + letrero
    s += f'<rect x="{x+16}" y="{y+50}" width="128" height="96" fill="{vidrio}" {S4}/><path d="M{x+16} {y+120} h128" stroke="{TINTA}" stroke-width="3"/>'
    s += f'<rect x="{x-6}" y="{y+150}" width="172" height="20" fill="#5a4030" {S4}/>'
    if cerrada:
        s += f'<rect x="{x+30}" y="{y+80}" width="100" height="30" fill="{CARMESI}" {S4}/><text x="{x+80}" y="{y+101}" font-family="DejaVu Sans" font-weight="bold" font-size="16" text-anchor="middle" fill="#fff">CERRADO</text>'
    return s


def sombra_inferior():
    return ('<defs><linearGradient id="v" x1="0" y1="0" x2="0" y2="1"><stop offset="0.55" stop-color="#000" stop-opacity="0"/>'
            '<stop offset="1" stop-color="#000" stop-opacity="0.45"/></linearGradient></defs>'
            f'<rect width="{W}" height="{H}" fill="url(#v)"/>')


# Combate en Mesa de Partes: pared de sillar con ventanillas, piso de baldosas
guardar('fondo-mesa', W, H, pared_sillar() + ventanilla(70, 70, 1) + ventanilla(300, 70, 2, True) + ventanilla(530, 70, 3) + ventanilla(760, 70, 4, True)
        + '<rect x="0" y="290" width="1024" height="14" fill="#8c7a5e"/>' + baldosas('#4a4262', '#3e3754')
        + ''.join(f'<rect x="{60+i*90}" y="{340 + (i%3)*14}" width="34" height="46" rx="6" fill="#ffffff" opacity="0.06"/>' for i in range(0))
        + sombra_inferior())

# Combate en la Sala de Contrataciones: estantes de bases y sobres
estantes = ''
for i in range(5):
    x = 40 + i * 200
    estantes += f'<rect x="{x}" y="40" width="160" height="240" fill="#6b4f3a" {S}/>'
    for f in range(3):
        estantes += f'<rect x="{x}" y="{110+f*70}" width="160" height="10" fill="#4d3829"/>'
        for k in range(5):
            alto = 40 + (k * 7 + i * 3 + f) % 18
            col = ['#d8cfb8', '#c9b98f', '#e6dcc6', '#b8a87e'][(k + f + i) % 4]
            estantes += f'<rect x="{x+10+k*29}" y="{110+f*70-alto}" width="24" height="{alto}" fill="{col}" stroke="{TINTA}" stroke-width="2"/>'
guardar('fondo-contrataciones', W, H, '<rect width="1024" height="300" fill="#d6cfe0"/>' + estantes
        + '<rect x="0" y="290" width="1024" height="14" fill="#5a5070"/>' + baldosas('#3d4a5e', '#33405a') + sombra_inferior())

# Trámite: la puerta del «requisito correcto»
guardar('escena-tramite', W, H, pared_sillar(H) + arco(362, 150, 300, 696, '#3a2f4f')
        + f'<rect x="392" y="320" width="240" height="526" fill="#6b4a32" {S}/><circle cx="600" cy="600" r="12" fill="{DORADO}" {S4}/>'
        + ''.join(f'<g transform="rotate({a} {x} {y})"><rect x="{x-60}" y="{y-24}" width="120" height="48" rx="6" fill="none" stroke="{c}" stroke-width="6"/><text x="{x}" y="{y+9}" font-family="DejaVu Sans" font-weight="bold" font-size="22" text-anchor="middle" fill="{c}">{t}</text></g>'
                  for a, x, y, c, t in [(-12, 470, 420, CARMESI, 'OBSERVADO'), (8, 560, 520, '#2f6b2f', 'RECIBIDO'), (-4, 500, 640, CARMESI, 'FALTA FIRMA'), (14, 560, 760, '#2b2540', 'COPIA')])
        + f'<rect x="150" y="420" width="150" height="210" fill="#fffaf0" {S4}/><text x="225" y="460" font-family="DejaVu Sans" font-weight="bold" font-size="20" text-anchor="middle" fill="{TINTA}">REQUISITOS</text>'
        + ''.join(f'<path d="M170 {490+i*30} h110" stroke="{TINTA}" stroke-width="3"/>' for i in range(4))
        + sombra_inferior())

# Audiencia: estrado de madera, sala con columnas
guardar('escena-audiencia', W, H, '<rect width="1024" height="846" fill="#5a3e2e"/>'
        + ''.join(f'<rect x="{60+i*240}" y="0" width="70" height="560" fill="#e9e2d2" {S}/><rect x="{48+i*240}" y="540" width="94" height="30" fill="#cfc5b0" {S4}/>' for i in range(5))
        + f'<rect x="212" y="300" width="600" height="230" fill="#7a4e30" {S}/><rect x="190" y="280" width="644" height="34" fill="#8e5c38" {S}/>'
        + f'<circle cx="512" cy="200" r="70" fill="{DORADO}" {S}/><path d="M512 150 L512 250 M462 190 L562 190" stroke="{TINTA}" stroke-width="8"/>'
        + f'<path d="M472 190 L452 230 L492 230 Z M552 190 L532 230 L572 230 Z" fill="none" {S4}/>'
        + baldosas('#4a3326', '#55392a', y0=570) + f'<rect x="80" y="660" width="300" height="150" fill="#6b4a32" {S}/><rect x="644" y="660" width="300" height="150" fill="#6b4a32" {S}/>'
        + sombra_inferior())

# Cafetería del juzgado
taza = lambda x, y: (f'<path d="M{x} {y} h70 v50 a35 30 0 0 1 -70 0 Z" fill="#fffaf0" {S}/><path d="M{x+70} {y+12} a18 18 0 0 1 0 34" fill="none" {S}/>'
                     f'<path d="M{x+20} {y-14} q10 -20 0 -40 M{x+44} {y-14} q10 -20 0 -40" fill="none" stroke="#ffffff" stroke-width="6" opacity="0.7"/>')
guardar('escena-cafeteria', W, H, pared_sillar(H, '#e8d6b8', '#cdb894')
        + f'<rect x="80" y="70" width="380" height="240" fill="{TINTA2}" {S}/><text x="270" y="120" font-family="DejaVu Sans" font-weight="bold" font-size="34" text-anchor="middle" fill="{DORADO}">MENÚ</text>'
        + ''.join(f'<text x="110" y="{170+i*40}" font-family="DejaVu Sans" font-size="24" fill="{PERG}">{t}</text>' for i, t in enumerate(['Café pasado ....... S/ 3', 'Pan con chicharrón  S/ 6', 'Emoliente ......... S/ 2']))
        + f'<rect x="0" y="540" width="1024" height="306" fill="#8e5c38" {S}/><rect x="0" y="520" width="1024" height="40" fill="#a86e44" {S}/>'
        + taza(300, 440) + taza(620, 440)
        + f'<circle cx="830" cy="190" r="80" fill="#fffaf0" {S}/><path d="M830 190 L830 136 M830 190 L866 210" fill="none" stroke="{TINTA}" stroke-width="8"/>'
        + sombra_inferior())

# Librería jurídica
libros = ''
for f in range(4):
    for k in range(14):
        x = 70 + k * 64
        alto = 120 + (k * 13 + f * 7) % 40
        col = ['#8a2f2f', '#2f4f8a', '#2f6b4a', '#6b4a2f', '#4a2f6b'][(k + f) % 5]
        libros += f'<rect x="{x}" y="{60+f*190+(160-alto)}" width="52" height="{alto}" fill="{col}" stroke="{TINTA}" stroke-width="3"/><rect x="{x+8}" y="{60+f*190+(160-alto)+20}" width="36" height="8" fill="{DORADO}"/>'
guardar('escena-libreria', W, H, '<rect width="1024" height="846" fill="#4d3829"/>' + libros
        + ''.join(f'<rect x="40" y="{220+f*190}" width="944" height="16" fill="#2e2018"/>' for f in range(4))
        + sombra_inferior())

# Pasillo (el Coimero): pasillo en penumbra con una lámpara
guardar('escena-pasillo', W, H, '<rect width="1024" height="846" fill="#1f1a2c"/>'
        + f'<path d="M0 0 L380 260 L380 600 L0 846 Z" fill="#2b2540"/><path d="M1024 0 L644 260 L644 600 L1024 846 Z" fill="#2b2540"/>'
        + f'<rect x="380" y="260" width="264" height="340" fill="#15111f"/><path d="M380 600 L644 600 L1024 846 L0 846 Z" fill="#3a3355"/>'
        + f'<path d="M512 0 L512 120" stroke="{TINTA}" stroke-width="6"/><path d="M470 120 L554 120 L530 160 L494 160 Z" fill="{DORADO}" {S4}/>'
        + '<defs><radialGradient id="luz"><stop offset="0" stop-color="#ffd27a" stop-opacity="0.45"/><stop offset="1" stop-color="#ffd27a" stop-opacity="0"/></radialGradient></defs>'
        + '<ellipse cx="512" cy="330" rx="360" ry="260" fill="url(#luz)"/>'
        + ''.join(f'<rect x="{x}" y="{y}" width="60" height="140" fill="#3d3550" {S4}/>' for x, y in [(120, 300), (844, 300)])
        + sombra_inferior())


# ───────────────────────── Portada (1024 × 1024) ─────────────────────────
import random
_r = random.Random(7)
estrellas = ''.join(f'<circle cx="{_r.randint(0,1024)}" cy="{_r.randint(0,460)}" r="{_r.choice([1,1.5,2,3])}" fill="#fff" opacity="{_r.uniform(0.3,0.9):.2f}"/>' for i in range(90))
torre = ''
for i in range(9):
    w = 230 - i * 10
    x = 640 - w / 2 + (i % 2) * 8 - 4
    y = 860 - i * 52
    torre += f'<rect x="{x}" y="{y}" width="{w}" height="50" fill="{["#efe6d2","#e3d6ba","#f4ecdc"][i%3]}" {S}/>'
    torre += f'<path d="M{x} {y+25} h{w}" stroke="{CARMESI}" stroke-width="6"/>'
    torre += f'<path d="M{x+8} {y+10} h{w-16}" stroke="{TINTA}" stroke-width="2" opacity="0.4"/>'
dragon = f'''
<g transform="translate(150 230) scale(1.5)">
<path d="M40 40 C10 10 -30 30 -40 -10 C0 0 20 -20 60 0 Z" fill="#5a2a3a" {S}/>
<path d="M120 40 C140 -40 220 -60 260 -40 C220 -20 200 10 190 40 Z" fill="#7a3248" {S}/>
<path d="M80 60 C40 40 30 -10 70 -30 C80 0 100 10 120 20 Z" fill="#7a3248" {S}/>
<path d="M30 80 C60 40 140 30 190 50 C220 62 230 90 210 100 C170 110 110 104 60 110 C20 112 0 100 30 80 Z" fill="#8a3a4a" {S}/>
<path d="M200 60 C230 40 262 44 276 60 C266 74 246 80 222 80 Z" fill="#8a3a4a" {S}/>
<circle cx="252" cy="58" r="5" fill="{DORADO}"/>
<path d="M276 64 L296 60 L280 72" fill="#f08a2c" {S4}/>
<path d="M30 92 C0 100 -30 130 -70 120" fill="none" stroke="#8a3a4a" stroke-width="16" stroke-linecap="round"/>
<path d="M30 92 C0 100 -30 130 -70 120" fill="none" stroke="{TINTA}" stroke-width="4"/>
<rect x="110" y="96" width="40" height="28" fill="#fffaf0" {S4} transform="rotate(10 130 110)"/>
</g>'''
guardar('portada', 1024, 1024,
        '<defs><linearGradient id="cielo" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#0E0C1A"/><stop offset="0.7" stop-color="#2b2540"/><stop offset="1" stop-color="#4a3d6a"/></linearGradient></defs>'
        + '<rect width="1024" height="1024" fill="url(#cielo)"/>' + estrellas
        + f'<circle cx="760" cy="200" r="120" fill="#f4ecd0"/><circle cx="730" cy="180" r="22" fill="#e6dcb8"/><circle cx="800" cy="240" r="14" fill="#e6dcb8"/>'
        # volcán nevado (evoca el paisaje arequipeño, sin copiar una imagen)
        + f'<path d="M-40 760 L330 380 C350 362 370 362 392 380 L780 760 Z" fill="#5d5476" {S}/>'
        + f'<path d="M250 462 L330 380 C350 362 370 362 392 380 L470 462 C440 450 420 476 400 458 C380 440 360 470 340 452 C320 436 290 470 250 462 Z" fill="#f4f1ea" {S4}/>'
        + f'<path d="M560 760 L760 560 C772 548 786 548 798 560 L1080 760 Z" fill="#4a4262" {S}/>'
        + dragon
        # juzgado de sillar con arcos
        + f'<rect x="0" y="760" width="1024" height="264" fill="#e9e2d2" {S}/>'
        + ''.join(arco(30 + i * 170, 820, 110, 204, '#2b2540') for i in range(6))
        + '<rect x="0" y="740" width="1024" height="30" fill="#d8cfb8" stroke="#1b1626" stroke-width="6"/>'
        + torre
        + f'<path d="M598 910 L682 910 L682 834 A42 42 0 0 0 598 834 Z" fill="{TINTA2}" {S}/><circle cx="668" cy="876" r="6" fill="{DORADO}"/><path d="M598 860 h84 M598 885 h84" stroke="{TINTA}" stroke-width="4"/>')

print('SVG generados:', len(os.listdir(OUT)))
