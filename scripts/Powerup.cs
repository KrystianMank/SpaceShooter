using Godot;
using GameEnums;
using System;

public partial class Powerup : Area2D
{
	[Signal]
    public delegate void PowerupPickedEventHandler(PowerupEnum powerupEnum);
	[Export]
	public Texture2D[] PowerupTextures;
	private string _powerupName;
	private int _powerupSpeed = 200;
	private PowerupEnum powerupEnum;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Monitoring = true;
		Monitorable = true;

		int randomPowerup = (int)GD.RandRange(0, (double)PowerupTextures.Length);
		GetNode<Sprite2D>("Sprite2D").Texture = PowerupTextures[randomPowerup];

		string name = PowerupTextures[randomPowerup].ResourcePath.GetFile().GetBaseName();
        _powerupName = name;
		Name = _powerupName;

		Enum.TryParse<PowerupEnum>(_powerupName, out powerupEnum);

		AddToGroup("powerups");
	}

    public override void _Process(double delta)
    {
		Position += Vector2.Down * _powerupSpeed * (float)delta;
    }

	
	private void OnAreaEntered(Area2D body)
    {
        if (body is Player player)
        {
			EmitSignal(SignalName.PowerupPicked, (int)powerupEnum);
			
			var powerupContainer = GetTree().GetNodesInGroup("powerup_holders");
        	powerupContainer[0].GetNode<Sprite2D>("PanelContainer/PowerupContainer").Texture = GetNode<Sprite2D>("Sprite2D").Texture;

            QueueFree();
        }
    }

	public void OnVisibleOnScreenNotifier2dScreenExited()
	{
		QueueFree();
	}
}
