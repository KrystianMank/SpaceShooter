using GenericObervable;
using Godot;
using StaticClasses;

public partial class Entity : RigidBody2D
{
	[Signal]
	public delegate void EnitityHPDepletedEventHandler(Entity entity);
	[Signal]
	public delegate void EntityHPValueChangedEventHandler(Entity entity);
	[Export]
	public PackedScene PowerupScene;
	[Export]
	public HealthComponent EntityHP = new();
	public Observable<double> EntityMaxHP = new();
	public PlayerStats PlayerStats { get; set; }
	public bool EntitiesHitEachOther = false;
	private bool _hit = false;
    public override void _Ready()
    {
        ContactMonitor = true;
		EntityHP.HPDepleted += OnEnityHpDepleted;
		EntityHP.HPValueChanged += OnEnityHpValueChanged;

		GetNode<HurtboxComponent>("HurtboxComponent").HealthComponent = EntityHP;
		// GetNode<HurtboxComponent>("HurtboxComponent").AreaEntered += (area) =>
		// {
		// 	if(area.GetParent() is Entity) EntitiesHitEachOther = true;
		// 	else EntitiesHitEachOther = false;	
		// };
    }


	public void SetHealthBarMaxValue(double value)
    {
        GetNode<ProgressBar>("EntityHealthbar/ProgressBar").MaxValue = value;
    }

	public void SetHealthBarValue(double value)
    {
        GetNode<ProgressBar>("EntityHealthbar/ProgressBar").Value = value;
    }

	public void SpawnPowerup()
    {
		var luck = PlayerStats.Luck.Value;
		double randomNumber = GD.RandRange(0.0, luck);

		if (randomNumber > TresholdValues.POWERUP_SPAWN_TRESHOLD)
		{
			var powerup = PowerupScene.Instantiate<Powerup>();

			powerup.Position = Position;

			GetParent().AddChild(powerup);
		}
    }
	public void SetEnityHealthRandom(double minValue, double maxValue)
    {
        double randomHealthValue = Mathf.Floor(GD.RandRange(minValue, maxValue));
		EntityHP.SetHP(randomHealthValue);
		EntityMaxHP.Value = randomHealthValue;
    }

	public void InitializeHealthBar()
	{
		if (EntityHP != null && EntityHP.GetHP() != null)
		{
			SetHealthBarMaxValue(EntityMaxHP.Value);
			SetHealthBarValue(EntityHP.GetHP().Value);
		}
	}

	public void SetHitboxShape(Shape2D collisionShape)
    {
        GetNode<CollisionShape2D>("HitboxComponent/CollisionShape2D").Shape = collisionShape;
    }
	public void SetHurtboxboxShape(Shape2D collisionShape)
    {
        GetNode<CollisionShape2D>("HurtboxComponent/CollisionShape2D").Shape = collisionShape;
    }

	public void  OnEnityHpDepleted()
    {
		if(_hit) return;

		_hit = true;
		GD.Print(EntitiesHitEachOther);
        EmitSignal(SignalName.EnitityHPDepleted, this);
    }

	public void OnEnityHpValueChanged(double newHp){
		SetHealthBarValue(newHp);
	}

	async public void EnityDeath()
    {
        var explosionAnimation = GetNode<AnimatedSprite2D>("ExplosionAnimation");
		explosionAnimation.SpriteFrames.SetAnimationLoop("default", false);
		explosionAnimation.Play();
		GetNode<AudioStreamPlayer2D>("ExplosionSound").Play();

		GetNode<CollisionShape2D>("HitboxComponent/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		GetNode<CollisionShape2D>("HurtboxComponent/CollisionShape2D").SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		SetDeferred(RigidBody2D.PropertyName.Freeze, true);

		await ToSignal(explosionAnimation, AnimatedSprite2D.SignalName.AnimationFinished);

		SpawnPowerup();

		QueueFree();
    } 

	public void VisibleOnScreenNotifier2D()
    {
		QueueFree();
    }

	public void OnBodyEntered(Node body)
	{
		if(body is Entity)
		{
			EntitiesHitEachOther = true;
		}
		else
		{
			EntitiesHitEachOther = false;
		}
	}
}
