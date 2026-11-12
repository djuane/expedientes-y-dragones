# Arma el manual de Expedientes y Dragones como HTML paginado (A4); pdf.js lo convierte a PDF.
# Autor: Leonel Díaz.
import os, shutil

AQUI = os.path.dirname(os.path.abspath(__file__))
SAL = os.path.join(AQUI, 'salida')
os.makedirs(SAL, exist_ok=True)

paginas = []


def pagina(seccion, html, clase=''):
    paginas.append((seccion, html, clase))


def img(nombre, clase='', alt=''):
    ext = 'png' if nombre in ('marco', 'd20') or nombre.startswith('ia-') else 'jpg'
    return f'<img class="{clase}" src="img/{nombre}.{ext}" alt="{alt}">'


def caja(titulo, cuerpo, tipo='nota'):
    return f'<div class="caja {tipo}"><div class="caja-t">{titulo}</div>{cuerpo}</div>'


def tabla(cab, filas, clase=''):
    h = ''.join(f'<th>{c}</th>' for c in cab)
    b = ''.join('<tr>' + ''.join(f'<td>{c}</td>' for c in f) + '</tr>' for f in filas)
    return f'<table class="{clase}"><thead><tr>{h}</tr></thead><tbody>{b}</tbody></table>'


def amarillo(t):
    return f'<mark>{t}</mark>'


ATR = ['Firmeza', 'Agilidad procesal', 'Aguante', 'Doctrina', 'Criterio', 'Oratoria']
ABR = ['FIR', 'AGI', 'AGU', 'DOC', 'CRI', 'ORA']


def mod(v):
    m = (v - 10) // 2
    return f'+{m}' if m >= 0 else f'−{-m}'


def bloque(nombre, tipo, valores, pac, defe, mov, ataque, rasgo, tactica, imagen=None, nivel=1):
    celdas = ''.join(f'<div><b>{a}</b><span>{v} ({mod(v)})</span></div>' for a, v in zip(ABR, valores))
    im = img(imagen, 'bl-img') if imagen else ''
    return (f'<div class="bloque">{im}<div class="bl-nom">{nombre}</div><div class="bl-tipo">{tipo}</div>'
            f'<div class="bl-lin"><b>Paciencia</b> {pac} · <b>Defensa</b> {defe} · <b>Movimiento</b> {mov} casillas · <b>Nivel</b> {nivel}</div>'
            f'<div class="bl-atr">{celdas}</div>'
            f'<div class="bl-lin"><b>Ataque.</b> {ataque}</div>'
            f'<div class="bl-lin"><b>Rasgo.</b> {rasgo}</div>'
            f'<div class="bl-lin"><b>Táctica.</b> {tactica}</div></div>')


# ═════════════════════════ 1. PORTADA ═════════════════════════
pagina('', f'''
{img('ia-portada', 'portada-img') if os.path.exists(os.path.join(AQUI, 'img', 'ia-portada.png')) else img('portada', 'portada-img')}
<div class="portada-tit">
  <div class="sobre">Un juego de rol jurídico</div>
  {img('ia-logo', 'logo-portada', 'Expedientes y Dragones')}
  <div class="lema">Litiga, investiga y vence al dragón de la burocracia</div>
</div>
<div class="portada-pie">Manual del juego · Reglas, especialidades, bestiario y aventura<br><b>Leonel Díaz</b></div>
''', 'portada')

# ═════════════════════════ 2. ÍNDICE ═════════════════════════
indice = [
    ('Expedientes y Dragones', 2), ('Qué es esto', 2), ('Licencia', 2),
    ('Bienvenidos a Expedientia', 3), ('Crear un abogado', 5), ('Características', 5), ('Habilidades de oficio', 6),
    ('Especialidades', 7), ('Sistema de juego', 10), ('La tirada', 10), ('La prueba jurídica', 11), ('La acción', 12),
    ('Poderes de las especialidades', 13), ('Progresión', 14), ('Recompensas', 15),
    ('Montando la partida', 16), ('Tablas aleatorias', 18), ('Personajes no jugadores', 20), ('Los socios del estudio', 21), ('Bestiario', 22),
    ('Las zonas de Expedientia', 27), ('Aventura: El primer caso', 30), ('Reglas rápidas', 32), ('Plantillas', 33),
]
filas_ind = ''.join(f'<div class="ind"><span>{t}</span><i></i><b>{p}</b></div>' for t, p in indice)
pagina('Expedientes y Dragones', f'''
<div class="dos">
<h1>Expedientes y Dragones</h1>
<h3>Índice</h3>
<div class="indice">{filas_ind}</div>
<h3>Licencia</h3>
<p class="chico">Texto, reglas adaptadas e ilustraciones de este manual: © 2026 Leonel Díaz. Todos los personajes, lugares e instituciones de la República de Expedientia son ficticios; cualquier parecido con una ventanilla real es pura coincidencia y algo de experiencia.</p>
<p class="chico">El sistema de tiradas se basa en el <i>System Reference Document 5.2</i>, publicado bajo licencia Creative Commons Atribución 4.0. Atribución exigida por la licencia:</p>
<p class="chico atrib">This work includes material from the System Reference Document 5.2 ("SRD 5.2") by Wizards of the Coast LLC, available at https://www.dndbeyond.com/srd. The SRD 5.2 is licensed under the Creative Commons Attribution 4.0 International License, available at https://creativecommons.org/licenses/by/4.0/legalcode.</p>
<p class="chico">Este juego no usa marcas, logotipos, personajes ni ambientaciones de ningún otro juego de rol.</p>
<div class="salto"></div>
<h2>Qué es esto</h2>
<p class="capital">Expedientes y Dragones es un juego de rol cooperativo en el que cada jugador interpreta a un abogado o abogada recién salido de la facultad que abre su estudio frente a la Corte, en un país inventado con instituciones sospechosamente familiares: la República de Expedientia.</p>
<p>Ahí, una noche, los expedientes olvidados del Archivo Central empezaron a moverse. Los formularios desarrollaron colmillos, los plazos aprendieron a explotar y las colas se volvieron, literalmente, infinitas. Detrás de todo crece una criatura que nadie ha visto completa: <b>la Carga Procesal</b>, un dragón hecho de trámites que nadie atendió.</p>
<p>Los jugadores no blanden espadas: presentan escritos, piden medidas cautelares, plantean nulidades y, cuando todo falla, se toman un café en la cafetería del juzgado. Ganan expedientes, reputación y honorarios, y deben decidir qué hacer cuando un señor muy amable ofrece «agilizar» el trámite.</p>
<p>Este manual sirve para dos cosas: jugar en mesa con lápiz, papel y dados, y entender las reglas que usa la aplicación para celular del mismo nombre. Las dos versiones comparten el corazón del sistema: un dado de veinte caras, seis características y la prueba jurídica.</p>
{caja('Lo que necesitas', '<p>Uno a cuatro jugadores y una persona que dirija la partida (el <b>Relator</b>). Un dado de veinte caras (d20) y, si es posible, dados de 4, 6, 8 y 100 caras. Copias de la hoja de abogado (página 33), lápices y una tarde libre de audiencias.</p>')}
{caja('Advertencia del estudio', '<p>Es un juego de humor de oficio. No es asesoría legal ni reemplaza la consulta con un abogado colegiado. Las preguntas jurídicas que aparecen en la aplicación se verifican en fuentes oficiales peruanas antes de publicarse.</p>', 'aviso')}
</div>''')

# ═════════════════════════ 3-4. MUNDO ═════════════════════════
pagina('Expedientes y Dragones', f'''
<h1>Bienvenidos a Expedientia</h1>
<div class="dos">
<p class="capital">La República de Expedientia es un país de volcanes nevados, ciudades de piedra blanca y ventanillas. Muchas ventanillas. Su capital judicial se levanta a los pies de un volcán dormido, con arcos de sillar que brillan al atardecer y un río que todos llaman el Trámite porque nunca se sabe cuándo va a llegar.</p>
<p>En el centro de la ciudad está el <b>Palacio de Justicia</b>, de donde parten las calles hacia la Mesa de Partes, la Sala de Contrataciones y el temido Archivo Central. Alrededor crecen los negocios de siempre: la cafetería del juzgado, donde se resuelven más casos que en las salas; la librería jurídica, donde todo cuesta un poco más; y el Pasaje del Coimero, donde nada es lo que parece.</p>
<h3>El estudio de los jugadores</h3>
<p>Los personajes acaban de alquilar un cuarto frente a la Corte con <b>Gonzabio</b>, el socio fundador, que arma los expedientes y los defiende como un escudo, y con <b>Lion</b>, el investigador que lee hasta los anexos que nadie abre. Tiene un escritorio cojo, una impresora que funciona los martes y una placa en la puerta con el apellido mal escrito. Ahí llegan los clientes, ahí se guardan los honorarios y ahí se decide qué caso aceptar.</p>
<h3>La Carga Procesal</h3>
<p>Nadie sabe cuándo nació. Don Eusebio, que lleva treinta años en el juzgado, jura que la vio por primera vez «un martes». Se alimenta de cada escrito sin proveer, de cada notificación que no llegó y de cada «vuelva mañana». Cuanto más crece, más criaturas escapan del Archivo Central. Derrotarla es la meta de toda la campaña; contenerla, el trabajo de cada día.</p>
{caja('Tono del juego', '<p>Expedientia se parece a la vida real del litigante, pero exagerada hasta lo fantástico. Las criaturas representan problemas del sistema, nunca a personas concretas: no hay jueces, servidores ni colegas reales retratados. El humor apunta a los trámites, no a la gente que los atiende.</p>')}
</div>
{img('mapa', 'mapa', 'Mapa de Expedientia')}
<p class="pie-img">Mapa de la República de Expedientia, tal como lo dibujó un practicante en la parte de atrás de una cédula de notificación.</p>
''')

pagina('Expedientes y Dragones', f'''
{img('esc-dragon', 'ancho', 'La Carga Procesal sobre la ciudad')}
<p class="pie-img">La Carga Procesal sobrevuela la ciudad. Sus escamas son carpetas; su aliento, sellos de «NO HA LUGAR».</p>
<div class="dos">
<h2>Cómo se juega</h2>
<p class="capital">Una partida de Expedientes y Dragones es un <b>expediente</b>: un caso que llega al estudio y que el equipo debe llevar de la primera ventanilla hasta la resolución final. Cada expediente se divide en <b>salas</b>, y en cada sala sucede algo: un combate contra criaturas del sistema, un trámite que hay que superar, una audiencia con un cliente que cambia su versión, una pausa en la cafetería o una tentación.</p>
<p>El Relator describe la sala; los jugadores dicen qué hacen sus abogados; los dados deciden si lo logran. Al final del expediente espera un <b>jefe</b>: una criatura mayor que custodia la resolución. Si el equipo vence, el caso se declara <b>fundado</b>; si cae, el expediente se <b>archiva</b>… y en Expedientia lo archivado también resucita.</p>
<p>Una partida en la aplicación dura entre 15 y 20 minutos. En mesa, un expediente completo ocupa una sesión de dos o tres horas, y una campaña encadena varios expedientes hasta enfrentar a la Carga Procesal.</p>
<h3>Los tres pilares</h3>
<p><b>Litigar.</b> El combate táctico en una cuadrícula, con poderes inspirados en figuras procesales.</p>
<p><b>Investigar.</b> Revisar expedientes, encontrar la fecha que salva el plazo, descubrir qué requisito pide la criatura.</p>
<p><b>Decidir.</b> Aceptar o rechazar atajos, proteger la reputación del estudio, elegir qué casos tomar.</p>
{caja('Glosario rápido', '<p><b>Relator:</b> quien dirige la partida. <b>Paciencia:</b> los puntos de vida del abogado. <b>Fundado:</b> sacar 20 natural. <b>Improcedente:</b> sacar 1 natural. <b>Sala:</b> cada escena del expediente. <b>Honorarios:</b> la moneda del juego. <b>Reputación:</b> lo que el gremio piensa del estudio, de 0 a 100.</p>')}
</div>''')

# ═════════════════════════ 5-6. CREAR UN ABOGADO ═════════════════════════
tabla_mod = tabla(['Valor', '1', '2–3', '4–5', '6–7', '8–9', '10–11', '12–13', '14–15', '16–17', '18–19', '20'],
                  [['Modificador', '−5', '−4', '−3', '−2', '−1', '+0', '+1', '+2', '+3', '+4', '+5']], 'mini')
pagina('Crear un abogado', f'''
<h1>Crear un abogado</h1>
<div class="dos">
<p class="capital">Cada jugador interpreta a un abogado o abogada del estudio. No hace falta saber derecho para jugar: el personaje sabe lo que el jugador no sabe, y los dados lo representan. Crear un personaje toma unos diez minutos.</p>
<ol class="pasos">
<li><b>Elige una especialidad.</b> Civilista, Administrativista, Penalista, Laboralista, Constitucionalista o Tributarista (páginas 7 a 9).</li>
<li><b>Reparte las características.</b> Asigna los valores 16, 14, 13, 12, 10 y 10 a las seis características, o usa los valores sugeridos de tu especialidad.</li>
<li><b>Anota tu Paciencia y tu Defensa.</b> Vienen indicadas en la especialidad.</li>
<li><b>Elige cuatro habilidades de oficio</b> en las que tu abogado es competente (página 6).</li>
<li><b>Anota tus poderes</b> de especialidad y el poder compartido Casación.</li>
<li><b>Dale vida.</b> Nombre, universidad, por qué estudió derecho y una manía: siempre lleva copia de todo, no confía en los ascensores del Palacio, colecciona sellos de recepción…</li>
</ol>
<h2>Características</h2>
<p>Seis números del 1 al 20 describen al abogado. Cada uno tiene un <b>modificador</b> que se suma a las tiradas: es el valor menos 10, dividido entre 2 y redondeado hacia abajo.</p>
{tabla_mod}
<p><b>Firmeza.</b> Fuerza física y temple: cargar expedientes de mil folios, sostener una puerta que se cierra, no retroceder ante un «Vuelva Mañana».</p>
<p><b>Agilidad procesal.</b> Rapidez de manos y de pies: llegar antes del cierre de ventanilla, esquivar una notificación defectuosa, actuar primero.</p>
<p><b>Aguante.</b> Resistencia: soportar la cola, la audiencia de seis horas y el tercer café. Se suma a la Paciencia en las reglas avanzadas.</p>
<p><b>Doctrina.</b> Conocimiento jurídico y memoria: citar la norma, recordar el criterio del tribunal, revisar el expediente sin perderse.</p>
<p><b>Criterio.</b> Intuición y buen juicio: leer al cliente, detectar el requisito correcto, darse cuenta de que el plazo vence hoy.</p>
<p><b>Oratoria.</b> Presencia y palabra: alegar, negociar, convencer a un cliente de que diga la verdad.</p>
<h3>Paciencia y Defensa</h3>
<p>La <b>Paciencia</b> son los puntos de vida del abogado. Los ataques de las criaturas la reducen; cuando llega a 0, el abogado <b>cae</b>: se va a su casa a dormir y no participa en el resto del expediente. La <b>Defensa</b> es el número que una criatura debe igualar o superar con su tirada para afectarlo.</p>
{caja('Consejo para el Relator', '<p>Si un jugador nuevo no sabe qué elegir, ofrécele Civilista o Administrativista: son las especialidades del prototipo de la aplicación y las más probadas.</p>')}
</div>''')

habilidades = [
    ('Cargar expedientes', 'Firmeza', 'Mover, sostener o arrastrar algo pesado.'),
    ('Aguantar la fila', 'Aguante', 'Resistir el cansancio, el hambre y la espera.'),
    ('Llegar a tiempo', 'Agilidad procesal', 'Correr, esquivar y moverse entre la multitud.'),
    ('Pasar desapercibido', 'Agilidad procesal', 'Moverse sin llamar la atención por los pasillos.'),
    ('Presentar al vuelo', 'Agilidad procesal', 'Deslizar un escrito por la ventanilla justo antes del cierre.'),
    ('Normativa', 'Doctrina', 'Recordar leyes, reglamentos y procedimientos.'),
    ('Jurisprudencia', 'Doctrina', 'Recordar cómo se resolvieron casos parecidos.'),
    ('Revisar el expediente', 'Doctrina', 'Encontrar la fecha, el folio o el error escondido.'),
    ('Cuentas y tasas', 'Doctrina', 'Calcular plazos, montos, aranceles e intereses.'),
    ('Leer al cliente', 'Criterio', 'Saber si alguien miente, oculta algo o tiene miedo.'),
    ('Atención a plazos', 'Criterio', 'Notar lo que está por vencer o lo que cambió.'),
    ('Orientarse en la burocracia', 'Criterio', 'Encontrar la oficina correcta a la primera.'),
    ('Primeros auxilios del ánimo', 'Criterio', 'Calmar a un cliente o a un colega desbordado.'),
    ('Trato con mascotas del cliente', 'Criterio', 'Ganarse al perro que custodia los documentos.'),
    ('Alegar', 'Oratoria', 'Exponer con claridad y fuerza ante cualquiera.'),
    ('Negociar', 'Oratoria', 'Llegar a un acuerdo que convenga al cliente.'),
    ('Imponerse', 'Oratoria', 'Exigir con firmeza, sin perder las formas.'),
    ('Farolear', 'Oratoria', 'Aparentar más de lo que se sabe. Mal visto: si sale mal, baja la reputación.'),
]
pagina('Crear un abogado', f'''
<h1>Habilidades de oficio</h1>
{tabla(['Habilidad', 'Característica', 'Para qué sirve'], [list(h) for h in habilidades], 'hab')}
<div class="dos">
<p class="capital">Las habilidades representan lo que el abogado aprendió fuera del aula: en la cola, en la ventanilla y en el pasillo. Cada habilidad depende de una característica. Si tu personaje es <b>competente</b> en una habilidad, suma su <b>bonificador de competencia</b> (+2 en los primeros niveles) además del modificador de la característica.</p>
<p>Al crear el personaje elige cuatro habilidades. Las especialidades sugieren algunas, pero ninguna está prohibida: un Tributarista que sabe alegar es perfectamente posible, aunque raro.</p>
<h3>Pruebas sin habilidad</h3>
<p>Cuando nadie es competente, se tira igual: d20 más el modificador de la característica. Un abogado sin experiencia también puede tener suerte en Mesa de Partes; lo raro es que la tenga dos veces seguidas.</p>
{caja('Ejemplo', '<p>Lucía, Civilista, quiere encontrar en un expediente de 800 folios la fecha en que se notificó la demanda. El Relator pide una prueba de <i>Revisar el expediente</i> (Doctrina) de dificultad 15. Lucía tiene Doctrina 16 (+3) y es competente (+2): tira el d20, saca 11 y suma 5. Total 16: encuentra la fecha en el folio 612, escondida entre dos copias del DNI del abuelo.</p>', 'ejemplo')}
</div>''')

# ═════════════════════════ 7-9. ESPECIALIDADES ═════════════════════════
ESP = [
    dict(n='Civilista', img='esp-civilista', lema='La memoria del código y la paciencia de un archivero.',
         t='Domina contratos, propiedad, familia y sucesiones. Es quien recuerda la norma exacta en el momento justo y quien sabe que una buena medida cautelar vale más que mil alegatos. En el combate prefiere controlar el campo: inmoviliza al enemigo más peligroso y anula sus jugadas.',
         atr=[10, 12, 14, 16, 13, 10], pac='12 (+6 por nivel)', defe=13, mov=3, ataque='Doctrina, 1d8 de daño, alcance 2 casillas.',
         hab='Normativa, Revisar el expediente, Jurisprudencia, Leer al cliente.',
         pod='<b>Medida cautelar</b> e <b>Nulidad</b> (página 13).', pers='Metódica, desconfía de las fotocopias y lleva siempre un código con cuarenta separadores de colores.', base=True),
    dict(n='Administrativista', img='esp-administrativista', lema='Conoce cada ventanilla y a cada sello por su nombre.',
         t='Se mueve en el laberinto de la administración pública como en su casa. Sabe qué formulario pide cada oficina y qué hacer cuando una entidad no responde. En el combate apoya al equipo: deja repetir tiradas, paraliza enemigos con una queja bien puesta y gana salas por silencio.',
         atr=[10, 13, 13, 12, 16, 12], pac='11 (+6 por nivel)', defe=12, mov=3, ataque='Criterio, 1d6 de daño, alcance 3 casillas.',
         hab='Orientarse en la burocracia, Atención a plazos, Normativa, Negociar.',
         pod='<b>Recurso de apelación</b>, <b>Queja</b> y la pasiva <b>Silencio administrativo</b>.', pers='Optimista incorregible; colecciona cargos de recepción como otros coleccionan estampillas.', base=True),
    dict(n='Penalista', img='esp-penalista', lema='Llega primero a la escena y nunca suelta la lupa.',
         t='Investigadora nata, trabaja con pruebas, testigos y tiempos cortos. Es la más rápida del estudio y la que mejor detecta lo que no encaja. En el combate golpea antes que nadie y rompe las defensas formales de las criaturas.',
         atr=[12, 16, 12, 12, 14, 10], pac='11 (+6 por nivel)', defe=13, mov=4, ataque='Agilidad procesal, 1d8 de daño, alcance 1 casilla.',
         hab='Revisar el expediente, Leer al cliente, Llegar a tiempo, Pasar desapercibido.',
         pod='<b>Flagrancia</b> y <b>Allanamiento</b>.', pers='Habla poco, anota todo y jamás se sienta de espaldas a la puerta.'),
    dict(n='Laboralista', img='esp-laboralista', lema='Donde hay un equipo cansado, él enciende el farol.',
         t='Defiende a trabajadores y empresas en conflictos de planilla, despidos y beneficios. Conoce el valor de un descanso a tiempo. En el combate sostiene al grupo: recupera la Paciencia de sus aliados y revela las debilidades de las criaturas.',
         atr=[13, 10, 16, 12, 13, 12], pac='14 (+6 por nivel)', defe=13, mov=3, ataque='Firmeza, 1d6 de daño, alcance 1 casilla.',
         hab='Aguantar la fila, Primeros auxilios del ánimo, Negociar, Cuentas y tasas.',
         pod='<b>Beneficio social</b> e <b>Inspección</b>.', pers='Saluda a todos los vigilantes por su nombre y siempre trae pan para compartir.'),
    dict(n='Constitucionalista', img='esp-constitucionalista', lema='Cuando todo falla, ella invoca los derechos.',
         t='Veterana de mil procesos, conoce los derechos fundamentales y las garantías que los protegen. Su presencia tranquiliza al equipo. En el combate protege: libera a los aliados inmovilizados y desvía el daño que reciben.',
         atr=[8, 10, 12, 15, 14, 16], pac='11 (+6 por nivel)', defe=12, mov=3, ataque='Oratoria, 1d6 de daño, alcance 3 casillas.',
         hab='Alegar, Jurisprudencia, Imponerse, Leer al cliente.',
         pod='<b>Amparo</b> y <b>Hábeas corpus</b>.', pers='Cita de memoria, lleva una manta tejida por su abuela y no ha perdido la calma desde 1993.'),
    dict(n='Tributarista', img='esp-tributarista', lema='Cada sol cuenta; cada error de cálculo, también.',
         t='Experto en impuestos, aranceles y cuentas públicas. Encuentra el error en la liquidación que nadie vio. En el combate debilita: rebaja la Defensa de las criaturas y reparte el daño que recibe el equipo en cuotas.',
         atr=[10, 12, 13, 16, 14, 10], pac='12 (+6 por nivel)', defe=14, mov=3, ataque='Doctrina, 1d6 de daño, alcance 2 casillas.',
         hab='Cuentas y tasas, Normativa, Atención a plazos, Revisar el expediente.',
         pod='<b>Fiscalización</b> y <b>Fraccionamiento</b>.', pers='Calcula la propina al céntimo y tiene una calculadora para cada día de la semana.'),
]


def ficha_esp(e):
    celdas = ''.join(f'<div><b>{a}</b><span>{v} ({mod(v)})</span></div>' for a, v in zip(ABR, e['atr']))
    marca = '' if e.get('base') else '<div class="etiqueta">Especialidad de expansión</div>'
    return (f'<div class="esp">{img(e["img"], "esp-img")}<div class="esp-txt">'
            f'<h2>{e["n"]}</h2>{marca}<p class="lema">{e["lema"]}</p><p>{e["t"]}</p>'
            f'<div class="bl-atr">{celdas}</div>'
            f'<p class="dato"><b>Paciencia</b> {e["pac"]} · <b>Defensa</b> {e["defe"]} · <b>Movimiento</b> {e["mov"]}</p>'
            f'<p class="dato"><b>Ataque.</b> {e["ataque"]}</p>'
            f'<p class="dato"><b>Habilidades sugeridas.</b> {e["hab"]}</p>'
            f'<p class="dato"><b>Poderes.</b> {e["pod"]}</p>'
            f'<p class="dato"><b>Personalidad.</b> {e["pers"]}</p></div></div>')


for i in range(0, 6, 2):
    intro = ''
    if i == 0:
        intro = '<h1>Especialidades</h1><p class="intro">Cada abogado del estudio domina una rama del derecho. La especialidad fija sus características sugeridas, su forma de atacar y sus poderes. Civilista y Administrativista son las especialidades del prototipo de la aplicación; las otras cuatro son de expansión y sus números pueden ajustarse tras las pruebas de juego.</p>'
    pagina('Crear un abogado', intro + ficha_esp(ESP[i]) + ficha_esp(ESP[i + 1]), 'esp-pag' + (' con-intro' if i == 0 else ''))

# ═════════════════════════ 10-11. SISTEMA DE JUEGO ═════════════════════════
pagina('Sistema de juego', f'''
<h1>Sistema de juego</h1>
<div class="dos">
<h2>La tirada</h2>
<p class="capital">Cuando un abogado intenta algo cuyo resultado es incierto, el Relator pide una tirada. Se lanza un dado de veinte caras y se suma el modificador de la característica que corresponda y, si el personaje es competente, su bonificador de competencia. Si el total iguala o supera la <b>dificultad</b>, lo logra.</p>
<div class="formula">d20 + modificador + competencia ≥ dificultad</div>
<p>Contra una criatura, la dificultad es su <b>Defensa</b>. Contra el entorno, el Relator la fija con esta tabla:</p>
{tabla(['Dificultad', 'Ejemplo en Expedientia'], [
    ['5 · Muy fácil', 'Encontrar la Mesa de Partes en el primer piso.'],
    ['10 · Fácil', 'Llenar un formulario sin tacharlo.'],
    ['12 · Normal', 'Presentar el requisito correcto en un trámite.'],
    ['15 · Media', 'Encontrar un folio perdido en un expediente grueso.'],
    ['20 · Difícil', 'Conseguir una cita para hoy.'],
    ['25 · Muy difícil', 'Que la impresora del estudio funcione un viernes.'],
    ['30 · Casi imposible', 'Que la cola avance por sí sola.'],
])}
<h3>Fundado e Improcedente</h3>
<p>Si el dado muestra un <b>20 natural</b>, la tirada es <b>Fundada</b>: en un ataque acierta siempre y se tiran el doble de dados de daño (el modificador no se duplica). Si muestra un <b>1 natural</b>, es <b>Improcedente</b>: el ataque falla siempre, sin importar los modificadores. En las pruebas de características solo cuenta el total, pero el Relator puede narrar un 20 o un 1 con especial gracia.</p>
<h3>Ventaja y desventaja</h3>
<p>Algunas situaciones dan <b>ventaja</b>: se tiran dos d20 y se usa el mayor. Otras imponen <b>desventaja</b>: se tiran dos y se usa el menor. Si una tirada tiene ventaja y desventaja a la vez, se anulan y se tira un solo dado. Nunca se acumulan dos ventajas.</p>
<p>Dan ventaja, por ejemplo: acertar una prueba jurídica, usar un Escrito pre-firmado o un Código anotado, o que un aliado distraiga a la criatura. Dan desventaja: actuar con el plazo vencido, atacar a una criatura que no se ve o trabajar sin dormir.</p>
{caja('Pruebas enfrentadas', '<p>Cuando dos personajes compiten (un abogado intenta pasar desapercibido y un Tramitador lo vigila), ambos tiran y gana el total más alto. En caso de empate, se mantiene la situación como estaba.</p>')}
</div>''')

pagina('Sistema de juego', f'''
<div class="dos">
<h2>La prueba jurídica</h2>
<p class="capital">La prueba jurídica es el sello del juego: una pregunta real de derecho peruano con cuatro opciones que el <i>jugador</i>, no el personaje, debe responder. Se usa antes de una tirada importante.</p>
<ul>
<li>Se pueden usar como máximo <b>tres pruebas jurídicas por expediente</b>, entre todo el equipo.</li>
<li>El jugador tiene <b>20 segundos</b> para responder.</li>
<li>Si acierta, su siguiente tirada tiene <b>ventaja</b>.</li>
<li>Si falla o se le acaba el tiempo, tira de forma normal. La prueba nunca castiga a quien no sabe.</li>
</ul>
<p>En la aplicación las preguntas salen de un banco revisado por un abogado. Cada pregunta indica la norma o resolución en la que se basa, con fecha y enlace a la fuente oficial, y solo entra al juego cuando está verificada. En mesa, el Relator puede preparar sus propias preguntas siguiendo la misma regla: <b>si no se pudo verificar en la fuente oficial, no se usa.</b></p>
{caja('Del expediente a la realidad', '<p>En el Perú, el procedimiento ante las entidades públicas se rige por la Ley 27444, Ley del Procedimiento Administrativo General, cuyo Texto Único Ordenado vigente fue aprobado por el Decreto Supremo 006-2026-JUS (publicado en El Peruano el 30 de abril de 2026). Edición oficial del Ministerio de Justicia en gob.pe: ' + amarillo('https://www.gob.pe/institucion/minjus/informes-publicaciones/8441836') + '.</p>', 'real')}
<h2>Narración</h2>
<p>Las tiradas no reemplazan a la historia: la cuentan. El Relator describe qué sucede con cada resultado, y conviene que un fallo también haga avanzar la partida. En Expedientia, fallar un trámite casi nunca detiene el caso: lo encarece. Se subsana a la segunda, se pierde tiempo, el cliente se impacienta.</p>
{tabla(['d20', 'Resultado narrativo de un trámite'], [
    ['1', 'Improcedente. El escrito vuelve con tres observaciones nuevas y un sello que nadie había visto.'],
    ['2–6', 'Observado. Hay que subsanar: se pierden 20 minutos y 2 de Paciencia.'],
    ['7–11', 'Recibido «a condición». Pasa, pero el Relator guarda una complicación para después.'],
    ['12–19', 'Recibido. El sello cae con un sonido que alegra el alma.'],
    ['20', 'Fundado. Recibido, proveído y con una copia de cortesía para el cliente.'],
])}
<h3>Simplificar</h3>
<p>No hace falta tirar por todo. Si el abogado tiene tiempo y no hay presión, lo logra. Las tiradas se reservan para cuando fallar es interesante.</p>
<h3>El reloj del caso</h3>
<p>Algunos expedientes tienen <b>plazo</b>: un número de minutos de juego antes de que el caso se declare «improcedente por extemporáneo». Cada sala consume minutos (un combate, 4 más 2 por ronda; un trámite, 10 si sale bien y 20 si sale mal; la cafetería, 15). El Relator lleva la cuenta a la vista de todos.</p>
</div>''')

# ═════════════════════════ 12. LA ACCIÓN ═════════════════════════
def tablero():
    s = '<svg viewBox="0 0 380 300" class="tablero" xmlns="http://www.w3.org/2000/svg">'
    s += '<rect x="0" y="0" width="380" height="300" rx="8" fill="#3a2a2e"/>'
    for f in range(6):
        for c in range(8):
            col = '#e9dcbc' if (f + c) % 2 == 0 else '#dccba4'
            s += f'<rect x="{10 + c * 45}" y="{10 + f * 46}" width="45" height="46" fill="{col}" stroke="#8a6a4a" stroke-width="1"/>'
    for c in (1, 2, 3):
        s += f'<rect x="{10 + c * 45}" y="{10 + 3 * 46}" width="45" height="46" fill="#7fc8ff" opacity=".45"/>'
    for c, f in ((2, 2), (2, 4), (3, 3)):
        s += f'<rect x="{10 + c * 45}" y="{10 + f * 46}" width="45" height="46" fill="#7fc8ff" opacity=".45"/>'
    piezas = [(0, 3, '#7c1f2b', 'C'), (1, 1, '#24365e', 'A'), (6, 2, '#c0282a', 'F'), (5, 4, '#e07a1a', 'P'), (7, 0, '#6a4a8a', 'N')]
    for c, f, col, l in piezas:
        x, y = 10 + c * 45 + 22.5, 10 + f * 46 + 23
        s += f'<circle cx="{x}" cy="{y}" r="17" fill="{col}" stroke="#1d1320" stroke-width="3"/><text x="{x}" y="{y + 6}" text-anchor="middle" font-family="Cinzel" font-weight="700" font-size="16" fill="#fff">{l}</text>'
    s += '<path d="M47 166 Q120 120 287 120" fill="none" stroke="#b02a2a" stroke-width="3" stroke-dasharray="6 5"/><path d="M280 113 L290 120 L280 127" fill="none" stroke="#b02a2a" stroke-width="3"/>'
    s += '</svg>'
    return s


pagina('Sistema de juego', f'''
<h1>La acción</h1>
<div class="dos">
<p class="capital">Cuando las criaturas del sistema bloquean el paso, empieza el combate. Se juega por <b>rondas</b>, y en cada ronda todos actúan una vez, en orden de <b>iniciativa</b>.</p>
<h3>Iniciativa</h3>
<p>Al comenzar, cada participante tira d20 + su modificador de Agilidad procesal. Se actúa de mayor a menor. Si hay empate, actúa primero quien tenga mayor Agilidad procesal y, si persiste, los abogados antes que las criaturas.</p>
<h3>El turno</h3>
<p>En su turno, cada personaje puede hacer, en el orden que quiera:</p>
<ul>
<li><b>Moverse</b> tantas casillas como su Movimiento. Las criaturas rivales bloquean el paso; los aliados dejan pasar.</li>
<li><b>Una acción</b>: atacar, usar un poder, usar un objeto o ayudar a un aliado (que tendrá ventaja en su próxima tirada).</li>
<li><b>Una reacción</b> por ronda, incluso fuera de su turno, solo si un poder lo permite (como la Nulidad o el Recurso de apelación).</li>
</ul>
<h3>Atacar</h3>
<p>Para atacar, el objetivo debe estar dentro del <b>alcance</b>, contado en casillas. Se tira d20 + modificador de la característica de ataque + competencia, contra la Defensa del objetivo. Si acierta, se tira el dado de daño de la especialidad más el mismo modificador y se resta de la Paciencia del objetivo.</p>
<h3>Caer</h3>
<p>Un abogado con Paciencia 0 <b>cae</b> y deja el expediente. Una criatura con Paciencia 0 queda <b>desestimada</b> y se retira del tablero. Si todo el equipo cae, el expediente se archiva.</p>
<h3>Estados</h3>
<p><b>Inmovilizado:</b> no puede moverse ni actuar durante 2 turnos. <b>Observado:</b> el golpe no cumple el requisito de la criatura y no hace daño. <b>En silencio:</b> la criatura no actuó en su turno; si una criatura queda 2 turnos en silencio y hay un Administrativista en la sala, el equipo gana la sala.</p>
{tablero()}
<p class="pie-img">El tablero de 6 × 8 casillas. La Civilista (C) puede moverse por las casillas azules; el Formalismo (F) está a su alcance de ataque tras moverse. P es un Plazo que Vence y N una Notificación Defectuosa.</p>
</div>''')

# ═════════════════════════ 13. PODERES ═════════════════════════
poderes = [
    ['Medida cautelar', 'Civilista', 'Acción', 'Inmoviliza a una criatura a 3 casillas o menos durante 2 turnos. Se recarga en 3 rondas.'],
    ['Nulidad', 'Civilista', 'Reacción', 'Anula la última acción de una criatura y devuelve la Paciencia que quitó. Se recarga en 3 rondas.'],
    ['Recurso de apelación', 'Administrativista', 'Reacción', 'Un aliado repite su última tirada de ataque fallida.'],
    ['Queja', 'Administrativista', 'Acción', 'Inmoviliza a una criatura durante 2 turnos. Se recarga en 3 rondas.'],
    ['Silencio administrativo', 'Administrativista', 'Pasiva', 'Si una criatura no actúa 2 turnos seguidos, el equipo gana la sala.'],
    ['Flagrancia', 'Penalista', 'Reacción', 'Ataca de inmediato a una criatura que entra en su alcance.'],
    ['Allanamiento', 'Penalista', 'Acción', 'Un ataque que ignora el requisito de la criatura y la mitad de su Defensa por encima de 10.'],
    ['Beneficio social', 'Laboralista', 'Acción', 'Todos los aliados a 2 casillas recuperan 1d6 de Paciencia. Una vez por sala.'],
    ['Inspección', 'Laboralista', 'Acción', 'Revela el rasgo y el requisito vigente de una criatura; el equipo tiene ventaja contra ella esta ronda.'],
    ['Amparo', 'Constitucionalista', 'Reacción', 'Cuando un aliado recibe daño, lo reduce a la mitad.'],
    ['Hábeas corpus', 'Constitucionalista', 'Acción', 'Libera a un aliado inmovilizado o empujado y le permite moverse de inmediato.'],
    ['Fiscalización', 'Tributarista', 'Acción', 'La Defensa de una criatura baja 2 puntos hasta el final de la sala.'],
    ['Fraccionamiento', 'Tributarista', 'Reacción', 'Reparte el daño que recibe un aliado entre él y el Tributarista.'],
    ['Casación', 'Todo el equipo', 'Especial', 'Una vez por expediente, el equipo repite cualquier tirada, pero con dificultad 18.'],
]
pagina('Sistema de juego', f'''
<h1>Poderes de las especialidades</h1>
<p class="intro">Los poderes llevan el nombre de figuras procesales, pero en el juego funcionan como habilidades especiales. No describen cómo operan esas figuras en el derecho real; para eso están los libros y las fuentes oficiales.</p>
{tabla(['Poder', 'Especialidad', 'Tipo', 'Efecto'], poderes, 'poderes')}
<div class="roles">{''.join(f'<div class="rol"><div class="rol-img"><img src="img/{e["img"]}.jpg"></div><b>{e["n"]}</b><span>{r}</span></div>' for e, r in zip(ESP, ['Control', 'Apoyo', 'Golpe rápido', 'Sostén', 'Protección', 'Debilitar']))}</div>
<div class="dos">
{caja('Recargas', '<p>Un poder que «se recarga en 3 rondas» no puede volver a usarse hasta que pasen tres rondas completas. Así la Medida cautelar y la Queja no se encadenan sin fin, y las criaturas siempre tienen una oportunidad de actuar.</p>')}
{caja('Combinaciones', '<p>La Civilista inmoviliza al Plazo que Vence con una Medida cautelar: mientras está inmovilizado, el plazo se suspende y no explota. Si además el Administrativista está en la sala, dos turnos de silencio ganan la sala sin un solo golpe.</p>', 'ejemplo')}
</div>
''')

# ═════════════════════════ 14. PROGRESIÓN ═════════════════════════
umbrales = [0, 300, 900, 2700, 6500, 14000, 23000, 34000, 48000, 64000, 85000, 100000, 120000, 140000, 165000, 195000, 225000, 265000, 305000, 355000]


def titulo(n):
    return 'Practicante' if n <= 4 else 'Bachiller' if n <= 8 else 'Abogado' if n <= 12 else 'Asociado' if n <= 16 else 'Socio'


filas_niv = [[str(n), f'{umbrales[n - 1]:,}'.replace(',', ' '), f'+{2 + (n - 1) // 4}', titulo(n)] for n in range(1, 21)]
pagina('Sistema de juego', f'''
<h1>Progresión</h1>
<div class="dos">
<p class="capital">Cada expediente cerrado da <b>experiencia</b> al estudio. Al acumular suficiente, los abogados suben de nivel: ganan 6 puntos de Paciencia máxima, mejoran su competencia y avanzan en la carrera, de practicante a socio.</p>
{tabla(['Nivel', 'Experiencia', 'Competencia', 'Título'], filas_niv, 'niv')}
<h3>Experiencia por expediente</h3>
<p>El Relator reparte experiencia al cerrar cada expediente: 100 por cada criatura menor desestimada, 300 por un jefe, 50 por cada sala superada sin pelear y 100 extra si el caso se declara fundado. En la aplicación los números exactos dependen del caso.</p>
<h3>Mejoras</h3>
<p>En los niveles 4, 8, 12, 16 y 19, cada abogado suma 2 puntos a una característica o 1 punto a dos características, sin pasar de 20.</p>
{caja('La carrera del abogado', '<p><b>Practicante</b> (niveles 1 a 4): carga los expedientes y aprende dónde queda cada ventanilla. <b>Bachiller</b> (5 a 8): ya presenta escritos con su nombre. <b>Abogado</b> (9 a 12): litiga sin que nadie le revise los escritos. <b>Asociado</b> (13 a 16): los clientes preguntan por él. <b>Socio</b> (17 a 20): su apellido va en la placa, bien escrito.</p>', 'ejemplo')}
{caja('Reputación', '<p>El estudio empieza con <b>Reputación 50</b>, en una escala de 0 a 100. Rechazar sobornos, ganar casos difíciles y denunciar a la red del Coimero la suben; los atajos la bajan. Con Reputación 70 o más aparecen los aliados honestos: el Suboficial Quispe y la Especialista Mendoza.</p>')}
{caja('El expediente disciplinario', '<p>Cada vez que el estudio acepta la oferta del Coimero, el Relator suma 10 puntos a un número secreto: el <b>expediente disciplinario</b>. Los jugadores no lo ven. Algún día, en el peor momento, ese expediente aparecerá sobre la mesa.</p>', 'aviso')}
</div>''')

# ═════════════════════════ 15. RECOMPENSAS ═════════════════════════
pagina('Sistema de juego', f'''
<h1>Recompensas</h1>
<div class="dos">
<p class="capital">Además de experiencia, cada expediente deja <b>honorarios</b>, la moneda del juego. Se ganan al desestimar criaturas (15 por sala de combate, 60 por el jefe, 50 por la red del Coimero) y al resolver bien las audiencias (10 o 20). Los honorarios se gastan en la librería jurídica o, para los más supersticiosos, en el puesto del Ekeko de los Honorarios, que cobra en honorarios del juego y nunca «a cuenta».</p>
{tabla(['Objeto', 'Precio', 'Efecto'], [
    ['Escrito pre-firmado', '30', 'Se gasta para tener ventaja en una tirada cualquiera.'],
    ['Café doble', '20', 'Todo el equipo recupera su Paciencia máxima.'],
    ['Código anotado', '40', 'La próxima prueba o trámite se tira con ventaja.'],
    ['Sello de recepción de la suerte', '25', 'Una vez por expediente, convierte un «Observado» en «Recibido».'],
    ['Termo de anís', '15', 'Recupera 1d6 de Paciencia a un abogado.'],
    ['Agenda con alarma', '35', 'El equipo sabe siempre cuántos minutos quedan y gana 10 minutos una vez.'],
    ['Copias certificadas', '10', 'Ventaja contra el Formalismo durante una sala.'],
    ['Paraguas del juzgado', '20', 'Defensa +1 contra Notificaciones Defectuosas.'],
])}
<p class="chico">Los tres primeros objetos existen en la aplicación; los demás son para el juego en mesa.</p>
<h3>Sobre las recompensas</h3>
<p>Un buen Relator no regala honorarios: los pone a prueba. Ofrece una recompensa grande a cambio de un riesgo grande, o enfrenta a los jugadores con una elección: ¿gastar en el Café doble para sobrevivir al jefe, o guardar para la próxima campaña?</p>
<h3>Recompensas que no son dinero</h3>
<p>Un cliente agradecido que trae a su familia al estudio. Un servidor público honesto que recuerda el nombre del abogado. Una Especialista que comparte el requisito vigente. En Expedientia, la reputación abre más puertas que los honorarios.</p>
{caja('Regla opcional: gastos del estudio', '<p>Entre expediente y expediente, el estudio paga 10 honorarios de alquiler y 5 de fotocopias. Si no puede pagar, empieza el siguiente caso con un abogado de menos Paciencia máxima (−2 a todos) hasta ponerse al día. Es una regla para grupos que disfrutan administrar su estudio; los demás pueden ignorarla.</p>')}
{caja('El Coimero', '<p>En algunas salas aparece un señor muy amable que ofrece «agilizar» el trámite. No ataca: tienta. El equipo puede <b>aceptar</b> (recupera toda la Paciencia, pero suma 10 al expediente disciplinario), <b>rechazar</b> (Reputación +3) o <b>denunciar</b> (Reputación +2 y se abre una sala extra, más difícil, contra la red del Coimero, con la mejor recompensa del caso).</p>', 'aviso')}
</div>''')

# ═════════════════════════ 16-17. MONTANDO LA PARTIDA ═════════════════════════
pagina('Montando la partida', f'''
<h1>Montando la partida</h1>
<div class="dos">
<h2>El Relator</h2>
<p class="capital">El Relator es quien describe el mundo, interpreta a las criaturas y a los personajes no jugadores, y aplica las reglas. No juega contra el equipo: juega para que la historia sea buena. Su trabajo es que cada expediente tenga tensión, humor y al menos una decisión difícil.</p>
<h2>La estructura de un expediente</h2>
<p>Cada expediente sigue la misma forma, que también usa la aplicación:</p>
<ol class="pasos">
<li><b>La llegada.</b> El cliente entra al estudio y cuenta su problema (siempre a su manera).</li>
<li><b>La primera sala</b> es siempre un combate: algo bloquea la entrada.</li>
<li><b>Las salas intermedias</b> mezclan trámites, audiencias, la cafetería, la librería y, a veces, el Coimero.</li>
<li><b>La sala del jefe</b> cierra el caso.</li>
<li><b>El cierre.</b> Fundado o archivado; se reparten honorarios, experiencia y reputación.</li>
</ol>
{tabla(['Sala', 'Qué pasa', 'Tirada típica'], [
    ['Combate', 'Criaturas del sistema bloquean el paso.', 'Ataques contra Defensa'],
    ['Trámite', 'Una puerta exige «el requisito correcto».', 'Criterio, dificultad 12'],
    ['Audiencia', 'El cliente cambia su versión de los hechos.', 'Oratoria o Criterio, 13'],
    ['Cafetería', 'Pausa: todos recuperan la mitad de su Paciencia.', 'Ninguna'],
    ['Librería', 'Se compran objetos con honorarios.', 'Ninguna'],
    ['Coimero', 'Una oferta tentadora.', 'Decisión del equipo'],
    ['Jefe', 'La criatura mayor del caso.', 'Combate especial'],
])}
<h2>La audiencia</h2>
<p>El Cliente cambia su versión. El equipo elige: <b>confrontarlo</b> (Oratoria, dificultad 13), y si sale bien el Cliente reconoce la versión correcta y paga 20 de honorarios extra; o <b>revisar el expediente</b> (Criterio, dificultad 13), y si sale bien se encuentra una fecha que da 20 minutos más de plazo. Si falla, la audiencia se reprograma y se pierde tiempo.</p>
<h2>Un expediente en cinco minutos</h2>
<p>Cuando no hay tiempo de preparar, tira en la tabla de clientes (página 19), elige la zona y tira 1d6 para la complicación:</p>
{tabla(['d6', 'Complicación'], [['1', 'El caso tiene plazo: 120 minutos.'], ['2', 'El Cliente oculta un documento clave.'], ['3', 'La contraparte también tiene abogado… y es mejor.'], ['4', 'El Coimero aparece dos veces.'], ['5', 'Una criatura del Archivo Central se coló en la zona.'], ['6', 'Don Eusebio está de vacaciones.']], 'aleat')}
{caja('Llevar la cuenta', '<p>Usa una ficha a la vista de todos con el reloj del caso, las pruebas jurídicas usadas (máximo 3), la Casación (una por expediente) y las recargas de los poderes. En la aplicación, todo esto aparece en pantalla.</p>', 'ejemplo')}
{caja('Consejo', '<p>Prepara tres frases del Cliente antes de la sesión. Nada da más vida a una audiencia que un cuñado que vino de testigo y recuerda una tercera fecha.</p>')}
</div>''')

pagina('Montando la partida', f'''
{img('esc-mesa', 'ancho', 'La Mesa de Partes')}
<p class="pie-img">La Mesa de Partes en hora punta. El letrero marca el turno 0947; la cola no ha avanzado desde la mañana.</p>
<div class="dos">
<h2>Exploración</h2>
<p class="capital">Entre sala y sala, los abogados recorren Expedientia: pasillos, escaleras, oficinas sin letrero y sótanos que no figuran en los planos. El Relator describe lo que ven con dos o tres detalles concretos: el olor a papel húmedo, el vigilante que ya los conoce, el reloj de pared que adelanta diez minutos.</p>
<p>Las tablas de las páginas siguientes ayudan a improvisar. Tira cuando el equipo entra a un lugar nuevo, cuando pierde tiempo o cuando la partida necesita un empujón.</p>
<h3>Ritmo</h3>
<p>Alterna salas tensas con salas tranquilas. Después de un combate difícil, una cafetería se agradece; después de dos trámites seguidos, el equipo agradecerá algo a lo que pegarle.</p>
<h3>Cuando los jugadores se desvían</h3>
<p>Si el equipo decide ignorar la Mesa de Partes y buscar al juez en su casa, no lo impidas: haz que el camino sea interesante. Las criaturas del sistema no viven solo en el Palacio; también hay Formalismos en las notarías y Plazos que Vencen en la puerta de cualquier oficina.</p>
{caja('Seguridad en la mesa', '<p>Antes de empezar, acuerden qué temas quedan fuera del juego. Un caso de familia puede tocar fibras sensibles; el Relator cambia de tema sin pedir explicaciones si alguien lo solicita.</p>')}
</div>''')

# ═════════════════════════ 18-19. TABLAS ALEATORIAS ═════════════════════════
sucesos = [
    'Se va la luz en todo el piso. Las ventanillas siguen atendiendo «con linterna».', 'Un Usuario en cola reconoce a un abogado: «¿Usted no es el del caso de mi tía?».',
    'Cambia el sistema informático. Todos los turnos se reinician desde el 001.', 'Aparece un Formalismo exigiendo una copia que nadie conoce.',
    'Don Eusebio pasa con un termo y ofrece café: +1d4 de Paciencia a todos.', 'Una Notificación Defectuosa revolotea buscando «al señor de la casa».',
    'Rosita cierra la ventanilla «por cinco minutos». Se pierden 15.', 'Se cae una pila de expedientes; uno de ellos abre los ojos.',
    'Un abogado de pasillo susurra que «conoce a alguien». Es el Coimero.', 'El cliente llama por teléfono para decir que encontró otro documento… de 1987.',
    'Llega un Plazo que Vence con el número 3 encendido.', 'El vigilante pide que todos se pongan en fila india. La fila se convierte en una Cola.',
    'Alguien gritó «¡Mesa de Partes virtual!». Nadie sabe dónde queda.', 'Se encuentra un sello perdido: ventaja en el próximo trámite.',
    'La Especialista Mendoza pasa apurada y deja caer una pista sobre el requisito vigente.', 'Una impresora imprime sola la palabra «IMPROCEDENTE» cien veces.',
    'El reloj de pared se detiene. El Relator decide si el plazo también.', 'Un Fraccionador se esconde entre los recibos de pago.',
    'Un estudiante de derecho pide ayuda con su tarea. Ayudarle da Reputación +1.', 'Silencio absoluto. Algo grande se mueve en el sótano.',
]
excusas = ['Falta firma.', 'Falta la firma de la firma.', 'El sistema está lento.', 'Ese formato ya no se usa.', 'Ese formato todavía no se usa.',
           'Venga con su abogado. ¿Usted es el abogado? Venga con otro.', 'Es en la otra ventanilla.', 'La otra ventanilla es esta, pero mañana.',
           'Se pasó la hora por un minuto.', 'Falta copia simple del original certificado.', 'No hay sello.', 'El jefe está en reunión desde marzo.',
           'Tiene que pagar la tasa en el banco del frente.', 'El banco del frente se mudó.', 'Su DNI está vencido. Ah, no; es el mío.',
           'Hay que llenarlo con lapicero azul.', 'Tiene que traer un folder manila. No, ese no.', 'Saque turno.', 'Vuelva mañana.', 'Ya está. ¿Algo más?']
clientes = ['Una señora con tres bolsas de documentos que «por si acaso» trajo todo.', 'Un joven emprendedor que quiere ganar una licitación con una sola fotocopia.',
            'Un jubilado que lleva once años esperando una resolución.', 'Una empresa de transporte con un problema que llegó «hace poco» (en 2009).',
            'Una pareja que no se pone de acuerdo ni en por qué vino.', 'Un ingeniero que trae su caso en una hoja de cálculo de 40 pestañas.',
            'Una vecina cuya pared invadió el vecino… o al revés.', 'Un dirigente de mercado que quiere formalizar a 300 comerciantes.',
            'Un estudiante cuyo título se perdió en el Archivo Central.', 'Una heredera que acaba de descubrir que tiene catorce primos.',
            'Un panadero al que le llegó una multa por un horno que no tiene.', 'Una periodista que investiga a la red del Coimero.',
            'Un señor que jura que presentó su escrito y tiene el cargo «en algún lado».', 'Una profesora a la que nunca le pagaron sus vacaciones truncas.',
            'Un municipio pequeño que necesita contratar un puente.', 'Una familia que quiere inscribir la casa de la abuela.',
            'Un músico al que le copiaron su huaino.', 'Una cooperativa de cafetaleros con una deuda tributaria misteriosa.',
            'Un perro (con su dueño) involucrado en un conflicto vecinal.', 'Alguien que no dice su nombre y paga por adelantado. Sospechoso.']
fil_suc = [[f'{i * 5 + 1:02d}–{i * 5 + 5:02d}' if i < 19 else '96–00', t] for i, t in enumerate(sucesos)]
pagina('Montando la partida', f'''
<h1>Tablas aleatorias</h1>
<div class="dos">
<p class="intro2">Tira 1d100 cuando el equipo entra a un lugar del Palacio de Justicia o cuando pierde tiempo en una sala.</p>
{tabla(['d100', 'Sucesos en el Palacio de Justicia'], fil_suc, 'aleat')}
</div>''')
pagina('Montando la partida', f'''
<div class="dos">
{tabla(['d20', 'Excusas de ventanilla'], [[str(i + 1), t] for i, t in enumerate(excusas)], 'aleat')}
<p class="intro2">Tira 1d20 cuando un abogado intenta presentar algo y el Relator necesita un motivo para que no funcione.</p>
{tabla(['d20', 'Clientes que llegan al estudio'], [[str(i + 1), t] for i, t in enumerate(clientes)], 'aleat')}
<p class="intro2">Cada cliente es una semilla de expediente: decide qué criatura del sistema custodia su problema.</p>
</div>''')

# ═════════════════════════ 20. PNJ ═════════════════════════
pnj = [
    ('pnj-eusebio', 'Don Eusebio', 'Auxiliar con treinta años en el juzgado', 'Conoce cada pasillo y cada atajo honesto. Siempre tiene un termo de café y un comentario: «En Mesa de Partes los escritos no se pierden, doctor. Se independizan.» Es el guía del equipo y nunca acepta coimas.'),
    ('pnj-rosita', 'Rosita', 'Encargada de la ventanilla 3', 'Sabe dónde está cada escrito, aunque no siempre quiere decirlo. Amable con quien es amable. «Si lo presentó, está en la cola. Todo termina en la cola.»'),
    ('pnj-valdivia', 'Doctora Valdivia', 'Mentora del estudio', 'Abogada veterana, socia retirada de un gran estudio. Da consejos secos y certeros: «Los atajos siempre cobran después.» Sospecha que algo en el Archivo Central se está comiendo los expedientes.'),
    ('pnj-mendoza', 'Especialista Mendoza', 'Servidor de la Sala de Contrataciones', 'Honesto y abrumado. Si la Reputación del estudio es 70 o más, revela el requisito vigente del jefe de la Sala de Contrataciones.'),
    ('pnj-quispe', 'Suboficial Quispe', 'Aliado honesto', 'Policía de pocas palabras que se suma al equipo contra la red del Coimero si la Reputación es alta. Firmeza 14, Paciencia 16, Defensa 14, ataque 1d8.'),
    ('pnj-cliente', 'El Cliente', 'El motivo de todo', 'Cambia de cara en cada caso, pero siempre es el mismo: preocupado, impaciente y con una versión de los hechos que muta en cada audiencia.'),
]
pnj_html = ''.join(f'<div class="pnj">{img(i, "pnj-img")}<div><h3>{n}</h3><div class="bl-tipo">{t}</div><p>{d}</p></div></div>' for i, n, t, d in pnj)
pagina('Montando la partida', f'''
<h1>Personajes no jugadores</h1>
<p class="intro">Los personajes no jugadores dan voz a Expedientia. Todos son ficticios: humor de oficio, sin retratar a personas reales.</p>
<div class="pnjs">{pnj_html}</div>
<div class="dos-fijo"><div>{tabla(['d6', 'Lo que dice Don Eusebio'], [['1', '«En Mesa de Partes los escritos no se pierden, doctor. Se independizan.»'], ['2', '«Esa cola no avanza desde 1998. Suerte.»'], ['3', '«Aquí se resuelven más casos en la cafetería que en las salas.»'], ['4', '«Lo archivado también resucita.»'], ['5', '«El sello bueno lo tiene Rosita; el malo, todos los demás.»'], ['6', '«Treinta años en este juzgado y todavía me sorprendo, doctor.»']], 'aleat')}</div>
<div>{caja('Crear adversarios', '<p>Toda criatura del sistema nace de un problema concreto: un requisito absurdo, un plazo injusto, una notificación que no llega. Para crear una nueva, elige el problema, dale un <b>rasgo</b> que lo represente en el tablero y fija sus números comparándola con las del bestiario: una criatura menor tiene entre 3 y 9 de Paciencia y Defensa de 9 a 12; un jefe, 30 o más de Paciencia y una mecánica que obligue al equipo a pensar.</p>')}</div></div>
''')

def socio(imagen, nombre, cargo, texto, frase, poder, nombre_poder):
    return (f'<div class="socio">{img(imagen, "socio-img")}<div class="socio-txt"><h2>{nombre}</h2><div class="bl-tipo">{cargo}</div>'
            f'<p>{texto}</p><p class="cita">{frase}</p>{caja("Aliado: " + nombre_poder, poder, "ejemplo")}</div></div>')


pagina('Montando la partida', '<h1>Los socios del estudio</h1><p class="intro">Dos personajes acompañan al equipo desde el primer caso. El Relator los interpreta, aunque un jugador puede llevar a uno de ellos si el grupo es pequeño. Sus poderes son propuestas para el juego en mesa.</p>' +
       socio('ia-gonzabio', 'Gonzabio', 'Socio fundador del estudio',
             'Puso la mitad del alquiler y toda la terquedad. Aprendió el oficio en la ventanilla, no en la facultad, y sabe que un expediente bien foliado aguanta cualquier golpe. En combate se planta delante del equipo con el maletín abierto; fuera de él, es quien sigue la pista de los escritos perdidos hasta el Archivo Central.',
             '«Usted litiga y yo armo los expedientes. Mientras no se nos caiga el expediente, no nos gana.»',
             '<p><b>Escudo de expedientes.</b> Una vez por sala, cuando un abogado a 2 casillas o menos recibe daño, Gonzabio lo reduce en 1d6 + 2. <b>Expediente foliado:</b> el equipo empieza cada caso con un Escrito pre-firmado gratis.</p>', 'escudo de expedientes') +
       socio('ia-lion', 'Lion', 'Abogado investigador',
             'Terno gris, corbata roja, lentes y una lapicera que nunca suelta. Lee la letra pequeña, cuenta los días hábiles dos veces y abre los anexos que nadie abre, porque ahí se esconden los monstruos. Fue él quien cruzó las firmas de los tres primeros casos y encontró el mismo sello: el del Archivo Central.',
             '«Leí el anexo 7. El requisito se escribió pensando en una sola empresa, y no es la nuestra.»',
             '<p><b>Letra pequeña.</b> Una vez por expediente, Lion revela el rasgo o el requisito vigente de una criatura, aunque la Reputación sea baja. <b>Doble conteo:</b> en un caso con reloj, el equipo gana 10 minutos la primera vez que pierde tiempo en un trámite.</p>', 'letra pequeña'), 'socios')

# ═════════════════════════ 21-25. BESTIARIO ═════════════════════════
B = [
    ('Formalismo', 'Criatura menor de Mesa de Partes', [10, 10, 10, 8, 8, 6], 8, 11, 2, 'Firmeza, 1d4 de daño, alcance 1.',
     '<b>Exige requisito.</b> Sin el «requisito correcto» (tirar con ventaja o sacar Fundado) solo recibe la mitad del daño.', 'Avanza lento y se planta frente a la puerta. Nunca huye: prefiere quedar observado.', 'cr-formalismo'),
    ('Plazo que Vence', 'Criatura menor de Mesa de Partes', [8, 12, 8, 8, 10, 6], 5, 12, 3, 'Agilidad procesal, 1d4 de daño, alcance 1.',
     '<b>Vence.</b> Si sigue en pie al comenzar su tercer turno, explota y daña a todos los que estén cerca. Inmovilizado, el plazo se suspende.', 'Corre hacia el grupo más numeroso. Es lo primero que hay que atender.', 'cr-plazo'),
    ('Notificación Defectuosa', 'Criatura menor voladora', [8, 10, 10, 8, 10, 8], 6, 11, 3, 'Agilidad procesal, 1d4 de daño, alcance 2.',
     '<b>Notifica al azar.</b> Ataca a quien no corresponde: el objetivo se elige al azar entre todos los que estén a su alcance.', 'Revolotea sin orden. Puede herir a otras criaturas si el Relator así lo decide.', 'cr-notificacion'),
    ('Usuario en cola', 'Esbirro de La Cola Infinita', [10, 8, 8, 6, 6, 6], 4, 9, 2, 'Firmeza, 1d3 de daño, alcance 1.',
     '<b>Paciencia infinita.</b> No es malo: solo lleva demasiado tiempo esperando. Desestimarlo lo devuelve a casa.', 'Ocupa casillas y estorba el paso. Se suma uno nuevo cada turno mientras La Cola siga en pie.', 'cr-usuario'),
    ('La Cola Infinita', 'Jefe de Mesa de Partes', [14, 8, 16, 8, 10, 6], 30, 12, 1, 'Firmeza, 1d8 de daño, alcance 1.',
     '<b>Siempre crece.</b> Cada turno suma un Usuario en cola a la sala, hasta un tope, para que la sala siempre pueda ganarse.', 'Se queda quieta y deja que la fila haga el trabajo. Al fondo guarda el escrito del cliente… desde 1998.', 'cr-cola'),
    ('Vuelva Mañana', 'Criatura menor de la Sala de Contrataciones', [10, 10, 12, 8, 10, 8], 7, 11, 2, 'Firmeza, 1d4 de daño, alcance 2.',
     '<b>Empuja.</b> Cada golpe empuja hacia atrás al rival más cercano.', 'Se coloca en los pasillos estrechos para devolver a todos a la entrada.', 'cr-vuelva'),
    ('Fraccionador', 'Criatura menor de la Sala de Contrataciones', [10, 12, 10, 8, 8, 6], 9, 10, 3, 'Agilidad procesal, 1d4 de daño, alcance 1.',
     '<b>Se fracciona.</b> Cada vez que recibe daño y sigue en pie, se parte en una Fracción (Paciencia 3, Defensa 9, 1d3). Como máximo, dos copias.', 'Busca que lo golpeen poco y muchas veces. Conviene derribarlo de un solo golpe fuerte.', 'cr-fraccionador'),
    ('El Requisito Imposible', 'Jefe de la Sala de Contrataciones', [14, 10, 16, 12, 12, 8], 32, 12, 2, 'Firmeza, 1d6 de daño, alcance 2.',
     '<b>Cambia el requisito.</b> Cada 2 rondas cambia la condición para recibir daño y suma un Formalismo a la sala. Los golpes que no cumplen quedan observados.', 'Se rodea de Formalismos. La Especialista Mendoza o un golpe observado revelan el requisito vigente.', 'cr-requisito'),
    ('Tramitador', 'Criatura menor de la red del Coimero', [12, 12, 12, 8, 10, 12], 9, 12, 3, 'Agilidad procesal, 1d6 de daño, alcance 1.',
     '<b>Conoce a alguien.</b> Una vez por sala puede llamar a un Expediente olvidado para que lo ayude.', 'Ataca al abogado más débil y huye cuando le queda poca Paciencia.', 'cr-tramitador'),
    ('El Coimero', 'Tentación (no combate)', [10, 12, 10, 12, 14, 18], '—', '—', '—', 'No ataca.',
     '<b>Ofrece agilizar.</b> Su oferta recupera toda la Paciencia del equipo a cambio de 10 puntos en el expediente disciplinario.', 'Sonríe, insiste y desaparece. Si lo denuncian, su red espera en la trastienda.', 'cr-coimero'),
    ('Expediente olvidado', 'Criatura menor del Archivo Central', [12, 8, 14, 6, 8, 6], 10, 11, 2, 'Firmeza, 1d6 de daño, alcance 1.',
     '<b>Folios sueltos.</b> Al caer, deja 1d4 hojas que pueden contener una pista sobre el caso (prueba de Revisar el expediente, dificultad 12).', 'Ataca en grupos de tres en los pasillos estrechos del Archivo.', 'cr-expediente'),
]
pagina('Bestiario', f'''
<h1>Bestiario</h1>
<p class="intro">Las criaturas del sistema nacieron de los trámites que nadie atendió. Cada ficha indica sus seis características, su Paciencia, su Defensa, su Movimiento y el rasgo que la hace única. Los números son los del prototipo y pueden ajustarse.</p>
<div class="bestias tres">{''.join(bloque(*b[:9], imagen=b[9]) for b in B[0:3])}</div>''')
for k in (3, 6):
    grupo = B[k:k + 3]
    pagina('Bestiario', '<div class="bestias tres">' + ''.join(bloque(*b[:9], imagen=b[9], nivel=2 if 'Jefe' in b[1] else 1) for b in grupo) + '</div>')
pagina('Bestiario', '<div class="bestias tres">' + ''.join(bloque(*b[:9], imagen=b[9]) for b in B[9:11]) + '</div>' + tabla(['Nivel del equipo', 'Sala de combate sugerida', 'Jefe sugerido'], [
    ['1', '3 criaturas menores (Paciencia total 15 a 25)', 'Paciencia 30, Defensa 12'],
    ['2–4', '3 a 4 criaturas menores, una con rasgo peligroso', 'Paciencia 40 a 55, Defensa 13'],
    ['5–8', '4 criaturas menores o 2 con Paciencia 15', 'Paciencia 60 a 80, Defensa 14'],
    ['9 o más', 'Grupos mixtos con Tramitadores y Expedientes olvidados', 'La Carga Procesal (Paciencia 120)'],
]) + caja('Escalar a las criaturas', '<p>Para un equipo de más nivel, suma 4 de Paciencia y 1 de Defensa a cada criatura menor por cada 2 niveles por encima del primero, y aumenta un paso su dado de daño (1d4 pasa a 1d6). A los jefes, súmales 10 de Paciencia por nivel del equipo.</p>'))

pagina('Bestiario', f'''
{img('esc-dragon', 'ancho alto', 'La Carga Procesal')}
<div class="dos">
<h1 class="sin-margen">La Carga Procesal</h1>
<p class="bl-tipo">Gran dragón, jefe final de la campaña</p>
<p class="capital">Nadie la ha visto completa. Sus escamas son carpetas de colores; sus alas, actas sin firmar; su aliento, una lluvia de sellos de «NO HA LUGAR». Duerme enroscada alrededor de la torre de expedientes del Archivo Central y crece con cada escrito que no se provee.</p>
<p>La Carga Procesal no aparece en la aplicación del prototipo: es la meta de la campaña completa. Estos números son una propuesta para el juego en mesa.</p>
<h3>Fases del combate final</h3>
<p><b>1. El despertar.</b> La Carga Procesal sigue enroscada en la torre; solo ataca con la cola y lanza Expedientes olvidados.</p>
<p><b>2. El vuelo.</b> Al perder un tercio de su Paciencia, despliega las alas y usa el Aliento de sellos.</p>
<p><b>3. La resolución.</b> Con menos de la mitad, la torre empieza a caer. Cada ronda, un abogado puede intentar uno de los tres trámites finales.</p>
<p class="cita">«Ese escrito no se perdió solo, doctores. Algo se lo comió.» Doctora Valdivia</p>
{bloque('La Carga Procesal', 'Jefe final', [20, 10, 22, 16, 14, 12], 120, 16, 2, 'Firmeza, 2d8 de daño, alcance 2. <i>Aliento de sellos</i> (cada 3 rondas): todos los abogados en una línea de 4 casillas pierden 2d6 de Paciencia; prueba de Agilidad procesal 15 para recibir la mitad.',
        '<b>Se alimenta de lo pendiente.</b> Recupera 5 de Paciencia por cada criatura menor que siga en pie al final de la ronda. <b>Escamas de expediente:</b> la primera vez que baja de la mitad de Paciencia, suelta tres Expedientes olvidados.',
        'Primero derriba al Administrativista (teme al silencio). El equipo gana si la desestima o si resuelve tres trámites de dificultad 18 en la sala, uno por ronda, mientras los demás la contienen.', nivel=10)}
</div>''')

# ═════════════════════════ 26-28. ZONAS ═════════════════════════
def zona(nombre, intro, encuentros, semillas, criaturas):
    enc = tabla(['d20', 'Encuentro'], encuentros, 'aleat')
    sem = '<ol class="pasos">' + ''.join(f'<li>{s}</li>' for s in semillas) + '</ol>'
    return (f'<h2>{nombre}</h2><p class="capital">{intro}</p>{enc}<h3>Semillas de aventura</h3>{sem}'
            f'<p class="dato"><b>Criaturas:</b> {criaturas}</p>')


pagina('Las zonas', f'''
<h1>Las zonas de Expedientia</h1>
<div class="dos">
{zona('Zona 1: Mesa de Partes', 'El primer piso del Palacio de Justicia: un pasillo infinito de ventanillas numeradas, un letrero de turnos que avanza cuando quiere y una cola que serpentea hasta perderse en la penumbra. Aquí empieza casi todo caso, y aquí terminan muchos.',
      [['1–4', 'Tres Formalismos bloquean el pasillo.'], ['5–7', 'Dos Plazos que Vencen y una Notificación Defectuosa.'], ['8–10', 'Un trámite: «falta copia legalizada del original de la copia».'],
       ['11–13', 'Rosita cierra la ventanilla 3. Audiencia improvisada con el Cliente.'], ['14–16', 'La cafetería del juzgado, abierta de milagro.'],
       ['17–18', 'Un abogado de pasillo: el Coimero.'], ['19–20', 'La cola se agita. La Cola Infinita despierta.']],
      ['El escrito fantasma: el cliente jura que presentó su escrito, pero no aparece.', 'Plazo en llamas: quedan dos horas para apelar y la ventanilla está por cerrar.', 'El turno 0947 lleva tres días en pantalla. Alguien lo está reteniendo.'],
      'Formalismo, Plazo que Vence, Notificación Defectuosa, Usuario en cola. Jefe: La Cola Infinita.')}
<div class="salto"></div>
{zona('Zona 2: Sala de Contrataciones', 'Una sala amplia con mesas largas, sobres cerrados y un reloj que marca la hora del cierre de propuestas. Aquí las empresas pequeñas intentan competir con requisitos hechos a la medida de nadie.',
      [['1–4', 'Dos Vuelva Mañana custodian la mesa de recepción.'], ['5–8', 'Un Fraccionador escondido entre los recibos.'], ['9–11', 'Un trámite: las bases piden experiencia de 40 años a una empresa de 3.'],
       ['12–14', 'La Especialista Mendoza, abrumada, pide ayuda para ordenar folios.'], ['15–17', 'La librería jurídica en el pasillo.'],
       ['18–19', 'Un Tramitador ofrece «la buena pro asegurada».'], ['20', 'El Requisito Imposible se pone la corona.']],
      ['La buena pro imposible: una empresa pequeña quiere ganar con requisitos que nadie cumple.', 'Alguien partió una compra grande en cien compras pequeñas.', 'Las bases cambiaron anoche y nadie avisó.'],
      'Vuelva Mañana, Fraccionador, Fracción, Formalismo. Jefe: El Requisito Imposible.')}
</div>''')

pagina('Las zonas', f'''
{img('esc-archivo', 'ancho', 'El Archivo Central')}
<p class="pie-img">El sótano del Archivo Central, donde los expedientes olvidados abrieron los ojos por primera vez.</p>
<div class="dos">
{zona('Zona 3: El Archivo Central', 'Kilómetros de estantes bajo el Palacio. La luz no llega más allá del tercer pasillo y el polvo tiene su propio clima. Aquí duerme la Carga Procesal, y desde aquí escapan sus criaturas.',
      [['1–5', 'Tres Expedientes olvidados en un pasillo estrecho.'], ['6–8', 'Un derrumbe de cajas: prueba de Agilidad procesal 13 o 1d6 de daño.'], ['9–11', 'Un índice escrito a mano que solo entiende Don Eusebio.'],
       ['12–14', 'Ojos rojos en la oscuridad: un Plazo que Vence de 1998.'], ['15–17', 'Un rincón seco con un termo olvidado: descanso.'],
       ['18–19', 'La red del Coimero vende expedientes «recuperados».'], ['20', 'Un temblor. La Carga Procesal se da vuelta en su sueño.']],
      ['Un título universitario perdido hace veinte años.', 'Un expediente que se escapa cada vez que alguien lo encuentra.', 'La primera pista de dónde nació la Carga Procesal.'],
      'Expediente olvidado, Plazo que Vence, Tramitador. Jefe final: La Carga Procesal.')}
</div>''')

pagina('Las zonas', f'''
<div class="dos">
{zona('Zona 4: El Pasaje del Coimero', 'Una galería angosta detrás del Palacio, llena de puestos de fotocopias, sellos y «asesorías». Todo se ofrece rápido y barato. Nada es lo que parece. Solo se entra a pelear si el equipo denunció al Coimero.',
      [['1–5', 'Dos Tramitadores y un Expediente olvidado.'], ['6–9', 'Un puesto vende «sellos originales». Prueba de Criterio 14 para notar que son falsos.'], ['10–12', 'Una periodista pide protección: sabe quién está detrás.'],
       ['13–15', 'El Suboficial Quispe llega si la Reputación es 70 o más.'], ['16–18', 'Una trampa: el pasaje se cierra; prueba de Firmeza 15.'],
       ['19–20', 'El Coimero en persona ofrece una última tentación, el doble de grande.']],
      ['Una red de tramitadores vende turnos de Mesa de Partes.', 'El cliente del caso aceptó una coima a espaldas del estudio.', 'Un expediente disciplinario aparece en la mesa del estudio.'],
      'Tramitador, Expediente olvidado. Aliado posible: Suboficial Quispe.')}
<div class="salto"></div>
<h2>La cafetería del juzgado</h2>
<p class="capital">Un mostrador, seis bancos y el mejor pan con chicharrón de la ciudad. Es zona neutral: ninguna criatura entra. Descansar aquí devuelve a todos la mitad de su Paciencia máxima y consume 15 minutos del reloj. Don Eusebio suele estar en el último banco y siempre tiene un dato.</p>
<h2>La librería jurídica</h2>
<p>Estantes hasta el techo, códigos de todos los años y una vendedora que sabe exactamente qué libro necesitas antes de que lo pidas. En la esquina, el puesto del Ekeko de los Honorarios, cargado de miniaturas de todo lo que un abogado desea: un código, un sello, una toga, una laptop que no se cuelga.</p>
{tabla(['d6', 'Rumores de la cafetería'], [['1', 'Dicen que en el sótano hay un expediente de 1950 que todavía espera respuesta.'], ['2', 'La ventanilla 7 atiende rápido los jueves, pero nadie sabe por qué.'], ['3', 'Alguien vio al Coimero almorzando con un Tramitador.'], ['4', 'El Requisito Imposible le teme a la Especialista Mendoza.'], ['5', 'La Carga Procesal ronca: por eso tiemblan los estantes.'], ['6', 'Hay una Mesa de Partes virtual. Nadie la ha encontrado todavía.']], 'aleat')}
{caja('Crear tus propias zonas', '<p>Usa la plantilla de zona de la página 34. Una buena zona tiene una imagen fuerte (una sala, un olor, un sonido), entre tres y cinco criaturas propias, un jefe con una mecánica que obligue a pensar y al menos un personaje no jugador honesto que ayude al equipo.</p>')}
</div>''')

# ═════════════════════════ 29-30. AVENTURA ═════════════════════════
pagina('Aventura', f'''
<h1>Aventura: El primer caso</h1>
<div class="dos">
<p class="capital">Esta aventura introduce a los jugadores en Expedientia. Está pensada para personajes de nivel 1 y ocupa tres sesiones, una por expediente. Es el mismo Acto 1 que cuenta la aplicación.</p>
<h2>Prólogo</h2>
<p class="cita">«República de Expedientia. Un país inventado, con instituciones sospechosamente familiares. Una noche, en el sótano del Archivo Central, los expedientes olvidados empezaron a moverse.»</p>
<p>Lee el prólogo en voz alta. Luego presenta a Don Eusebio, que pasa por el estudio con su termo: «Treinta años en este juzgado, doctor, y nunca había visto caminar a un expediente. Hasta el martes». La Doctora Valdivia deja una advertencia: «Cierre casos, gane reputación y no acepte atajos». Gonzabio golpea el escritorio cojo: «Socio, el cuarto será chico, pero el estudio es nuestro». Lion, sin levantar la vista de un anexo, añade: «Y yo leo la letra pequeña. Ahí se esconden los monstruos».</p>
<h2>Caso 1: El escrito fantasma</h2>
<p><b>Planteamiento.</b> El Cliente jura que presentó su escrito, pero no aparece en el sistema. «Tenía el cargo… bueno, tenía el cargo hasta el lunes.»</p>
{caja('Para leer en voz alta', '<p class="cita">El Cliente entra sin tocar. Trae una bolsa de plástico con papeles, un paraguas que no necesita y la cara de alguien que acaba de ver cerrarse una ventanilla. «Doctor, yo presenté el escrito. Lo juro por mi madre. Tenía el cargo… bueno, tenía el cargo hasta el lunes.»</p>', 'real')}
<p><b>Desarrollo.</b> Cuatro o cinco salas en la Mesa de Partes: un combate de entrada contra tres Formalismos, un trámite, una audiencia en la que el Cliente cambia la fecha de presentación, la cafetería y, con suerte, el Coimero.</p>
<p><b>Jefe.</b> Rosita señala el fondo de la cola: «Al fondo está su escrito… y algo bastante más grande». La Cola Infinita, con 30 de Paciencia, suma un Usuario en cola por turno. Lion nota que la numeración de la cola está salteada: donde falta un número hay un escrito escondido. Un abogado que llegue a esa casilla y supere Revisar el expediente (dificultad 13) da ventaja a todo el equipo durante una ronda.</p>
<p><b>Cierre.</b> Si ganan, el escrito aparece «con sello y todo». Gonzabio revisa los cargos de la semana: faltan cuarenta escritos, y todos pasaron por el Archivo Central.</p>
<h2>Caso 2: Plazo en llamas</h2>
<p><b>Planteamiento.</b> Quedan dos horas para presentar una apelación y la ventanilla está por cerrar. El expediente tiene <b>reloj</b>: 120 minutos.</p>
<p><b>Desarrollo.</b> Las salas consumen tiempo; los Plazos que Vencen abundan. La audiencia es clave: revisar el expediente puede encontrar una fecha que dé 20 minutos más.</p>
<p><b>Jefe.</b> La Cola Infinita de nuevo, ahora con el reloj en contra. Si se acaba el tiempo, el caso es «improcedente por extemporáneo».</p>
{caja('Para leer en voz alta', '<p class="cita">El reloj de pared marca las tres. La ventanilla cierra a las cinco. Entre ustedes y la ventanilla hay un pasillo, dos Plazos que Vencen con los números encendidos y una cola que empieza a moverse como si respirara.</p>', 'real')}
<h3>Salas sugeridas del caso 2</h3>
{tabla(['Sala', 'Contenido'], [['1', 'Combate: dos Plazos que Vencen y un Formalismo.'], ['2', 'Trámite: «el formato de apelación cambió ayer».'], ['3', 'Audiencia: el Cliente no recuerda cuándo lo notificaron.'], ['4', 'Cafetería o Coimero, a elección del Relator.'], ['5', 'Jefe: La Cola Infinita, con el reloj en contra.']], 'aleat')}
<div class="vineta-c"><img src="img/cr-cola.jpg"><p class="pie-img">La Cola Infinita custodia el escrito del cliente desde 1998.</p></div>
</div>''')

pagina('Aventura', f'''
<div class="dos">
<h2>Caso 3: La buena pro imposible</h2>
<p class="capital"><b>Planteamiento.</b> Una empresa pequeña quiere ganar un proceso de selección, pero las bases piden requisitos que nadie cumple. La acción se traslada a la Sala de Contrataciones.</p>
<p><b>Desarrollo.</b> Vuelva Mañana y Fraccionadores. La Especialista Mendoza aparece si la Reputación es 70 o más y revela el requisito vigente del jefe.</p>
{caja('Para leer en voz alta', '<p class="cita">La Sala de Contrataciones huele a sobres cerrados y café frío. Sobre la mesa principal, un pergamino con corona los mira por encima de una lista de requisitos que crece mientras la leen. «Experiencia mínima: cuarenta años. Su empresa tiene tres. Observado.»</p>', 'real')}
<p><b>Jefe.</b> El Requisito Imposible, con 32 de Paciencia, cambia la condición para recibir daño cada dos rondas y llama Formalismos. El equipo debe observar qué golpes funcionan y adaptarse. Si Lion está en la sala, ya leyó el anexo 7: puede revelar el primer requisito sin esperar a Mendoza.</p>
<p><b>Cierre.</b> Si ganan, la empresa compite en igualdad de condiciones. Mendoza encuentra entre las bases un sello antiguo del Archivo Central: alguien está alimentando a la Carga Procesal desde adentro.</p>
<h2>Si aparece el Coimero</h2>
<p>En cualquiera de los tres casos puede aparecer el Coimero. Si el equipo lo denuncia, añade una sala extra: la red del Coimero en la trastienda, con dos Tramitadores y, si la Reputación es alta, el Suboficial Quispe como aliado. Es la sala más difícil del caso y la que mejor paga: 50 de honorarios y Reputación +10.</p>
<h2>Después del primer caso</h2>
<p>Lion cruza las firmas de los tres casos y encuentra un mismo sello, el del Archivo Central. Gonzabio cierra el maletín: «Las tres pistas apuntan al mismo sótano, socio. Bajamos al Archivo Central». El Acto 1 termina con una certeza: la Carga Procesal existe y está despertando. Los siguientes actos llevan al equipo al Archivo Central, al Pasaje del Coimero y, finalmente, a la torre de expedientes donde duerme el dragón.</p>
{caja('Lo que dicen al perder', '<p>Si el equipo cae, no hay drama: Don Eusebio se encoge de hombros. «Archivado, doctor. No se preocupe: aquí lo archivado también resucita.» El caso puede reabrirse en la siguiente sesión, con las criaturas un poco más fuertes.</p>')}
{caja('Recompensas del Acto 1', '<p>Por cada caso fundado: 60 de honorarios por el jefe, 15 por cada sala de combate y la experiencia correspondiente. Al cerrar los tres casos, los abogados deberían estar cerca del nivel 2 y el estudio, con una Reputación entre 55 y 70 si no aceptó atajos.</p>', 'ejemplo')}
</div>''')

# ═════════════════════════ REGLAS RÁPIDAS ═════════════════════════
pagina('Reglas rápidas', f'''
<h1>Reglas rápidas</h1>
<p class="intro">Una página para tener abierta sobre la mesa. Todo lo demás está en el manual.</p>
<div class="dos rapidas">
{caja('La tirada', '<div class="formula">d20 + modificador + competencia ≥ dificultad</div><p>Modificador = (valor − 10) ÷ 2, hacia abajo. Competencia: +2 en niveles 1 a 4, +1 cada 4 niveles.</p><p><b>Dificultades:</b> 5 muy fácil · 10 fácil · 12 normal · 15 media · 20 difícil · 25 muy difícil · 30 casi imposible.</p>')}
{caja('Fundado e Improcedente', '<p><b>20 natural:</b> Fundado. En ataque acierta siempre y duplica los dados de daño. <b>1 natural:</b> Improcedente. En ataque falla siempre.</p>')}
{caja('Ventaja y desventaja', '<p>Ventaja: dos d20, el mayor. Desventaja: dos d20, el menor. Si hay ambas, se anulan. No se acumulan.</p>')}
{caja('Prueba jurídica', '<p>Máximo 3 por expediente. 20 segundos, 4 opciones. Acierto: ventaja en la siguiente tirada. Fallo o tiempo agotado: tirada normal. Solo preguntas verificadas en fuente oficial.</p>', 'real')}
{caja('El turno', '<p>Iniciativa: d20 + Agilidad procesal. Cada turno: <b>moverse</b> (Movimiento en casillas; los rivales bloquean, los aliados dejan pasar), <b>una acción</b> (atacar, poder, objeto, ayudar) y <b>una reacción</b> por ronda si un poder lo permite.</p>')}
{caja('Atacar y caer', '<p>Objetivo dentro del alcance. d20 + mod. + competencia contra Defensa. Daño: dado de la especialidad + mod. Paciencia 0: el abogado cae; la criatura queda desestimada.</p>')}
{caja('Estados', '<p><b>Inmovilizado:</b> 2 turnos sin moverse ni actuar. <b>Observado:</b> el golpe no hace daño. <b>Silencio:</b> 2 turnos sin actuar con un Administrativista en la sala = sala ganada.</p>')}
{caja('Las salas', '<p><b>Combate</b> · <b>Trámite</b> (Criterio 12; si falla, −2 de Paciencia a todos y 20 min) · <b>Audiencia</b> (Oratoria o Criterio 13) · <b>Cafetería</b> (mitad de la Paciencia, 15 min) · <b>Librería</b> · <b>Coimero</b> (aceptar, rechazar o denunciar) · <b>Jefe</b>.</p>')}
{caja('Las seis especialidades', tabla(['Especialidad', 'Pac.', 'Def.', 'Mov.', 'Ataque'], [[e['n'], e['pac'].split(' ')[0], str(e['defe']), str(e['mov']), e['ataque'].split(' de daño')[0].replace(',', '')] for e in ESP]))}
{caja('Experiencia y niveles', '<p>100 por criatura menor desestimada, 300 por jefe, 50 por sala superada sin pelear y 100 extra si el caso es fundado.</p><p><b>Umbrales:</b> nivel 2 con 300 · 3 con 900 · 4 con 2700 · 5 con 6500 · 6 con 14 000. Cada nivel suma 6 de Paciencia y, cada 4, un punto de competencia.</p>')}
{caja('Recompensas', '<p>Honorarios: 15 por combate, 60 por jefe, 50 por la red del Coimero, 10 o 20 por audiencia. Reputación inicial 50; con 70 o más llegan los aliados honestos. Casación: una vez por expediente, repetir una tirada con dificultad 18.</p>', 'ejemplo')}
</div>''')

# ═════════════════════════ 31-32. PLANTILLAS ═════════════════════════
lineas = lambda n: ''.join('<div class="linea"></div>' for _ in range(n))
hab_hoja = ''.join(f'<div class="hh"><span class="cuad"></span>{h[0]} <i>({h[1][:3]})</i><span class="lin"></span></div>' for h in habilidades)
pagina('Plantillas', f'''
<h1 class="hoja-t">Hoja de abogado</h1>
<div class="hoja">
  <div class="hoja-cab">
    <div><label>Nombre</label><div class="linea"></div></div>
    <div><label>Especialidad</label><div class="linea"></div></div>
    <div><label>Nivel y título</label><div class="linea"></div></div>
    <div><label>Universidad</label><div class="linea"></div></div>
  </div>
  <div class="hoja-cuerpo">
    <div class="hoja-col">
      {''.join(f'<div class="atrib"><div class="atrib-n">{a}</div><div class="atrib-v"></div><div class="atrib-m">mod.</div></div>' for a in ATR)}
    </div>
    <div class="hoja-col2">
      <div class="contadores">
        <div><label>Paciencia</label><div class="grande"></div><small>actual / máxima</small></div>
        <div><label>Defensa</label><div class="grande"></div></div>
        <div><label>Movimiento</label><div class="grande"></div></div>
        <div><label>Competencia</label><div class="grande"></div></div>
      </div>
      <label>Ataque (característica, daño, alcance)</label>{lineas(2)}
      <label>Poderes y recargas</label>{lineas(4)}
      <label>Objetos</label>{lineas(3)}
      <div class="contadores"><div><label>Honorarios</label><div class="grande"></div></div><div><label>Experiencia</label><div class="grande"></div></div><div><label>Reputación del estudio</label><div class="grande"></div></div></div>
    </div>
    <div class="hoja-col3">
      <label>Habilidades de oficio</label>
      {hab_hoja}
      <label>Personalidad y manías</label>{lineas(4)}
    </div>
  </div>
</div>''', 'plantilla')

pagina('Plantillas', f'''
<h1 class="hoja-t">Plantilla de zona</h1>
<div class="hoja">
  <div class="hoja-cab"><div><label>Nombre de la zona</label><div class="linea"></div></div><div><label>Nivel sugerido</label><div class="linea"></div></div></div>
  <div class="hoja-cuerpo dos-z">
    <div>
      <label>Descripción (lo que se ve, se oye y se huele)</label>{lineas(6)}
      <label>Mapa o esquema</label><div class="recuadro"></div>
      <label>Personajes no jugadores</label>{lineas(4)}
    </div>
    <div>
      <label>Tabla de encuentros (d20)</label>
      {''.join(f'<div class="hh"><b>{r}</b><span class="lin"></span></div>' for r in ['1–4', '5–7', '8–10', '11–13', '14–16', '17–18', '19–20'])}
      <label>Criaturas (Paciencia · Defensa · Ataque · Rasgo)</label>{lineas(7)}
      <label>Jefe y su mecánica</label>{lineas(4)}
      <label>Semillas de aventura</label>{lineas(4)}
    </div>
  </div>
</div>
<p class="pie-final">Expedientes y Dragones · Manual del juego · © 2026 Leonel Díaz · Se permite fotocopiar las plantillas para uso personal.</p>
''', 'plantilla')

# ═════════════════════════ CONTRAPORTADA ═════════════════════════
pagina('', f'''
<div class="contra">
  {img('d20', 'contra-d20')}
  <h2>Los expedientes olvidados se están moviendo.</h2>
  <p>En la República de Expedientia, los formularios tienen colmillos, los plazos explotan y las colas no terminan nunca. Tu estudio acaba de abrir frente a la Corte y el primer cliente ya está en la puerta.</p>
  <p>Este manual contiene todo lo necesario para jugar: creación de abogados con seis especialidades, el sistema del d20 con la prueba jurídica, combate táctico, un bestiario de criaturas del sistema, cuatro zonas, tablas para improvisar y la aventura completa <i>El primer caso</i>.</p>
  <div class="contra-roles">{''.join(f'<div class="rol-img"><img src="img/{e["img"]}.jpg"></div>' for e in ESP)}</div>
  <p class="contra-lema">Litiga, investiga y vence al dragón de la burocracia.</p>
  <p class="contra-autor">Leonel Díaz · Arequipa, 2026</p>
</div>''', 'contraportada')


# ═════════════════════════ ENSAMBLAR ═════════════════════════
def html():
    cuerpo = ''
    n = 0
    for i, (sec, contenido, clase) in enumerate(paginas):
        n = i + 1
        especial = 'portada' in clase or 'contraportada' in clase
        cab = '' if especial else f'<div class="cinta">{sec}</div><div class="num">{n}</div>'
        cuerpo += f'<section class="pag {clase}" data-n="{n}">{cab}<div class="contenido">{contenido}</div></section>\n'
    css = open(os.path.join(AQUI, 'estilo.css'), encoding='utf-8').read()
    fuentes = open(os.path.join(AQUI, 'fuentes', 'fuentes.css'), encoding='utf-8').read()
    return f'''<!doctype html><html lang="es"><head><meta charset="utf-8"><title>Expedientes y Dragones · Manual del juego</title>
<meta name="author" content="Leonel Díaz"><style>{fuentes}{css}</style></head><body>{cuerpo}</body></html>'''


if __name__ == '__main__':
    with open(os.path.join(AQUI, 'manual.html'), 'w', encoding='utf-8') as f:
        f.write(html())
    print(len(paginas), 'páginas')
