using System;
using Godot;

public class GroundMovement : PlayerState
{
    [Export]
    public float Friction = 600f; // Deceleration rate

    [Export]
    public float Speed = 4500f;

    [Export]
    public float SpeedWhileAttacking = 30f; // Maximum horizontal speed

    public GroundMovement(Player player)
        : base(player) { }

    public override PlayerStateType NextState()
    {
        var input = player.GetInput();
        if (player.CanClimbLadder() && input.Axis.Y < 0)
        {
            return PlayerStateType.LadderClimb;
        }
        var isOnFloor = player.IsOnFloor();
        if (player.IsOnOnewayPlatform() && input.Axis.Y > 0 && input.JumpStart)
        {
            return PlayerStateType.DropThroughPlatformMovement;
        }
        if (input.JumpStart && isOnFloor)
        {
            return PlayerStateType.Jump;
        }
        if (!isOnFloor)
        {
            return PlayerStateType.Fall;
        }

        return PlayerStateType.None;
    }

    public override void Update(double delta)
    {
        var input = player.GetInput();
        var velocity = player.Velocity;
        var direction = Mathf.Clamp(input.Axis.X, -1, 1);
        if (direction != 0)
        {
            // velocity.X += direction * Acceleration * (float)delta;
            velocity.X = direction * Speed * (float)delta;
        }
        else
        {
            int sign = Math.Sign(velocity.X);
            velocity.X -= sign * Friction * (float)delta;
            bool frictionOvershot = Math.Sign(velocity.X) != sign;
            if (frictionOvershot)
            {
                velocity.X = 0f;
            }
        }
        ApplyGravity(ref velocity, delta);
        if (player.IsAttacking())
        {
            velocity.X = Mathf.Clamp(velocity.X, -SpeedWhileAttacking, SpeedWhileAttacking);
        }
        player.Velocity = velocity;
    }
}
