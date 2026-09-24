class_name MovimientoLateralContiguo
extends CartaMovimiento

func get_nombre() -> String:
	return "Movimiento lateral contiguo"

func es_valido(tablero: TableroReglas, fila_a: int, columna_a: int, fila_b: int, columna_b: int) -> bool:
	var dif_fila = abs(fila_a - fila_b)
	var dif_columna = abs(columna_a - columna_b)

	var horizontal_contiguo = dif_fila == 0 and dif_columna == 1
	var vertical_contiguo = dif_columna == 0 and dif_fila == 1

	return horizontal_contiguo or vertical_contiguo