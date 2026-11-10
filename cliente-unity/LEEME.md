# Cliente Unity

Proyecto Unity 6 (6000.3.28f1), 2D, vertical. Toda la interfaz se arma por código (`Assets/Scripts/Juego`), así el arte temporal se reemplaza sin tocar las reglas.

- **Sin conexión:** el celular corre el mismo motor de partida que el servidor (`Assets/Plugins/Expedientia.Logica.dll`), con bots.
- **En línea:** juega contra el servidor de partidas (por defecto `http://192.168.1.252:8080`, la VM 113).
- Preguntas: solo las verificadas; el interruptor «Incluir borradores (pruebas)» deja entrar los borradores marcados en amarillo.

## Primer uso

1. Abrir la carpeta `cliente-unity` con Unity Hub (Unity 6000.3.28f1 con módulos de Android).
2. Menú **Expedientes → Preparar proyecto** (crea la escena y la configuración de Android).
3. Menú **Expedientes → Compilar APK de Android**, o por línea de comandos:

```
Unity.exe -batchmode -quit -projectPath cliente-unity -executeMethod Expedientia.Cliente.Editor.Construir.Android -logFile -
```

El APK queda en `cliente-unity/Builds/`.

Después de cambiar la lógica o las preguntas: `./cliente-unity/actualizar-logica.sh`.
