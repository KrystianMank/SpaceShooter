using Godot;

public partial class Bullet : RigidBody2D
{
	[Export]
	public int Speed = 1200;
	[Export]
	public Texture2D BulletSprite;
	[Export]
	public HitboxComponent hitbox;
	public bool PlayerBullet;
	public float FiringAngle;
	public int MaxPierce = 1;
	private int _currentPierceCount = 0;
	private bool _hit = false;
	private Node _shooter;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
		ContactMonitor = true;
		GetNode<Sprite2D>("Sprite2D").Texture = BulletSprite;
		if (!PlayerBullet)
		{
			AddToGroup("entities");
		}

		_currentPierceCount = 0;
		
		SetCollisionMaskValue(1, false);
    
		var timer = new Timer();
		AddChild(timer);
		timer.WaitTime = 0.1;
		timer.OneShot = true;
		timer.Timeout += () => SetCollisionMaskValue(1, true);
		timer.Start();
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        // Setting the roation to 0 before physics calculations
		state.Transform = new Transform2D(0, state.Transform.Origin);

		//state.LinearVelocity = Vector2.Right.Rotated(FiringAngle) * Speed;
		Vector2 direction = new Vector2(
			Mathf.Sin(Mathf.DegToRad(FiringAngle)),
			-Mathf.Cos(Mathf.DegToRad(FiringAngle))
		);

		state.LinearVelocity = direction * Speed;
		state.Transform = new Transform2D(Mathf.DegToRad(FiringAngle), state.Transform.Origin);
    }

	public void Initialize(Node shooter, bool isPlayerBullet)
	{
		_shooter = shooter;
		PlayerBullet = isPlayerBullet;
		_hit = false; 
	}

	// public void OnBodyEntered(Node body)
	// {
	// 	if(_hit) return;
		
	// 	if(body == _shooter)
	// 	{
	// 		return;
	// 	}

	// 	if(hitbox == null) return;
		
	// 	if(body is Entity entity && PlayerBullet == true)
	// 	{
	// 		_hit = true;
	// 		// entity.EntityHP.DealDamage(hitbox.Damage);
			
	// 		
	// 	}
		
	// }

	public void OnAreaEntered(Area2D area)
	{
		if(_hit) return;
    
		if(area == _shooter)
		{
			return;
		}
		
		if(hitbox == null) return;

		_currentPierceCount++;
		
		if(area is HurtboxComponent hurtboxComponent)
		{
			_hit = true;
			if(hurtboxComponent.GetParent() is Player player)
			{
				GD.Print("THIS is player");
			}
			else if(hurtboxComponent.GetParent() is Entity entity && PlayerBullet == false)
			{
				entity.EntitiesHitEachOther = true;
				GD.Print("This is entity");
			}
			//ShowDamageLabel(hitbox.Damage);
			//hurtboxComponent.HealthComponent.DealDamage(hitbox.Damage);
			
			if(_currentPierceCount >= MaxPierce)
			{
				QueueFree();
			}
			else
			{
				SetCollisionMaskValue(1, false);
				SetCollisionLayerValue(1, false);
			}
		}
	}

	// public void ShowDamageLabel(double damage)
    // {
    //     Label label = new()
    //     {
    //         Text = $"-{damage}",
	// 		Position = GlobalPosition,
	// 	};
	// 	var customTheme = new Theme();
	// 	customTheme.SetColor("font_color", "Label", PlayerBullet ? Colors.White : Colors.Red);
	// 	label.Theme = customTheme;
	// 	GetParent().AddChild(label);

	// 	var timer = new Timer();
	// 	label.AddChild(timer);
	// 	timer.WaitTime = 0.5;
	// 	timer.OneShot = true;
	// 	timer.Timeout += () => 
	// 	{
	// 		if (IsInstanceValid(label))
	// 			label.QueueFree();
	// 	};
	// 	timer.Start();
    // }

	public void VisibleOnScreenNotifier2D()
    {
		QueueFree();
    }
	
}
