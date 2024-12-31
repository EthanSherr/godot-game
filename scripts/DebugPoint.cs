using Godot;

public partial class DebugPoint : Node2D
{
    private static PackedScene debugPointScene;

    private int removeTimeout = 2;

    public override void _Ready()
    {
        var timer = GetTree().CreateTimer(removeTimeout);
        timer.Connect("timeout", new Callable(this, nameof(OnTimeout)));
    }

    private void OnTimeout()
    {
        GD.Print("Timeout reached removing");
        QueueFree();
    }

    // Static method to create and place a DebugPoint
    public static void Create(Vector2 position, Node context)
    {
        debugPointScene ??= GD.Load<PackedScene>("res://scenes/DebugPoint.tscn");

        // Instance the DebugPoint scene
        DebugPoint debugPointInstance = (DebugPoint)debugPointScene.Instantiate();
        debugPointInstance.GlobalPosition = position;

        // Add it to the specified parent or the scene root if no parent is provided
        context.AddChild(debugPointInstance);
    }
}
