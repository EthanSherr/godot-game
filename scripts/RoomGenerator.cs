using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class RoomGenerator : Node2D
{
    Vector2I MinDims = new Vector2I(5, 5);
    Vector2I MaxDims = new Vector2I(125, 100);

    float Radius = 300;

    [Export]
    private ulong Seed = 0;

    private RandomNumberGenerator rng;

    private List<RoomVisualizer> rooms = new List<RoomVisualizer>();

    public override void _Ready()
    {
        rng = new RandomNumberGenerator();
        rng.Seed = Seed;

        GenerateRooms();
    }

    public async void GenerateRooms()
    {
        for (int i = 0; i < 150; i++)
        {
            Vector2I dimension = GenerateRandomDims();
            RoomVisualizer block = new RoomVisualizer
            {
                Width = dimension.X,
                Height = dimension.Y,
                Dim = 1,
                BorderColor = new Color(1, 0, 0),
                FillColor = new Color(0, 0, 1),
                BorderThickness = 1,
            };
            block.Position = RandomPointInCircle(Radius);
            AddChild(block);

            rooms.Add(block);
            await Task.Delay(1);
        }

        foreach (var room in rooms)
        {
            room.Activate();
        }
    }

    public Vector2 RandomPointInCircle(float radius)
    {
        float theta = 2 * (float)Math.PI * rng.Randf();
        float u = rng.Randf() + rng.Randf();
        float r = u > 1 ? 2 - u : u;
        return new Vector2(
            radius * r * (float)Math.Cos(theta),
            radius * r * (float)Math.Sin(theta)
        );
    }

    public Vector2I GenerateRandomDims()
    {
        return new Vector2I(
            rng.RandiRange(MinDims.X, MaxDims.X),
            rng.RandiRange(MinDims.Y, MaxDims.Y)
        );
    }
}
