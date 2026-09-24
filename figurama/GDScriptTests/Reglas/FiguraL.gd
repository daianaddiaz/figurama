class_name FiguraL
extends CartaFigura

func get_nombre() -> String:
	return "L"

func get_cantidad_fichas() -> int:
	return 4

func get_patrones() -> Array:
	return [
		[[0,0],[1,0],[2,0],[2,1]],
		[[0,0],[0,1],[0,2],[1,0]],
		[[0,0],[0,1],[1,1],[2,1]],
		[[0,2],[1,0],[1,1],[1,2]]
	]