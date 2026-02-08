using Godot;
using System;

public partial class EntitySpawnerComponent : Node
{
	[Export]
	public PackedScene[] EntityScenes;
	public class EntitySpawnTimer
	{
		public Timer SpawnTimer;
		public EntitySpawnTimer(double initialWaitTime)
		{
			SpawnTimer = new Timer
			{
				WaitTime = initialWaitTime,
				Autostart = false,
				OneShot = false
			};
		}
	}
	public EntitySpawnTimer MeteorSpawnTimer, AlienSpawnTimer;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		MeteorSpawnTimer = new(1);
		AlienSpawnTimer = new(2);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
