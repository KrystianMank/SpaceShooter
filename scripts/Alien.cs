using System;
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

        FiringComponent.TryShoot();
         
    }

    public override void InitializeValues(EntitySpawnParams spawnParams)
    {
		if (spawnParams is not AlienSpawnParams alienParams)
            throw new ArgumentException("Nieprawidłowy typ parametrów");

		VelocityValue = alienParams.VelocityValue;
		Position = alienParams.Position;
		SpawnSide = alienParams.SpawnSide;
        SpawnDirection = alienParams.SpawnDirection;
        BulletDamage = alienParams.AlienBulletDamage;
        BulletFireRate = alienParams.AlienFirerate;
    }

}
public enum SpawnDirection
{
    Horizontal,
    Vertical
}