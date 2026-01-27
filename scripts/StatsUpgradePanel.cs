using System.Collections.Generic;
using Godot;
using GameEnums;
using System.Linq;
using StaticClasses;
using GenericObervable;
public partial class StatsUpgradePanel : CanvasLayer
{
	[Signal]
	public delegate void Option1PickedEventHandler();
	[Signal]
	public delegate void Option2PickedEventHandler();
	
	Dictionary<UpgradableStatsEnum, double> UpgradableStats;
	Dictionary<UpgradableStatsEnum, double> DrawnUpgradableStats = new();
	public Button OptionButton1, OptionButton2, HealButton, RefreshButton;
	public AnimatedSprite2D Animation;
	private PlayerStats _playerStats;
	private UpgradableStatsEnum _pickedUpgradableStatsEnum;
	private PanelContainer _container;
	private double _pickedValue;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		OptionButton1 = GetNode<Button>("Animation/PanelContainer/MarginContainer/Control/GridContainer/Option_1_Button");
		OptionButton2 = GetNode<Button>("Animation/PanelContainer/MarginContainer/Control/GridContainer/Option_2_Button");
		HealButton = GetNode<Button>("Animation/PanelContainer/MarginContainer/Control/GridContainer2/HealButton");
		RefreshButton = GetNode<Button>("Animation/PanelContainer/MarginContainer/Control/GridContainer2/RefreshStatsButton");
		_container = GetNode<PanelContainer>("Animation/PanelContainer");
		Animation = GetNode<AnimatedSprite2D>("Animation");
		UpgradableStats = new Dictionary<UpgradableStatsEnum, double>
        {
            {UpgradableStatsEnum.Speed, PlayerStatsMultipliers.SPEED_MULTIPLIER},
			{UpgradableStatsEnum.FireRate, PlayerStatsMultipliers.FIRERATE_MULTIPLIER},
			{UpgradableStatsEnum.Luck, PlayerStatsMultipliers.LUCK_MULTIPLIER},
			{UpgradableStatsEnum.BulletSpeed, PlayerStatsMultipliers.BULLET_SPEED_MULTIPLIER},
			{UpgradableStatsEnum.Damage, PlayerStatsMultipliers.DAMAGE_MULTIPLIER},
			{UpgradableStatsEnum.MaxHealth, PlayerStatsMultipliers.MAX_HEALTH_MULTIPLIER},
			{UpgradableStatsEnum.PiercingPowerupDuration, PlayerStatsMultipliers.PIERCING_POWERUP_DURATION_MULTIPLIER},
			{UpgradableStatsEnum.InvincibilityPowerupDuration, PlayerStatsMultipliers.INVINCIBILITY_POWERUP_DURATION_MULTIPLIER},
			{UpgradableStatsEnum.MultishotPowerupDuration, PlayerStatsMultipliers.MULTISHOT_POWERUP_DURATION_MULTIPLIER},
			{UpgradableStatsEnum.DashPowerupDuration, PlayerStatsMultipliers.DASH_POWERUP_DURATION_MULTIPLIER},
			{UpgradableStatsEnum.RocketsPowerupDuration, PlayerStatsMultipliers.ROCKETS_POWERUP_DURATION_MULTIPLIER}
        };
    }
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("close_upgrade_panel"))
        {
            EmitSignal(SignalName.Option1Picked);
        }
    }

	public void OnPlayerStatsUpgradePanelReady(Player player)
    {
		
        _playerStats = player == null 
			? GetTree().Root.GetNode<Main>("Main").GetNode<Player>("Player").playerStats
			: player.playerStats;
		_playerStats.SkillPoints.Changed += OnPlayerStatsSkillPointsChanged;
    }
	async public void OnVisibilityChanged()
    {
		//_container.Visible = false;
        GenererateNewStats();

		DisableButton(CallerButton.Option1, DrawnUpgradableStats.Keys.First(), DrawnUpgradableStats[DrawnUpgradableStats.Keys.First()]);
		DisableButton(CallerButton.Option2, DrawnUpgradableStats.Keys.Last(), DrawnUpgradableStats[DrawnUpgradableStats.Keys.Last()]);

		GetNode<Label>("Animation/PanelContainer/MarginContainer/Control/SkillPointsLabel").Text = $"Skill points: {_playerStats.SkillPoints.Value}";

		DisableHealButton();
		
		Animation.Frame = 0;
		Animation.Play();

		await ToSignal(Animation, AnimatedSprite2D.SignalName.AnimationFinished);

		_container.Visible = true;
	}
	void GenererateNewStats()
    {
        DrawnUpgradableStats = UpgradableStats.OrderBy(x => GD.Randi()).Take(2).ToDictionary();
		OptionButton1.Text = DrawnUpgradableStats.Keys.First().ToString();
		OptionButton2.Text = DrawnUpgradableStats.Keys.Last().ToString();

		OptionButton1.TooltipText = $"+{DrawnUpgradableStats[DrawnUpgradableStats.Keys.First()]}";
		OptionButton2.TooltipText = $"+{DrawnUpgradableStats[DrawnUpgradableStats.Keys.Last()]}";

		Animation.Scale = _container.Scale;
    }
	void DisableButton(CallerButton callerButton, bool disable)
    {
        switch (callerButton){
			case CallerButton.Option1:
				{
					OptionButton1.Disabled = disable;
				}
			break;
			case CallerButton.Option2:
                {
                    OptionButton2.Disabled = disable;
                }
				break;
			case CallerButton.HealButton:
                {
                    HealButton.Disabled = disable;
                }
			break;

			case CallerButton.RefreshButton:
                {
                    RefreshButton.Disabled = disable;
                }
			break;
		}
    }
	void DisableButton(CallerButton callerButton, UpgradableStatsEnum upgradableStatsEnum, double value)
    {
        bool shouldDisable = false;
        switch (upgradableStatsEnum)
        {
            case UpgradableStatsEnum.Speed:
                {
					if(_playerStats.Speed.Value + value > (int)TresholdValues.MAX_SPEED)
						
						shouldDisable = true;
                    break;
                }
			case UpgradableStatsEnum.FireRate:
                {
					if(_playerStats.FireRate.Value - value< TresholdValues.MIN_FIRERATE)

						shouldDisable = true;
                    break;
                }
			case UpgradableStatsEnum.Luck:
                {
					if(_playerStats.Luck.Value + value> TresholdValues.MAX_LUCK)
						shouldDisable = true;
                    break;
                }
			case UpgradableStatsEnum.BulletSpeed:
                {
					if(_playerStats.BulletSpeed.Value + value > TresholdValues.MAX_BULLET_SPEED)
						shouldDisable = true;
                    break;
                }
			case UpgradableStatsEnum.Damage:
                {
                    if(_playerStats.Damage.Value + value > TresholdValues.MAX_DAMAGE)
						shouldDisable = true;
                }
				break;
			case UpgradableStatsEnum.Health:
                {
                    if(_playerStats.Health.GetHP().Value + value >= _playerStats.MaxHealth.Value)
						shouldDisable = true;
                }
				break;
			case UpgradableStatsEnum.MaxHealth:
                {
                    if(_playerStats.MaxHealth.Value + value > TresholdValues.MAX_HEALTH)
						shouldDisable = true;
                }
				break;
			case UpgradableStatsEnum.InvincibilityPowerupDuration:
                {
					if(_playerStats.InvincibilityPowerupDuration.Value + value> TresholdValues.MAX_INVINCIBILITY_POWERUP_DURATION)
						shouldDisable = true;
                    break;
                }
			case UpgradableStatsEnum.PiercingPowerupDuration:
                {
					if(_playerStats.PiercingPowerupDuration.Value + value> TresholdValues.MAX_PIERCING_POWERUP_DURATION)

						shouldDisable = true;
                    break;
                }
			case UpgradableStatsEnum.MultishotPowerupDuration:
                {
					if(_playerStats.MultishotPowerupDuration.Value + value> TresholdValues.MAX_MULTISHOT_POWERUP_DURATION)

						shouldDisable = true;
                    break;
                }
			case UpgradableStatsEnum.DashPowerupDuration:
                {
					if(_playerStats.DashPowerupDuration.Value + value> TresholdValues.MAX_DASH_POWERUP_DURATION)

						shouldDisable = true;
                    break;
                }
       		case UpgradableStatsEnum.RocketsPowerupDuration:
                {
					if(_playerStats.RocketsPowerupDuration.Value + value> TresholdValues.MAX_ROCKETS_POWERUP_DURATION)

						shouldDisable = true;
                    break;
                }
		}
		DisableButton(callerButton, shouldDisable);
    }
	void UpgradePlayerStats(UpgradableStatsEnum upgradableStatsEnum, double value)
    {
        switch (upgradableStatsEnum)
        {
            case UpgradableStatsEnum.Speed:
                {
					_playerStats.Speed.Value += (int)value;
                    break;
                }
			case UpgradableStatsEnum.FireRate:
                {
					_playerStats.FireRate.Value -= value;
                    break;
                }
			case UpgradableStatsEnum.Luck:
                {
					_playerStats.Luck.Value += value;
                    break;
                }
			case UpgradableStatsEnum.BulletSpeed:
                {
					_playerStats.BulletSpeed.Value += (int)value;
                    break;
                }
			case UpgradableStatsEnum.Damage:
				{
					_playerStats.Damage.Value += value;
					break;	
				}
			case UpgradableStatsEnum.Health:
                {
                    _playerStats.Health.GetHP().Value += value;
					if(_playerStats.Health.GetHP().Value > _playerStats.MaxHealth.Value)
                    {
                        _playerStats.Health.GetHP().Value = Mathf.Min(_playerStats.Health.GetHP().Value, _playerStats.MaxHealth.Value);
                    }
                }
				break;
			case UpgradableStatsEnum.MaxHealth:
                {
                    _playerStats.MaxHealth.Value += value;
                }
				break;
			case UpgradableStatsEnum.InvincibilityPowerupDuration:
                {
					_playerStats.InvincibilityPowerupDuration.Value += value;
                    break;
                }
			case UpgradableStatsEnum.PiercingPowerupDuration:
                {
					_playerStats.PiercingPowerupDuration.Value += value;
                    break;
                }
			case UpgradableStatsEnum.MultishotPowerupDuration:
                {
					_playerStats.MultishotPowerupDuration.Value += value;
                    break;
                }
			case UpgradableStatsEnum.DashPowerupDuration:
                {
					_playerStats.PiercingPowerupDuration.Value += value;
                    break;
                }
			case UpgradableStatsEnum.RocketsPowerupDuration:
				{
					_playerStats.RocketsPowerupDuration.Value += value;
					break;
				}
        }
		_playerStats.SkillPoints.Value--;
    }

	void DisableHealButton()
    {
        if(_playerStats.Health.GetHP().Value >= _playerStats.MaxHealth.Value)
        {
            DisableButton(CallerButton.HealButton, true);
        }
        else
        {
            DisableButton(CallerButton.HealButton, false);
        }
    }

	async public void OnOption1ButtonPressed()
	{
		_container.Visible = false;
		Animation.Frame = 4;
		Animation.PlayBackwards();

		await ToSignal(Animation, AnimatedSprite2D.SignalName.AnimationFinished);

		_pickedUpgradableStatsEnum = DrawnUpgradableStats.Keys.First();
		_pickedValue = DrawnUpgradableStats[DrawnUpgradableStats.Keys.First()];
		if(_playerStats.SkillPoints.Value > 0)
			UpgradePlayerStats(_pickedUpgradableStatsEnum, _pickedValue);

		EmitSignal(SignalName.Option1Picked);
	}
	async public void OnOption2ButtonPressed()
	{
		_container.Visible = false;
		Animation.Frame = 4;
		Animation.PlayBackwards();

		await ToSignal(Animation, AnimatedSprite2D.SignalName.AnimationFinished);

		_pickedUpgradableStatsEnum = DrawnUpgradableStats.Keys.Last();
		_pickedValue = DrawnUpgradableStats[DrawnUpgradableStats.Keys.Last()];
		if(_playerStats.SkillPoints.Value > 0)
			UpgradePlayerStats(_pickedUpgradableStatsEnum, _pickedValue);

		EmitSignal(SignalName.Option2Picked);
	}
	async public void OnCloseButtonPressed()
    {	
		_container.Visible = false;
		Animation.Frame = 4;
		Animation.PlayBackwards();

		await ToSignal(Animation, AnimatedSprite2D.SignalName.AnimationFinished);
        
		EmitSignal(SignalName.Option1Picked);
    }
	public void OnRefreshButtonPressed()
    {
		GenererateNewStats();
        _playerStats.SkillPoints.Value--;
    }

	public void OnHealButtonPressed()
    {
        var healValue = _playerStats.MaxHealth.Value * 0.5; // heal multipier
		DisableButton(CallerButton.HealButton, UpgradableStatsEnum.Health, healValue);
		UpgradePlayerStats(UpgradableStatsEnum.Health, healValue);
	}

	public void OnHealButtonMouseEntered()
    {
        var healValue = _playerStats.MaxHealth.Value * 0.5;
		HealButton.TooltipText = $"-1 Skill point\n+{healValue} HP";
    }

	public void OnPlayerStatsSkillPointsChanged(object target, Observable<int>.ChanedEventArgs eventArgs)
    {
        GetNode<Label>("Animation/PanelContainer/MarginContainer/Control/SkillPointsLabel").Text = $"Skill points: {eventArgs.NewValue}";
		if(eventArgs.NewValue <= 0)
        {
            DisableButton(CallerButton.Option1, true);
			DisableButton(CallerButton.Option2, true);
			DisableButton(CallerButton.HealButton, true);
			DisableButton(CallerButton.RefreshButton, true);
        }
        else
        {
            DisableButton(CallerButton.Option1, DrawnUpgradableStats.Keys.First(), DrawnUpgradableStats[DrawnUpgradableStats.Keys.First()]);
			DisableButton(CallerButton.Option2, DrawnUpgradableStats.Keys.Last(), DrawnUpgradableStats[DrawnUpgradableStats.Keys.Last()]);
			DisableButton(CallerButton.HealButton, false);
			DisableButton(CallerButton.RefreshButton, false);
		}
		DisableHealButton();
    }

}
public enum CallerButton
{
    Option1,
	Option2,
	HealButton,
	RefreshButton
}