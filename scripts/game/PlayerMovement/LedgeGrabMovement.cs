using Godot;

public class LedgeGrabMovement : PlayerState
{
    public LedgeGrabMovement(Player player)
        : base(player) { }

    public override PlayerStateType NextState()
    {
        var input = player.GetInput();
        if (input.Axis.Y > 0f)
        {
            return PlayerStateType.Fall;
        }
        if (input.JumpStart)
        {
            return PlayerStateType.Jump;
        }
        return PlayerStateType.None;
    }

    public override void Enter()
    {
        player.Velocity = new Vector2();
        var (_, offset) = player.GetLedgeGrabInfo();
        player.GlobalPosition = player.GlobalPosition + offset;
    }

    public override void Update(double delta) { }
}
