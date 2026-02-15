using System;
using Godot;

public abstract class EntitySpawnParams{}

public class MeteorSpawnParams : EntitySpawnParams
{
    public Vector2 Position {get; set;}
    public Vector2 Velocity {get; set;}
    public float RotationDirection {get; set;}
}

public class AlienSpawnParams : EntitySpawnParams
{
    public float VelocityValue {get; set;}
    public SpawnDirection SpawnDirection {get; set;}
    public Vector2 Position {get; set;}
    public int SpawnSide {get; set;}
    public double AlienBulletDamage {get;set;}
    public double AlienFirerate {get; set;}
}
