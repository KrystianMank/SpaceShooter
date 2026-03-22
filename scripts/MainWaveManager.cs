using Godot;
using System.Linq;
using GameEnums;

public partial class MainWaveManager : Node
{
	[Export]
	public EntitySpawnerComponent EntitySpawner;
	public Main Main;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Main = (Main)GetTree().GetFirstNodeInGroup("main");
		// Subscribtion to Action<Entity> delegates
		EntitySpawner.OnEntitySpawnerEntityHealthDepleted += OnEntitySpawnerEntityHealthDepleted;
		EntitySpawner.OnEntitySpawnerEntityHealthValueChanged += OnEntitySpawnerEntityHealtValueChanged;
	}
	
	public override void _Process(double delta)
	{
	}


	public void Start()
	{
		EntitySpawner.DestroyAllEntities();
		EntitySpawner.RestComponent();
	}

	async public void WaveManager(int currentWave)
	{
		EntitySpawner.StopSpawning();
		switch (currentWave)
		{
			case 0:
				{
					//EntitySpawner.EntitySpawnTimers.First(x => x.EntityCreator.EntityType == EntityType.Meteor).Start();
					EntitySpawner.BeginSpawning([EntityType.Meteor]);
					break;
				}
			case 1:
				{
					EntitySpawner.BeginSpawning([EntityType.Meteor, EntityType.ShootingAlien]);

					await ToSignal(GetTree().CreateTimer(15d), Timer.SignalName.Timeout);

					EntitySpawner.BeginSpawning([EntityType.Meteor, EntityType.ShootingAlien, EntityType.SeekingAlien]);
					break;
				}
			case 2:
				{
					EntitySpawner.BeginSpawning([EntityType.Meteor, EntityType.SeekingAlien]);
					break;
				}
			default:
				{
					EntitySpawner.BeginSpawning([EntityType.Meteor]);
					break;
				}
		};
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	
	public void OnEntitySpawnerEntityHealthDepleted(Entity entity)
    {
		if(!entity.EntitiesHitEachOther){
			Main.Score.Value += (int)entity.EntityMaxHP.Value;
			Main.Hud.UpdateScore(Main.Score.Value);
		}

		entity.EnityDeath();
	
    }

	public void OnEntitySpawnerEntityHealtValueChanged(Entity entity)
    {
		entity.SetHealthBarValue(entity.EntityHP.GetHP().Value);
    }
}
