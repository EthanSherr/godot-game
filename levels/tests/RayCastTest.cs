using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class RayCastTest : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        doStuff();
    }

    public async void doStuff()
    {
        var room1 = new RoomVisualizer();
        AddChild(room1);
        room1.Position = new Vector2(30, -16);

        var room2 = new RoomVisualizer();
        AddChild(room2);
        room2.Position = new Vector2(30, -16);

        await Task.Delay(2 * 1000);

        GD.Print("setcollision enabled");
        room1.SetCollidesWithOtherRooms(true);
        room2.SetCollidesWithOtherRooms(true);

        await Task.Delay(2 * 1000);

        var intersectingObjects = new List<Node2D>();
        var rectangle = new RectangleShape2D();
        rectangle.Size = new Vector2(50, 16);
        var position = new Vector2(0, 0);

        var queryParameters = new PhysicsShapeQueryParameters2D
        {
            Shape = rectangle,
            Transform = new Transform2D(0, position), // Position of the rectangle
            CollisionMask = CollisionLayers.ROOM, // Filter objects by collision layers
        };
        var spaceState = GetWorld2D().DirectSpaceState;
        var results = spaceState.IntersectShape(queryParameters, 150);
        GD.Print("results", results);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}
