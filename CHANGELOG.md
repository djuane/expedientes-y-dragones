# Cambios

## v0.10.2 (11/11/2026)
- Nuevo personaje de la trama: **Lion**, abogado investigador del estudio (terno gris, corbata roja, lentes, cabello negro). Lee la letra pequeña y los anexos. Aparece en el prólogo y en cada caso, y descubre el sello del Archivo Central que une los tres casos.
- Retrato provisional de Lion, generado con IA.

## v0.10.1 (11/11/2026)
- Nuevo personaje de la trama: **Gonzabio**, socio fundador del estudio. Arma los expedientes y los defiende como un escudo. Aparece en el prólogo y en la llegada, el jefe y la victoria de los casos, y es quien sigue la pista de los escritos perdidos hasta el Archivo Central.
- Retrato provisional de Gonzabio, generado con IA (`arte/LEEME.md`).

## v0.10.0 (11/11/2026)
Gráficos y tipografía con acabado de ilustración.

- **Tipografía:** los textos usan Alegreya y los títulos Cinzel, ambas con licencia libre OFL (ver `Resources/fuentes/LICENCIAS.txt`). Se usan sus variantes reales de negrita y cursiva. Los títulos grandes llevan contorno.
- **Logotipo propio:** letras doradas con relieve en Cinzel Decorative, filete y un sello de lacre con la balanza.
- **Portada nueva** (1024 × 1420, la proporción del celular): un dragón hecho de expedientes se enrosca en la Torre de Expedientes y lanza un torbellino de papeles. El equipo lo enfrenta de espaldas, con la pluma de la Civilista encendida. Detrás se ven el volcán, la luna y los arcos de sillar.
- **Estilo de dibujo animado clásico en todo el arte:** colores más saturados, sombras de corte nítido en tres tonos, brillo en los bordes y medallones con aro metálico y resplandor del bando. Los rótulos de las escenas usan la tipografía del juego.

## v0.9.0 (10/11/2026)
Optimización del código y mejora de gráficos con técnicas recursivas.

**Tablero y motor**
- Caminos reales: un rival bloquea el paso y un aliado deja pasar (pero no se puede terminar en su casilla). Rodear un obstáculo cuesta los pasos reales. Se calcula con un relleno recursivo con poda, sobre un mapa de 48 casillas que se reutiliza.
- El motor ya no repite búsquedas: un mapa de ocupación reemplaza las consultas sobre todos los combatientes, las fichas se ubican por id en un diccionario y el bot elige su casilla en una sola pasada.
- Medido con 9,000 partidas simuladas: de unas 2,300 a unas 4,800 partidas por segundo (2.1 veces más rápido) y de 368 KB a 135 KB de memoria por partida (63 % menos). El balance de los tres casos no cambió.

**Gráficos**
- Portada con cordillera y volcanes de crestas fractales (desplazamiento del punto medio) y un molle dibujado como árbol fractal.
- Muros de sillar con bloques de tamaños variados, partidos por subdivisión recursiva.
- Efectos nuevos: confeti al sacar ¡Fundado! y al ganar el caso, papelitos cuando cae un enemigo y chispas cuando explota el Plazo que Vence. Las piezas y los textos flotantes se reciclan, sin crear ni destruir objetos en cada golpe.

**Pruebas**
- 108 de lógica (5 nuevas de caminos) y 9 del servidor.

## v0.8.0 (10/11/2026)
Revisión general de gráficos, trama y jugabilidad.

**Jugabilidad**
- Cada enemigo tiene la mecánica del GDD: el Plazo que Vence explota en su tercer turno (inmovilizado, su plazo se suspende); la Notificación Defectuosa ataca a quien no corresponde; el Formalismo recibe la mitad del daño sin el «requisito correcto» (ventaja).
- Atacar a un enemigo inmovilizado es con ventaja (SRD), así Medida cautelar y Queja preparan el golpe.
- Nueva habilidad de la Civilista: Nulidad (reacción que anula la última acción enemiga y devuelve la paciencia quitada).
- Caso 3 «La buena pro imposible» en la Sala de Contrataciones, con Vuelva Mañana, Fraccionador y el jefe El Requisito Imposible, que cambia lo que exige cada 2 rondas.
- Con Reputación alta aparecen aliados: la Especialista Mendoza revela el requisito del jefe y el Suboficial Quispe pelea contra la red del Coimero.
- Dificultad ajustada con 1,000 partidas simuladas por caso y tamaño de equipo; los jefes escalan con el equipo.
- El turno termina solo cuando ya no queda acción ni movimiento; los botones muestran la recarga de las habilidades.

**Trama**
- Prólogo del Acto 1 y guion de cada caso: llegada, jefe, epílogo de victoria o derrota y descripciones de sala que varían entre partidas.
- Los colegas bot tienen nombre (Dra. Arce y Dr. Benítez).

**Gráficos y audio**
- 30 dibujos originales: fichas de personajes y monstruos, retratos de la trama, escenas de cada sala, portada, dado d20 y sello.
- Animaciones de movimiento, ataque, daño, caída, explosión del plazo y textos flotantes; cuadro de diálogo con retrato.
- Nueva portada y pantalla de preparación del expediente; botones redondeados.
- Efectos de sonido y tema de menú sintetizados por código (sin archivos de terceros), con botón para silenciar.
- 103 pruebas automáticas en la lógica y 9 en el servidor.

## v0.6.0 (10/11/2026)
- Motor de partida compartido (Expedientia.Logica.Sesion): el servidor y el celular juegan con las mismas reglas y el mismo protocolo de acciones.
- Cliente Unity 6: menú del estudio, tablero 6 × 8 vertical, dado animado con «Fundado» e «Improcedente», prueba jurídica de 20 segundos con corrección y enlace oficial, salas de trámite, audiencia, librería, cafetería y Coimero, y sentencia.
- Modo sin conexión (motor en el celular, progreso guardado) y modo en línea (servidor de partidas).
- Script de compilación del APK de Android para Unity en modo batch.
- 97 pruebas.

## v0.5.0 (10/11/2026)
- Script 28-vm-juego.sh: crea la VM 113 en Proxmox con Debian 13 y Docker y despliega el servidor; modos --desplegar, --estado y --revertir, con respaldo previo.

## v0.4.0 (10/11/2026)
- Servidor de partidas ASP.NET Core 8: API, WebSocket para jugar en equipo, turnos de 30 segundos con bot de reemplazo.
- Prueba jurídica servida por el servidor (la respuesta correcta no viaja al celular) con base legal y enlace al responder.
- Almacén en PostgreSQL (jugadores, estudios, resultados y preguntas) o en memoria.
- Panel de contenido en /panel: no deja marcar como verificada una pregunta sin revisor, fecha, base legal y enlace oficial.
- Dockerfile y docker-compose con PostgreSQL 17.
- Las 3 preguntas de ejemplo del GDD, en borrador.
- 93 pruebas (84 de lógica y 9 del servidor, también contra PostgreSQL).

## v0.3.0 (10/11/2026)
- Partida completa (expediente): 8 a 12 salas con combate, trámite, audiencia, librería, cafetería, Coimero y jefe.
- Jefe La Cola Infinita, que suma usuarios a la fila cada turno.
- Casos 1 y 2 del GDD; reloj del caso «Plazo en llamas» e «Improcedente por extemporáneo».
- Reputación, Expediente Disciplinario oculto, honorarios, experiencia y niveles del SRD.
- Bots más listos (habilidades y apelación); las habilidades de inmovilizar se recargan en 3 rondas.
- Silencio administrativo cuenta solo el silencio voluntario (estar inmovilizado no cuenta).
- 84 pruebas; con bots, el equipo gana cerca del 87 % de los expedientes (se ajustará con los colegas).

## v0.2.0 (10/11/2026)
- Combate por turnos en cuadrícula de 6 × 8 con acción, movimiento y reacción.
- Habilidades de Civilista y Administrativista, pasiva Silencio administrativo y Casación.
- Bot para enemigos y aliados; partidas reproducibles con semilla.
- 67 pruebas automáticas.

## v0.1.0 (10/11/2026)
- Estructura del repositorio, CI y plantilla de preguntas.
- Dados, atributos, competencia, iniciativa y prueba jurídica.
- 51 pruebas automáticas.
