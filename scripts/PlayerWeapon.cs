using Godot;
using GameEnums;
using System.Collections.Generic;
using System.Linq;
using GenericObervable;
public partial class PlayerWeapon : Node
{
	[Export]
	public PackedScene[] WeaponScenes;
	[Export]
	public Texture2D[] WeaponSprites;
	[Export]
	public Texture2D[] WeaponFrames;
	[Export]
	public AnimatedSprite2D WeaponChangeAnimation;
	public FiringComponent FiringComponent;
	public PlayerStats PlayerStats;
	public WeaponTypes CurrentWeaponType;
	private int _weaponIndex;
	public PackedScene CurrentWeapon;
	public Laser Laser;
	private List<Laser> _duplicates = [];
	private Texture2D _currentWeaponTexture;
	private Texture2D _currentWeaponFrame;
	public bool WeaponChangeAnimationFinished = true;
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
		FiringComponent.MaxPierce.Changed += MaxPierceValueChanged;
		WeaponChangeAnimation.SpeedScale = WEAPON_CHANGE_TIME;

		Reset();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		FiringComponent.BulletSpawn.GlobalPosition = GetParent().GetNode<Marker2D>("BulletSpawn").GlobalPosition;
		WeaponChangeAnimation.GlobalPosition = GetParent().GetNode<Marker2D>("BulletSpawn").GlobalPosition + new Vector2(20,0);
		
		// Update laser position to follow player
		if(Laser != null)
		{
			Laser.GlobalPosition = FiringComponent.BulletSpawn.GlobalPosition;
		}
		foreach(var dupli in _duplicates)
		{
			if(dupli != null)
			{
				dupli.GlobalPosition = FiringComponent.BulletSpawn.GlobalPosition;
			}
		}

		if(GetOwner<Player>().PlayerAlive)
		{
			// Changing to next weapon
			if (Input.IsActionJustPressed("next_weapon") && WeaponChangeAnimationFinished)
			{
				ChangeWeapon(true);
			}
			// Changing to previous weapon
			if (Input.IsActionJustPressed("previous_weapon") && WeaponChangeAnimationFinished)
			{
				ChangeWeapon(false);
			}
		}
	}

	public void Init()
	{
		Laser = WeaponScenes[2].Instantiate<Laser>();
		AddChild(Laser);
		Laser.GetNode<CanvasLayer>(nameof(CanvasLayer)).Visible = false;
		CreateLaserDuplicates(1);
	}

	public void TryFireLasers(bool fire)
	{
		Laser.IsCasting = fire;
		if(_duplicates.Count != 0)
		{
			foreach(var dupli in _duplicates){
				dupli.IsCasting = fire && dupli.Enabled;
			}
		}
	}

	/// <summary>
	/// Reset current weapon to default one (maschine gun)
	/// </summary>
	public void Reset()
	{
		_weaponIndex = 2;
		CurrentWeaponType = (WeaponTypes)_weaponIndex;

		CurrentWeapon = WeaponScenes[_weaponIndex];
		_currentWeaponTexture = WeaponSprites[_weaponIndex];
		_currentWeaponFrame = WeaponFrames[_weaponIndex];

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
		CurrentWeapon = WeaponScenes[_weaponIndex];
		_currentWeaponTexture = WeaponSprites[_weaponIndex];
		_currentWeaponFrame = WeaponFrames[_weaponIndex];

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

		if(CurrentWeaponType == WeaponTypes.Laser)
		{
			Laser.GetNode<CanvasLayer>(nameof(CanvasLayer)).Visible =  true;
			Laser.Damage = bulletDamage;
			Laser.CastSpeed = bulletSpeed;
			Laser.MaxResults = FiringComponent.MaxPierce.Value;
			OneLaser(); // Upewniamy się że duplikaty są wyłączone na początku
		}
		else
		{
			if(IsInstanceValid(Laser)) {
				Laser.GetNode<CanvasLayer>(nameof(CanvasLayer)).Visible = false;
			}
			FiringComponent.BulletScene = CurrentWeapon;
		}
		Laser.IsCasting = false;
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
		weaponHolder[0].GetNode<Sprite2D>("PanelContainer/WeaponContainer").Texture = _currentWeaponFrame;
	}
	/// <summary>
	/// Weapon change aniamtion, while it's active stop shooting. Can't change weapon during the aniamtion
	/// </summary>
	private async void WeaponChangeAnim()
	{
		WeaponChangeAnimation.Play();
		WeaponChangeAnimationFinished = false;
		Laser.IsCasting = false;
		
		//FiringComponent.StopShooting();

		await ToSignal(WeaponChangeAnimation, AnimatedSprite2D.SignalName.AnimationFinished);

		WeaponChangeAnimationFinished = true;
		SetWeapon();
		SetWeaponToHUD();
		//FiringComponent.StartShooting();
	}

	public void CreateLaserDuplicates(int amount){
		for(int i=0;i<amount;i++){
			var dupli = Laser.Duplicate() as Laser;
			dupli.SetScript(GD.Load("scripts/Laser.cs"));
			AddChild(dupli);
			_duplicates.Add(dupli);
		}
	}

	public void OneLaser()
	{
		Laser.RotationDegrees = 0f;
		foreach(var dupli in _duplicates){
			dupli.IsCasting = false;
			dupli.Enabled = false;
		}
	}

	public void MultiLaser(int beamCount = 2){
		if(beamCount < 2) return;

		float angleStep = 10f;
		int beamIndex = 0;
	
		
		for(int i=1; i<=beamCount; i++)
		{
			float angle;
			
			if(beamCount % 2 != 0)
			{
				// Odd: center laser at 0, build symmetric pairs around it
				int pairLevel = (beamIndex + 1) / 2;
				angle = (beamIndex % 2 == 0) ? -angleStep * pairLevel : angleStep * pairLevel;
			}
			else
			{
				// Even: symmetric pairs, no center
				int pairLevel = (beamIndex / 2) + 1;
				angle = (beamIndex % 2 == 0) ? -angleStep * pairLevel : angleStep * pairLevel;
			}
			
			if(i == 1)
			{
				Laser.RotationDegrees = angle;
				beamIndex++;
				continue;
			}
			
			var dupli = _duplicates[i-2];
			dupli.Enabled = true;
			dupli.RotationDegrees = angle;
			dupli.Temperature = Laser.Temperature;
			dupli.MaxResults = FiringComponent.MaxPierce.Value;
			beamIndex++;
		}
	}
	private void MaxPierceValueChanged(object sender, Observable<int>.ChanedEventArgs e)
    {
        Laser.MaxResults = e.NewValue;
    }
}
