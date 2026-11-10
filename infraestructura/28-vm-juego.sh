#!/usr/bin/env bash
# 28-vm-juego.sh — VM 113 "juego-dev" para el servidor de partidas de Expedientes y Dragones.
# Autor: Leonel Díaz. Se ejecuta desde Git Bash en la PC (DESKTOP-I1TQKFE).
#
# Uso:
#   ./28-vm-juego.sh              crea la VM (Debian 13 + Docker) y despliega el servidor
#   ./28-vm-juego.sh --desplegar  vuelve a desplegar la versión actual del repositorio (respalda la base antes)
#   ./28-vm-juego.sh --estado     muestra el estado de la VM y del servidor
#   ./28-vm-juego.sh --revertir   respalda la base en la PC y elimina la VM 113 (pide confirmación)
#
# No toca ningún otro invitado del servidor. Las claves se generan dentro de la VM, en /opt/juego/.env (chmod 600).
set -euo pipefail

HOST="${HOST:-root@192.168.1.200}"
LLAVE="${LLAVE:-$HOME/.ssh/id_ed25519_proxmox}"
VMID="${VMID:-113}"
NOMBRE="${NOMBRE:-juego-dev}"
IP="${IP:-192.168.1.252}"
PUERTA="${PUERTA:-192.168.1.1}"
USUARIO_VM="juego"
REPO="${REPO:-/e/juegos/expedientes-y-dragones}"
RESPALDOS_PC="${RESPALDOS_PC:-/d/aquilina/proxmox-integrum/respaldos}"
IMAGEN_URL="https://cloud.debian.org/images/cloud/trixie/latest"
IMAGEN="debian-13-genericcloud-amd64.qcow2"
FECHA="$(date +%Y%m%d-%H%M%S)"

SSH_OPC=(-i "$LLAVE" -o BatchMode=yes -o ConnectTimeout=10 -o StrictHostKeyChecking=accept-new)
host() { ssh "${SSH_OPC[@]}" "$HOST" "$@"; }
vm()   { ssh "${SSH_OPC[@]}" -o UserKnownHostsFile="$HOME/.ssh/known_hosts_juego" "$USUARIO_VM@$IP" "$@"; }
paso() { printf '\n== %s\n' "$*"; }
falla() { printf 'ERROR: %s\n' "$*" >&2; exit 1; }

estado() {
  paso "VM $VMID en Proxmox"
  host "qm status $VMID 2>/dev/null || echo 'no existe'; qm config $VMID 2>/dev/null | grep -E '^(name|memory|balloon|cores|net0|ipconfig0|onboot)' || true"
  paso "Servidor de partidas"
  if vm true 2>/dev/null; then
    vm "DC='sudo docker compose'; sudo docker compose version >/dev/null 2>&1 || DC='sudo docker-compose'; cd /opt/juego/actual 2>/dev/null && \$DC -p juego --env-file /opt/juego/.env ps || echo 'sin despliegue'"
    vm "curl -fsS http://127.0.0.1:8080/salud && echo || echo 'el servidor no responde'"
    vm "ls -1t /opt/juego/respaldos 2>/dev/null | head -3 | sed 's/^/respaldo: /' || true"
  else
    echo "Sin acceso SSH a $IP"
  fi
}

preparar_vm() {
  paso "Comprobaciones previas (solo lectura)"
  [ -f "$LLAVE" ] && [ -f "$LLAVE.pub" ] || falla "No encuentro la llave $LLAVE(.pub)"
  [ -d "$REPO/.git" ] || falla "No encuentro el repositorio en $REPO (ajusta REPO=...)"
  host "! qm status $VMID >/dev/null 2>&1" || falla "La VM $VMID ya existe. Usa --estado o --desplegar."
  host "! ping -c1 -W1 $IP >/dev/null 2>&1" || falla "La IP $IP responde: está en uso."
  host "pvesm status --storage local-zfs >/dev/null" || falla "No existe el almacenamiento local-zfs."

  paso "Respaldo de la configuración del host antes del cambio"
  host "set -e; d=/root/respaldos/28-juego-$FECHA; mkdir -p \$d; cp /etc/pve/storage.cfg \$d/; \
        [ -f /etc/pve/firewall/cluster.fw ] && cp /etc/pve/firewall/cluster.fw \$d/ || true; \
        qm list > \$d/qm-list.txt; pct list > \$d/pct-list.txt; echo \$d"
  scp "${SSH_OPC[@]}" "$LLAVE.pub" "$HOST:/root/respaldos/28-juego-$FECHA/llave-pc.pub" >/dev/null

  paso "Imagen Debian 13 (cloud), verificada con SHA512"
  host "set -e; mkdir -p /root/imagenes; cd /root/imagenes; \
        wget -q -N $IMAGEN_URL/$IMAGEN $IMAGEN_URL/SHA512SUMS; \
        grep \" $IMAGEN\$\" SHA512SUMS | sha512sum -c -"

  paso "Creación de la VM $VMID ($NOMBRE) — 4 núcleos, 6 GB (globo 4 GB), 60 GB en local-zfs, onboot 0"
  host "set -e; \
    qm create $VMID --name $NOMBRE --ostype l26 --cores 4 --memory 6144 --balloon 4096 \
      --net0 virtio,bridge=vmbr0,firewall=1 --scsihw virtio-scsi-single --agent enabled=1 \
      --serial0 socket --vga serial0 --onboot 0 --description 'Expedientes y Dragones: servidor de partidas (desarrollo). Script 28.'; \
    qm set $VMID --scsi0 local-zfs:0,import-from=/root/imagenes/$IMAGEN,discard=on,iothread=1; \
    qm resize $VMID scsi0 60G; \
    qm set $VMID --ide2 local-zfs:cloudinit --boot order=scsi0 \
      --ciuser $USUARIO_VM --sshkeys /root/respaldos/28-juego-$FECHA/llave-pc.pub \
      --ipconfig0 ip=$IP/24,gw=$PUERTA --nameserver $PUERTA --ciupgrade 1; \
    qm start $VMID"

  paso "Esperando a que la VM responda por SSH (hasta 4 minutos)"
  for _ in $(seq 1 48); do vm true 2>/dev/null && break; sleep 5; done
  vm true || falla "La VM no respondió por SSH. Revisa la consola de la VM $VMID."

  paso "Docker y agente QEMU dentro de la VM"
  vm "set -e; sudo cloud-init status --wait >/dev/null || true; \
      sudo DEBIAN_FRONTEND=noninteractive apt-get update -q; \
      sudo DEBIAN_FRONTEND=noninteractive apt-get install -y -q docker.io docker-compose qemu-guest-agent curl openssl; \
      sudo systemctl enable --now docker qemu-guest-agent; \
      sudo mkdir -p /opt/juego/respaldos && sudo chown -R $USUARIO_VM: /opt/juego; \
      sudo docker compose version 2>/dev/null || sudo docker-compose version"
}

desplegar() {
  [ -d "$REPO/.git" ] || falla "No encuentro el repositorio en $REPO"
  local version; version="$(git -C "$REPO" describe --tags --always)"
  paso "Despliegue de $version"
  git -C "$REPO" archive --format=tar.gz -o "/tmp/juego-$FECHA.tar.gz" HEAD
  scp "${SSH_OPC[@]}" -o UserKnownHostsFile="$HOME/.ssh/known_hosts_juego" "/tmp/juego-$FECHA.tar.gz" "$USUARIO_VM@$IP:/opt/juego/" >/dev/null
  rm -f "/tmp/juego-$FECHA.tar.gz"
  vm "set -e; DC='sudo docker compose'; sudo docker compose version >/dev/null 2>&1 || DC='sudo docker-compose'; cd /opt/juego; umask 077; \
    if [ ! -f .env ]; then \
      printf 'CLAVE_BASE=%s\nCLAVE_PANEL=%s\nPERMITIR_BORRADORES=true\n' \"\$(openssl rand -hex 24)\" \"\$(openssl rand -hex 12)\" > .env; \
      echo 'Claves nuevas en /opt/juego/.env (no se muestran).'; fi; \
    if sudo docker ps --format '{{.Names}}' | grep -q '^juego-base-1\$'; then \
      sudo docker exec juego-base-1 pg_dump -U juego juego | gzip > respaldos/antes-$FECHA.sql.gz; \
      echo 'Base respaldada en /opt/juego/respaldos/antes-$FECHA.sql.gz'; fi; \
    umask 022; mkdir -p app-$FECHA && tar -xzf juego-$FECHA.tar.gz -C app-$FECHA && rm juego-$FECHA.tar.gz; \
    echo '$version' > app-$FECHA/VERSION; \
    [ -L actual ] && ln -sfn \"\$(readlink actual)\" anterior; ln -sfn app-$FECHA actual; \
    cd actual && \$DC -p juego --env-file /opt/juego/.env up -d --build; \
    for i in \$(seq 1 30); do curl -fsS http://127.0.0.1:8080/salud && echo && break; sleep 3; done; \
    ls -1dt /opt/juego/app-* | tail -n +4 | xargs -r rm -rf; \
    ls -1t /opt/juego/respaldos/*.sql.gz 2>/dev/null | tail -n +15 | xargs -r rm -f"
  paso "Listo: http://$IP:8080/salud (red local). Panel: http://$IP:8080/panel/ (usuario panel; clave en /opt/juego/.env de la VM)"
}

revertir() {
  paso "Esto respalda la base del juego en la PC y ELIMINA la VM $VMID ($NOMBRE)."
  host "qm config $VMID | grep -q '^name: $NOMBRE\$'" || falla "La VM $VMID no es $NOMBRE; no hago nada."
  read -r -p "Escribe $VMID para confirmar: " ok
  [ "$ok" = "$VMID" ] || falla "Cancelado."
  mkdir -p "$RESPALDOS_PC"
  if vm true 2>/dev/null; then
    vm "sudo docker exec juego-base-1 pg_dump -U juego juego | gzip" > "$RESPALDOS_PC/juego-$FECHA.sql.gz" \
      && echo "Base respaldada en $RESPALDOS_PC/juego-$FECHA.sql.gz" || echo "Aviso: no se pudo respaldar la base."
  fi
  host "qm stop $VMID --timeout 60 >/dev/null 2>&1 || true; qm destroy $VMID --purge 1 --destroy-unreferenced-disks 1"
  paso "VM $VMID eliminada. La imagen Debian queda en /root/imagenes y el respaldo de configuración en /root/respaldos/28-juego-*."
}

case "${1:-}" in
  --estado)    estado ;;
  --desplegar) desplegar ;;
  --revertir)  revertir ;;
  "")          preparar_vm; desplegar; estado ;;
  *)           sed -n '2,12p' "$0"; exit 1 ;;
esac
