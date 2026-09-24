class_name CartaFigura

func get_nombre() -> String:
	return ""

func get_cantidad_fichas() -> int:
	return 0

func get_patrones() -> Array:
	return []

func es_valida(tablero: TableroReglas, fila_ancla: int, columna_ancla: int) -> Dictionary:
	for patron in get_patrones():
		var primer_offset = patron[0]
		var fila_primera = fila_ancla + primer_offset[0]
		var columna_primera = columna_ancla + primer_offset[1]

		var primera_ficha = tablero.obtener_ficha(fila_primera, columna_primera)
		if primera_ficha == null:
			continue

		var resultado = _coincide_patron(tablero, fila_ancla, columna_ancla, primera_ficha.color, patron)
		if resultado["valido"]:
			return resultado

	return { "valido": false, "celdas": [] }

func _coincide_patron(tablero: TableroReglas, fila_ancla: int, columna_ancla: int, color_esperado: int, patron: Array) -> Dictionary:
	var celdas := []

	for offset in patron:
		var fila = fila_ancla + offset[0]
		var columna = columna_ancla + offset[1]

		var ficha = tablero.obtener_ficha(fila, columna)
		if ficha == null or (not ficha.es_comodin and ficha.color != color_esperado):
			return { "valido": false, "celdas": [] }

		celdas.append([fila, columna])

	var delta_fila = [-1, 1, 0, 0]
	var delta_columna = [0, 0, -1, 1]

	for celda in celdas:
		for i in range(4):
			var fila_vecina = celda[0] + delta_fila[i]
			var columna_vecina = celda[1] + delta_columna[i]

			if celdas.has([fila_vecina, columna_vecina]):
				continue

			var vecino = tablero.obtener_ficha(fila_vecina, columna_vecina)
			if vecino != null and (vecino.es_comodin or vecino.color == color_esperado):
				return { "valido": false, "celdas": [] }

	return { "valido": true, "celdas": celdas }