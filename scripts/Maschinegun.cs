using GameEnums;
using Godot;
using System;
using WeaponNamescape;

public partial class Maschinegun : Node, IBaseWeapon
{
	public FiringComponent FiringComponent;
	private MaschineGunStats _maschineGunStats;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		FiringComponent = GetNode<FiringComponent>(nameof(FiringComponent));
	}

	public void SetWeaponVariables(BaseWeaponStats baseWeaponStats)
	{
		if(baseWeaponStats is not MaschineGunStats maschineGunStats)
			throw new ArgumentException("Invalid parameter type");
		_maschineGunStats = maschineGunStats;

		FiringComponent.BulletDamage.Value = maschineGunStats.Damage;
		FiringComponent.BulletSpeed.Value = (int)maschineGunStats.BulletSpeed;
		FiringComponent.BulletFirerate.Value = maschineGunStats.FireRate;
	}

    public BaseWeaponStats GetWeaponStats()
    {
        return _maschineGunStats;
    }
}
