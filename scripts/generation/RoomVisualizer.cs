using Godot;

public partial class RoomVisualizer : RigidBody2D
{
    // Parameters for the rectangle
    [Export]
    public Vector2 Size = new Vector2(4f, 4f);

    public float Dim = Constants.GridSize;

    public int Id;

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

        _collisionShape.Position = screenSize / 2;
        _collisionShape.Shape = shape;
        _collisionShape.Disabled = false;

        CollisionLayer = CollisionLayers.ROOM;
        // Initially I have no collision mask - so initially Rooms can overlap.
        SetCollidesWithOtherRooms(false);

        debugRectangle.Size = screenSize;

        var roomLines = new DebugDrawer();
        var lineColor = new Color(0.2f, 0.2f, 0.2f);
        // var upperLeft = -screenSize / 2;
        for (var x = 1; x < Size.X; x++)
        {
            var start = new Vector2(x * Dim, 0); // + upperLeft;
            var end = new Vector2(x * Dim, screenSize.Y); // + upperLeft;
            roomLines.AddLine(start, end, lineColor);
        }
        for (var y = 1; y < Size.Y; y++)
        {
            var start = new Vector2(0, y * Dim); // + upperLeft;
            var end = new Vector2(screenSize.X, y * Dim); // + upperLeft;
            roomLines.AddLine(start, end, lineColor);
        }

        Label label = new Label();
        label.Text = Id.ToString();
        label.Position = new Vector2(1, 1); // - screenSize / 2;

        AddChild(_collisionShape);
        AddChild(debugRectangle);
        AddChild(roomLines);
        AddChild(label);

        // AddChild(new DebugCircle { Position = new Vector2(0, 0), Radius = 16f });
    }

    public void SetCollidesWithOtherRooms(bool enabled)
    {
        CollisionMask = enabled ? CollisionLayers.ROOM : CollisionLayers.NONE;

        // re-awaken!
        _collisionShape.Disabled = true;
        _collisionShape.Disabled = false;
    }

    public Rect2 GetRect()
    {
        return new Rect2(Position, GetSize());
    }

    public void SnapToGrid()
    {
        // GD.Print(
        //     $"SnapToGrid BEFORE: {Id} = {GetRect()} , min: {Position} max: {Position + GetSize()}"
        // );
        // Position = Position.SnapToGrid();
        Position = (Position / Dim).Round() * Dim;
        // GD.Print($"SnapToGrid AFTER: {Id} = min: {Position} max: {Position + GetSize()}");
    }

    public Vector2 GetSize()
    {
        return Size * Dim;
    }

    public Vector2 GetCentroid()
    {
        return Position + GetSize() / 2;
    }
}
