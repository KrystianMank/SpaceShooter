using System;
using Godot;
using GameEnums;
using System.Collections.Generic;
interface IEntitySpawn
{
	void Spawn(Entity entity, Node parent);
	void SetHealth(Entity entity, double minHealth, double maxHealth, double healthMultiplier);
	void SetHealthBar(Entity entity, PackedScene entityHealthBar);
    void SetSpawnSpecs(EntitySpawnParams entitySpawnParams);
}