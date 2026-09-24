extends GutTest

func _crear_ficha(color: int, es_comodin: bool = false) -> FichaData:
	var ficha = FichaData.new()
	ficha.color = color
	ficha.es_comodin = es_comodin
	return ficha

func test_cruz_forma_exacta_es_valida():
	# Arrange
	var tablero = TableroReglas.new()
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 1)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 3)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 1, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 3, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 2)
	var figura = FiguraCruz.new()

	# Act
	var resultado = figura.es_valida(tablero, 2, 2)

	# Assert
	assert_true(resultado["valido"])
	assert_eq(resultado["celdas"].size(), 5)

func test_cruz_con_color_mezclado_no_es_valida():
	var tablero = TableroReglas.new()
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 1)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.AZUL), 2, 3) # rompe el patrón
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 1, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 3, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 2)
	var figura = FiguraCruz.new()

	var resultado = figura.es_valida(tablero, 2, 2)

	assert_false(resultado["valido"])

func test_cruz_con_ficha_extra_pegada_no_es_valida():
	var tablero = TableroReglas.new()
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 1)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 3)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 1, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 3, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 0, 2) # pegada afuera del patrón
	var figura = FiguraCruz.new()

	var resultado = figura.es_valida(tablero, 2, 2)

	assert_false(resultado["valido"])

func test_cruz_fuera_del_tablero_no_es_valida():
	var tablero = TableroReglas.new()
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 0, 0)
	var figura = FiguraCruz.new()

	# Ancla en el borde: el offset (-1,0) cae fuera del tablero
	var resultado = figura.es_valida(tablero, 0, 0)

	assert_false(resultado["valido"])

var rotaciones_l = [
	[[0,0],[1,0],[2,0],[2,1]],
	[[0,0],[0,1],[0,2],[1,0]],
	[[0,0],[0,1],[1,1],[2,1]],
	[[0,2],[1,0],[1,1],[1,2]],
]

func test_l_con_cualquier_rotacion_es_valida(patron = use_parameters(rotaciones_l)):
	var tablero = TableroReglas.new()
	for offset in patron:
		tablero.colocar_ficha(_crear_ficha(ColorFicha.VERDE), offset[0], offset[1])
	var figura = FiguraL.new()

	var resultado = figura.es_valida(tablero, 0, 0)

	assert_true(resultado["valido"])