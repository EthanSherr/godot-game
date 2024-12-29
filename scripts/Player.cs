using Godot;
using System;

public partial class Player : CharacterBody2D
{
    [Export] public float Speed = 200f; // Maximum horizontal speed
    [Export] public float Acceleration = 400f; // Acceleration rate
    [Export] public float Friction = 600f; // Deceleration rate
    [Export] public float Gravity = 500f; // Gravity strength
    [Export] public float MaxFallSpeed = 1000f; // Terminal velocity
    [Export] public float JumpSpeed = -100f; // Initial jump speed
    [Export] public float JumpHoldTime = 0.3f; // Maximum time jump can be held
    [Export] public float JumpHoldGravityScale = 0.5f; // Gravity scale when holding jump

    private bool _isJumping = false;
    private float _jumpTime = 0f;

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        // jumping gravity reduction
        if (_isJumping && Input.IsActionPressed("ui_up") && _jumpTime < JumpHoldTime)
        {
            _jumpTime += (float)delta;
            velocity.Y += Gravity * JumpHoldGravityScale * (float)delta; 
        }
        else
        {
            velocity.Y += Gravity * (float)delta;
        }
        velocity.Y = Mathf.Clamp(velocity.Y, -MaxFallSpeed, MaxFallSpeed);

        // Handle input for horizontal movement
        if (Input.IsActionPressed("ui_right"))
        {
            velocity.X += Acceleration * (float)delta;
        }
        else if (Input.IsActionPressed("ui_left"))
        {
            velocity.X -= Acceleration * (float)delta;
        }
        else
        {
            int sign = Math.Sign(velocity.X);
            velocity.X -= sign * Friction * (float)delta;
            bool frictionOvershot = Math.Sign(velocity.X) != sign;
            if (frictionOvershot) {
                velocity.X = 0f;
            }
        }
        // clamp horizontal
        velocity.X = Mathf.Clamp(velocity.X, -Speed, Speed);

        // Handle jumping
        if (IsOnFloor())
        {
            _isJumping = false;
            _jumpTime = 0f; // Reset jump time
            if (Input.IsActionJustPressed("ui_up")) // Jump if jump button is pressed
            {
                velocity.Y = JumpSpeed; // Apply initial jump velocity
                _isJumping = true;
            }
        }

        Velocity = velocity;
        MoveAndSlide();
    }
}
