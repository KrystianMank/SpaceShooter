using Godot;
using System;
using WeaponNamescape;

public partial class Rocketlauncher : Node, IBaseWeapon
{
	public FiringComponent FiringComponent;
	private RocketLauncherStats _rocketLauncherStats;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		FiringComponent = GetNode<FiringComponent>(nameof(FiringComponent));
	}

    public void SetWeaponVariables(BaseWeaponStats baseWeaponStats)
	{
		if(baseWeaponStats is not RocketLauncherStats rocketLauncherStats)
			throw new ArgumentException("Invalid parameter type");
		_rocketLauncherStats = rocketLauncherStats;
		
		FiringComponent.BulletDamage.Value = rocketLauncherStats.Damage;
		FiringComponent.BulletFirerate.Value = rocketLauncherStats.FireRate;
		// explosion radius
	}

    public BaseWeaponStats GetWeaponStats()
    {
        return _rocketLauncherStats;
    }
}
