using Godot;
using GameEnums;
using StaticClasses;
using System.Linq;

public partial class Player : Area2D
{
	[Signal]
	public delegate void PlayerHealthDepletedEventHandler();
	[Export]
	public Texture2D[] RocketTextures;
	[Export]
	public Texture2D PowerupContainerEmpty;
	[Export]
	public Godot.Timer PowerupTimer;
	[Export]
	public PlayerWeapon PlayerWeapon;
	// [Export]
	// public FiringComponent FiringComponent;
	// [Export]
	// public Texture2D[] BulletSprites;
	// [Export]
	// public PackedScene[] ProjectileScenes;
	
	public Vector2 ScreenSize;
	public TextureRect MainBackground;
	public int MainBackgroundMovementX = 100;

	public Marker2D LeftMarginWindow;


	public PlayerStats playerStats;
	public bool PlayerAlive = true;

	private Vector2 _velocity;
	private Sprite2D _rocketSprite;
	private AnimatedSprite2D _leftRightDashAnimation, _upDownDashAnimation, _dashBlinkAnimation;
	private bool _isDashActive = false;
	private Callable _powerupCallable;
	private PowerupEnum? _currentPowerup = null;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Hide();
		Monitoring = true;
		Monitorable = true;

		// New instance of PlayerStats
		playerStats = new PlayerStats(PlayerWeapon);

		// For moving the background
		ScreenSize = GetViewportRect().Size;
		MainBackground = GetParent().GetNode<TextureRect>("Background");
		LeftMarginWindow = GetParent().GetNode<Marker2D>("LeftMarginWindow");
		
		_leftRightDashAnimation = GetNode<AnimatedSprite2D>("DashAnimationLeftRight");
		_leftRightDashAnimation.SpriteFrames.SetAnimationLoop("default",false);
		_upDownDashAnimation = GetNode<AnimatedSprite2D>("DashAnimationUpDown");
		_upDownDashAnimation.SpriteFrames.SetAnimationLoop("default", false);
		_dashBlinkAnimation = GetParent().GetNode<AnimatedSprite2D>("DashAnimationBlink");
		_dashBlinkAnimation.SpriteFrames.SetAnimationLoop("default", false);

		_rocketSprite = GetNode<Sprite2D>("Sprite2D");

		// // Setting up some event handlers
		// playerStats.FireRate.Value = 0.5d;
		// GetNode<Godot.Timer>("ShootCooldown").WaitTime = playerStats.FireRate.Value;

        playerStats.Health.HPDepleted += OnHealthDepleted;

		// Showing damage that player received
        GetNode<HurtboxComponent>(nameof(HurtboxComponent)).AreaEntered += (area2D) =>
        {
            if(area2D is HitboxComponent hitboxComponent)
            {
                ShowDamageLabel(hitboxComponent);
            }
        };

		PlayerWeapon.PlayerStats = playerStats;
    }



	// Called every frame. 'delta' is the elapsed time since the previous frame.
	async public override void _Process(double delta)
	{
		_velocity = Vector2.Zero;
		_rocketSprite.Texture = RocketTextures[2];
		//PlayerWeapon.FiringComponent.BulletSpawn.Position = GetNode<Marker2D>("BulletSpawn").GlobalPosition;

		if(PlayerAlive){
			if (Input.IsActionPressed("turn_left"))
			{
				_velocity.X -= 1;
				_rocketSprite.Texture = RocketTextures[0];

				// When the player moves LEFT, the background moves RIGHT and vice versa
				if (MainBackground.Position.X < LeftMarginWindow.Position.X)
				{
					MainBackground.Position += Vector2.Right * (float)delta * MainBackgroundMovementX;
				}
				else
				{
					MainBackground.Position = new Vector2(LeftMarginWindow.Position.X, MainBackground.Position.Y);
				}
			}

			if (Input.IsActionPressed("turn_right"))
			{
				_velocity.X += 1;
				_rocketSprite.Texture = RocketTextures[1];

				if (MainBackground.Position.X > -120f) // Maximum left position that makes right border of MainBackground in the same position as right border of the Window
				{
					MainBackground.Position += Vector2.Left * (float)delta * MainBackgroundMovementX;
				}
				else
				{
					MainBackground.Position = new Vector2(LeftMarginWindow.Position.X - 120f, MainBackground.Position.Y);
				}
			}

			if (Input.IsActionPressed("go_up"))
			{
				_velocity.Y -= 1;
			}
			if (Input.IsActionPressed("go_down"))
			{
				_velocity.Y += 1;
			}

			if (_velocity.Length() > 0)
			{
				_velocity = _velocity.Normalized() * playerStats.Speed.Value;
			}
			Position += _velocity * (float)delta;
		}
		// Player can't go outside the screen boundaries
		Position = new Vector2(
			x: Mathf.Clamp(Position.X, 0, ScreenSize.X),
			y: Mathf.Clamp(Position.Y, 0, ScreenSize.Y)
		);
		
		if (_isDashActive)
		{
			var leftDashPos = new Vector2(Position.X - 100, Position.Y);
			var rightDashPos = new Vector2(Position.X + 100, Position.Y);
			var upDashPos = new Vector2(Position.X, Position.Y - 100);
			var oldPos = GlobalPosition;

			if(_rocketSprite.Texture == RocketTextures[0])
			{
				ShowDashLandLabel(leftDashPos);

				if (Input.IsActionJustPressed("shoot"))
				{
					GD.Print("lefr");
					_leftRightDashAnimation.FlipH = false;
					_leftRightDashAnimation.Play();

					Position = leftDashPos;
					_dashBlinkAnimation.Position = oldPos;

					_dashBlinkAnimation.Play();
				}
				
			}
			if(_rocketSprite.Texture == RocketTextures[1])
			{
				ShowDashLandLabel(rightDashPos);

				if (Input.IsActionJustPressed("shoot"))
				{
					GD.Print("right");
					_leftRightDashAnimation.FlipH = true;
					_leftRightDashAnimation.Play();

					Position = rightDashPos;	
					_dashBlinkAnimation.Position = oldPos;
					_dashBlinkAnimation.Play();
				}
			}
			if(_rocketSprite.Texture == RocketTextures[2])
			{
				ShowDashLandLabel(upDashPos);

				if (Input.IsActionJustPressed("shoot"))
				{
					GD.Print("up");
					_upDownDashAnimation.Play();

					Position = upDashPos;	
					_dashBlinkAnimation.Position = oldPos;
					_dashBlinkAnimation.Play();
				}
			}

				// invincibility after dashing
			Godot.Timer timer = new();
			timer.OneShot = true;
			timer.WaitTime = 0.5d;
			timer.Timeout += () =>
			{
				GetNode<HurtboxComponent>("HurtboxComponent").GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
			};
			AddChild(timer);
			
			await ToSignal(timer, Godot.Timer.SignalName.Timeout);
			GetNode<HurtboxComponent>("HurtboxComponent").GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
			timer.QueueFree();
		}
	}

	/// <summary>
    /// Method to inatiate the player and its starting position
    /// </summary>
    /// <param name="position"></param>
	public void Start(Vector2 position)
    {
		Position = position;
		PlayerAlive = true;
		Show();
		GetNode<CollisionShape2D>("HurtboxComponent/CollisionShape2D").Disabled = false;
		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
		
		// Setting player weapon component BEFORE resetting stats to avoid null reference
		PlayerWeapon.Reset();
		
		ResetStats();
		GetNode<HurtboxComponent>("HurtboxComponent").HealthComponent = playerStats.Health;
		PowerupEnded();
		_isDashActive = false;

		PlayerWeapon.SetWeaponVariables(playerStats.BulletSpeed.Value, playerStats.Damage.Value, playerStats.FireRate.Value);
		PlayerWeapon.SetBulletQuantity(1);
		PlayerWeapon.FiringComponent.StartShooting();
    }

	/// <summary>
    /// Player's death animation and sound
    /// </summary>
	async public void PlayerDeath()
	{
		PlayerWeapon.FiringComponent.StopShooting();
		PlayerAlive = false;

		var explosionAnimation = GetNode<AnimatedSprite2D>("ExplosionAnimation");
		explosionAnimation.SpriteFrames.SetAnimationLoop("default", false);
		explosionAnimation.Play();
		GetNode<AudioStreamPlayer2D>("ExplosionSound").Play();

		GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);

		await ToSignal(explosionAnimation, AnimatedSprite2D.SignalName.AnimationFinished);

		GetNode<CollisionShape2D>("HurtboxComponent/CollisionShape2D").Disabled = true;
		Hide();
	}

	// Picking up the powerups	
	public void OnAreaEntered(Area2D area)
    {
		if (area is Powerup powerup)
        {
			// End the current powerup completely (clean up effects + timer)
			if (_currentPowerup.HasValue)
			{
				EndCurrentPowerup();
			}

			PowerupEnded();
			powerup.PowerupPicked += OnPowerupPicked;
        }
    }

	// Based on type of powerup change player's behaviour
	public void OnPowerupPicked(PowerupEnum powerupEnum)
    {
		GD.Print("powerup picked");
		_currentPowerup = powerupEnum;
        switch (powerupEnum)
        {
			case PowerupEnum.piercing_powerup:
				{
					var bullets = GetTree().Root.GetNode<Main>("Main").GetChildren().Where(x => x.IsInGroup("bullet")).ToList();
					bullets.ForEach(x => {
						x.AddToGroup("piercing");
					});
					PowerupTimer.WaitTime = playerStats.PiercingPowerupDuration.Value;
					_powerupCallable = new Callable(this, nameof(OnPiercingTimeout));
					break;
                }
			case PowerupEnum.invincibility_powerup:
				{
					GetNode<HurtboxComponent>("HurtboxComponent").GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);

					PowerupTimer.WaitTime = playerStats.InvincibilityPowerupDuration.Value;
					_powerupCallable = new Callable(this, nameof(OnInvincibilityTimeout));
					
					break;
                }
			case PowerupEnum.multishot_powerup:
                {
					PlayerWeapon.SetBulletQuantity(2);			

					PowerupTimer.WaitTime = playerStats.MultishotPowerupDuration.Value;
					_powerupCallable = new Callable(this, nameof(OnMultishotTimeout));
					
					
                    break;
                }
			case PowerupEnum.dash_powerup:
                {
					_isDashActive = true;
					GetParent().GetNode<Label>("DashLandLabel").Visible = true;

					PowerupTimer.WaitTime = playerStats.DashPowerupDuration.Value;
					_powerupCallable = new Callable(this, nameof(OnDashTimeout));
					
                    break;
                }
        }

		PowerupTimer.Connect(Godot.Timer.SignalName.Timeout, _powerupCallable);
		PowerupTimer.Start();
    }
	// Cleanup effects for the current powerup
	private void EndCurrentPowerup()
	{
		if(!_currentPowerup.HasValue) return;

		// Stop and disconnect timer first
		if(PowerupTimer.IsConnected(Godot.Timer.SignalName.Timeout, _powerupCallable))
		{
			PowerupTimer.Disconnect(Godot.Timer.SignalName.Timeout, _powerupCallable);
		}
		PowerupTimer.Stop();

		switch (_currentPowerup.Value)
		{
			case PowerupEnum.piercing_powerup:
				GetTree().Root.GetNode<Main>("Main").GetChildren().Where(x => x.IsInGroup("bullet")).ToList().ForEach(x => {
					x.RemoveFromGroup("piercing");
				});
				break;
			case PowerupEnum.invincibility_powerup:
				GetNode<HurtboxComponent>("HurtboxComponent").GetNode<CollisionShape2D>("CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, false);
				break;
			case PowerupEnum.multishot_powerup:
				PlayerWeapon.SetBulletQuantity(1);
				break;
			case PowerupEnum.dash_powerup:
				_isDashActive = false;
				GetParent().GetNode<Label>("DashLandLabel").Visible =false;
				break;
		}

		_currentPowerup = null;
	}
	void OnPiercingTimeout()
	{
		EndCurrentPowerup();
		PowerupEnded();
	}
	void OnInvincibilityTimeout()
	{
		EndCurrentPowerup();
		PowerupEnded();
	}
	void OnMultishotTimeout()
	{
		EndCurrentPowerup();
		PowerupEnded();

	}
	void OnDashTimeout()
	{
		EndCurrentPowerup();
		PowerupEnded();
	}

	/// <summary>
    /// Change the powerup container to empty one and stop powerup timer
    /// </summary>
	public void PowerupEnded()
    {
        var powerupContainer = GetTree().GetNodesInGroup("powerup_holders");
		powerupContainer[0].GetNode<Sprite2D>("PanelContainer/PowerupContainer").Texture = PowerupContainerEmpty;	
    }

	public void AddSkillPoint()
    {
        playerStats.SkillPoints.Value++;
    }

	/// <summary>
    /// Reset the Player's stats
    /// </summary>
	public void ResetStats()
	{
		playerStats.SkillPoints.Value = DeafultPlayerStatsValues.SKILL_POINTS;

		playerStats.Speed.Value = DeafultPlayerStatsValues.SPEED;
		playerStats.FireRate.Value = DeafultPlayerStatsValues.FIRE_RATE;
		playerStats.Luck.Value = DeafultPlayerStatsValues.LUCK;
		playerStats.BulletSpeed.Value = DeafultPlayerStatsValues.BULLET_SPEED;
		playerStats.Damage.Value = DeafultPlayerStatsValues.DAMAGE;
		playerStats.Health.SetHP(DeafultPlayerStatsValues.HEALTH);
		playerStats.MaxHealth.Value = DeafultPlayerStatsValues.HEALTH;

		playerStats.InvincibilityPowerupDuration.Value = DeafultPlayerStatsValues.INVINCIBILITY_POWERUP_DURATION;
		playerStats.PiercingPowerupDuration.Value = DeafultPlayerStatsValues.PIERCING_POWERUP_DURATION;
		playerStats.MultishotPowerupDuration.Value = DeafultPlayerStatsValues.MULTISHOT_POWERUP_DURATION;
		playerStats.DashPowerupDuration.Value = DeafultPlayerStatsValues.DASH_POWERUP_DURATION;
	}

	/// <summary>
    /// Player hp drops to zero
    /// </summary>
	public void OnHealthDepleted()
    {
        EmitSignal(SignalName.PlayerHealthDepleted);
		PlayerDeath();
    }

	/// <summary>
    /// Shows label with damage text
    /// </summary>
    /// <param name="hitboxComponent">Entity's hitbox that entered the Player's area</param>
	public void ShowDamageLabel(HitboxComponent hitboxComponent)
    {
        Label label = new()
        {
            Text = $"-{hitboxComponent.Damage}",
			Position = GlobalPosition
		};
		var customTheme = new Theme();
		customTheme.SetColor("font_color", "Label", Colors.Red);
		label.Theme = customTheme;

		GetParent().AddChild(label);

		Godot.Timer timer = new();
		label.AddChild(timer);
		timer.WaitTime = 0.5;
		timer.OneShot = true;
		timer.Timeout += () => 
		{
			if (IsInstanceValid(label))
				label.QueueFree();
		};
		timer.Start();
    }

	void ShowDashLandLabel(Vector2 position)
	{
		var label = GetParent().GetNode<Label>("DashLandLabel");
		var styleBox = new StyleBoxFlat
		{
			BgColor = new Color(0, 0, 0, 0f),
			BorderColor = new Color(255, 0,0),
		};
		label.Position = position;
		var customTheme = new Theme();
		customTheme.SetColor("font_color", "Label", Colors.Red);
		customTheme.SetStylebox("normal", "Label", styleBox);
		label.Theme = customTheme;

	}
	
}