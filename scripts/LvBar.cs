using Godot;
using System;

public partial class LvBar : CanvasLayer
{
	private TextureProgressBar textureProgressBar;
	private int _progressBarMaxValue, _progressBarValue;
    public override void _Ready()
    {
        textureProgressBar = GetNode<TextureProgressBar>(nameof(TextureProgressBar));
    }

	public void SetLVBarMaxValue(int minValue, int maxValue)
	{
		textureProgressBar.MaxValue = maxValue;
		textureProgressBar.MinValue = minValue;
		textureProgressBar.GetNode<Label>("CurrentScoreLabel").Text = $"{_progressBarValue}/{maxValue}";
		_progressBarMaxValue = maxValue;
	}
	public void SetLVBarValue(int value)
	{
		textureProgressBar.Value = value;
		
		textureProgressBar.GetNode<Label>("CurrentScoreLabel").Text = $"{value}/{_progressBarMaxValue}";
		_progressBarValue = value;
	}
	public void SetLVLabel(int level)
	{
		textureProgressBar.GetNode<Label>("LevelLabel").Text = level.ToString();
	}
	
}
