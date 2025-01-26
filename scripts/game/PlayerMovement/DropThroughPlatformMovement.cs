using Godot;

public class DropThroughPlatformMovement : PlayerState
{
    public DropThroughPlatformMovement(Player player)
        : base(player) { }

    public override PlayerStateType NextState()
    {
        return PlayerStateType.Fall;
    }

    public override void Enter()
    {
        player.Position += new Vector2(0, 1);
    }
}
