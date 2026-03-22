using GameEnums;
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class EntitySpawnerComponent : Node
{
	[Export]
	public EntityResource[] EntityResources;
	[Export]
	public PackedScene EntityHealthbarScene;
	public class EntitySpawnTimer
	{
		public EntityCreator EntityCreator;
		public Timer SpawnTimer;
		public Action OnTimeout;
		private double _initialWaitTime;
		public EntitySpawnTimer(double initialWaitTime)
		{
			SpawnTimer = new Timer
			{
				WaitTime = initialWaitTime,
				Autostart = false,
				OneShot = false,
			};
			SpawnTimer.Timeout += OnSpawnTimerTimeout;
			_initialWaitTime = initialWaitTime;
			SpawnTimer.AddToGroup("timers");
		}

		public void ApplyNewWaitTime(double newWaitTime)
		{
			SpawnTimer.WaitTime = newWaitTime;
		}
		public void Start()
		{
			SpawnTimer.Start();
		}
		public void Stop()
		{
			SpawnTimer.Stop();
		}
		public void ResetWaitTime()
		{
			SpawnTimer.WaitTime = _initialWaitTime;
		}
		public virtual void OnSpawnTimerTimeout()
		{
			OnTimeout?.Invoke();
		}
	}
	public double EntityHealthMultiplier = 0d;
	public double EnititySpawnDificultyMultiplier = 0.1d;
	// Temporary
	public double AlienFirerateMultiplier = 0, AlienBulletDamageMultiplier = 0;
	public List<EntitySpawnTimer> EntitySpawnTimers = [];

	public Action<Entity> OnEntitySpawnerEntityHealthDepleted, OnEntitySpawnerEntityHealthValueChanged;

	public AnimatedSprite2D EntitySpawnAnimation;
	private int _horizontalLines;
	private int _verticalLines;
	private Vector2 _screenSize;
	private Player _player;
	public void CreateEntitySpawners()
	{
		foreach(var resource in EntityResources)
		{
			_screenSize = GetViewport().GetVisibleRect().Size;
			_horizontalLines = (int)_screenSize.X / 30;
			_verticalLines = (int)_screenSize.Y / 24;

			EntitySpawnTimer timer = new(resource.InitialSpawnTime)
			{ 
				EntityCreator = resource.CreateEntityCreator() 
			};
			EntitySpawnTimers.Add(timer);
			AddChild(timer.SpawnTimer);
			GD.Print($"Loaded entity type: {resource.EntityType}, spawn time: {resource.InitialSpawnTime}");

			if(resource.EntityType == EntityType.ShootingAlien)
			{	
				InstantiateAlienPaths();
			}
		}
	}

	// Called when the node enters the scene tree for the first time.
	async public override void _Ready()
	{
		EntitySpawnAnimation = GetNode<AnimatedSprite2D>("SpawnAnimation");
		
		// Wait one frame to ensure Player has added itself to group
		_player = null;
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		_player = (Player)GetTree().GetFirstNodeInGroup("player");
		
		if (_player == null)
		{
			GD.PrintErr("ERROR: Player not found in 'player' group!");
			return;
		}
		
		CreateEntitySpawners();
		foreach(var spawner in EntitySpawnTimers)
		{
			switch (spawner.EntityCreator.EntityType)
			{
				case EntityType.Meteor:
					{
						spawner.OnTimeout = () =>
						{
							// Instantiate new meteor
							Meteor meteor = spawner.EntityCreator.EntityScene.Instantiate<Meteor>();
							spawner.EntityCreator.SetHealthBar(meteor, EntityHealthbarScene);
							spawner.EntityCreator.SetHealth(meteor, 1, 3, EntityHealthMultiplier);

							// Subscribe to health events
							meteor.EnitityHPDepleted += OnEntityHealthDepleted;
							meteor.EntityHPValueChanged += OnEntityHealthValueChanged;

							// Random spawn location
							var meteorSpawnLocation = GetNode<PathFollow2D>("MeteorPath/MeteorSpawnLocation");
							meteorSpawnLocation.ProgressRatio = GD.Randf();

							// Random direction
							float direction = meteorSpawnLocation.Rotation + Mathf.Pi / 2;
							direction += (float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);

							// Velocity
							var velocity = new Vector2((float)GD.RandRange(150.0, 250.0), 0);

							MeteorSpawnParams meteorParams = new()
                            {
								Position = meteorSpawnLocation.Position,
								RotationDirection = direction,
								Velocity = velocity
							};

							spawner.EntityCreator.SetSpawnSpecs(meteorParams);

							spawner.EntityCreator.Spawn(meteor, this);
						};
					}
					break;
				case EntityType.ShootingAlien:
					{
						spawner.OnTimeout = () =>
						{
							Alien alien = spawner.EntityCreator.EntityScene.Instantiate<Alien>();
							spawner.EntityCreator.SetHealthBar(alien, EntityHealthbarScene);
							spawner.EntityCreator.SetHealth(alien, 2,5, EntityHealthMultiplier);

							alien.EnitityHPDepleted += OnEntityHealthDepleted;
							alien.EntityHPValueChanged += OnEntityHealthValueChanged;

							var velocity = (float)GD.RandRange(150.0, 250.0);
							//alien.VelocityValue = velocity;
							float bulletDamage = 0f;
							bulletDamage += MathF.Round((float)(double)(alien.BulletDamage * AlienBulletDamageMultiplier), 2);
							//alien.BulletFireRate -= MathF.Round((float)(double)(alien.BulletFireRate * AlienFirerateMultiplier), 2);
							float firerate = 1f;
							firerate -= MathF.Round((float)(double)(alien.BulletFireRate * AlienFirerateMultiplier), 2);

							SpawnDirection spawnDirection = SpawnDirection.Horizontal;
							int spawnSide = GD.RandRange(0,1);
							Vector2 position = Vector2.Zero;

							int choosePath = GD.RandRange(0,1);
							switch (choosePath)
							{
								case 0: // horizonatal path
									{
										int randomHorizontalPath = GD.RandRange(0, _horizontalLines-1);
										var alienSpawnLocation = GetNode<Path2D>($"AlienPath2DHorizontal_{randomHorizontalPath}");

										position = alienSpawnLocation.Curve.GetPointPosition(spawnSide);
										spawnDirection = SpawnDirection.Horizontal;
										alien.GetNode<Sprite2D>("Sprite2D").FlipH = spawnSide == 1;
										
									}
									break;
								case 1: // vertical path
									{
										int randomVerticalPath = GD.RandRange(0, _verticalLines-1);
										var alienSpawnLocation = GetNode<Path2D>($"AlienPath2DVertical_{randomVerticalPath}");

										spawnDirection = SpawnDirection.Vertical;
										position = alienSpawnLocation.Curve.GetPointPosition(spawnSide);
									}
									break;
							}

							AlienSpawnParams alienSpawnParams = new()
							{
								VelocityValue = velocity,
								Position = position,
								SpawnDirection = spawnDirection,
								SpawnSide = spawnSide,
								AlienBulletDamage = bulletDamage,
								AlienFirerate = firerate
							};
							spawner.EntityCreator.SetSpawnSpecs(alienSpawnParams);

							spawner.EntityCreator.Spawn(alien, this);
						};
					}
					break;
				case EntityType.SeekingAlien:
					{
						spawner.OnTimeout = async () =>
						{
							SeekingAlien seekingAlien = spawner.EntityCreator.EntityScene.Instantiate<SeekingAlien>();
							spawner.EntityCreator.SetHealthBar(seekingAlien, EntityHealthbarScene);
							spawner.EntityCreator.SetHealth(seekingAlien, 2,5, EntityHealthMultiplier);	

							seekingAlien.EnitityHPDepleted += OnEntityHealthDepleted;
							seekingAlien.EntityHPValueChanged += OnEntityHealthValueChanged;

							float velocityValue = (float)GD.RandRange(150.0, 250.0);

							float randomSpawnXPosition = (float)GD.RandRange(20, _screenSize.X - 20);
							float randomSpawnYPosition = (float)GD.RandRange(20, _screenSize.Y - 20);
							Vector2 spawnPosition = new Vector2(randomSpawnXPosition, randomSpawnYPosition);

							float direction = seekingAlien.Rotation + Mathf.Pi / 2;
							direction += (float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);

							SeekingAlienParams seekingAlienParams = new()
							{
								VelocityValue = velocityValue,
								Position = spawnPosition,
								RotationDirection = direction,
								Player = _player
							};

							spawner.EntityCreator.SetSpawnSpecs(seekingAlienParams);

							EntitySpawnAnimation.Position = spawnPosition;
							EntitySpawnAnimation.Play();

							await ToSignal(EntitySpawnAnimation, AnimatedSprite2D.SignalName.AnimationFinished);

							spawner.EntityCreator.Spawn(seekingAlien, this);
						};
						
					}
					break;
			}
		}
		//InstantiateAlienPaths();
	}

	public void BeginSpawning(List<EntityType> entityTypes)
	{
		foreach(var spawner in EntitySpawnTimers.Where(spawner => entityTypes.Contains(spawner.EntityCreator.EntityType)))
		{
			GD.Print("----------------------Spawning: "+spawner.EntityCreator.EntityType);
			spawner.Start();
		}
	}

	public void StopSpawning()
	{
		foreach(var spawner in EntitySpawnTimers)
		{
			if (!spawner.SpawnTimer.IsStopped())
			{
				spawner.Stop();
			}
		}
	}

	public void RestComponent()
	{
		EntitySpawnTimers.ForEach(timer =>
		{
			timer.ResetWaitTime();
		});
		EntityHealthMultiplier = 0d;
		AlienFirerateMultiplier = 0;
		AlienBulletDamageMultiplier = 0d;
	}

	public void IncreaseDifficulty()
	{
		EntitySpawnTimers.ForEach(timer =>
		{
			timer.ApplyNewWaitTime(timer.SpawnTimer.WaitTime - EnititySpawnDificultyMultiplier);
		});
		EntityHealthMultiplier += 0.5d;
		AlienFirerateMultiplier -= 0.1d;
		AlienBulletDamageMultiplier += 0.5d;
	}

	public void DestroyAllEntities()
	{
		GetTree().CallGroup("entities", Node.MethodName.QueueFree);
	}

	async private void SpawnAnimation(Vector2 position)
	{
		EntitySpawnAnimation.Position = position;
		EntitySpawnAnimation.Play();

		await ToSignal(EntitySpawnAnimation, AnimatedSprite2D.SignalName.AnimationFinished);
	}
	private void InstantiateAlienPaths()
    {
		 // horizontal
		for(int i = 0; i < _horizontalLines; i++)
		{
            Path2D path2D = new()
            {
                Name = "AlienPath2DHorizontal_" + i.ToString()
            };
            Curve2D curve2D = new();
			var yPosition = _horizontalLines * i;
			curve2D.AddPoint(new Vector2(0, yPosition)); // left point
			curve2D.AddPoint(new Vector2(_screenSize.X, yPosition)); // right point

			path2D.Curve = curve2D;
			path2D.MoveLocalY(yPosition);

            PathFollow2D pathFollow2D = new()
            {
                Name = "AlienPathFollow2DHorizontal_" + i.ToString()
            };
            path2D.AddChild(pathFollow2D);

			AddChild(path2D);
		}

		//vertical
		for(int i = 0; i < _verticalLines; i++)
        {
            Path2D path2D = new()
            {
                Name = "AlienPath2DVertical_" + i.ToString()
            };
            Curve2D curve2D = new();
			var xPosition = _verticalLines * i;
			curve2D.AddPoint(new Vector2(xPosition, 0)); // up point
			curve2D.AddPoint(new Vector2(xPosition, _screenSize.Y)); // dowin point

			path2D.Curve = curve2D;
			path2D.MoveLocalX(xPosition);

            PathFollow2D pathFollow2D = new()
            {
                Name = "AlienPathFollow2DVertical_" + i.ToString()
            };
            path2D.AddChild(pathFollow2D);

			AddChild(path2D);
        }
    }

	private void OnEntityHealthDepleted(Entity entity)
	{
		OnEntitySpawnerEntityHealthDepleted?.Invoke(entity);
	}
	private void OnEntityHealthValueChanged(Entity entity)
	{
		OnEntitySpawnerEntityHealthValueChanged?.Invoke(entity);
	}
}
