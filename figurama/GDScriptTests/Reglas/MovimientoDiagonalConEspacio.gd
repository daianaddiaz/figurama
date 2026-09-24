class_name MovimientoDiagonalConEspacio
extends CartaMovimiento

func get_nombre() -> String:
	return "Diagonal con Espacio"

func es_valido(tablero: TableroReglas, fila_a: int, columna_a: int, fila_b: int, columna_b: int) -> bool:
	var dif_fila = abs(fila_a - fila_b)
	var dif_columna = abs(columna_a - columna_b)

	return dif_fila == 2 and dif_columna == 2