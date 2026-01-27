using Godot;
using GenericObervable;
public partial class Main : Node
{
	[Signal]
	public delegate void HudReadyEventHandler(Player player);
	[Signal]
	public delegate void ShowPlayerStatsUpgradePanelEventHandler(bool show);
	[Signal]
	public delegate void ShowPlayerStatsUpgradePanelReadyEventHandler(Player player);
	[Export]
	public PackedScene MeteorScene { get; set; }
	[Export]
	public PackedScene PlayerScene { get; set; }
	[Export]
	public PackedScene AlienScene{get;set;}
	[Export]
	public PackedScene EntityHealthbarScene;
	[Export]
	public Vector2 ScreenSize;
	public Player PlayerNode;
	public float BackgroundSpeed = 100f;
	public Observable<int> Score = new Observable<int>();
	public TextureRect Background;
	public Vector2 BackgroundStartPosition;
	public float BackgroundRepeatHeight;

	private double _enititySpawnDificultyMultiplier = 0.1d;
	private double _entityHealthMultipier = 0d;
	private bool _increaseDificulty = false;
	private int[] _difficultyTresholds = [5,15, 25, 50, 75, 100, 125, 150, 200, 250, 300, 400, 500,650,800, 1000, 1200, 1500, 2000, 2250];
	private int _nextTresholdIndex = 0;


	private int _horizontalLines;
	private int _verticalLines;
	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
    {
		PlayerNode = GetNode<Player>("Player");
        Background = GetNode<TextureRect>("Background");
		BackgroundStartPosition = Background.Position;
		BackgroundRepeatHeight = Background.Size.Y / 2;

		ScreenSize = GetNode<Player>("Player").ScreenSize;
		_horizontalLines = (int)ScreenSize.Y / 30;
		_verticalLines = (int)ScreenSize.X / 24;

		Score.Changed += OnScoreValueChanged;

		InstantiateAlienPaths();
    }

	public override void _Process(double delta)
	{
		Background.Position += Vector2.Up * BackgroundSpeed * (float)delta;
		if (Background.Position.Y < BackgroundStartPosition.Y - BackgroundRepeatHeight)
		{
			var bgRepeat = new Vector2(Background.Position.X, BackgroundStartPosition.Y);
			Background.SetPosition(bgRepeat);
		}

        if (Input.IsActionPressed("show_stats_panel"))
        {
            GetNode<Hud>("HUD").ShowPlayerStatsPanel(true);
        }
        else
        {
            GetNode<Hud>("HUD").ShowPlayerStatsPanel(false);
        }
    }


	public void NewGame()
	{
		Score.Value = 0;
		var player = GetNode<Player>("Player");
		var startPosition = GetNode<Marker2D>("StartPosition");

		player.Start(startPosition.Position);
		EmitSignal(SignalName.HudReady, GetNode<Player>("Player"));

		GetNode<Timer>("MeteorSpawnTimer").WaitTime = 1d;

		var hud = GetNode<Hud>("HUD");
		hud.ShowMessage("Get Ready!");

		GetTree().CallGroup("entities", Node.MethodName.QueueFree);

		GetNode<Timer>("StartTimer").Start();

		GetNode<AudioStreamPlayer2D>("BackgroundMusic").Play();

		_increaseDificulty = false;
		_entityHealthMultipier = 0.5;
		_nextTresholdIndex = 0;

		hud.SetNewLVBarLevel(_nextTresholdIndex + 1,0 , _difficultyTresholds[_nextTresholdIndex]);
		hud.UpdateScore(Score.Value);
	}

	public void GameOver()
	{
		GetNode<Hud>("HUD").ShowGameOver();
		GetNode<Timer>("MeteorSpawnTimer").Stop();
		GetNode<Timer>("AlienSpawnTimer").Stop();

		GetTree().CallGroup("entities", Node.MethodName.QueueFree);

		GetNode<AudioStreamPlayer2D>("BackgroundMusic").Stop();
		GetNode<AudioStreamPlayer2D>("DeathSound").Play();
    }

	public void MetorSpawnTimeout()
	{
		// Instantiating the meteor
		Meteor meteor = MeteorScene.Instantiate<Meteor>();

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

		meteor.PlayerStats = GetNode<Player>("Player").playerStats;

		meteor.SetEnityHealthRandom(1 + _entityHealthMultipier, 3 + _entityHealthMultipier);

		// Meteor's healthbar
		EntityHealthbar entityHealthbar = EntityHealthbarScene.Instantiate<EntityHealthbar>();
		meteor.AddChild(entityHealthbar);

		meteor.InitializeHealthBar();

		meteor.EnitityHPDepleted += OnEntityHealthDepleted;
		meteor.EntityHPValueChanged += OnEntityHealtValueChanged;

		AddChild(meteor);

		// meteor.SetHealthBarMaxValue(meteor.EntityHP.GetHP().Value);
		// meteor.SetHealthBarValue(meteor.EntityHP.GetHP().Value);
	}

	public void AlienSpawnTimeout()
    {
		Alien alien = AlienScene.Instantiate<Alien>();
		var velocity = (float)GD.RandRange(150.0, 250.0);
		alien.VelocityValue = velocity;
		alien.PlayerStats = GetNode<Player>("Player").playerStats;
		alien.BulletDamage += _nextTresholdIndex % 5 == 0 ? 0.5 : 0;
		alien.BulletFireRate -= _nextTresholdIndex % 5 == 0 ? 0.1 : 0;

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

		EntityHealthbar entityHealthbar = EntityHealthbarScene.Instantiate<EntityHealthbar>();
		alien.AddChild(entityHealthbar);

		alien.SetEnityHealthRandom(2 + _entityHealthMultipier,5 + _entityHealthMultipier);
		alien.InitializeHealthBar();

		alien.EnitityHPDepleted += OnEntityHealthDepleted;
		alien.EntityHPValueChanged += OnEntityHealtValueChanged;

		AddChild(alien);

		// alien.SetHealthBarMaxValue(alien.EntityHP.GetHP().Value);
		// alien.SetHealthBarValue(alien.EntityHP.GetHP().Value);
    }

	void InstantiateAlienPaths()
    {

		 // horizontal
		for(int i = 0; i < _horizontalLines; i++)
		{
			Path2D path2D = new();
			path2D.Name = "AlienPath2DHorizontal_"+i.ToString();
			Curve2D curve2D = new();
			var yPosition = _horizontalLines * i;
			curve2D.AddPoint(new Vector2(0, yPosition)); // left point
			curve2D.AddPoint(new Vector2(ScreenSize.X, yPosition)); // right point

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
			curve2D.AddPoint(new Vector2(xPosition, ScreenSize.Y)); // dowin point

			path2D.Curve = curve2D;
			path2D.MoveLocalX(xPosition);

			PathFollow2D pathFollow2D = new();
			pathFollow2D.Name = "AlienPathFollow2DVertical_"+i.ToString();
			path2D.AddChild(pathFollow2D);

			AddChild(path2D);
        }

    }

	public void StartTimerTimeout()
    {
		GetNode<Timer>("MeteorSpawnTimer").Start();
		GetNode<Timer>("AlienSpawnTimer").Start();
    }

	public void OnEntityHealthDepleted(Entity entity)
    {
		if(!entity.EntitiesHitEachOther){
			Score.Value += (int)entity.EntityMaxHP.Value;
			GetNode<Hud>("HUD").UpdateScore(Score.Value);
		}

		entity.EnityDeath();
		
		if (_increaseDificulty)
		{
			var waitTime = GetNode<Timer>("MeteorSpawnTimer").WaitTime;
			waitTime -= waitTime * _enititySpawnDificultyMultiplier;
			GetNode<Timer>("MeteorSpawnTimer").WaitTime = waitTime;

			_increaseDificulty = false;
		}
    }

	public void OnEntityHealtValueChanged(Entity entity)
    {
		entity.SetHealthBarValue(entity.EntityHP.GetHP().Value);
    }

	// public void OnMeteorHit(Entity meteor, Node body)
	// {
	// 	if (body is Bullet)
	// 	{
	// 		meteor.EntityHP.HPDepleted += () =>
    //         {
    //           	Score.Value++;
	// 			GetNode<Hud>("HUD").UpdateScore(Score.Value);

	// 			if (_increaseDificulty)
	// 			{
	// 				var waitTime = GetNode<Timer>("MeteorSpawnTimer").WaitTime;
	// 				waitTime -= waitTime * _meteorSpawnDificultyMultenitity;
	// 				GetNode<Timer>("MeteorSpawnTimer").WaitTime = waitTime;

	// 				_increaseDificulty = false;
	// 			}  
    //         };
	// 	}
	// }

	public void OnScoreValueChanged(object target, Observable<int>.ChanedEventArgs eventArgs)
    {
		if (_nextTresholdIndex < _difficultyTresholds.Length
			&& Score.Value >= _difficultyTresholds[_nextTresholdIndex])
		{
			_increaseDificulty = true;
			_entityHealthMultipier += 0.5;
			_nextTresholdIndex++;

			GetNode<Hud>("HUD").SetNewLVBarLevel(_nextTresholdIndex + 1,_difficultyTresholds[_nextTresholdIndex - 1] ,_difficultyTresholds[_nextTresholdIndex]);

			GetNode<Player>("Player").AddSkillPoint();
			EmitSignal(SignalName.ShowPlayerStatsUpgradePanelReady, GetNode<Player>("Player"));
			EmitSignal(SignalName.ShowPlayerStatsUpgradePanel, true);
		}
        else
        {
            _increaseDificulty = false;
        }
    }
	
}
