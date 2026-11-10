# Infraestructura

`28-vm-juego.sh` crea la VM 113 «juego-dev» (Debian 13, Docker, 4 núcleos, 6 GB, 60 GB en local-zfs, IP 192.168.1.252) en el Proxmox del estudio y despliega el servidor de partidas con `docker-compose.yml`.

Sigue las reglas del servidor: respalda antes de cambiar, tiene `--revertir` y `--estado`, y no toca a los demás invitados. Se copia a `D:\aquilina\proxmox-integrum\` y se ejecuta en Git Bash; espera el repositorio en `E:\juegos\expedientes-y-dragones` (o la ruta en `REPO=`).

| Modo | Qué hace |
| --- | --- |
| (sin opción) | Crea la VM, instala Docker y despliega |
| `--desplegar` | Despliega la versión actual del repositorio; respalda la base antes |
| `--estado` | Estado de la VM, contenedores y `/salud` |
| `--revertir` | Respalda la base en la PC y elimina la VM (pide escribir 113) |

Claves: se generan dentro de la VM en `/opt/juego/.env` (permisos 600) y no se copian a ningún documento.

Pendiente para la Fase 4 (seguridad) y para abrir el prototipo a los colegas fuera de la red local: publicar `juego-api.integrilexaqp.com` por el túnel de Cloudflare y el proxy CT 102 (requiere aprobación, porque toca servicios en producción).
