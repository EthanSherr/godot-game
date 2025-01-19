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

    public static PlayerSpawn Create()
    {
        return NodeUtils.CreateFromScene<PlayerSpawn>(SpawnScenePath);
    }

    public override void _Ready()
    {
        var rect = new DebugRectangle();
        rect.FillColor = new Color(0, 1, 0);
        rect.Size = new Vector2(Constants.GridSize, Constants.GridSize);
        rect.Position = new Vector2(0, 0);
        AddChild(rect);
    }
}
