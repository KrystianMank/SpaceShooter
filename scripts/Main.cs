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
	public EntitySpawnerComponent EntitySpawner;
	[Export]
	public Vector2 ScreenSize;
	public Player PlayerNode;
	public float BackgroundSpeed = 100f;
	public Observable<int> Score = new Observable<int>();

	public TextureRect Background;
	public Vector2 BackgroundStartPosition;
	public float BackgroundRepeatHeight;

	private bool _increaseDificulty = false;
	private int[] _difficultyTresholds = [5,15, 25, 50, 75, 100, 125, 150, 200, 250, 300, 400, 500,650,800, 1000, 1200, 1500, 2000, 2250];
	private int _nextTresholdIndex = 0;


	private bool _toogleStatsPanelView = false;
	// Called when the node enters the scene tree for the first time.

	public override void _Ready()
    {
		PlayerNode = GetNode<Player>("Player");
        Background = GetNode<TextureRect>("Background");
		BackgroundStartPosition = Background.Position;
		BackgroundRepeatHeight = Background.Size.Y / 2;

		ScreenSize = PlayerNode.ScreenSize;

		Score.Changed += OnScoreValueChanged;

		//InstantiateAlienPaths();

		// Subscribtion to Action<Entity> delegates
		EntitySpawner.OnEntitySpawnerEntityHealthDepleted += OnEntitySpawnerEntityHealthDepleted;
		EntitySpawner.OnEntitySpawnerEntityHealthValueChanged += OnEntitySpawnerEntityHealtValueChanged;
    }

	public override void _Process(double delta)
	{
		Background.Position += Vector2.Up * BackgroundSpeed * (float)delta;
		if (Background.Position.Y < BackgroundStartPosition.Y - BackgroundRepeatHeight)
		{
			var bgRepeat = new Vector2(Background.Position.X, BackgroundStartPosition.Y);
			Background.SetPosition(bgRepeat);
		}
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if(@event is InputEventKey eventKey)
		{
			if(eventKey.Pressed && eventKey.Keycode == Key.Tab)
			{
				_toogleStatsPanelView = !_toogleStatsPanelView;
            	GetNode<Hud>("HUD").ShowPlayerStatsPanel(_toogleStatsPanelView);
			}
		}
    }



	public void NewGame()
	{
		Score.Value = 0;
		var startPosition = GetNode<Marker2D>("StartPosition");

		PlayerNode.Start(startPosition.Position);
		EmitSignal(SignalName.HudReady, GetNode<Player>("Player"));

		var hud = GetNode<Hud>("HUD");
		hud.ShowMessage("Get Ready!");

		EntitySpawner.DestroyAllEntities();
		EntitySpawner.RestComponent();

		GetNode<Timer>("StartTimer").Start();

		GetNode<AudioStreamPlayer2D>("BackgroundMusic").Play();

		_increaseDificulty = false;
		_nextTresholdIndex = 0;

		hud.SetNewLVBarLevel(_nextTresholdIndex + 1,0 , _difficultyTresholds[_nextTresholdIndex]);
		hud.UpdateScore(Score.Value);
	}

	public void GameOver()
	{
		GetNode<Hud>("HUD").ShowGameOver();
		EntitySpawner.StopSpawning();

		EntitySpawner.DestroyAllEntities();

		GetNode<AudioStreamPlayer2D>("BackgroundMusic").Stop();
		GetNode<AudioStreamPlayer2D>("DeathSound").Play();
    }

	public void StartTimerTimeout()
    {
		EntitySpawner.BeginSpawning();
    }

	public void OnEntitySpawnerEntityHealthDepleted(Entity entity)
    {
		if(!entity.EntitiesHitEachOther){
			Score.Value += (int)entity.EntityMaxHP.Value;
			GetNode<Hud>("HUD").UpdateScore(Score.Value);
		}

		entity.EnityDeath();
	
    }

	public void OnEntitySpawnerEntityHealtValueChanged(Entity entity)
    {
		entity.SetHealthBarValue(entity.EntityHP.GetHP().Value);
    }

	public void OnScoreValueChanged(object target, Observable<int>.ChanedEventArgs eventArgs)
    {
		if (_nextTresholdIndex < _difficultyTresholds.Length
			&& Score.Value >= _difficultyTresholds[_nextTresholdIndex])
		{
			_increaseDificulty = true;
			_nextTresholdIndex++;

			if((_nextTresholdIndex + 1) % 4 == 0)
			{
				EntitySpawner.IncreaseDifficulty();
			}

			GetNode<Hud>("HUD").SetNewLVBarLevel(_nextTresholdIndex + 1,_difficultyTresholds[_nextTresholdIndex - 1] ,_difficultyTresholds[_nextTresholdIndex]);

			PlayerNode.AddSkillPoint();
			EmitSignal(SignalName.ShowPlayerStatsUpgradePanelReady, PlayerNode);
			EmitSignal(SignalName.ShowPlayerStatsUpgradePanel, true);
		}
        else
        {
            _increaseDificulty = false;
        }
    }
	
}
