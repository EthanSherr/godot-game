using Godot;

public partial class RoomVisualizer : RigidBody2D
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

    private CollisionShape2D collisionShape;

    public override void _Ready()
    {
        LockRotation = true;
        FreezeMode = FreezeModeEnum.Kinematic;
        collisionShape = new CollisionShape2D();
        var shape = new RectangleShape2D { Size = GetSize() };
        GravityScale = 0;

        collisionShape.Shape = shape;
        AddChild(collisionShape);
        collisionShape.Disabled = true;

        var debugRect = new DebugRectangle
        {
            BorderColor = BorderColor,
            BorderThickness = BorderThickness,
            FillColor = FillColor,
            Size = new Vector2(Width, Height),
        };
        AddChild(debugRect);
    }

    public void Activate()
    {
        collisionShape.Disabled = false;
    }

    public Rect2 GetRect()
    {
        var size = GetSize();
        return new Rect2(Position - size / 2, size);
    }

    public Vector2 GetSize()
    {
        return new Vector2(Width * Dim, Height * Dim);
    }
}
