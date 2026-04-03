namespace WeaponNamescape
{
    public interface IBaseWeapon
	{
		void SetWeaponVariables(BaseWeaponStats weaponStats);
		BaseWeaponStats GetWeaponStats();
	};
    public abstract class BaseWeaponStats
	{
		public double Damage;
		public double FireRate;
	}

	public class MaschineGunStats : BaseWeaponStats
	{
		public double BulletSpeed;
		public MaschineGunStats(double bulletSpeed, double damage, double fireRate)
		{
			BulletSpeed = bulletSpeed;
			Damage = damage;
			FireRate = fireRate;
		}
	}

	public class RocketLauncherStats : BaseWeaponStats
	{
		public double ExplosionRadius;
		public RocketLauncherStats(double explosionRadius, double damage, double fireRate)
		{
			ExplosionRadius = explosionRadius;
			Damage = damage;
			FireRate = fireRate;
		}
	}

	public class LaserStats : BaseWeaponStats
	{
		public double LaserWidth;
		public LaserStats(double laserWidth, double damage, double fireRate)
		{
			LaserWidth = laserWidth;
			Damage = damage;
			FireRate = fireRate;
		}
	}

	public readonly struct WeaponStatsMultiplier
	{
		public readonly double BulletDamageMultiplier;
		public readonly double FireRateMultiplier;

		public WeaponStatsMultiplier(double damageMultiplier, double fireRateMultiplier)
		{
			BulletDamageMultiplier = damageMultiplier;
			FireRateMultiplier = fireRateMultiplier;
		}
		public WeaponStatsMultiplier(){}
	}
}