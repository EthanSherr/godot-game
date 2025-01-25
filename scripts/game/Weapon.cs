using System;
using Godot;

public partial class Weapon : Node2D
{
    RayCast2D ray;

    public override void _Ready()
    {
        ray = GetNode<RayCast2D>("RayCast2D");
        EndDamage();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }

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
    }
}
