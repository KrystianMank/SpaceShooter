using Godot;
using System;

public partial class RocketProjectile : Bullet
{
    public override void _Ready()
    {
        base._Ready();
        
    }

    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);
    }
}
