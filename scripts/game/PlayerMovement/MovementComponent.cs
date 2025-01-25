using System.Collections.Generic;
using Godot;

public partial class MovementComponent : Node {

  public Player CurPlayer;
  private PlayerStateType StateType;
  private PlayerState State;

  public Dictionary<PlayerStateType, PlayerState> Movements;

  [Export]
  public bool DebugStateTransition = false;

  public void TransitionState(PlayerStateType stateType) {
    if (stateType == PlayerStateType.None) {
      return;
    }
    if (DebugStateTransition) {
      GD.Print($"Stage change {stateType}");
    }
    Movements.TryGetValue(stateType, out var nextState);
    if (nextState != null) {
      State?.Exit();
      State = nextState;
      StateType = stateType;
      State.Enter();
    } else {
      GD.PrintErr($"Player cannot enter state.  No PlayerState for key {nextState}");
    }
  }

  public MovementComponent(Player player) {
    CurPlayer = player;
    Movements = new Dictionary<PlayerStateType, PlayerState> {
     {  PlayerStateType.Ground, new GroundMovement(player)},
     {  PlayerStateType.Jump, new JumpMovement(player)},
     {  PlayerStateType.Fall, new FallMovement(player)},
     {  PlayerStateType.LedgeGrab, new LedgeGrabMovement(player)},
     {  PlayerStateType.LadderClimb, new LadderClimbMovement(player)}
    };
    TransitionState(PlayerStateType.Ground);
  }

  public void Update(double delta) {
    var nextStateType = State.NextState();
    if (nextStateType != PlayerStateType.None) {
      TransitionState(nextStateType);
    }
    State.Update(delta);
  }

  public PlayerStateType GetPlayerState() {
    return StateType;
  }
}