using System.Collections.Generic;
using Godot;

public partial class DebugDrawer : Node2D
{
    private List<(Vector2, Vector2, Color)> _lines = new List<(Vector2, Vector2, Color)>();

    public float Thickness = 2f;

    public void AddLine(Vector2 start, Vector2 end, Color color)
    {
        _lines.Add((start, end, color));
    }

    public void AddTriangle(Vector2 A, Vector2 B, Vector2 C, Color color)
    {
        AddLine(A, B, color);
        AddLine(B, C, color);
        AddLine(C, A, color);
    }

    public override void _Draw()
    {
        foreach (var (start, end, color) in _lines)
        {
            DrawLine(start, end, color, Thickness); // Draw each line with width 2
        }
    }
}
