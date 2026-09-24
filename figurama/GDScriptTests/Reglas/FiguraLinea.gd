class_name FiguraLinea
extends CartaFigura

func get_nombre() -> String:
	return "Linea"

func get_cantidad_fichas() -> int:
	return 4

func get_patrones() -> Array:
	return [
		[[0,0],[0,1],[0,2],[0,3]],
		[[0,0],[1,0],[2,0],[3,0]]
	]