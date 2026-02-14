using Godot;
using GameEnums;
public partial class EntityResource : Resource
{
    [Export]
	public PackedScene EntityScene;
	[Export]
	public EntityType EntityType;
    [Export]
    public double InitialSpawnTime;
    public EntityCreator CreateEntityCreator()
    {
        var creator = new EntityCreator
        {
            EntityScene = EntityScene,
            EntityType = EntityType
        };
        return creator;
    }
}