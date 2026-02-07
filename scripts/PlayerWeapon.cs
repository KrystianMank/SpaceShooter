using Godot;
using GameEnums;
using System.Collections.Generic;
using System.Linq;
public partial class PlayerWeapon : Node
{
	[Export]
	public PackedScene[] WeaponScenes;
	[Export]
	public Texture2D[] WeaponSprites;
	[Export]
	public AnimatedSprite2D WeaponChangeAnimation;
	public FiringComponent FiringComponent;
	public PlayerStats PlayerStats;
	public WeaponTypes CurrentWeaponType;
	private int _weaponIndex;
	private PackedScene _currentWeapon;
	private Texture2D _currentWeaponTexture;
	private bool _animationFinished = true;
	const float WEAPON_CHANGE_TIME = 2F;

	public readonly struct WeaponStatsMultiplier
	{
		public readonly double BulletSpeedMultiplier;
		public readonly double BulletDamageMultiplier;
		public readonly double FireRateMultiplier;

		public WeaponStatsMultiplier(double speedMultiplier, double damageMultiplier, double fireRateMultiplier)
		{
			BulletSpeedMultiplier = speedMultiplier;
			BulletDamageMultiplier = damageMultiplier;
			FireRateMultiplier = fireRateMultiplier;
		}
		public WeaponStatsMultiplier(){}
	}
	private readonly WeaponStatsMultiplier _maschineGunStatsMultipier = new(1,1,1);
	private readonly WeaponStatsMultiplier _rocketLauncherStatsMultiplier = new(0.5, 3, 2);
	private readonly WeaponStatsMultiplier _laserStatsMultiplier = new(10, 0.1, 5);

	public struct WeaponStats
	{
		public readonly WeaponTypes WeaponType;
		public double BulletSpeed;
		public double BulletDamage;
		public double FireRate;

		public WeaponStats(WeaponTypes weaponType, PlayerStats playerStats, WeaponStatsMultiplier weaponStatsMultiplier)
		{
			WeaponType = weaponType;
			BulletSpeed = playerStats.BulletSpeed.Value * weaponStatsMultiplier.BulletSpeedMultiplier;
			BulletDamage = playerStats.Damage.Value * weaponStatsMultiplier.BulletDamageMultiplier;
			FireRate = playerStats.FireRate.Value * weaponStatsMultiplier.FireRateMultiplier;
		}
		public WeaponStats(){}
	}


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		FiringComponent = GetNode<FiringComponent>(nameof(FiringComponent));
		FiringComponent.PlayerBullet = true;
		WeaponChangeAnimation.SpeedScale = WEAPON_CHANGE_TIME;
		Reset();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		FiringComponent.BulletSpawn.GlobalPosition = GetParent().GetNode<Marker2D>("BulletSpawn").GlobalPosition;
		WeaponChangeAnimation.GlobalPosition = GetParent().GetNode<Marker2D>("BulletSpawn").GlobalPosition + new Vector2(20,0);

		if(GetOwner<Player>().PlayerAlive)
			// Changing to next weapon
			if (Input.IsActionJustPressed("next_weapon") && _animationFinished)
			{
				ChangeWeapon(true);
			}
			// Changing to previous weapon
			if (Input.IsActionJustPressed("previous_weapon") && _animationFinished)
			{
				ChangeWeapon(false);
			}
	}

	/// <summary>
	/// Reset current weapon to default one (maschine gun)
	/// </summary>
	public void Reset()
	{
		_weaponIndex = 0;
		CurrentWeaponType = WeaponTypes.MaschineGun;

		_currentWeapon = WeaponScenes[_weaponIndex];
		_currentWeaponTexture = WeaponSprites[_weaponIndex];

		if (PlayerStats == null)
			return;

		// MaschineGunStats = new( WeaponTypes.MaschineGun,PlayerStats, _maschineGunStatsMultipier);
		// RocketLauncherStats = new(WeaponTypes.RocketLauncher, PlayerStats, _rocketLauncherStatsMultiplier);
		// LaserStats = new(WeaponTypes.Laser, PlayerStats, _laserStatsMultiplier);
		// WeaponStatsList =new List<WeaponStats>
        // {
        //     MaschineGunStats, RocketLauncherStats, LaserStats
		// };

		SetWeapon();
		SetWeaponToHUD();
	}

	/// <summary>
	/// If set to true change to next weapon else to previous one
	/// </summary>
	/// <param name="next"></param>
	public void ChangeWeapon(bool next)
	{
		_weaponIndex = next
			? (_weaponIndex < WeaponScenes.Length -1 ? ++_weaponIndex : _weaponIndex = 0)
			: (_weaponIndex > 0 ? --_weaponIndex : _weaponIndex = WeaponScenes.Length - 1);
		CurrentWeaponType = (WeaponTypes)_weaponIndex;
		_currentWeapon = WeaponScenes[_weaponIndex];
		_currentWeaponTexture = WeaponSprites[_weaponIndex];

		WeaponChangeAnim();
	}
	
	/// <summary>
	/// Set weapon variables
	/// </summary>
	/// <param name="bulletSpeed"></param>
	/// <param name="damage"></param>
	/// <param name="fireRate"></param>
	public void SetWeaponVariables(int bulletSpeed, double damage, double fireRate)
	{
		FiringComponent.BulletSpeed.Value = bulletSpeed;
		FiringComponent.BulletDamage.Value = damage;
		FiringComponent.BulletFirerate.Value = fireRate;
	}
	/// <summary>
	/// Sets FiringsComponent's bullet aunatity and firing angle
	/// </summary>
	/// <param name="bulletQuantity"></param>
	public void SetBulletQuantity(int bulletQuantity)
	{
		switch (bulletQuantity)
		{
			case 1:
				{
					FiringComponent.SetBulletSpawnVariables(1, 0f);
				}
				break;
			case 2: 
				{
					FiringComponent.SetBulletSpawnVariables(2, 2f);
				}
				break;
			case 3: 
				{
					FiringComponent.SetBulletSpawnVariables(3, 3f);
				}
				break;
		}
	}
	/// <summary>
	/// Save picked weapon to FiringComponent
	/// </summary>
	public void SetWeapon()
	{
		if (PlayerStats == null)
			return;

		FiringComponent.BulletScene = _currentWeapon;
		
		// Recalculate weapon stats based on current player stats
		WeaponStatsMultiplier multiplier = CurrentWeaponType switch
		{
			WeaponTypes.MaschineGun => _maschineGunStatsMultipier,
			WeaponTypes.RocketLauncher => _rocketLauncherStatsMultiplier,
			WeaponTypes.Laser => _laserStatsMultiplier,
			_ => new WeaponStatsMultiplier()
		};

		double bulletSpeed = PlayerStats.BulletSpeed.Value * multiplier.BulletSpeedMultiplier;
		double bulletDamage = PlayerStats.Damage.Value * multiplier.BulletDamageMultiplier;
		double fireRate = PlayerStats.FireRate.Value * multiplier.FireRateMultiplier;

		SetWeaponVariables((int)bulletSpeed, bulletDamage, fireRate);
		FiringComponent.BulletSprite = _currentWeaponTexture;
	}
	
	/// <summary>
	/// </summary>
	/// <returns>Currently held weapon stats</returns>
	// public WeaponStats GetCurrentWeaponStats()
	// {
	// 	return WeaponStatsList.First(x => x.WeaponType == CurrentWeaponType);
	// }

	/// <summary>
	/// Save picekd weapon to WeaponHolder UI
	/// </summary>
	private void SetWeaponToHUD()
	{
		 var weaponHolder = GetTree().GetNodesInGroup("weapon_holder");
		weaponHolder[0].GetNode<Sprite2D>("PanelContainer/WeaponContainer").Texture = _currentWeaponTexture;
	}
	/// <summary>
	/// Weapon change aniamtion, while it's active stop shooting. Can't change weapon during the aniamtion
	/// </summary>
	private async void WeaponChangeAnim()
	{
		WeaponChangeAnimation.Play();
		_animationFinished = false;
		FiringComponent.StopShooting();

		await ToSignal(WeaponChangeAnimation, AnimatedSprite2D.SignalName.AnimationFinished);

		_animationFinished = true;
		SetWeapon();
		SetWeaponToHUD();
		FiringComponent.StartShooting();
	}
}
