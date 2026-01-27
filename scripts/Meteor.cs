using Godot;
public partial class Meteor : Entity
{
	public Vector2 Velocity;
	public override void _Ready()
	{
		base._Ready();
		//GetNode<AnimatedSprite2D>("MeteorAnimation").Play();
		// ContactMonitor = true;
		// MaxContactsReported = 1;
		AddToGroup("entities");

		SetHitboxShape(GetNode<CollisionShape2D>("CollisionShape2D").Shape);
		SetHurtboxboxShape(GetNode<CollisionShape2D>("CollisionShape2D").Shape);
	}

    public override void _Process(double delta)
    {
       Position += Velocity.Rotated(Rotation) * (float)delta;
	   GetNode<AnimatedSprite2D>("MeteorAnimation").Rotate((float)delta);
    }


}