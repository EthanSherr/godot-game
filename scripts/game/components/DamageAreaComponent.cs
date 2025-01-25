using Godot;

public partial class DamageAreaComponent : Area2D, IDamageable
{
    [Export]
    HealthComponent HealthComponent;

    public void TakeDamage(float damage)
    {
        if (HealthComponent != null)
        {
            HealthComponent.TakeDamage(damage);
        }
    }
}
