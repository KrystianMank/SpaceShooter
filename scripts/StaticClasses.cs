using System.Collections.Generic;
using GameEnums;

namespace StaticClasses
{
    public static class TresholdValues
    {
        public static readonly double MAX_SPEED = 2000d;
        public static readonly double MIN_FIRERATE = 0.1d;
        public static readonly double MAX_LUCK = 4d;
        public static readonly double MAX_BULLET_SPEED = 3000;
        public static readonly double MAX_DAMAGE = 20d;
        public static readonly double MAX_HEALTH = 90d;
        public static readonly double MAX_INVINCIBILITY_POWERUP_DURATION = 6d;
        public static readonly double MAX_PIERCING_POWERUP_DURATION = 10d;
        public static readonly double MAX_MULTISHOT_POWERUP_DURATION = 15d;
        public static readonly double MAX_DASH_POWERUP_DURATION = 15d;

        public static readonly double POWERUP_SPAWN_TRESHOLD = 0.85d;

       // public static List<double> TresholdValuesList = [];
        public static Dictionary<UpgradableStatsEnum, double> TresholdValuesDictionary = [];
        
        static TresholdValues()
        {
            // TresholdValuesList.Add(MAX_SPEED);
            // TresholdValuesList.Add(MIN_FIRERATE);
            // TresholdValuesList.Add(MAX_LUCK);
            // TresholdValuesList.Add(MAX_BULLET_SPEED);
            // TresholdValuesList.Add(MAX_DAMAGE);
            // TresholdValuesList.Add(MAX_HEALTH);
            // TresholdValuesList.Add(MAX_INVINCIBILITY_POWERUP_DURATION);
            // TresholdValuesList.Add(MAX_PIERCING_POWERUP_DURATION);
            // TresholdValuesList.Add(MAX_MULTISHOT_POWERUP_DURATION);
            // TresholdValuesList.Add(MAX_DASH_POWERUP_DURATION);

            TresholdValuesDictionary.Add(UpgradableStatsEnum.Speed, MAX_SPEED);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.FireRate, MIN_FIRERATE);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.Luck, MAX_LUCK);
            TresholdValuesDictionary.Add(UpgradableStatsEnum.BulletSpeed, MAX_BULLET_SPEED);
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
        public const double DAMAGE = 1d;
        public const double HEALTH = 10d;

        public const double INVINCIBILITY_POWERUP_DURATION = 3d;
        public const double PIERCING_POWERUP_DURATION = 6d;
        public const double MULTISHOT_POWERUP_DURATION = 10d;
        public const double DASH_POWERUP_DURATION = 10d;
    }
}