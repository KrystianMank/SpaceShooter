using Godot;
using GameEnums;

public partial class EntityCreator : IEntitySpawn
{
	[Export]
	public PackedScene EntityScene;
	public EntityType EntityType;
    private EntitySpawnParams _spawnParams;

	public void Spawn(Entity entity, Node parent)
    {
        entity.InitializeValues(_spawnParams);
        parent.GetTree().Root.AddChild(entity);
    }

    public void SetHealth(Entity entity, double minHealth, double maxHealth, double healthMultiplier)
    {
        entity.SetEnityHealthRandom(minHealth * (1 + healthMultiplier), maxHealth * (1 + healthMultiplier));
		entity.InitializeHealthBar();
    }

    public void SetHealthBar(Entity entity, PackedScene entityHealthBar)
    {
        EntityHealthbar entityHealthbar = entityHealthBar.Instantiate<EntityHealthbar>();
		entity.AddChild(entityHealthbar);
    }

    public void SetSpawnSpecs(EntitySpawnParams entitySpawnParams)
    {
        _spawnParams = entitySpawnParams;
    }

}
