using System;
using Godot;

public partial class PlayerSpawn : Node2D
{
    public Vector2I _gridPosition;
    public Vector2I GridPosition
    {
        get => _gridPosition;
        set
        {
            _gridPosition = value;
            Position = Constants.GridSize * (Vector2)_gridPosition;
        }
    }

    private const string SpawnScenePath = "res://scripts/game/PlayerSpawn.tscn";

    public static PlayerSpawn CreatePlayerSpawn()
    {
        return NodeUtils.CreateFromScene<PlayerSpawn>(SpawnScenePath);
    }
}
