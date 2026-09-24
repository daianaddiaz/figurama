extends GutTest

func _crear_ficha(color: int, es_comodin: bool = false) -> FichaData:
	var ficha = FichaData.new()
	ficha.color = color
	ficha.es_comodin = es_comodin
	return ficha

func test_figura_vieja_sin_relacion_al_movimiento_no_cuenta():
	# Arrange
	var tablero = TableroReglas.new()
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 1)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 3)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 1, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 3, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 2)
	var figura = FiguraCruz.new()

	# Act: el movimiento fue en una celda sin ninguna relación con la cruz
	var resultado = tablero.buscar_figura(figura, [[5, 5]])

	# Assert
	assert_true(resultado.is_empty())

func test_figura_completada_por_el_movimiento_cuenta():
	var tablero = TableroReglas.new()
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 1)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 3)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 1, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 3, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 2)
	var figura = FiguraCruz.new()

	# El movimiento tocó justo el centro de la cruz
	var resultado = tablero.buscar_figura(figura, [[2, 2]])

	assert_false(resultado.is_empty())
	assert_eq(resultado.size(), 5)

func test_figura_revelada_al_mover_una_ficha_vecina_cuenta():
	# Arrange: la cruz ya está armada; (0,2) es la celda vecina donde se hizo el intercambio
	var tablero = TableroReglas.new()
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 1)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 3)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 1, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 3, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.ROJO), 2, 2)
	tablero.colocar_ficha(_crear_ficha(ColorFicha.AZUL), 0, 2)
	var figura = FiguraCruz.new()

	# Act: el movimiento fue en (0,2), vecina a la cruz (no parte de ella)
	var resultado = tablero.buscar_figura(figura, [[0, 2]])

	# Assert: cuenta, porque (0,2) es vecina de (1,2), que sí es parte de la cruz
	assert_false(resultado.is_empty())