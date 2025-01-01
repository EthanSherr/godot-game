using Godot;

public partial class RoomVisualizer : Node2D
{
    // Parameters for the rectangle
    [Export]
    public float Width = 10f;

    [Export]
    public float Height = 10f;

    [Export]
    public int Dim = 16; // Scaling factor for screen space

    [Export]
    public Color BorderColor = new Color(1, 1, 1); // White

    [Export]
    public Color FillColor = new Color(0.5f, 0.5f, 0.5f); // Gray

    [Export]
    public float BorderThickness = 2f;

    private StaticBody2D staticBody;
    private RectangleShape2D collisionShape;

    public override void _Ready()
    {
        staticBody = new StaticBody2D();
        collisionShape = new RectangleShape2D();

        // Set collision shape dimensions
        collisionShape.Size = new Vector2(Width * Dim / 2, Height * Dim / 2);

        var shape = new CollisionShape2D { Shape = collisionShape };
        staticBody.AddChild(shape);

        // Add the StaticBody2D to this node
        AddChild(staticBody);
    }

    public override void _Draw()
    {
        // Calculate actual dimensions
        float actualWidth = Width * Dim;
        float actualHeight = Height * Dim;

        // Rectangle position and size
        Rect2 rect = new Rect2(0, 0, actualWidth, actualHeight);

        // Draw the filled rectangle
        DrawRect(rect, FillColor);

        // Draw the border
        DrawStyleboxBorder(rect, BorderColor, BorderThickness);
    }

    private void DrawStyleboxBorder(Rect2 rect, Color color, float thickness)
    {
        // Draw the border lines manually to simulate a border
        DrawLine(rect.Position, rect.Position + new Vector2(rect.Size.X, 0), color, thickness); // Top
        DrawLine(rect.Position, rect.Position + new Vector2(0, rect.Size.Y), color, thickness); // Left
        DrawLine(
            rect.Position + new Vector2(0, rect.Size.Y),
            rect.Position + rect.Size,
            color,
            thickness
        ); // Bottom
        DrawLine(
            rect.Position + new Vector2(rect.Size.X, 0),
            rect.Position + rect.Size,
            color,
            thickness
        ); // Right
    }
}
