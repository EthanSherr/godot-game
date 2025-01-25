// movement state has two purposes
// 1.  transition logic
// 2.  update the model

using Godot;

public struct PlayerInput
{
    public Vector2 Axis;

    public bool JumpHeld;
    public bool JumpStart;
}

public enum PlayerStateType
{
    None,
    Ground,
    Jump,
    Fall,
    LedgeGrab,
    LadderClimb,
}

public abstract class PlayerState
{
    protected Player player;

    [Export]
    public float Gravity = 650f;

    public PlayerState(Player player)
    {
        this.player = player;
    }

    public virtual PlayerStateType NextState()
    {
        return PlayerStateType.None;
    }

    public virtual void Enter() { }

    public virtual void Update(double delta) { }

    public virtual void Exit() { }

    protected void ApplyGravity(ref Vector2 velocity, double delta)
    {
        velocity.Y += Gravity * (float)delta;
    }
}
