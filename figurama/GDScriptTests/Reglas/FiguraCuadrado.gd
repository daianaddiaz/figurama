class_name FiguraCuadrado
extends CartaFigura

func get_nombre() -> String:
	return "Cuadrado"

func get_cantidad_fichas() -> int:
	return 4

func get_patrones() -> Array:
	return [
		[[0,0],[0,1],[1,0],[1,1]]
	]