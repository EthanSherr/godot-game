using System;
using System.Collections.Generic;
using Godot;

public partial class Weapon : Node2D
{
    RayCast2D ray;

    HashSet<ulong> damageTracker = new HashSet<ulong> { };

    public override void _Ready()
    {
        ray = GetNode<RayCast2D>("RayCast2D");
        EndDamage();
    }

    public void AddIgnore(CollisionObject2D ignore)
    {
        ray.AddException(ignore);
    }

    public void RemoveIgnore(CollisionObject2D ignore)
    {
        ray.RemoveException(ignore);
    }

    public void BeginDamage()
    {
        ray.Enabled = true;
    }

    public void EndDamage()
    {
        ray.Enabled = false;
        damageTracker.Clear();
    }

    public override void _PhysicsProcess(double delta)
    {
        DamageScan();
    }

    private void DamageScan()
    {
        if (!ray.Enabled || !ray.IsColliding())
        {
            return;
        }
        var collider = ray.GetCollider();
        if (collider is IDamageable damageable && collider is Node2D node2D)
        {
            if (!damageTracker.Contains(collider.GetInstanceId()))
            {
                damageTracker.Add(collider.GetInstanceId());
                damageable.TakeDamage(5);
            }
        }
    }
}
