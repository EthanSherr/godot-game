using Godot;

public partial class RoomVisualizer : RigidBody2D
{
    // Parameters for the rectangle
    [Export]
    public Vector2 Size = new Vector2(4f, 4f);

    [Export]
    public int Dim = 16;

    [Export]
    public Color BorderColor
    {
        get { return debugRectangle.BorderColor; }
        set { debugRectangle.BorderColor = value; }
    }

    [Export]
    public Color FillColor
    {
        get { return debugRectangle.FillColor; }
        set { debugRectangle.FillColor = value; }
    }

    [Export]
    public float BorderThickness
    {
        get { return debugRectangle.BorderThickness; }
        set { debugRectangle.BorderThickness = value; }
    }

    private CollisionShape2D _collisionShape;

    private DebugRectangle _debugRectangle;
    private DebugRectangle debugRectangle
    {
        get
        {
            if (_debugRectangle == null)
            {
                _debugRectangle = new DebugRectangle();
            }
            return _debugRectangle;
        }
        set { _debugRectangle = value; }
    }

    public RoomVisualizer() { }

    public override void _Ready()
    {
        LockRotation = true;
        FreezeMode = FreezeModeEnum.Kinematic;
        _collisionShape = new CollisionShape2D();
        var screenSize = GetSize();
        var shape = new RectangleShape2D { Size = screenSize };
        GravityScale = 0;

        _collisionShape.Shape = shape;
        _collisionShape.Disabled = true;

        debugRectangle.Size = screenSize;

        var roomLines = new DebugDrawer();
        var lineColor = new Color(0.2f, 0.2f, 0.2f);
        var upperLeft = -screenSize / 2;
        for (var x = 1; x < Size.X; x++)
        {
            var start = new Vector2(x * Dim, 0) + upperLeft;
            var end = new Vector2(x * Dim, screenSize.Y) + upperLeft;
            roomLines.AddLine(start, end, lineColor);
        }
        for (var y = 1; y < Size.Y; y++)
        {
            var start = new Vector2(0, y * Dim) + upperLeft;
            var end = new Vector2(screenSize.X, y * Dim) + upperLeft;
            roomLines.AddLine(start, end, lineColor);
        }

        AddChild(_collisionShape);
        AddChild(_debugRectangle);
        AddChild(roomLines);
    }

    public void SetCollisionEnabled(bool enabled)
    {
        _collisionShape.Disabled = !enabled;
    }

    public Rect2 GetRect()
    {
        var size = GetSize();
        return new Rect2(Position - size / 2, size);
    }

    public Vector2 GetSize()
    {
        return Size * Dim;
    }
}
