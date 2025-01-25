using System;
using Godot;

public partial class Player : CharacterBody2D
{
    private static string InputUp = "ui_up";
    private static string InputDown = "ui_down";
    private static string InputJump = "ui_select";
    private static string InputRight = "ui_right";
    private static string InputLeft = "ui_left";

    private static string Attack = "ui_accept";



    private RayCast2D ledgeDetector;
    private Sprite2D body;

    // The original Scale of the graphic representation
    private Vector2 originalBodyScale;

    private RayCast2D ladderDetector;
    private RayCast2D ladderTopDetector;

    private AnimationPlayer animationPlayer;

    private Node2D meleAttachment;

    private FogOfWar fogOfWar;

    public Camera2D Camera;

    private bool isInitialized = false;

    public MovementComponent Movement;

    public void Initialize()
    {
        ledgeDetector = GetNode<RayCast2D>("Body/LedgeDetector");
        ladderDetector = GetNode<RayCast2D>("LadderDetector");
        ladderTopDetector = GetNode<RayCast2D>("LadderTopDetector");
        Camera = GetNode<Camera2D>("Camera2D");
        body = GetNode<Sprite2D>("Body");
        // initialize the meleattachment
        meleAttachment = GetNode<Node2D>("Body/MeleAttachment");
        meleAttachment.Visible = false;

        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.Connect(
            "animation_finished",
            new Callable(this, nameof(OnAnimationFinished))
        );

        equipedWeapon = GetNodeOrNull<Weapon>("Body/MeleAttachment/Weapon");
        originalBodyScale = body.Scale;

        Movement = new MovementComponent(this);
        isInitialized = true;
    }

    public override void _Ready()
    {
        fogOfWar = GetParent().GetNodeOrNull<FogOfWar>("FogOfWar");
        if (!isInitialized)
        {
            Initialize();
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        Movement.Update(delta);
        Vector2 velocity = Velocity;
        ApplyPlayerOrientation(ref velocity);
        MoveAndSlide();
    }

    bool isAttacking = false;

    public bool IsAttacking() {
        return isAttacking;
    }


    public override void _Process(double delta) 
    {
        if (Input.IsKeyPressed(Key.Ctrl) && !isAttacking)
        {
            attack();
        }

        if (fogOfWar != null)
        {
            fogOfWar.Reveal(GlobalPosition, 75);
        }
        if (isAttacking)
        {
            return;
        }
        
        
        var playerState = Movement.GetPlayerState();
        
        if (playerState == PlayerStateType.LadderClimb)
        {
            var epsilon = 5;
            if (Velocity.Y < -epsilon)
            {
                animationPlayer.Play("climb");
            }
            else if (Velocity.Y > epsilon)
            {
                // DOWN!
                animationPlayer.Play("climb");
                animationPlayer.Seek(0.2, true);
            }
            else
            {
                animationPlayer.Pause();
            }
        }
        else if (playerState == PlayerStateType.LedgeGrab)
        {
            animationPlayer.Play("hang");
        }
        else if (playerState == PlayerStateType.Fall || playerState == PlayerStateType.Jump)
        {
            var zeroGThreshold = 50;
            animationPlayer.Play("jump");
            if (Velocity.Y < -zeroGThreshold)
            {
                animationPlayer.Seek(0.0, true);
            }
            else if (Velocity.Y < zeroGThreshold)
            {
                animationPlayer.Seek(0.1, true);
            }
            else
            {
                animationPlayer.Seek(0.2, true);
            }
        }
        else if (playerState == PlayerStateType.Ground)
        {
            if (Mathf.Abs(Velocity.X) > 0) {
                animationPlayer.Play("walk_right");
            } else {
                animationPlayer.Play("idle");
            }
        }
    }

    private void attack()
    {
        switch (Movement.GetPlayerState()) {
            case PlayerStateType.LadderClimb: return;
            case PlayerStateType.LedgeGrab: return;
        }
        isAttacking = true;
        meleAttachment.Visible = true;
        animationPlayer.Play("attack");
    }

    private void OnAnimationFinished(string animName)
    {
        // Check if the finished animation is the attack animation
        if (animName == "attack")
        {
            GD.Print("Attack animation finished!");
            isAttacking = false;
            meleAttachment.Visible = false;
        }
    }

    Weapon equipedWeapon;

    public void SetWeapon(Weapon newWeap)
    {
        if (equipedWeapon != null)
        {
            equipedWeapon.QueueFree();
        }
        equipedWeapon = newWeap;
        equipedWeapon.AddIgnore(this);
    }

    public void BeginMeleDamage()
    {
        GD.Print("begin damage!");
        if (equipedWeapon == null)
        {
            return;
        }
        equipedWeapon.BeginDamage();
    }

    public void EndMeleDamage()
    {
        if (equipedWeapon == null)
        {
            return;
        }
        equipedWeapon.EndDamage();
    }

    private float GetHorizontalInput()
    {
        return GetInput().Axis.X;
    }

    private void ApplyPlayerOrientation(ref Vector2 velocity)
    {
        if (isAttacking)
            return;
        int direction = Math.Sign(velocity.X);
        if (direction == 0)
            return;
        body.Scale = new Vector2(direction * originalBodyScale.X, originalBodyScale.Y);
    }




    public PlayerInput GetInput() {
        var inputVector = new Vector2();
        if (Input.IsActionPressed(InputLeft)) {
            inputVector.X += -1;
        }
        if (Input.IsActionPressed(InputRight)) {
            inputVector.X += 1;
        }
        if (Input.IsActionPressed(InputUp)) {
            inputVector.Y += -1;
        }
        if (Input.IsActionPressed(InputDown)) {
            inputVector.Y += 1;
        }
        var jumpHeld = Input.IsActionPressed(InputJump);
        var jumpStart = Input.IsActionJustPressed(InputJump);
        
        return new PlayerInput{
            Axis = inputVector,
            JumpStart = jumpStart,
            JumpHeld = jumpHeld,
        };
    }

    public (bool canLedgeGrab, Vector2 offset) GetLedgeGrabInfo() {
        int playerDirection = Math.Sign(GetHorizontalInput());
        Vector2 ledgeDetectorCollision = ledgeDetector.GetCollisionPoint();
        float collisionOffsetX = ledgeDetectorCollision.X - GlobalPosition.X;
        int directionOfLedge = Math.Sign(collisionOffsetX);
        var isLedgeGrabbing = ledgeDetector.IsColliding() && directionOfLedge == playerDirection;

        Vector2 collisionOffset = new Vector2();
        if (isLedgeGrabbing) //&& !freeSpaceChecker.IsColliding())
        {
            GD.Print("Begin ledge grab");
            // Snap to grab position
            collisionOffset = ledgeDetectorCollision - ledgeDetector.GlobalPosition;
        }

        return (isLedgeGrabbing, collisionOffset);
    }

    public bool CanClimbLadder() {
        return ladderDetector.IsColliding();
    }
    public bool IsAtTopOfLadder() {
        return ladderDetector.IsColliding() && !ladderTopDetector.IsColliding();
    }

    private static string ScenePath = "res://scripts/game/Player.tscn";

    public static Player Create()
    {
        return NodeUtils.CreateFromScene<Player>(ScenePath);
    }
}


