using Godot;

public partial class GodPawn : Node2D
{
    [Export]
    private float ZoomSpeed = 0.1f; // Speed of zooming

    [Export]
    private float MinZoom = 0.05f; // Minimum zoom level

    [Export]
    private float MaxZoom = 10.0f; // Maximum zoom level

    [Export]
    private float DragSpeed = 1.0f; // Speed of dragging

    private Camera2D _camera;
    private Vector2 _dragStartPosition;
    private Vector2 _dragStartMousePosition;
    private bool _isDragging = false;

    private static string ScenePath = "res://scripts/game/GodPawn.tscn";

    public static GodPawn Create()
    {
        return NodeUtils.CreateFromScene<GodPawn>(ScenePath);
    }

    public override void _Ready()
    {
        _camera = GetNode<Camera2D>("Camera2D"); // Ensure there's a Camera2D as a child
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent)
        {
            // Zoom with mouse scroll
            if (mouseEvent.ButtonIndex == MouseButton.WheelUp && mouseEvent.Pressed)
            {
                _camera.Zoom *= 1.0f - ZoomSpeed;
                ClampZoom();
            }
            else if (mouseEvent.ButtonIndex == MouseButton.WheelDown && mouseEvent.Pressed)
            {
                _camera.Zoom *= 1.0f + ZoomSpeed;
                ClampZoom();
            }

            // Start drag
            if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
            {
                _isDragging = true;
                _dragStartMousePosition = GetViewport().GetMousePosition();
                _dragStartPosition = Position;
            }

            // End drag
            if (mouseEvent.ButtonIndex == MouseButton.Left && !mouseEvent.Pressed)
            {
                _isDragging = false;
            }
        }
    }

    public override void _Process(double delta)
    {
        // Handle drag movement
        if (_isDragging)
        {
            Vector2 currentMousePosition = GetViewport().GetMousePosition();
            Vector2 dragDelta =
                -(currentMousePosition - _dragStartMousePosition) * (1 / _camera.Zoom.X); // Adjust for zoom level
            Position = _dragStartPosition + dragDelta * DragSpeed;
        }
    }

    private void ClampZoom()
    {
        _camera.Zoom = new Vector2(
            Mathf.Clamp(_camera.Zoom.X, MinZoom, MaxZoom),
            Mathf.Clamp(_camera.Zoom.Y, MinZoom, MaxZoom)
        );
    }
}
