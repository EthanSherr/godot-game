using Godot;

public partial class DebugCircle : Node2D
{
    // public Vector2 Center { get; set; } = Vector2.Zero;
    public float Radius { get; set; } = 50f;
    public Color CircleColor { get; set; } = new Color(1, 0, 0, 0.5f);

    public override void _Draw()
    {
        DrawCircle(new Vector2(0, 0), Radius, CircleColor);
    }

    public void SetCircle(Vector2 center, float radius, Color color)
    {
        Radius = radius;
        CircleColor = color;
        QueueRedraw();
    }
}
