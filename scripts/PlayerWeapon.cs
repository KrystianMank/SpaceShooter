using Godot;
using GameEnums;
using System.Collections.Generic;
using System.Linq;
using GenericObervable;
using WeaponNamescape;

public partial class PlayerWeapon : Node
{
	[Export]
	public Laser Laser;
	[Export]
	public Maschinegun Maschinegun;
	[Export]
	public Rocketlauncher Rocketlauncher;
	public Node[] Weapons;
	public List<Node> WeaponsWithFiringComponent;
	[Export]
	public Texture2D[] WeaponFrames;
	[Export]
	public AnimatedSprite2D WeaponChangeAnimation;
	//public FiringComponent FiringComponent;
	public PlayerStats PlayerStats;
	public WeaponTypes CurrentWeaponType;
	private int _weaponIndex;
	public IBaseWeapon CurrentWeapon;
	//public Laser Laser;
	private List<Laser> _duplicates = [];
	private Texture2D _currentWeaponFrame;
	public bool WeaponChangeAnimationFinished = true;
	const float WEAPON_CHANGE_TIME = 2F;

	private readonly WeaponStatsMultiplier _maschineGunStatsMultipier = new(1,1);
	private readonly WeaponStatsMultiplier _rocketLauncherStatsMultiplier = new(4, 2);
	private readonly WeaponStatsMultiplier _laserStatsMultiplier = new(0.1, 0.15);

	public Observable<int> MaxPierce = new()
	{
		Value = 1
	};


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Weapons = [Maschinegun, Rocketlauncher, Laser];
		MaxPierce.Changed += MaxPierceValueChanged;
		WeaponChangeAnimation.SpeedScale = WEAPON_CHANGE_TIME;
		WeaponsWithFiringComponent = Weapons
			.Where(weapon => weapon.HasNode(nameof(FiringComponent)))
			.ToList();

		Reset();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		var bulletSpawn = GetParent().GetNode<Marker2D>("BulletSpawn").GlobalPosition;
		WeaponsWithFiringComponent.ForEach(weapon => 
			weapon.GetNode<FiringComponent>(nameof(FiringComponent)).BulletSpawn.GlobalPosition = bulletSpawn);
		WeaponChangeAnimation.GlobalPosition = bulletSpawn + new Vector2(20,0);
		
		// Update laser position to follow player
		Laser.GlobalPosition = bulletSpawn;
		foreach(var dupli in _duplicates)
		{
			if(dupli != null)
			{
				dupli.GlobalPosition = bulletSpawn;
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
		// Laser = WeaponScenes[2].Instantiate<Laser>();
		// AddChild(Laser);

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

	public void TryFireWeapon(bool fire)
	{
		switch (CurrentWeaponType)
		{
			case WeaponTypes.Laser:
				{
					TryFireLasers(fire &&  !Laser.IsOnCooldown);
				}
			break;
			case WeaponTypes.MaschineGun:
				{
					Maschinegun.FiringComponent.TryShoot(fire);
				}
				break;
			
			case WeaponTypes.RocketLauncher:
				{
					Rocketlauncher.FiringComponent.TryShoot(fire);
				}
			break;
		}
	}

	/// <summary>
	/// Reset current weapon to default one (maschine gun)
	/// </summary>
	public void Reset()
	{
		_weaponIndex = 2;
		CurrentWeaponType = (WeaponTypes)_weaponIndex;

		CurrentWeapon =(IBaseWeapon) Weapons[_weaponIndex];
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

		Laser.ResetLaserProperties();
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
			? (_weaponIndex < Weapons.Length -1 ? ++_weaponIndex : _weaponIndex = 0)
			: (_weaponIndex > 0 ? --_weaponIndex : _weaponIndex = Weapons.Length - 1);
		CurrentWeaponType = (WeaponTypes)_weaponIndex;
		CurrentWeapon = (IBaseWeapon)Weapons[_weaponIndex];
		_currentWeaponFrame = WeaponFrames[_weaponIndex];

		WeaponChangeAnim();
	}
	
	/// <summary>
	/// Set weapon variables
	/// </summary>
	/// <param name="weapon"></param>
	/// <param name="weaponStats"></param>
	/// <param name="weaponType"></param>
	public void SetWeaponVariables(IBaseWeapon weapon, BaseWeaponStats weaponStats, WeaponTypes weaponType)
	{
		switch (weaponType)
		{
			case WeaponTypes.MaschineGun:
				{
					if(weapon is Maschinegun maschinegun && weaponStats is MaschineGunStats maschineGunStats)
					{
						maschinegun.SetWeaponVariables(maschineGunStats);
					}
					break;
				}
			case WeaponTypes.RocketLauncher:
				{
					if(weapon is Rocketlauncher rocketlauncher && weaponStats is RocketLauncherStats rocketLauncherStats)
					{
						rocketlauncher.SetWeaponVariables(rocketLauncherStats);
					}
					break;
				}
			case WeaponTypes.Laser:
				{
					if(weapon is Laser laser && weaponStats is LaserStats laserStats)
					{
						laser.SetWeaponVariables(laserStats);
					}
					break;
				}
		}
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
					WeaponsWithFiringComponent.ForEach(weapon => 
						weapon.GetNode<FiringComponent>(nameof(FiringComponent)).SetBulletSpawnVariables(1, 0f));
				}
				break;
			case 2: 
				{
					WeaponsWithFiringComponent.ForEach(weapon => 
						weapon.GetNode<FiringComponent>(nameof(FiringComponent)).SetBulletSpawnVariables(2, 2f));
				}
				break;
			case 3: 
				{
					WeaponsWithFiringComponent.ForEach(weapon => 
						weapon.GetNode<FiringComponent>(nameof(FiringComponent)).SetBulletSpawnVariables(3, 3f));
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


		double damage = PlayerStats.Damage.Value * multiplier.BulletDamageMultiplier;
		double fireRate = PlayerStats.FireRate.Value * multiplier.FireRateMultiplier;

		// Set current weapon stats
		BaseWeaponStats weaponStats = CurrentWeaponType switch
		{
			WeaponTypes.MaschineGun => new MaschineGunStats(PlayerStats.BulletSpeed.Value, damage, fireRate),
			WeaponTypes.RocketLauncher => new RocketLauncherStats(200d, damage, fireRate),
			WeaponTypes.Laser => new LaserStats(10d, damage, fireRate),
			_ => new MaschineGunStats(PlayerStats.BulletSpeed.Value, damage, fireRate)
		};

		SetWeaponVariables(CurrentWeapon, weaponStats, CurrentWeaponType);

		if(CurrentWeaponType == WeaponTypes.Laser && weaponStats is LaserStats laserStats)
		{
			Laser.GetNode<CanvasLayer>(nameof(CanvasLayer)).Visible =  true;
			Laser.MaxResults = MaxPierce.Value;
			OneLaser();
		}
		else
		{
			if(IsInstanceValid(Laser)) {
				Laser.GetNode<CanvasLayer>(nameof(CanvasLayer)).Visible = false;
			}
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
			dupli.MaxResults = MaxPierce.Value;
			beamIndex++;
		}
	}
	private void MaxPierceValueChanged(object sender, Observable<int>.ChanedEventArgs e)
    {
        Laser.MaxResults = e.NewValue;
		WeaponsWithFiringComponent.ForEach(w => w.GetNode<FiringComponent>(nameof(FiringComponent)).MaxPierce = e.NewValue);
    }
}
