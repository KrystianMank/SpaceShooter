using Godot;
using System;
using System.Collections.Generic;

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
	//public EntitySpawnTimer MeteorSpawnTimer, AlienSpawnTimer;
	public List<EntitySpawnTimer> EntitySpawnTimers = [];

	public Action<Entity> OnEntitySpawnerEntityHealthDepleted, OnEntitySpawnerEntityHealthValueChanged;

	private int _horizontalLines;
	private int _verticalLines;
	private Vector2 _screenSize;
	public void CreateEntitySpawners()
	{
		foreach(var resource in EntityResources)
		{
			EntitySpawnTimer timer = new(resource.InitialSpawnTime)
			{ 
				EntityCreator = resource.CreateEntityCreator() 
			};
			EntitySpawnTimers.Add(timer);
			AddChild(timer.SpawnTimer);
		}
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		CreateEntitySpawners();
		foreach(var spawner in EntitySpawnTimers)
		{
			switch (spawner.EntityCreator.EntityType)
			{
				case GameEnums.EntityType.Meteor:
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

							meteor.Position = meteorSpawnLocation.Position;

							// Random direction
							float direction = meteorSpawnLocation.Rotation + Mathf.Pi / 2;
							direction += (float)GD.RandRange(-Mathf.Pi / 4, Mathf.Pi / 4);
							meteor.Rotation = direction;

							// Velocity
							var velocity = new Vector2((float)GD.RandRange(150.0, 250.0), 0);
							meteor.Velocity = velocity;

							spawner.EntityCreator.Spawn(meteor, this);
						};
					}
					break;
				case GameEnums.EntityType.ShootingAlien:
					{
						spawner.OnTimeout = () =>
						{
							Alien alien = spawner.EntityCreator.EntityScene.Instantiate<Alien>();
							spawner.EntityCreator.SetHealthBar(alien, EntityHealthbarScene);
							spawner.EntityCreator.SetHealth(alien, 2,5, EntityHealthMultiplier);

							alien.EnitityHPDepleted += OnEntityHealthDepleted;
							alien.EntityHPValueChanged += OnEntityHealthValueChanged;

							var velocity = (float)GD.RandRange(150.0, 250.0);
							alien.VelocityValue = velocity;
							alien.BulletDamage += 1;//_nextTresholdIndex % 5 == 0 ? 0.5 : 0;
							alien.BulletFireRate -= 1;//_nextTresholdIndex % 5 == 0 ? 0.1 : 0;

							int choosePath = GD.RandRange(0,1);
							switch (choosePath)
							{
								case 0: // horizonatal path
									{
										alien.SpawnDirection = SpawnDirection.Horizontal;
										int randomHorizontalPath = GD.RandRange(0, _horizontalLines-1);
										var alienSpawnLocation = GetNode<Path2D>($"AlienPath2DHorizontal_{randomHorizontalPath}");

										int spawnSide = GD.RandRange(0,1);
										alien.SpawnSide = spawnSide;
										alien.Position = alienSpawnLocation.Curve.GetPointPosition(spawnSide);
										alien.GetNode<Sprite2D>("Sprite2D").FlipH = spawnSide == 1;
										
									}
									break;
								case 1: // vertical path
									{
										alien.SpawnDirection = SpawnDirection.Vertical;

										int randomVerticalPath = GD.RandRange(0, _verticalLines-1);
										var alienSpawnLocation = GetNode<Path2D>($"AlienPath2DVertical_{randomVerticalPath}");

										int spawnSide = GD.RandRange(0,1);
										alien.SpawnSide = spawnSide;
										alien.Position = alienSpawnLocation.Curve.GetPointPosition(spawnSide);
									}
									break;
							}

							spawner.EntityCreator.Spawn(alien, this);
						};
					}
					break;
			}
		}

		_screenSize = GetViewport().GetVisibleRect().Size;
		_horizontalLines = (int)_screenSize.X / 30;
		_verticalLines = (int)_screenSize.Y / 24;
		InstantiateAlienPaths();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void BeginSpawning()
	{
		foreach(var spawner in EntitySpawnTimers)
		{
			spawner.Start();
		}
	}

	public void StopSpawning()
	{
		foreach(var spawner in EntitySpawnTimers)
		{
			spawner.Stop();
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
			GD.Print(timer.SpawnTimer.WaitTime - EnititySpawnDificultyMultiplier);
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


	private void InstantiateAlienPaths()
    {
		 // horizontal
		for(int i = 0; i < _horizontalLines; i++)
		{
			Path2D path2D = new();
			path2D.Name = "AlienPath2DHorizontal_"+i.ToString();
			Curve2D curve2D = new();
			var yPosition = _horizontalLines * i;
			curve2D.AddPoint(new Vector2(0, yPosition)); // left point
			curve2D.AddPoint(new Vector2(_screenSize.X, yPosition)); // right point

			path2D.Curve = curve2D;
			path2D.MoveLocalY(yPosition);

			PathFollow2D pathFollow2D = new();
			pathFollow2D.Name = "AlienPathFollow2DHorizontal_"+i.ToString();
			path2D.AddChild(pathFollow2D);

			AddChild(path2D);
		}

		//vertical
		for(int i = 0; i < _verticalLines; i++)
        {
            Path2D path2D = new();
			path2D.Name = "AlienPath2DVertical_"+i.ToString();
			Curve2D curve2D = new();
			var xPosition = _verticalLines * i;
			curve2D.AddPoint(new Vector2(xPosition, 0)); // up point
			curve2D.AddPoint(new Vector2(xPosition, _screenSize.Y)); // dowin point

			path2D.Curve = curve2D;
			path2D.MoveLocalX(xPosition);

			PathFollow2D pathFollow2D = new();
			pathFollow2D.Name = "AlienPathFollow2DVertical_"+i.ToString();
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
