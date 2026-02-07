using Godot;
using System;

public partial class PlayerHealthBar : CanvasLayer
{
    private double _currentHP, _maxHP;
	// Called when the node enters the scene tree for the first time.
	public void SetHealthBarMaxValue(double value)
    {
        _maxHP = value;
        GetNode<TextureProgressBar>("TextureProgressBar").MaxValue = value;
        GetNode<Label>("TextureProgressBar/Label").Text = $"{_currentHP}/{_maxHP}";
    }

	public void SetHealthBarValue(double value)
    {
        _currentHP = value;
        GetNode<TextureProgressBar>("TextureProgressBar").Value = value;
        GetNode<Label>("TextureProgressBar/Label").Text = _currentHP > 0 ? $"{_currentHP}/{_maxHP}" : $"{0}/{_maxHP}";
    }


}
