using Godot;

public partial class HealthComponent : Node2D
{
    [Export]
    public float MaxHealth = 20;
    public float Health;

    public override void _Ready()
    {
        Health = MaxHealth;
    }

    public void TakeDamage(float damage)
    {
        GD.Print($"Take {damage}");
        Health -= damage;
        if (Health < 0)
        {
            GetParent().QueueFree();
        }
    }
}
