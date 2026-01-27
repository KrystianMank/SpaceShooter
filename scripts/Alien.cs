using Godot;
using StaticClasses;

public partial class Alien : Entity
{
    [Export]
    public FiringComponent FiringComponent;
    [Export]
    public Texture2D EnemyBulletSprite;
	public SpawnDirection SpawnDirection;
    public float VelocityValue;
    public double BulletDamage = 0;
    public double BulletFireRate = 1;
    public int SpawnSide;
    private bool _hit = false;
    private Bullet _enemyBullet;
    public override void _Ready()
    {
        base._Ready();
        AddToGroup("entities");
        SetHitboxShape(GetNode<CollisionShape2D>("CollisionShape2D").Shape);
        SetHurtboxboxShape(GetNode<CollisionShape2D>("CollisionShape2D").Shape);

        FiringComponent.BulletDamage.Value = 1 + BulletDamage;
        FiringComponent.BulletFirerate.Value = BulletFireRate;
        FiringComponent.BulletSpeed.Value = 600;
        FiringComponent.BulletSprite = EnemyBulletSprite;
        FiringComponent.PlayerBullet = false;
        FiringComponent.SetBulletSpawnVariables(1,0f);
        FiringComponent.StartShooting();
    }

    public override void _Process(double delta)
    {
        var velocity = Vector2.Zero;

        Vector2 bulletPosition = Vector2.Zero;
        Vector2 direction = Vector2.Zero;

        switch (SpawnDirection)
        {
            case SpawnDirection.Horizontal:
                {
                    switch (SpawnSide)
                    {
                        case 0: // left side
                            {
                                bulletPosition = GetNode<Marker2D>("RightShootPosition").GlobalPosition;
                                velocity.X += 1;
                            }
							break;
						case 1: // right side
                            {
                                bulletPosition = GetNode<Marker2D>("LeftShootPosition").GlobalPosition;
                                velocity.X -= 1;
                            }
							break;
                    }
                    direction = SpawnSide == 0 ? Vector2.Right : Vector2.Left;
                }
                break;
            case SpawnDirection.Vertical:
                {
                    switch (SpawnSide)
                    {
                        case 0: // up side
                            {
                                bulletPosition = GetNode<Marker2D>("UpShootPosition").GlobalPosition;
                                velocity.Y += 1;
                            }
							break;
						case 1: // down side
                            {
                                bulletPosition = GetNode<Marker2D>("DownShootPosition").GlobalPosition;
                                velocity.Y -= 1;
                            }
							break;
                    }
                    direction = SpawnSide == 0 ? Vector2.Up : Vector2.Down;
                }
                break;
        }
        if(velocity.Length() > 0)
        {
            velocity = velocity.Normalized() * VelocityValue;
        }
		Position += velocity * (float)delta;
        // bullet logic
        FiringComponent.BulletSpawn.Position = bulletPosition;
        FiringComponent.FiringAngle = direction switch
        {
            (-1, 0) => 90f, //left
            (1, 0) => -90f, // right
            (0, -1) => 0f, //up
            (0, 1) => -180f, //down
            _ => 0f

        };
         
    }


	// async public void ShootCooldownTimeout()
	// {
	// 	_enemyBullet = EnemyBulletScene.Instantiate<EnemyBullet>();

	// 	var bulletFireAnimation = GetNode<AnimatedSprite2D>("BulletFireAnimation");
    //     var bulletFireAnimationRotation = bulletFireAnimation.Rotation;

    //     Vector2 bulletPosition = Vector2.Zero;
    //     Vector2 animationPosition = Vector2.Zero;
    //     Vector2 direction = Vector2.Zero;

    //     switch (SpawnDirection)
    //     {
    //         case SpawnDirection.Horizontal:
    //             {
    //                 bulletFireAnimation.Rotation = bulletFireAnimationRotation;
    //                 if(SpawnSide == 0) // left
    //                 {
    //                     bulletPosition = GetNode<Marker2D>("RightShootPosition").GlobalPosition;
    //                     animationPosition = GetNode<Marker2D>("RightShootPosition").GlobalPosition;
    //                     bulletFireAnimation.Rotate(90f);
    //                 }
    //                 else // right
    //                 {
    //                     bulletPosition = GetNode<Marker2D>("LeftShootPosition").GlobalPosition;
    //                     animationPosition = GetNode<Marker2D>("LeftShootPosition").GlobalPosition;
    //                     bulletFireAnimation.Rotate(-90f);
    //                 }
    //                 direction = SpawnSide == 0 ? Vector2.Right : Vector2.Left;
                                    
    //             }
    //             break;
    //         case SpawnDirection.Vertical:
    //             {
    //                 bulletFireAnimation.Rotation = bulletFireAnimationRotation;
    //                 if(SpawnSide == 0) // up
    //                 {
    //                     bulletPosition = GetNode<Marker2D>("UpShootPosition").GlobalPosition;
    //                     animationPosition = GetNode<Marker2D>("UpShootPosition").GlobalPosition;
    //                 }
    //                 else // down
    //                 {
    //                     bulletPosition = GetNode<Marker2D>("DownShootPosition").GlobalPosition;
    //                     animationPosition = GetNode<Marker2D>("DownShootPosition").GlobalPosition;
    //                 }
    //                 direction = SpawnSide == 0 ? Vector2.Up : Vector2.Down;
    //                 _enemyBullet.Rotate(90f);
    //                 bulletFireAnimation.FlipV = true;
    //             }
    //             break;
    //     }
    //     _enemyBullet.Position = bulletPosition;
    //     _enemyBullet.Direction = direction;

    //     bulletFireAnimation.Position = animationPosition;

	// 	bulletFireAnimation.Play();
	// 	await ToSignal(bulletFireAnimation, AnimatedSprite2D.SignalName.AnimationFinished);
	// 	bulletFireAnimation.Stop();

	// 	//GetNode<AudioStreamPlayer2D>("ShootSound").Play();

    //     if (_enemyBullet.GetParent() != null)
    //     {
    //         _enemyBullet.GetParent().RemoveChild(_enemyBullet);
    //     }
    //     _enemyBullet.AddToGroup("entities");

	// 	GetParent().AddChild(_enemyBullet);
	// }

}
public enum SpawnDirection
{
    Horizontal,
    Vertical
}