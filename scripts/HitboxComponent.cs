using Godot;
using System;

public partial class HitboxComponent : Area2D
{
	[Export]
	public double Damage {get; set;}
}
