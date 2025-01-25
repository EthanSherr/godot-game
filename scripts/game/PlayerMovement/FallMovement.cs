using Godot;

public class FallMovement : PlayerState
{

    // Keep in sync with jump movement?
    [Export]
    public float HorizontalControlAcceleration = 1000f;

    [Export]
    public float MaxHorizontalSpeed = 100f;

    public FallMovement(Player player) : base(player) {}

    public override PlayerStateType NextState() {
      var input = player.GetInput();
      if (player.CanClimbLadder() && input.Axis.Y < 0) {
        return PlayerStateType.LadderClimb;
      }
      if (player.GetLedgeGrabInfo().canLedgeGrab) {
        return PlayerStateType.LedgeGrab;
      }
      if (player.IsOnFloor()) {
        return PlayerStateType.Ground;
      }
      return PlayerStateType.None;
    }

    public override void Enter() { }

    public override void Update(double delta) {
      var input = player.GetInput();
      var direction = Mathf.Clamp(input.Axis.X, -1, 1);

      // Keep in sync with jump movement?
      var velocity = player.Velocity;
      velocity.X += direction * HorizontalControlAcceleration * (float)delta;
      // could be better tho if i wanted a boosted run...
      velocity.X = Mathf.Clamp(velocity.X, -MaxHorizontalSpeed, MaxHorizontalSpeed);

      ApplyGravity(ref velocity, delta);
      player.Velocity = velocity;
    }
}