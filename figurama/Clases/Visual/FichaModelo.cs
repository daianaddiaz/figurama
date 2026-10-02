using Godot;
using System.Collections.Generic;

public partial class FichaModelo : CsgCombiner3D
{
	[Export] Texture2D _simboloRojo;
	[Export] Texture2D _simboloVerde;
	[Export] Texture2D _simboloAzul;
	[Export] Texture2D _simboloAmarillo;

	private List<Texture2D> _simbolos = new List<Texture2D>();

	private CsgBox3D _base;

	public override void _Ready()
	{
		_base = GetNode<CsgBox3D>("Base");
		_simbolos.AddRange(new Texture2D[] { _simboloRojo, _simboloVerde, _simboloAzul, _simboloAmarillo });
	}

	public void SetearColor(Color colorElegido, Color colorSecundario, int indice)
	{
		var material = new StandardMaterial3D();
		material.AlbedoColor = colorElegido;
		_base.Material = material;
		_base.GetNode<Decal>("Circuitos").Modulate = colorSecundario;
		_base.GetNode<Decal>("Simbolo").TextureAlbedo = _simbolos[indice];
	}
}
