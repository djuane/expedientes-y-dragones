# Expedientes y Dragones

Juego de rol cooperativo por turnos para celular, en español, donde un estudio de abogados vence a los monstruos del sistema de justicia con dados, decisiones éticas y derecho verificado.

Autor: Leonel Díaz.

## Estructura

| Carpeta | Contenido |
| --- | --- |
| `logica/Expedientia.Logica` | Reglas del juego en C# (.NET Standard 2.1), sin dependencias de Unity. La usan la app y el servidor |
| `logica/Expedientia.Logica.Pruebas` | Pruebas automáticas (xUnit) |
| `servidor/` | Servidor de partidas ASP.NET Core + WebSocket + PostgreSQL |
| `panel/` | Panel de contenido para publicar preguntas |
| `cliente-unity/` | Proyecto Unity 6 (2D) |
| `contenido/preguntas/` | Banco de preguntas jurídicas |

## Lo que ya hace la lógica

- Dados: d20 con ventaja y desventaja (se anulan entre sí), sumas de varios dados y azar con semilla para que el servidor reproduzca cada partida.
- Momentos del dado: 20 natural «Fundado» (en ataques acierta siempre y duplica los dados de daño) y 1 natural «Improcedente».
- Atributos con nombre de oficio (Firmeza, Agilidad procesal, Aguante, Doctrina, Criterio, Oratoria), modificadores y competencia por nivel del SRD 5.2, y títulos de carrera.
- Iniciativa con desempate determinista.
- Prueba jurídica: máximo 3 por partida, 20 segundos, acierto da ventaja y fallo da tirada normal; solo entran preguntas verificadas.

## Cómo probar

```
dotnet test ExpedientesYDragones.sln
```

## Licencia de las reglas

This work includes material from the System Reference Document 5.2 ("SRD 5.2") by Wizards of the Coast LLC, available at https://www.dndbeyond.com/srd. The SRD 5.2 is licensed under the Creative Commons Attribution 4.0 International License, available at https://creativecommons.org/licenses/by/4.0/legalcode.
