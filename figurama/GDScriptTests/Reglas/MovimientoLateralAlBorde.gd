class_name MovimientoLateralAlBorde
extends CartaMovimiento

func get_nombre() -> String:
	return "Lateral al Borde"

func es_valido(tablero: TableroReglas, fila_a: int, columna_a: int, fila_b: int, columna_b: int) -> bool:
	if fila_a == fila_b and columna_a == columna_b:
		return false

	var misma_fila_borde_columna = fila_a == fila_b and (columna_b == 0 or columna_b == TableroReglas.COLUMNAS - 1)
	var misma_columna_borde_fila = columna_a == columna_b and (fila_b == 0 or fila_b == TableroReglas.FILAS - 1)

	return misma_fila_borde_columna or misma_columna_borde_fila