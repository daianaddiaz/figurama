class_name TableroReglas

const FILAS := 6
const COLUMNAS := 6

var _grilla := []

func _init() -> void:
	_grilla.resize(FILAS)
	for f in range(FILAS):
		var fila_array = []
		fila_array.resize(COLUMNAS)
		_grilla[f] = fila_array

func colocar_ficha(ficha: FichaData, fila: int, columna: int) -> void:
	_grilla[fila][columna] = ficha
	ficha.fila = fila
	ficha.columna = columna

func obtener_ficha(fila: int, columna: int) -> FichaData:
	if fila < 0 or fila >= FILAS or columna < 0 or columna >= COLUMNAS:
		return null
	return _grilla[fila][columna]

func intercambiar_fichas(fila_a: int, columna_a: int, fila_b: int, columna_b: int) -> void:
	var ficha_a = obtener_ficha(fila_a, columna_a)
	var ficha_b = obtener_ficha(fila_b, columna_b)
	colocar_ficha(ficha_a, fila_b, columna_b)
	colocar_ficha(ficha_b, fila_a, columna_a)

func buscar_figura(figura: CartaFigura, celdas_movidas: Array) -> Array:
	print("BuscarFigura llamado. Figura: %s. CeldasMovidas: %s" % [figura.get_nombre(), str(celdas_movidas)])

	var celdas_disparadoras := celdas_movidas.duplicate()

	var delta_fila = [-1, 1, 0, 0]
	var delta_columna = [0, 0, -1, 1]

	for celda in celdas_movidas:
		for i in range(4):
			var fila_vecina = celda[0] + delta_fila[i]
			var columna_vecina = celda[1] + delta_columna[i]

			if fila_vecina < 0 or fila_vecina >= FILAS or columna_vecina < 0 or columna_vecina >= COLUMNAS:
				continue

			if not celdas_disparadoras.has([fila_vecina, columna_vecina]):
				celdas_disparadoras.append([fila_vecina, columna_vecina])

	for fila in range(FILAS):
		for columna in range(COLUMNAS):
			var resultado = figura.es_valida(self, fila, columna)
			if resultado["valido"]:
				var celdas = resultado["celdas"]
				print("EsValida=true en ancla (%d,%d). Celdas: %s" % [fila, columna, str(celdas)])

				for celda in celdas:
					if celdas_disparadoras.has(celda):
						print("MATCH con disparador, retornando.")
						return celdas

				print("Figura encontrada en: " + str(celdas))

	print("BuscarFigura terminó sin encontrar nada.")
	return []