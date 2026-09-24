class_name CartaMovimiento

func get_nombre() -> String:
	return ""

func es_valido(tablero: TableroReglas, fila_a: int, columna_a: int, fila_b: int, columna_b: int) -> bool:
	return false

func ejecutar(tablero: TableroReglas, fila_a: int, columna_a: int, fila_b: int, columna_b: int) -> void:
	tablero.intercambiar_fichas(fila_a, columna_a, fila_b, columna_b)