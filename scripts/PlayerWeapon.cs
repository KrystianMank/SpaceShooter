using Godot;
using GameEnums;
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

	public struct WeaponStatsMultiplier
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

	public WeaponStatsMultiplier MaschineGunStatsMultipier = new(1,1,1);
	public WeaponStatsMultiplier RocketLauncherStatsMultiplier = new(0.5, 3, 0.5);
	public WeaponStatsMultiplier LaserStatsMultiplier = new(10, 0.1, 5);

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		FiringComponent = GetNode<FiringComponent>(nameof(FiringComponent));
		FiringComponent.PlayerBullet = true;
		WeaponChangeAnimation.SpeedScale = WEAPON_CHANGE_TIME;
		Reset();

		SetWeapon();
		SetWeaponToHUD();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		FiringComponent.BulletSpawn.GlobalPosition = GetParent().GetNode<Marker2D>("BulletSpawn").GlobalPosition;
		WeaponChangeAnimation.GlobalPosition = GetParent().GetNode<Marker2D>("BulletSpawn").GlobalPosition + new Vector2(20,0);

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
	public async void WeaponChangeAnim()
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
	private void SetWeapon()
	{
		FiringComponent.BulletScene = _currentWeapon;
		switch (CurrentWeaponType)
		{
			case WeaponTypes.MaschineGun:
				{
					SetWeaponVariables(
						(int)(PlayerStats.BulletSpeed.Value * MaschineGunStatsMultipier.BulletSpeedMultiplier),
						PlayerStats.Damage.Value * MaschineGunStatsMultipier.BulletDamageMultiplier, 
						PlayerStats.FireRate.Value * MaschineGunStatsMultipier.FireRateMultiplier
					);
				}
				break;
			case WeaponTypes.RocketLauncher:
				{
					SetWeaponVariables(
						(int)(PlayerStats.BulletSpeed.Value * RocketLauncherStatsMultiplier.BulletSpeedMultiplier),
						PlayerStats.Damage.Value * RocketLauncherStatsMultiplier.BulletDamageMultiplier,
						PlayerStats.FireRate.Value / RocketLauncherStatsMultiplier.FireRateMultiplier
					);
				}
				break;
			case WeaponTypes.Laser:
				{
					
				}
				break;
		}
		FiringComponent.BulletSprite = _currentWeaponTexture;
	}
	/// <summary>
	/// Save picekd weapon to WeaponHolder UI
	/// </summary>
	private void SetWeaponToHUD()
	{
		 var weaponHolder = GetTree().GetNodesInGroup("weapon_holder");
		weaponHolder[0].GetNode<Sprite2D>("PanelContainer/WeaponContainer").Texture = _currentWeaponTexture;
	}
	
}
