class_name MovimientoEnL
extends CartaMovimiento

enum TipoDireccion { CUALQUIERA, DERECHA, IZQUIERDA }

var direccion_permitida: int = TipoDireccion.CUALQUIERA

func get_nombre() -> String:
	return "Movimiento en L"

func es_valido(tablero: TableroReglas, fila_a: int, columna_a: int, fila_b: int, columna_b: int) -> bool:
	var d_fila = fila_b - fila_a
	var d_columna = columna_b - columna_a

	var abs_fila = abs(d_fila)
	var abs_columna = abs(d_columna)

	var es_l = (abs_fila == 1 and abs_columna == 2) or (abs_fila == 2 and abs_columna == 1)
	if not es_l:
		return false

	if direccion_permitida == TipoDireccion.CUALQUIERA:
		return true

	if direccion_permitida == TipoDireccion.DERECHA:
		return d_columna > 0
	else:
		return d_columna < 0