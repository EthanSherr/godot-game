using System.Collections.Generic;
using Godot;

public partial class LineIntersectsTest : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var dd = new DebugDrawer();
        AddChild(dd);

        var markers = FindAllMarker2DNodes(this);
        var p1 = markers[0].Position;
        var d1 = markers[1].Position - markers[0].Position;

        var p2 = markers[2].Position;
        var d2 = markers[3].Position - markers[2].Position;

        dd.AddLine(p1, d1 * 50 + p1, new Color(1, 1, 1));
        dd.AddLine(p2, d2 * 50 + p2, new Color(1, 1, 1));

        var intersect = MathUtils.IntersectLines(p1, d1, p2, d2);

        var rect = new DebugRectangle
        {
            BorderColor = new Color(1, 0, 0),
            BorderThickness = 1,
            FillColor = new Color(0, 1, 0),
            Size = new Vector2(1, 1),
            Position = intersect,
        };
        AddChild(rect);
    }

    private List<Marker2D> FindAllMarker2DNodes(Node parent)
    {
        List<Marker2D> marker2DNodes = new List<Marker2D>();

        foreach (Node child in parent.GetChildren())
        {
            if (child is Marker2D marker)
            {
                marker2DNodes.Add(marker);
            }

            // Recursively search the child's children
            marker2DNodes.AddRange(FindAllMarker2DNodes(child));
        }

        return marker2DNodes;
    }
}
