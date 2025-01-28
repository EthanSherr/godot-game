using Godot;

public partial class DebugCircle : Node2D
{
    public float Radius { get; set; } = 50f;
    public Color? FillColor { get; set; } = new Color(1, 0, 0, 0.5f);

    public Color LineColor = new Color(1, 0, 0, 0.75f);

    public float LineWidth = 0;

    public override void _Draw()
    {
        if (FillColor != null)
        {
            DrawCircle(new Vector2(0, 0), Radius, FillColor.Value);
        }
        if (LineWidth > 0)
        {
            DrawArc(new Vector2(0, 0), Radius, 0, Mathf.Tau, 64, LineColor, LineWidth);
        }
    }

    public void SetCircle(Vector2 center, float radius, Color color)
    {
        Position = center;
        Radius = radius;
        FillColor = color;
        QueueRedraw();
    }
}
