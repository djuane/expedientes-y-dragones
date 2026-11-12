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

## Arte, animación y audio (v0.8.0, efectos en v0.9.0)

- **Arte:** `Assets/Resources/arte` (fichas, retratos, escenas de cada sala y portada). Se genera desde `arte/` en la raíz del repositorio. `Arte.cs` lo carga; si falta un dibujo, la interfaz sigue con colores planos.
- **Animación:** las fichas se deslizan al moverse, embisten al atacar, tiemblan al recibir daño, muestran el daño flotando y caen girando; el dado es un d20 con sello de «¡Fundado!» o «Improcedente».
- **Efectos:** `Efectos.cs` dibuja confeti, papelitos y chispas sobre una capa propia, encima de toda la interfaz. Recicla las piezas y los textos flotantes y mueve todas las piezas en una sola actualización por cuadro.
- **Trama:** cuadro de diálogo con retrato y texto que aparece letra por letra. El prólogo sale la primera vez y se puede volver a ver desde la portada.
- **Audio:** `Sonido.cs` sintetiza por código los efectos (dado, sello, golpe, ventanilla, monedas y voces) y un tema de menú original en escala pentatónica. No usa archivos ni música de terceros. Se silencia desde la portada.
- `Assets/link.xml` evita que IL2CPP recorte la lógica, que se serializa por reflexión.
