# Servidor de partidas

ASP.NET Core 8 con WebSocket y PostgreSQL. Usa `logica/Expedientia.Logica` para decidir cada turno: el servidor es la autoridad y el celular solo muestra.

## API

| Método | Ruta | Qué hace |
| --- | --- | --- |
| GET | `/salud` | Estado del servidor |
| GET | `/api/casos` | Casos disponibles |
| POST | `/api/jugadores` | Crea un jugador (`{"alias": "..."}`) y devuelve su `token` |
| GET | `/api/yo` | Estudio y últimos resultados del jugador |
| POST | `/api/partidas` | Crea una partida (`caso_id`, `especialidad`, `bots`, `semilla` opcional) |
| GET | `/api/partidas/{id}` | Estado de la partida |
| POST | `/api/partidas/{id}/unirse` | Otro jugador toma el personaje de un bot |
| POST | `/api/partidas/{id}/acciones` | Ejecuta una acción |
| WS | `/ws/partidas/{id}?token=` | Mismas acciones en tiempo real para todo el equipo |
| GET | `/panel/` | Panel de preguntas (usuario `panel`, clave `Juego:ClavePanel`) |

El token va en la cabecera `X-Token`. Los nombres JSON van en `snake_case`.

Acciones (`tipo`): `avanzar`, `mover` (`columna`, `fila`), `atacar` (`objetivo`, `opcion: "escrito"` para usar el escrito pre-firmado), `habilidad` (`habilidad`, `objetivo`), `apelar` (`objetivo`), `casacion` (`objetivo`), `terminarTurno`, `piloto` (el bot juega tu turno), `tramite`, `audiencia` (`opcion: confrontar | revisar`), `descansar`, `comprar` (`objeto`), `salirTienda`, `coimero` (`decision: aceptar | rechazar | denunciar`), `pedirPregunta`, `responder` (`indice`).

Si un jugador no actúa en 30 segundos, un bot juega su turno.

## Configuración

| Variable | Uso |
| --- | --- |
| `ConnectionStrings__Juego` | PostgreSQL. Sin ella, todo queda en memoria |
| `Juego__PermitirBorradores` | `true` solo en desarrollo: deja entrar preguntas en borrador, marcadas como tales |
| `Juego__CarpetaPreguntas` | Carpeta con las fichas JSON que se cargan al arrancar |
| `Juego__ClavePanel` | Clave del panel; sin clave, el panel queda apagado |

## Correr en local

```
docker compose --env-file .env up -d --build
```

con un `.env` que tenga `CLAVE_BASE=...` y `CLAVE_PANEL=...`.
