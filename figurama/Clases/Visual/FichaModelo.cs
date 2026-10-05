using Godot;
using System.Collections.Generic;

public partial class FichaModelo : CsgCombiner3D
{
	[Export] Texture2D _simboloRojo;
	[Export] Texture2D _simboloVerde;
	[Export] Texture2D _simboloAzul;
	[Export] Texture2D _simboloAmarillo;
	[Export] Texture2D _simboloComodin;
	[Export] Texture2D _simboloFuego;

	private List<Texture2D> _simbolos = new List<Texture2D>();

	private CsgBox3D _base;

	private Color colorBase;
	private Color colorSecundarioBase;
	private int indiceBase;

	public override void _Ready()
	{
		_base = GetNode<CsgBox3D>("Base");
		_simbolos.AddRange(new Texture2D[] { _simboloRojo, _simboloVerde, _simboloAzul, _simboloAmarillo });
	}

	public void SetearColor(Color colorElegido, Color colorSecundario, int indice)
	{
		colorBase = colorElegido;
		colorSecundarioBase = colorSecundario;
		indiceBase = indice;
		var material = new StandardMaterial3D();
		material.AlbedoColor = colorElegido;
		_base.Material = material;
		_base.GetNode<Decal>("Circuitos").Modulate = colorSecundario;
		_base.GetNode<Decal>("Simbolo").TextureAlbedo = _simbolos[indice];
	}

	public void VolverABase()
	{
		var material = new StandardMaterial3D();
		material.AlbedoColor = colorBase;
		_base.Material = material;
		_base.GetNode<Decal>("Circuitos").Modulate = colorSecundarioBase;
		_base.GetNode<Decal>("Simbolo").TextureAlbedo = _simbolos[indiceBase];
	}

	public void SetearColorComodin()
	{
		var material = new StandardMaterial3D();
		material.AlbedoColor = Colors.MidnightBlue;
		_base.Material = material;
		_base.GetNode<Decal>("Circuitos").Modulate = Colors.Chocolate;
		_base.GetNode<Decal>("Simbolo").TextureAlbedo = _simboloComodin;
	}

	public void SetearColorFuego()
	{
		var material = new StandardMaterial3D();
		material.AlbedoColor = Colors.Chocolate;
		_base.Material = material;
		_base.GetNode<Decal>("Circuitos").Modulate = Colors.MidnightBlue;
		_base.GetNode<Decal>("Simbolo").TextureAlbedo = _simboloFuego;
	}
}
