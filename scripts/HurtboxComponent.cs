using Godot;
using System;

public partial class HurtboxComponent : Area2D
{
	public HealthComponent HealthComponent;

	public void OnAreaEntered(Area2D area2D)
    {
        if(area2D.GetParent() == GetParent()) return;
        if(area2D is HitboxComponent hitbox && area2D is not HurtboxComponent)
        {
            HealthComponent.DealDamage(hitbox.Damage);
        }
    }
}
