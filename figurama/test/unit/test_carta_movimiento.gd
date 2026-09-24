extends GutTest

# --- Lateral Contiguo ---

func test_lateral_contiguo_horizontal_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoLateralContiguo.new()

	assert_true(movimiento.es_valido(tablero, 0, 0, 0, 1))

func test_lateral_contiguo_vertical_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoLateralContiguo.new()

	assert_true(movimiento.es_valido(tablero, 0, 0, 1, 0))

func test_lateral_contiguo_con_distancia_2_no_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoLateralContiguo.new()

	assert_false(movimiento.es_valido(tablero, 0, 0, 0, 2))

func test_lateral_contiguo_diagonal_no_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoLateralContiguo.new()

	assert_false(movimiento.es_valido(tablero, 0, 0, 1, 1))

# --- Lateral Con Espacio ---

func test_lateral_con_espacio_distancia_2_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoLateralConEspacio.new()

	assert_true(movimiento.es_valido(tablero, 0, 0, 0, 2))

func test_lateral_con_espacio_contiguo_no_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoLateralConEspacio.new()

	assert_false(movimiento.es_valido(tablero, 0, 0, 0, 1))

# --- Diagonal Contiguo ---

func test_diagonal_contiguo_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoDiagonalContiguo.new()

	assert_true(movimiento.es_valido(tablero, 0, 0, 1, 1))

func test_diagonal_contiguo_lateral_no_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoDiagonalContiguo.new()

	assert_false(movimiento.es_valido(tablero, 0, 0, 1, 0))

# --- Diagonal Con Espacio ---

func test_diagonal_con_espacio_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoDiagonalConEspacio.new()

	assert_true(movimiento.es_valido(tablero, 0, 0, 2, 2))

func test_diagonal_con_espacio_contiguo_no_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoDiagonalConEspacio.new()

	assert_false(movimiento.es_valido(tablero, 0, 0, 1, 1))

# --- Lateral Al Borde ---

func test_lateral_al_borde_hacia_columna_0_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoLateralAlBorde.new()

	assert_true(movimiento.es_valido(tablero, 2, 3, 2, 0))

func test_lateral_al_borde_hacia_ultima_columna_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoLateralAlBorde.new()

	assert_true(movimiento.es_valido(tablero, 2, 3, 2, TableroReglas.COLUMNAS - 1))

func test_lateral_al_borde_hacia_columna_intermedia_no_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoLateralAlBorde.new()

	assert_false(movimiento.es_valido(tablero, 2, 3, 2, 1))

func test_lateral_al_borde_misma_celda_no_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoLateralAlBorde.new()

	assert_false(movimiento.es_valido(tablero, 2, 3, 2, 3))

# --- Movimiento En L ---

func test_en_l_cualquiera_forma_valida_hacia_derecha_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoEnL.new()
	movimiento.direccion_permitida = MovimientoEnL.TipoDireccion.CUALQUIERA

	assert_true(movimiento.es_valido(tablero, 0, 0, 1, 2))

func test_en_l_cualquiera_forma_invalida_no_es_valido():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoEnL.new()
	movimiento.direccion_permitida = MovimientoEnL.TipoDireccion.CUALQUIERA

	assert_false(movimiento.es_valido(tablero, 0, 0, 1, 1))

func test_en_l_restringido_a_derecha_permite_movimiento_hacia_la_derecha():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoEnL.new()
	movimiento.direccion_permitida = MovimientoEnL.TipoDireccion.DERECHA

	assert_true(movimiento.es_valido(tablero, 0, 0, 1, 2))

func test_en_l_restringido_a_derecha_rechaza_movimiento_hacia_la_izquierda():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoEnL.new()
	movimiento.direccion_permitida = MovimientoEnL.TipoDireccion.DERECHA

	assert_false(movimiento.es_valido(tablero, 0, 2, 1, 0))

func test_en_l_restringido_a_izquierda_permite_movimiento_hacia_la_izquierda():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoEnL.new()
	movimiento.direccion_permitida = MovimientoEnL.TipoDireccion.IZQUIERDA

	assert_true(movimiento.es_valido(tablero, 0, 2, 1, 0))

func test_en_l_restringido_a_izquierda_rechaza_movimiento_hacia_la_derecha():
	var tablero = TableroReglas.new()
	var movimiento = MovimientoEnL.new()
	movimiento.direccion_permitida = MovimientoEnL.TipoDireccion.IZQUIERDA

	assert_false(movimiento.es_valido(tablero, 0, 0, 1, 2))