# Alumnos:
***Kevin Alemanno
Alejandro Decurgez
Daiana Diaz***

¡Bienvenido a *Figurama*! Un juego de tablero digital donde la astucia, los patrones
geométricos y los mitos clásicos se cruzan.
Tu objetivo principal es formar en el tablero las figuras geométricas exactas que
indican tus Cartas de Figura (usando 4 o 5 fichas) mediante el movimiento de
fichas en la grilla. ¡Usa tus Cartas de Movimiento, aprovecha la habilidad única de
tu personaje legendario y completa tus figuras antes que tu rival para consagrarte
con la victoria!

# Mecánicas Principales y Reglas de Juego

El Turno de Juego (Paso a Paso)
1. Robo de Mano: Al comenzar tu turno, recibes tus cartas de movimiento y tus cartas
de figuras a armar.
2. Planificación: Revisa tus Cartas de Figura objetivo. Observa el tablero y busca qué
fichas necesitas mover para replicar la forma exacta (horizontal, vertical o rotada).
3. Ejecución de Movimientos:
-  Haz clic en una Carta de Movimiento (ej. Intercambiar fichas adyacentes,
Desplazar línea, etc.).
-  Ejecuta la acción en el tablero seleccionando las fichas indicadas.
-  La carta utilizada se volteará automáticamente mostrando su dorso.
4. Uso de Habilidad Única:
-  Si la situación lo requiere y el botón de habilidad está activo, presiónalo para
ejecutar la habilidad especial de tu personaje (Lobizón, Pomberito, Luz Mala
-  Mulánima).
-  Al usarse, la habilidad quedará marcada como usada durante el resto del
turno/partida.

5. Validación Automática: El juego detectará en tiempo real si las fichas en el tablero
forman una de tus figuras asignadas. De ser así, la figura cambiará a estado
completado (resaltado verde) y sumará puntos a tu marcador.
Las Cartas y Formas
- Figuras de 4 Fichas: Formas compactas como la L4 (en sus variantes
izquierda/derecha) o la Línea 4.
- Figuras de 5 Fichas: Formas más complejas como la Cruz, la L5 (en sus variantes
quirales izquierda/derecha) y la Zig-Zag.
- Rotación y Espejo: ¡Atención! Las figuras pueden requerir una orientación
específica. Asegúrate de verificar las matrices de rotación permitidas.

# Interfaz de Usuario (HUD)

## En la pantalla principal de juego verás los siguientes componentes organizados en la interfaz:

- *Tablero de Juego (Grid):* Grilla central con las fichas de colores (amarillo, verde,
azul y morado) donde realizarás los desplazamientos e intercambios.
- *Panel de Cartas de Figuras (Izquierda):* Muestra las figuras que debes armar.
- Borde / Tono Normal: Figura pendiente por completar.
- *Resaltado Verde:* ¡Figura completada con éxito!
- *Mano de Cartas de Movimiento (Inferior):* Muestra tus cartas de acción
disponibles en el turno actual.
*Frente visible:* Carta disponible para usar.
*Dorso / Volteada*: Carta consumida en este turno.
- *Botón de Reroll:* Te permite descartar tu mano actual de movimientos para obtener
cartas nuevas si la estrategia actual no te sirve, perdiendo un turno completo. Si
usas una carta de movimiento, éste botón se desactiva sin posibilidad de uso.
- *Botón de Habilidad Especial:* Muestra la imagen de tu personaje. Si cumples los
requisitos de activación, se ilumina para desatar su poder. Si ya la usaste o no está
disponible, cambiará a su estado desactivado.
- *Marcador de Puntuación (PuntuaciónView):* Muestra el conteo de figuras
armadas.
- *Timer:* Timer puesto en 02:00 con countdown ubicado en el medio-superior para
vista más veloz.
*- Visualización de jugador actual:* Cambio de color de fondo y en la zona superior
izquierda estará su nombre con el personaje que eligió al comienzo del juego.
- *Pantalla de Menú Principal:*
 Tutorial/Cómo Jugar: Slides de tutorial, dividido en elementos y flujo de juego.
Jugar -> Elección de Personaje + Descripción + Nombre de Usuario.

# Documentación Técnica
[Documento Técnico Figurama](https://docs.google.com/document/d/1bJNM056d6-P9cfpTCB3ImmbCYZXrp_qQbvZtp-u7Ojg/edit?tab=t.0)
