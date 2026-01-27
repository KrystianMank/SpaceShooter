using Godot;
using System;

public partial class ExplosionArea : Area2D
{
	[Export]
	public float ExplosionRadius = 150;
	[Export]
	public float ExplosionForce = 500;
	[Export]
    public double ExplosionDamageMultiplier = 0.5d;

	[Export]
	public AnimatedSprite2D ExplosionAnimation;
	[Export]
	public GpuParticles2D ExplosionVFX;
	const float DISTANCE_TO_EXPLODE = 200;
	private Vector2 _startPos, _distanceToExplode;
	

    public override void _Ready()
    {
        // GetNode<CollisionShape2D>("CollisionShape2D").Shape = new CapsuleShape2D
		// {
		// 	Radius = ExplosionRadius
		// };
		_startPos = GlobalPosition;
		
    }

	public void OnBodyEntered(Node2D body)
	{
		if(body is not Bullet)
			Explode();
		GD.Print(body.Name);
	}


    public override void _Process(double delta)
    {
		// Explosion after certain distance
		var straightVector = new Vector2(_startPos.X, GlobalPosition.Y);
		if((straightVector - _startPos).Length() >= DISTANCE_TO_EXPLODE)
		{
			Explode();
		}
    }
	public void Explode()
	{
		ExplosionVFX.Emitting = true;
		ExplosionAnimation.Play();
		
		// Hide and disable the projectile
		GetParent().SetDeferred(RigidBody2D.PropertyName.Freeze, true); 
		GetParent().SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
		GetParent().GetNode<Sprite2D>("Sprite2D").Hide();

		// Apply explosion physics
		foreach(var child in GetTree().GetNodesInGroup("entities"))
		{
			if(child is Entity entity)
			{
				var direction = entity.GlobalPosition - GlobalPosition;
				if(direction.Length() < ExplosionRadius)
				{
					var impulse = direction.Normalized() * ExplosionForce * (1 - (direction.Length() / ExplosionRadius));
					entity.ApplyImpulse(impulse);
					double damage = ExplosionDamageMultiplier * Math.Ceiling(Mathf.Sqrt(Mathf.Abs(ExplosionRadius - direction.Length())));
					GD.Print(entity.Name + " is in Range of explosion "+damage);
					// farthest - 1dmg, closest - 3.5dmg
					entity.EntityHP.DealDamage(damage); 
				}
			}
		}

		// Cleanup after animation finishes
		CleanupAfterAnimation();
	}

	async private void CleanupAfterAnimation()
	{
		await ToSignal(ExplosionAnimation, AnimatedSprite2D.SignalName.AnimationFinished);
		GetParent().QueueFree();
	}

}
