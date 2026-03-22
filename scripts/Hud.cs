using System.Linq;
using GameEnums;
using GenericObervable;
using Godot;
using StaticClasses;
public partial class Hud : CanvasLayer
{
	[Signal]
	public delegate void StartGameEventHandler();
	[Signal]
	public delegate void PlayerStatsPanelReadyEventHandler(Player player, bool show);
	[Signal]
	public delegate void PlayerStatsUpgradePanelReadyEventHandler(Player player);
	[Export]
	public PackedScene MainMenuScene;

	public Main MainNode;
	public Label WaveTimeLabel;
	
	Vector2 entityVelocity = Vector2.Zero;
	private Player _player;

    public override void _Ready()
    {
        WaveTimeLabel = GetNode<Label>("WaveTimeLabel");
    }


    public void OnHudReady(Player player)
    {
		_player = player;
		MainNode = GetTree().Root.GetNode<Main>("Main");

		var healtBar = GetNode<PlayerHealthBar>(nameof(PlayerHealthBar));
		healtBar.SetHealthBarMaxValue(_player.playerStats.MaxHealth.Value);
		healtBar.SetHealthBarValue(_player.playerStats.Health.GetHP().Value);
		_player.playerStats.MaxHealth.Changed += OnPlayerMaxHPValueChanged;
		_player.playerStats.Health.HPValueChanged += OnPlayerHPValueChanged;
		
    }
	void OnPlayerHPValueChanged(double value)
    {
        var healtBar = GetNode<PlayerHealthBar>(nameof(PlayerHealthBar));
		healtBar.SetHealthBarValue(value);
		
    }
	void OnPlayerMaxHPValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
        GetNode<PlayerHealthBar>(nameof(PlayerHealthBar)).SetHealthBarMaxValue(eventArgs.NewValue);
    }

	public void ShowPlayerStatsPanel(bool show)
    {
        var player = GetParent().GetNode<Player>("Player");
		EmitSignal(SignalName.PlayerStatsPanelReady, player, show);
    }
	public void OnShowPlayerStatsUpgradePanelReady(Player player)
    {
        EmitSignal(SignalName.PlayerStatsUpgradePanelReady, player);
    }

	public void ShowMessage(string text)
	{
		var message = GetNode<Label>("Message");
		message.Text = text;
		message.Show();

		GetNode<Timer>("MessageTimer").Start();
	}

	async public void ShowGameOver()
    {
		ShowMessage("Game Over!");

		var messageTimer = GetNode<Timer>("MessageTimer");
		await ToSignal(messageTimer, Timer.SignalName.Timeout);

		var message = GetNode<Label>("Message");
		message.Text = "Space Shooter";
		message.Show();

		await ToSignal(GetTree().CreateTimer(1.0), Timer.SignalName.Timeout);
		GetNode<Button>("StartButton").Show();
    }

	public void SetNewLVBarLevel(int level, int oldScoreTreshold, int newScoreTreshold)
	{
		var lvBar = GetNode<LvBar>("LVBar");
		lvBar.SetLVBarMaxValue(oldScoreTreshold, newScoreTreshold);
		lvBar.SetLVLabel(level);
	}

	public void UpdateScore(int score)
	{
		var lvBar = GetNode<LvBar>("LVBar");
		lvBar.SetLVBarValue(score);
		//GetNode<Label>("ScoreLabel").Text = score.ToString();
	}

	public void OnStartButtonPressed()
	{
		GetNode<Button>("StartButton").Hide();
		EmitSignal(SignalName.StartGame);
		
	}

	public void OnMainMenuButtonPressed()
    {
        GetNode<MainMenuPanel>(nameof(MainMenuPanel)).Visible = true;
		Engine.TimeScale = 0;
    }

	public void OnMessageTimerTimeout()
	{
		GetNode<Label>("Message").Hide();
	}

	public void UpdateWaveTimeLabelText(double time)
	{
		WaveTimeLabel.Text = time.ToString("00:00");
	}

	public void ShowPlayerUpgradeStatsPanel(bool show)
	{
		GetNode<CanvasLayer>("StatsUpgradePanel").Visible = show;
		foreach (var child in GetTree().Root.GetChildren())
		{
			if(child is Entity entity)
            {
				// if(entity.LinearVelocity != Vector2.Zero)
				// {
				// 	entityVelocity = entity.LinearVelocity;
				// }
                // if (show)
                // {
				// 	entity.LinearVelocity = Vector2.Zero;
                // }
                // else
                // {
				// 	entity.LinearVelocity = entityVelocity;
                // }
				entity.SetPhysicsProcess(!show);
            }
			if(child is Main main)
			{
				foreach(var mainChild in main.GetChildren())
				{
					mainChild.SetProcess(!show);
				}
			}
		}
		SetAllTimersEnabled(!show);
		var laser = (Laser)GetTree().GetFirstNodeInGroup("laser");
		if(laser != null) laser.IsCasting = false;
	}

	public void SetAllTimersEnabled(bool enabled)
	{
		var timers = GetTree().GetNodesInGroup("timers");
		foreach (Timer timer in timers.Cast<Timer>())
		{
			timer.ProcessMode = enabled ? Node.ProcessModeEnum.Inherit : Node.ProcessModeEnum.Disabled;
		}
	}

	public void OnCloseButtonPicked()
	{
		ShowPlayerUpgradeStatsPanel(false);

		MainNode.WaveTimer.Start();
		MainNode.MainWaveManager.WaveManager(MainNode.CurrentWaveIndex);
	}
}