class_name MovimientoLateralConEspacio
extends CartaMovimiento

func get_nombre() -> String:
	return "Movimiento lateral con espacio"

func es_valido(tablero: TableroReglas, fila_a: int, columna_a: int, fila_b: int, columna_b: int) -> bool:
	var dif_fila = abs(fila_a - fila_b)
	var dif_columna = abs(columna_a - columna_b)

	var horizontal_con_espacio = dif_fila == 0 and dif_columna == 2
	var vertical_con_espacio = dif_columna == 0 and dif_fila == 2

	return horizontal_con_espacio or vertical_con_espacio