using System;
using Godot;

public class LadderClimbMovement : PlayerState
{
    [Export]
    private float climbUpSpeed = 4500f;

    [Export]
    private float climbDownSpeed = 8000f;


    public LadderClimbMovement(Player player) : base(player) {}

    public override PlayerStateType NextState() {
      var input = player.GetInput();
      
      // ladder disappears?
      if (!player.CanClimbLadder()) {
        return PlayerStateType.Fall;
      }
      if (input.JumpStart) {
        return PlayerStateType.Jump;
      }
      if (player.IsOnFloor() && input.Axis.X != 0) {
        return PlayerStateType.Ground;
      }
      return PlayerStateType.None;
    }

    public override void Enter() { 
      var snappedPosition = player.Position.SnapToGrid() + new Vector2(16/2, 0);
      snappedPosition.Y = player.Position.Y;

      player.Position = snappedPosition;
      player.Velocity = new Vector2();
    }

    public override void Update(double delta) {
      var input = player.GetInput();

      if (input.Axis.Y == 0) {
        player.Velocity = new Vector2();
        return;
      }
      // at top of ladder, can't climb up more!
      if (input.Axis.Y < 0 && player.IsAtTopOfLadder()) {
        player.Velocity = new Vector2();
        return;
      }

      var speed = input.Axis.Y > 0 ? climbDownSpeed : climbUpSpeed;
      var velocity = player.Velocity;
      velocity.Y = speed * input.Axis.Y * (float)delta;
      player.Velocity = velocity;
    }
}