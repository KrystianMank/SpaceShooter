using Godot;

public partial class PlayerWeapon : Node
{
	[Export]
	public PackedScene[] WeaponScenes;
	[Export]
	public Texture2D[] WeaponSprites;
	public FiringComponent FiringComponent;
	public PlayerStats PlayerStats;
	private int _weaponIndex;
	private PackedScene _currentWeapon;
	private Texture2D _currentWeaponTexture;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		FiringComponent = GetNode<FiringComponent>(nameof(FiringComponent));
		_weaponIndex = 0;
		_currentWeapon = WeaponScenes[_weaponIndex];
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	/// <summary>
	/// If set to true change to next weapon else to previous one
	/// </summary>
	/// <param name="next"></param>
	public void ChangeWeapon(bool next)
	{
		_weaponIndex = next
			? (_weaponIndex < WeaponScenes.Length ? _weaponIndex++ : _weaponIndex = 0)
			: (_weaponIndex > 0 ? _weaponIndex-- : _weaponIndex = WeaponScenes.Length);
		_currentWeapon = WeaponScenes[_weaponIndex];
	}
	/// <summary>
	/// Save picked weapon to FiringComponent
	/// </summary>
	public void SetWeapon()
	{
		FiringComponent.BulletScene = _currentWeapon;
	}
	/// <summary>
	/// Save picekd weapon to WeaponHolder UI
	/// </summary>
	private void SetWeaponToHUD()
	{
		Sprite2D weaponHolder = (Sprite2D)GetTree().GetFirstNodeInGroup("weapon_holder");
		weaponHolder.Texture = _currentWeaponTexture;
	}
}
