using GenericObervable;
using Godot;
using System;

public partial class RocketProjectile : Bullet
{
    public Observable<double> ExplosionRadius = new();
    public override void _Ready()
    {
        base._Ready();
        ExplosionRadius.Changed += OnExplosionRadiusChanged;
    }

    private void OnExplosionRadiusChanged(object sender, Observable<double>.ChanedEventArgs e)
    {
        GetNode<ExplosionArea>(nameof(ExplosionArea)).ExplosionRadius = (float)e.NewValue;
    }


    public override void _IntegrateForces(PhysicsDirectBodyState2D state)
    {
        base._IntegrateForces(state);
    }
}
