using System.Collections.Generic;
using GameEnums;
using Godot;

namespace StaticClasses
{
    public static class TresholdValues
    {
        public static readonly double MAX_SPEED = 1500d;
        public static readonly double MAX_LUCK = 4d;
        public static readonly double MAX_HEALTH = 90d;

        public static readonly double MAX_INVINCIBILITY_POWERUP_DURATION = 6d;
        public static readonly double MAX_PIERCING_POWERUP_DURATION = 10d;
        public static readonly double MAX_MULTISHOT_POWERUP_DURATION = 15d;
        public static readonly double MAX_DASH_POWERUP_DURATION = 15d;

        public static readonly double MAX_BULLET_SPEED = 3000;
        public static readonly double MAX_EXPLOSION_RADIUS = 300d;
        public static readonly double MAX_LASER_WIDTH = 30d;
        public static readonly double MIN_FIRERATE = 0.1d;
        public static readonly double MAX_DAMAGE = 20d;

        public static readonly double POWERUP_SPAWN_TRESHOLD = 0.85d;

        public static Dictionary<UpgradableStatsEnum, double> TresholdValuesDictionary = [];
        
        static TresholdValues()
        {
            TresholdValuesDictionary.Add(UpgradableStatsEnum.Speed, MAX_SPEED);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.FireRate, MIN_FIRERATE);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.Luck, MAX_LUCK);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.BulletSpeed, MAX_BULLET_SPEED);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.ExplosionRadius, MAX_EXPLOSION_RADIUS);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.LaserWidth, MAX_LASER_WIDTH);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.Damage, MAX_DAMAGE);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.MaxHealth, MAX_HEALTH);
            
            TresholdValuesDictionary.Add(UpgradableStatsEnum.PiercingPowerupDuration, MAX_PIERCING_POWERUP_DURATION);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.InvincibilityPowerupDuration, MAX_INVINCIBILITY_POWERUP_DURATION);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.MultishotPowerupDuration, MAX_MULTISHOT_POWERUP_DURATION);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.DashPowerupDuration, MAX_DASH_POWERUP_DURATION);
        }
    }

    public static class PlayerStatsMultipliers
    {
        public const double FIRERATE_MULTIPLIER = 0.1d;
        public const double SPEED_MULTIPLIER = 100d;
        public const double BULLET_SPEED_MULTIPLIER = 200d;
        public const double LASER_WIDTH_MULTIPLIER = 5d;
        public const double EXPLOSION_RADIUS_MULTIPLIER = 25d;
        public const double LUCK_MULTIPLIER = 0.2d;
        public const double DAMAGE_MULTIPLIER = 0.5d;
        public const double MAX_HEALTH_MULTIPLIER = 15d;
        public const double INVINCIBILITY_POWERUP_DURATION_MULTIPLIER = 0.3d;
	    public const double PIERCING_POWERUP_DURATION_MULTIPLIER = 0.5d;
        public const double MULTISHOT_POWERUP_DURATION_MULTIPLIER = 1d;
        public const double DASH_POWERUP_DURATION_MULTIPLIER = 1d;
    }

    public static class DeafultPlayerStatsValues
    {
        public const int SKILL_POINTS = 0;
        public const int SPEED = 400;
        public const double FIRE_RATE = 0.5d;
        public const double LUCK = 1d;
        public const int BULLET_SPEED = 800;
        public const double LASER_WIDTH = 10d;
        public const double EXPLOSION_RADIUS = 150d;
        public const double DAMAGE = 1d;
        public const double HEALTH = 10d;

        public const double INVINCIBILITY_POWERUP_DURATION = 3d;
        public const double PIERCING_POWERUP_DURATION = 6d;
        public const double MULTISHOT_POWERUP_DURATION = 10d;
        public const double DASH_POWERUP_DURATION = 10d;
    }
    public static class Icons
    {
        public static Texture2D GetIcon(string iconPath)
        {
            return (Texture2D)GD.Load(ResourceLoader.Exists(iconPath) ? iconPath : "res://sprites/icon.svg");
        }

    }
}