using System;
using Godot;

public partial class TileMapper : Node2D
{
    [Export]
    public TileMapLayer mapLayer;

    public override void _Ready()
    {
        mapLayer = GetNode<TileMapLayer>("TileMapLayer");
    }

    public void FillCell(Vector2I Position)
    {
        int IMAGE_SOURCE = 0;
        Vector2I AtlastCoordsx = new Vector2I(2, 2);
        mapLayer.SetCell(Position, IMAGE_SOURCE, AtlastCoordsx);
        // mapLayer.UpdatePhysics();
        // mapLayer.setphysics
    }

    public void UnfillCell(Vector2I Position)
    {
        int IMAGE_SOURCE = -1; // Indicates no tile
        mapLayer.SetCell(Position, IMAGE_SOURCE);
    }

    public void AddLadder(Vector2I Position) {
        int IMAGE_SOURCE = 0;
        Vector2I AtlastCoordsx = new Vector2I(5, 11);
        mapLayer.SetCell(Position, IMAGE_SOURCE, AtlastCoordsx);
    }
}
