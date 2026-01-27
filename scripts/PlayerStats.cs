using System.Collections.Generic;
using GenericObervable;
using Godot;
public class PlayerStats
{
    public Observable<int> SkillPoints = new();
    public Observable<int> Speed = new();
    public Observable<double> FireRate = new();
    public Observable<double> Luck = new();
    public Observable<int> BulletSpeed = new();
    public Observable<double> Damage = new();
    public HealthComponent Health = new();
    public Observable<double> MaxHealth = new();
    //Powerups timer values
    public Observable<double> InvincibilityPowerupDuration = new();
    public Observable<double> PiercingPowerupDuration = new();
    public Observable<double> MultishotPowerupDuration = new();
    public Observable<double> DashPowerupDuration = new();
    public Observable<double> RocketsPowerupDuration = new();


    public Dictionary<string,object> PlayerStatsList = new Dictionary<string, object>();
    public FiringComponent FiringComponent;
    public PlayerStats(FiringComponent firingComponent)
    {
      FiringComponent = firingComponent;

      PlayerStatsList.Add(nameof(Speed), Speed.Value);
      PlayerStatsList.Add(nameof(FireRate), FireRate.Value);
      PlayerStatsList.Add(nameof(Luck), Luck.Value);
      PlayerStatsList.Add(nameof(BulletSpeed), BulletSpeed.Value);
      PlayerStatsList.Add(nameof(Damage), Damage.Value);
      PlayerStatsList.Add(nameof(MaxHealth), MaxHealth.Value);
      PlayerStatsList.Add(nameof(InvincibilityPowerupDuration), InvincibilityPowerupDuration.Value);
      PlayerStatsList.Add(nameof(PiercingPowerupDuration), PiercingPowerupDuration.Value);
      PlayerStatsList.Add(nameof(MultishotPowerupDuration), MultishotPowerupDuration.Value);
      PlayerStatsList.Add(nameof(DashPowerupDuration), DashPowerupDuration.Value);
      PlayerStatsList.Add(nameof(RocketsPowerupDuration), RocketsPowerupDuration.Value);

      Speed.Changed += OnSpeedValueChanged;
      FireRate.Changed += OnFirerateValueChanged;
      Luck.Changed += OnLuckValueChanged;
      BulletSpeed.Changed += OnBulletSpeedValueChanged;
      Damage.Changed += OnDamageValueChanged;
      Health.GetHP().Changed += OnHealthValueChanged;
      MaxHealth.Changed += OnMaxHealthValueChanged;

      InvincibilityPowerupDuration.Changed += OnInvinciblityPowerupDurationValueChanged;
      PiercingPowerupDuration.Changed += OnPiercingPowerupDurationValueChanged;
      MultishotPowerupDuration.Changed += OnMultishotPowerupDurationValueChanged;
      DashPowerupDuration.Changed += OnDashPowerupDurationValueChanged;
      RocketsPowerupDuration.Changed += OnRocketsPowerupDurationChanged;
    }
    public void OnFirerateValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
      PlayerStatsList[nameof(FireRate)] = eventArgs.NewValue;
      FiringComponent.BulletFirerate.Value = eventArgs.NewValue;
    }

    public void OnSpeedValueChanged(object target, Observable<int>.ChanedEventArgs eventArgs)
    {
    PlayerStatsList[nameof(Speed)] = eventArgs.NewValue;
    }
  public void OnLuckValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
    PlayerStatsList[nameof(Luck)] = eventArgs.NewValue;
    }
    public void OnBulletSpeedValueChanged(object target, Observable<int>.ChanedEventArgs eventArgs)
    {
      PlayerStatsList[nameof(BulletSpeed)] = eventArgs.NewValue;
      FiringComponent.BulletSpeed.Value = eventArgs.NewValue;
    }
    public void OnDamageValueChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
    PlayerStatsList[nameof(Damage)] = eventArgs.NewValue;
    FiringComponent.BulletDamage.Value = eventArgs.NewValue;
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
    public void OnRocketsPowerupDurationChanged(object target, Observable<double>.ChanedEventArgs eventArgs)
    {
      PlayerStatsList[nameof(RocketsPowerupDuration)] = eventArgs.NewValue;
    }
}