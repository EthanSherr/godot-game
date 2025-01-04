using System;
using Godot;

public partial class TriangleCircumcircle : Node2D
{
    public override void _Ready()
    {
        var markers = NodeUtils.FindAllNodesOfType<Marker2D>(this);

        var dd = new DebugDrawer();

        var A = markers[0].Position;
        var B = markers[1].Position;
        var C = markers[2].Position;
        var color = new Color(1, 0, 0);

        var (center, radiusSq) = MathUtils.CircumCircle(A, B, C);
        var circle = new DebugCircle { Radius = (float)Math.Sqrt(radiusSq), Position = center };
        AddChild(circle);

        AddChild(dd);
        dd.AddLine(A, B, color);
        dd.AddLine(B, C, color);
        dd.AddLine(C, A, color);
    }
}
