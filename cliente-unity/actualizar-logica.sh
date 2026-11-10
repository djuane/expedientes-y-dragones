#!/usr/bin/env bash
# Compila la librería de lógica y la copia al proyecto Unity (Assets/Plugins), junto con las preguntas.
set -euo pipefail
raiz="$(cd "$(dirname "$0")/.." && pwd)"
dotnet build "$raiz/logica/Expedientia.Logica/Expedientia.Logica.csproj" -c Release -o "$raiz/logica/salida" >/dev/null
cp "$raiz/logica/salida/Expedientia.Logica.dll" "$raiz/cliente-unity/Assets/Plugins/"
rm -f "$raiz/cliente-unity/Assets/Resources/preguntas/"*.json
for f in "$raiz/contenido/preguntas/"*.json; do
  [ "$(basename "$f")" = "plantilla.json" ] && continue
  cp "$f" "$raiz/cliente-unity/Assets/Resources/preguntas/"
done
echo "Lógica y preguntas actualizadas en cliente-unity."
