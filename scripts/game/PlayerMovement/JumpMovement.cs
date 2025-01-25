using System;
using Godot;

public class JumpMovement : PlayerState
{
    static float epsilon = 0.001f;

    [Export]
    public float JumpImpulse = 240f; // Deceleration rate

    [Export]
    public float Friction = 1500f; // Friction coefficient

    // Keep in sync with fall movement?
    [Export]
    public float HorizontalControlAcceleration = 1000f;

    [Export]
    public float MaxHorizontalSpeed = 100f;

    public JumpMovement(Player player)
        : base(player) { }

    public override PlayerStateType NextState()
    {
        var input = player.GetInput();
        if (player.CanClimbLadder() && input.Axis.Y < 0)
        {
            return PlayerStateType.LadderClimb;
        }
        if (player.GetLedgeGrabInfo().canLedgeGrab)
        {
            return PlayerStateType.LedgeGrab;
        }
        var isOnFloor = player.IsOnFloor();
        if (player.Velocity.Y > -epsilon && isOnFloor)
        {
            return PlayerStateType.Ground;
        }
        if (player.Velocity.Y > 0)
        {
            return PlayerStateType.Fall;
        }
        return PlayerStateType.None;
    }

    public override void Enter()
    {
        player.Velocity = new Vector2(player.Velocity.X, -JumpImpulse);
    }

    public override void Update(double delta)
    {
        var input = player.GetInput();

        var velocity = player.Velocity;
        if (!input.JumpHeld && player.Velocity.Y < -epsilon)
        {
            // apply air break
            velocity.Y -= Mathf.Sign(velocity.Y) * Friction * (float)delta;
            velocity.Y = Mathf.Min(velocity.Y, 0);
        }

        var direction = Mathf.Clamp(input.Axis.X, -1, 1);

        // Keep in sync with fall movement?
        velocity.X += direction * HorizontalControlAcceleration * (float)delta;
        // could be better tho if i wanted a boosted run...
        velocity.X = Mathf.Clamp(velocity.X, -MaxHorizontalSpeed, MaxHorizontalSpeed);

        ApplyGravity(ref velocity, delta);
        player.Velocity = velocity;
    }
}
