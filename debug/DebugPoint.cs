using Godot;

public partial class DebugPoint : Node2D
{
    private static PackedScene debugPointScene;

    private static float RemoveTimeout = 2;

    private float removeTimeout = DebugPoint.RemoveTimeout;

    public override void _Ready()
    {
        var timer = GetTree().CreateTimer(removeTimeout);
        timer.Connect("timeout", new Callable(this, nameof(OnTimeout)));
    }

    private void OnTimeout()
    {
        QueueFree();
    }

    // Static method to create and place a DebugPoint
    public static void Create(Vector2 position, Node context, float _removeTimeout)
    {
        debugPointScene ??= GD.Load<PackedScene>("res://scenes/DebugPoint.tscn");

        // Instance the DebugPoint scene
        DebugPoint debugPointInstance = (DebugPoint)debugPointScene.Instantiate();
        debugPointInstance.GlobalPosition = position;

        debugPointInstance.removeTimeout = _removeTimeout;

        // Add it to the specified parent or the scene root if no parent is provided
        context.AddChild(debugPointInstance);
    }

    public static void Create(Vector2 position, Node context)
    {
        Create(position, context, DebugPoint.RemoveTimeout);
    }
}
