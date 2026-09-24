class_name FiguraCruz
extends CartaFigura

func get_nombre() -> String:
	return "Cruz"

func get_cantidad_fichas() -> int:
	return 5

func get_patrones() -> Array:
	return [
		[[0,0],[-1,0],[1,0],[0,-1],[0,1]]
	]