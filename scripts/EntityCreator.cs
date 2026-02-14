using Godot;
using System;
using GameEnums;
using System.Diagnostics;

public partial class EntityCreator : Node, IEntitySpawn
{
	[Export]
	public PackedScene EntityScene;
	public EntityType EntityType;
    public override void _Ready()
    {
    }

	public void Spawn(Entity entity, Node parent)
    {
        parent.GetTree().Root.AddChild(entity);
    }

    public Entity Instantiate(EntityType entityType, PackedScene entityScene)
    {
        Entity entity = entityType switch
		{
			EntityType.Meteor => entityScene.Instantiate<Meteor>(),
			EntityType.ShootingAlien => entityScene.Instantiate<Alien>(),
			//EntityType.SeekingAlien => entityScene.Instantiate<SeekingAlien>(),
			_ => entityScene.Instantiate<Meteor>()
		};
		return entity;
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

    public Array SpawnSpecs()
    {
        throw new NotImplementedException();
    }
}
