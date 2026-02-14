using System;
using Godot;
using GameEnums;
interface IEntitySpawn
{
	void Spawn(Entity entity, Node parent);
	Entity Instantiate(EntityType entityType, PackedScene entityScene);
	void SetHealth(Entity entity, double minHealth, double maxHealth, double healthMultiplier);
	void SetHealthBar(Entity entity, PackedScene entityHealthBar);
    Array SpawnSpecs();
}