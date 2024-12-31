using System;
using Godot;

public partial class Player : CharacterBody2D
{
    [Export]
    public float Speed = 150f; // Maximum horizontal speed

    [Export]
    public float Acceleration = 600f; // Acceleration rate

    [Export]
    public float Friction = 600f; // Deceleration rate

    [Export]
    public float Gravity = 1000f; // Gravity strength

    private bool isJumping = false;
    private float jumpTime = 0f;

    private RayCast2D ledgeDetector;
    private Sprite2D body;

    // holding reference to original body scale, 0.125, 0.125
    private Vector2 originalBodyScale;

    public override void _Ready()
    {
        ledgeDetector = GetNode<RayCast2D>("Body/LedgeDetector");
        body = GetNode<Sprite2D>("Body");
        originalBodyScale = body.Scale;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        ApplyLedgeGrab(ref velocity, delta);

        ApplyHorizontalVelocity(ref velocity, delta);
        ApplyThresholdJump(ref velocity, delta);
        ApplyGravity(ref velocity, delta);

        ApplyPlayerOrientation(ref velocity);

        Velocity = velocity;
        MoveAndSlide();
    }

    [Export]
    public float JumpHoldTime = 0.3f; // Maximum time jump can be held

    [Export]
    public float MaxFallSpeed = 1000f; // Terminal velocity

    [Export]
    public float JumpHoldGravityScale = 0.3f; // Gravity scale when holding jump

    private static float Mult = 1f;

    [Export]
    public float SmallHop = -150f * Mult;

    [Export]
    public float MediumHop = -200f * Mult;

    [Export]
    public float HighHop = -300f * Mult;

    [Export]
    public float MaxJumpTime = 0.3f * Mult;

    [Export]
    public float TimeBetweenJumps = 0.048f;

    private bool ApplyThresholdJump(ref Vector2 velocity, double delta)
    {
        // start jumping?
        if (Input.IsActionJustPressed("ui_up") && CanJump())
        {
            isJumping = true;
            jumpTime = 0.0f;

            if (isLedgeGrabbing)
            {
                // end ledge grabbing from a jump
                isLedgeGrabbing = false;
            }
        }

        // apply jump if jumping!
        if (!isJumping)
            return false;

        jumpTime += (float)delta;

        bool isMaxJump = false;

        if (jumpTime < TimeBetweenJumps)
        {
            velocity.Y = SmallHop;
        }
        else if (jumpTime < 2 * TimeBetweenJumps)
        {
            velocity.Y = MediumHop;
        }
        else
        {
            velocity.Y = HighHop;
            isMaxJump = true;
        }

        if (isMaxJump || Input.IsActionJustReleased("ui_up") || jumpTime > MaxJumpTime)
        {
            isJumping = false;
            jumpTime = 0.0f;
        }

        return isJumping;
    }

    private bool CanJump()
    {
        return IsOnFloor() || isLedgeGrabbing;
    }

    private void ApplyHorizontalVelocity(ref Vector2 velocity, double delta)
    {
        if (isLedgeGrabbing)
            return;
        // Handle input for horizontal movement
        int direction = 0;
        if (Input.IsActionPressed("ui_right"))
        {
            direction = +1;
        }
        else if (Input.IsActionPressed("ui_left"))
        {
            direction = -1;
        }

        if (direction != 0)
        {
            velocity.X += direction * Acceleration * (float)delta;
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
        // clamp horizontal
        velocity.X = Mathf.Clamp(velocity.X, -Speed, Speed);
    }

    private void ApplyGravity(ref Vector2 velocity, double delta)
    {
        if (isLedgeGrabbing)
            return;
        velocity.Y += Gravity * (float)delta;
    }

    private bool isLedgeGrabbing = false;

    private bool ApplyLedgeGrab(ref Vector2 velocity, double delta)
    {
        // test if we can begin ledge grabbing
        if (!isLedgeGrabbing && !IsOnFloor() && Velocity.Y > 0)
        {
            if (Input.IsActionPressed("ui_up") && ledgeDetector.IsColliding()) //&& !freeSpaceChecker.IsColliding())
            {
                isLedgeGrabbing = true;
                velocity = Vector2.Zero;

                Vector2 collision = ledgeDetector.GetCollisionPoint();
                DebugPoint.Create(collision, GetTree().Root);

                GD.Print("Begin ledge grab");
                // Snap to grab position
                Vector2 collisionOffset = collision - ledgeDetector.GlobalPosition;
                GlobalPosition = GlobalPosition + collisionOffset;
            }
        }
        // test to exit ledge grab
        else if (isLedgeGrabbing && Input.IsActionJustPressed("ui_down"))
        {
            isLedgeGrabbing = false;
            velocity.Y = 100;

            GD.Print("End ledge grab");
        }

        return isLedgeGrabbing;
    }

    private void ApplyPlayerOrientation(ref Vector2 velocity)
    {
        int direction = Math.Sign(velocity.X);
        if (direction == 0)
            return;
        body.Scale = new Vector2(direction * originalBodyScale.X, originalBodyScale.Y);
    }
}
