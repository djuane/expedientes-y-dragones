# Banco de preguntas

Una pregunta por archivo JSON, con el formato de `plantilla.json`. Solo entran al juego las que tienen `estado: "verificada"`, con revisor, fecha y enlace oficial (SPIJ, El Peruano o el tribunal correspondiente).

Estados: `borrador`, `verificada`, `desactualizada`.

## Calabozo Mesa de Partes (mp-001 a mp-050)

50 preguntas cortas para la prueba jurídica de 20 segundos: 25 para el Civilista (12 de Procesal civil y 13 de Civil) y 25 para el Administrativista. Todas están en `borrador` hasta que el revisor las valide.

- **Administrativo (mp-026 a mp-050):** cotejadas con la Primera Edición Oficial del TUO de la Ley 27444 aprobado por D.S. 006-2026-JUS (MINJUS, actualizada al 08/07/2026), que derogó el TUO de 2019. Ojo: este TUO volvió a la numeración original de la Ley 27444 (por ejemplo, recursos en el art. 207 y observaciones de mesa de partes en el art. 125).
- **Procesal civil (mp-001 a mp-012):** cotejadas con el texto original del TUO del Código Procesal Civil (R.M. 010-93-JUS, archivo del Congreso). Falta confirmar en SPIJ que el artículo no haya sido modificado después.
- **Civil (mp-013 a mp-025):** el art. 42 está cotejado con el D. Leg. 1384 en El Peruano; las demás citan el Código Civil y falta verificarlas en SPIJ.

Para pasar una pregunta a `verificada`: revisar el artículo en SPIJ, completar `enlace_oficial`, `verificado_por` y `fecha_verificacion` (AAAA-MM-DD), y quitar la nota «por verificar» de `base_legal`.

El juego no baraja las opciones, así que la respuesta correcta se reparte entre las cuatro posiciones.
