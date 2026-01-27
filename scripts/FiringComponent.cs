using GenericObervable;
using Godot;
using System;

public partial class FiringComponent : Node
{
	[Export]
	public PackedScene BulletScene;
	public Observable<int> BulletSpeed =new();
	public Observable<double> BulletDamage=new();
	public Observable<double> BulletFirerate =new();
	public Marker2D BulletSpawn;
	public int BulletQuantity;
	public float FiringAngle;
	public bool PlayerBullet;
	
	public Texture2D BulletSprite;
	
	private Timer _shootCooldown;
	private AudioStreamPlayer2D _shootSound;
	private AnimatedSprite2D _firingAnimation;
	private Bullet _bullet;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		BulletSpawn = GetNode<Marker2D>("BulletSpawn");
		_shootCooldown = GetNode<Timer>("ShootCooldown");
		_firingAnimation = BulletSpawn.GetNode<AnimatedSprite2D>("FiringAnimation");
		
		_shootSound = GetNode<AudioStreamPlayer2D>("ShootSound");

		BulletFirerate.Changed += OnBulletFirerateChanged;

	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void SpawnBullets() 
    {
        for(int i = 1; i <= BulletQuantity; i++)
        {
            _bullet = BulletScene.Instantiate<Bullet>();
			_bullet.Position = BulletSpawn.GlobalPosition;
			_bullet.Speed = BulletSpeed.Value;

			var rotation = i % 2 == 0 ? FiringAngle * i : FiringAngle * -i;
			BulletSpawn.Rotation = FiringAngle;
			_bullet.FiringAngle = rotation;
			_bullet.PlayerBullet = PlayerBullet;
			_bullet.BulletSprite = BulletSprite;

			_bullet.Initialize(GetParent(), PlayerBullet);
			
			GetTree().Root.GetNode("Main").AddChild(_bullet);
			_bullet.AddToGroup("bullet");
			_bullet.GetNode<HitboxComponent>(nameof(HitboxComponent)).Damage = BulletDamage.Value;
        }
		_firingAnimation.Frame = 0;
		_firingAnimation.Play();
		_shootSound.Play();
    }

	public void StartShooting()
	{
		
		_shootCooldown.Start();
	}
	public void StopShooting()
	{
		_firingAnimation.Stop();
		_shootCooldown.Stop();
	}

	public void ShootCooldownTimeout()
	{
		SpawnBullets();
	}

	public void SetBulletSpawnVariables(int quantity, float firingAngle)
	{
		BulletQuantity = quantity;
		FiringAngle = firingAngle;
	}

	void OnBulletFirerateChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
	{
		_shootCooldown.WaitTime = eventArgs.NewValue;
	}
}
