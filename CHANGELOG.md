# Cambios

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
