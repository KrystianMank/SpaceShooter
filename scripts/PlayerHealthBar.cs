using Godot;
using System;

public partial class PlayerHealthBar : CanvasLayer
{
	// Called when the node enters the scene tree for the first time.
	public void SetHealthBarMaxValue(double value)
    {
        GetNode<TextureProgressBar>("TextureProgressBar").MaxValue = value;
    }

	public void SetHealthBarValue(double value)
    {
        GetNode<TextureProgressBar>("TextureProgressBar").Value = value;
    }
}
