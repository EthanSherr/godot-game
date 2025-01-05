using Godot;

public partial class DebugRectangle : Node2D
{
    [Export]
    public Vector2 Size;

    private Color _borderColor = new Color(0.5f, 0.5f, 0.5f);

    [Export]
    public Color BorderColor
    {
        get => _borderColor;
        set
        {
            if (_borderColor != value)
            {
                _borderColor = value;
                QueueRedraw();
            }
        }
    }

    private Color _fillColor = new Color(0.5f, 0.5f, 0.5f);

    [Export]
    public Color FillColor
    {
        get => _fillColor;
        set
        {
            if (_fillColor != value)
            {
                _fillColor = value;
                QueueRedraw();
            }
        }
    }

    private float _borderThickness = 2f;

    [Export]
    public float BorderThickness
    {
        get => _borderThickness;
        set
        {
            if (_borderThickness != value)
            {
                _borderThickness = value;
                QueueRedraw();
            }
        }
    }

    public override void _Draw()
    {
        Rect2 rect = new Rect2(-Size.X / 2, -Size.Y / 2, Size.X, Size.Y);
        DrawRect(rect, FillColor);
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

    // public override void _Process(double delta)
    // {
    //     QueueRedraw();
    // }
}
