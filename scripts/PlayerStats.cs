using System.Collections.Generic;
using GenericObervable;
using Godot;
using WeaponNamescape;
public class PlayerStats
{
    public Observable<int> SkillPoints = new();
    public Observable<int> Speed = new();
    public Observable<double> Luck = new();
    public HealthComponent Health = new();
    public Observable<double> MaxHealth = new();
    //Powerups timer values
    public Observable<double> InvincibilityPowerupDuration = new();
    public Observable<double> PiercingPowerupDuration = new();
    public Observable<double> MultishotPowerupDuration = new();
    public Observable<double> DashPowerupDuration = new();

    // Weapon stats
    public Observable<int> BulletSpeed = new();
    public Observable<double> ExplosionRadius = new();
    public Observable<double> LaserWidth = new();
    public Observable<double> Damage = new();
    public Observable<double> FireRate = new();

    public Dictionary<string,double> PlayerStatsList = new Dictionary<string, double>();
    public PlayerWeapon PlayerWeapon;
    public PlayerStats(PlayerWeapon playerWeapon)
    {
      PlayerWeapon = playerWeapon;
      PlayerStatsList.Add(nameof(BulletSpeed), BulletSpeed.Value);
      PlayerStatsList.Add(nameof(ExplosionRadius), ExplosionRadius.Value);
      PlayerStatsList.Add(nameof(LaserWidth), LaserWidth.Value);
      PlayerStatsList.Add(nameof(Damage), Damage.Value);
      PlayerStatsList.Add(nameof(FireRate), FireRate.Value);
      PlayerStatsList.Add(nameof(Speed), Speed.Value);
      PlayerStatsList.Add(nameof(Luck), Luck.Value);
      PlayerStatsList.Add(nameof(MaxHealth), MaxHealth.Value);
      PlayerStatsList.Add(nameof(InvincibilityPowerupDuration), InvincibilityPowerupDuration.Value);
      PlayerStatsList.Add(nameof(PiercingPowerupDuration), PiercingPowerupDuration.Value);
      PlayerStatsList.Add(nameof(MultishotPowerupDuration), MultishotPowerupDuration.Value);
      PlayerStatsList.Add(nameof(DashPowerupDuration), DashPowerupDuration.Value);

      Speed.Changed += OnSpeedValueChanged;
      FireRate.Changed += OnFirerateValueChanged;
      Luck.Changed += OnLuckValueChanged;
      Damage.Changed += OnDamageValueChanged;
      Health.GetHP().Changed += OnHealthValueChanged;
      MaxHealth.Changed += OnMaxHealthValueChanged;

      BulletSpeed.Changed += OnBulletSpeedValueChanged;
      ExplosionRadius.Changed += OnExplosionRadiusValueChanged;
      LaserWidth.Changed += OnLaserWidthValueChanged;

      InvincibilityPowerupDuration.Changed += OnInvinciblityPowerupDurationValueChanged;
      PiercingPowerupDuration.Changed += OnPiercingPowerupDurationValueChanged;
      MultishotPowerupDuration.Changed += OnMultishotPowerupDurationValueChanged;
      DashPowerupDuration.Changed += OnDashPowerupDurationValueChanged;
    }
    public void OnFirerateValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
      if (PlayerWeapon?.CurrentWeapon != null)
      {
          PlayerStatsList[nameof(FireRate)] = PlayerWeapon.CurrentWeapon.GetWeaponStats().FireRate;
          PlayerWeapon.SetWeapon();
      }
    }
    public void OnDamageValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
      if(PlayerWeapon?.CurrentWeapon != null)
      {
        PlayerStatsList[nameof(Damage)] = PlayerWeapon.CurrentWeapon.GetWeaponStats().Damage;
        PlayerWeapon.SetWeapon();
      }
    }
    // Maschinegun specific stats
    public void OnBulletSpeedValueChanged(object target, Observable<int>.ChanedEventArgs eventArgs)
    {
      if(PlayerWeapon?.CurrentWeapon != null && PlayerWeapon?.CurrentWeapon.GetWeaponStats() is MaschineGunStats maschineGunStats)
      {
        PlayerStatsList[nameof(BulletSpeed)] = maschineGunStats.BulletSpeed;
        PlayerWeapon.SetWeapon();
      }
    }

    // RocketLauncher specific stats
    public void OnExplosionRadiusValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
      if(PlayerWeapon?.CurrentWeapon != null && PlayerWeapon?.CurrentWeapon.GetWeaponStats() is LaserStats laserStats)
      {
        PlayerStatsList[nameof(LaserWidth)] = laserStats.LaserWidth;
        PlayerWeapon.SetWeapon();
      }
    }
    // Laser specific stats
    public void OnLaserWidthValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
      if(PlayerWeapon?.CurrentWeapon != null && PlayerWeapon?.CurrentWeapon.GetWeaponStats() is RocketLauncherStats rocketLauncherStats)
      {
        PlayerStatsList[nameof(ExplosionRadius)] = rocketLauncherStats.ExplosionRadius;
        PlayerWeapon.SetWeapon();
      }
    }

    public void OnSpeedValueChanged(object target, Observable<int>.ChanedEventArgs eventArgs)
    {
    PlayerStatsList[nameof(Speed)] = eventArgs.NewValue;
    }
  public void OnLuckValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
    PlayerStatsList[nameof(Luck)] = eventArgs.NewValue;
    }
    
    
    public void OnHealthValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
      PlayerStatsList[nameof(Health)] = eventArgs.NewValue;
    }
    public void OnMaxHealthValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
      PlayerStatsList[nameof(MaxHealth)] = eventArgs.NewValue;
      Health.SetHP(MaxHealth.Value);
    }
  public void OnInvinciblityPowerupDurationValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
    PlayerStatsList[nameof(InvincibilityPowerupDuration)] = eventArgs.NewValue;
    }
  public void OnPiercingPowerupDurationValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
    PlayerStatsList[nameof(PiercingPowerupDuration)] = eventArgs.NewValue;
    }
    public void OnMultishotPowerupDurationValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
    PlayerStatsList[nameof(MultishotPowerupDuration)] = eventArgs.NewValue;
    }
    public void OnDashPowerupDurationValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
    PlayerStatsList[nameof(DashPowerupDuration)] = eventArgs.NewValue;
    }
    // / <summary>
    // / Updates weapon-related stats from PlayerWeapon to ensure they're current
    // / </summary>
    public void UpdateWeaponStats()
    {
      if (PlayerWeapon?.CurrentWeapon != null)
      {
          var weaponStats = PlayerWeapon.CurrentWeapon.GetWeaponStats();
          if(weaponStats != null){
              PlayerStatsList[nameof(FireRate)] = weaponStats.FireRate;
              PlayerStatsList[nameof(Damage)] = weaponStats.Damage;

              if(weaponStats is MaschineGunStats maschineGunStats)
              {
                PlayerStatsList[nameof(BulletSpeed)] = maschineGunStats.BulletSpeed;
              }
              if(weaponStats is RocketLauncherStats rocketLauncherStats)
              {
                PlayerStatsList[nameof(ExplosionRadius)] = rocketLauncherStats.ExplosionRadius;
              }
              if(weaponStats is LaserStats laserStats)
              {
                PlayerStatsList[nameof(LaserWidth)] = laserStats.LaserWidth;
              }
          }
      }
    }
  }