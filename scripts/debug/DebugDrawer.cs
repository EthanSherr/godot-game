using System.Collections.Generic;
using Godot;

public partial class DebugDrawer : Node2D
{
    private List<(Vector2, Vector2, Color)> _lines = new List<(Vector2, Vector2, Color)>();

    public void AddLine(Vector2 start, Vector2 end, Color color)
    {
        _lines.Add((start, end, color));
    }

    public override void _Draw()
    {
        foreach (var (start, end, color) in _lines)
        {
            DrawLine(start, end, color, 2); // Draw each line with width 2
        }
    }
}
