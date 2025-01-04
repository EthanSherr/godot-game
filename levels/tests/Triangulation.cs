using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Triangulation : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var Size = new Vector2(16, 16);

        var rects = new List<DebugRectangle>
        {
            // new DebugRectangle { Size = Size, Position = new Vector2(0, -50) * 2 },
            new DebugRectangle { Size = Size, Position = new Vector2(-150, -50) * 2 },
            new DebugRectangle { Size = Size, Position = new Vector2(-50, 50) * 2 },
            new DebugRectangle { Size = Size, Position = new Vector2(50, 50) * 2 },
            new DebugRectangle { Size = Size, Position = new Vector2(25, 50) * 2 },
            new DebugRectangle { Size = Size, Position = new Vector2(30, -50) * 2 },
            new DebugRectangle { Size = Size, Position = new Vector2(15, 2) * 2 },
        };

        rects.ForEach(r => AddChild(r));
        var centroids = rects.Select(r => r.Position);

        // start old
        // var triangulator = new OldDelaunayTriangulation();
        // var triangles = triangulator.Triangulate(centroids.ToList());

        // var debugLines = new DebugDrawer();
        // AddChild(debugLines);

        // var color = new Color(1, 0, 0);

        // foreach (var triangle in triangles)
        // {
        //     debugLines.AddLine(triangle.A, triangle.B, color);
        //     debugLines.AddLine(triangle.B, triangle.C, color);
        //     debugLines.AddLine(triangle.C, triangle.A, color);
        // }
        // end old

        var debugNewLines = new DebugDrawer();
        AddChild(debugNewLines);

        var newTriangle = new DelaunayTriangulation();
        // newTriangle.Triangulate(centroids.ToList());
        var output = newTriangle.Triangulate(centroids.ToList());

        // GD.Print("Done with all triangles:", triangles.Count);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}
