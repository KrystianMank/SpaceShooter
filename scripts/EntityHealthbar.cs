using Godot;
using System;

public partial class EntityHealthbar : MarginContainer
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        Rotation = 0f;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        Rotation = 0f;
    }
}
