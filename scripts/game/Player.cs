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

	[Export]
	public float Speed = 120f; // Maximum horizontal speed

	
	[Export]
	public float SpeedWhileAttacking = 30f; // Maximum horizontal speed

	[Export]
	public float Acceleration = 800f; // Acceleration rate

	[Export]
	public float Friction = 600f; // Deceleration rate

	[Export]
	public float Gravity = 1000f;

	// state tracking for jumping
	private bool isJumping = false;
	private float jumpTime = 0f;

	private RayCast2D ledgeDetector;
	private Sprite2D body;

	// The original Scale of the graphic representation
	private Vector2 originalBodyScale;

	private RayCast2D ladderDetector;

	private AnimationPlayer animationPlayer;

	private Node2D meleAttachment;

	private FogOfWar fogOfWar;

	public Camera2D Camera;

	private bool isInitialized = false;

	public void Initialize()
	{
		ledgeDetector = GetNode<RayCast2D>("Body/LedgeDetector");
		ladderDetector = GetNode<RayCast2D>("LadderDetector");
		Camera = GetNode<Camera2D>("Camera2D");
		body = GetNode<Sprite2D>("Body");
		// initialize the meleattachment
		meleAttachment = GetNode<Node2D>("Body/MeleAttachment");
		meleAttachment.Visible = false;

		animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
    animationPlayer.Connect("animation_finished", new Callable(this, nameof(OnAnimationFinished)));


		equipedWeapon = GetNodeOrNull<Weapon>("Body/MeleAttachment/Weapon");

		originalBodyScale = body.Scale;
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
		Vector2 velocity = Velocity;

		ApplyLedgeGrab(ref velocity, delta);

		ApplyClimb(ref velocity, delta);

		ApplyHorizontalVelocity(ref velocity, delta);
		ApplyThresholdJump(ref velocity, delta);
		ApplyGravity(ref velocity, delta);

		ApplyPlayerOrientation(ref velocity);

		Velocity = velocity;
		MoveAndSlide();
	}

	bool isAttacking = false;
	public override void _Process(double delta)
	{
		if (Input.IsKeyPressed(Key.Ctrl) && !isAttacking) {
			attack();
		}

		if (fogOfWar != null)
		{
			fogOfWar.Reveal(GlobalPosition, 75);
		}
		if (isAttacking) {
			return;
		}
		if (isLedgeGrabbing) {
			animationPlayer.Play("hang");
		} else
		if (!IsOnFloor()) {
			var zeroGThreshold = 50;
			animationPlayer.Play("jump");
			if (Velocity.Y < - zeroGThreshold) {
				animationPlayer.Seek(0.0, true);
			} else if (Velocity.Y < zeroGThreshold)
			{
				animationPlayer.Seek(0.1, true);
			} else {
				animationPlayer.Seek(0.2, true);
			}
		} else
		if (Mathf.Abs(Velocity.X) > 0) {
			animationPlayer.Play("walk_right");
		} else {
			animationPlayer.Play("idle");
		}
	}

	private void attack() {
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
	public void SetWeapon(Weapon newWeap) {
		if (equipedWeapon != null) {
			equipedWeapon.QueueFree();
		}
		equipedWeapon = newWeap;
		equipedWeapon.AddIgnore(this);
	}
	public void BeginMeleDamage() {
		GD.Print("begin damage!");
		if (equipedWeapon == null) {
			return;
		}
		equipedWeapon.BeginDamage();
	}

	public void EndMeleDamage() {
				if (equipedWeapon == null) {
			return;
		}
		equipedWeapon.EndDamage();
	}

	private static float Mult = 1f;

	[Export]
	public float SmallHop = -150f * Mult;

	[Export]
	public float MediumHop = -200f * Mult;

	[Export]
	public float HighHop = -300f * Mult;

	[Export]
	public float TimeBetweenJumps = 0.048f;

	private bool ApplyThresholdJump(ref Vector2 velocity, double delta)
	{
		// start jumping?
		if (Input.IsActionJustPressed(InputJump) && CanJump())
		{
			isJumping = true;
			jumpTime = 0.0f;

			if (isLedgeGrabbing)
			{
				// end ledge grabbing from a jump
				isLedgeGrabbing = false;
			}
			if (isClimbing)
			{
				isClimbing = false;
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
		else if (jumpTime < 3 * TimeBetweenJumps)
		{
			velocity.Y = HighHop;
			isMaxJump = true;
		}

		if (isMaxJump || !Input.IsActionPressed(InputJump))
		{
			isJumping = false;
			jumpTime = 0.0f;
		}

		return isJumping;
	}

	private bool CanJump()
	{
		return IsOnFloor() || isLedgeGrabbing || isClimbing;
	}

	private void ApplyHorizontalVelocity(ref Vector2 velocity, double delta)
	{
		if (isLedgeGrabbing)
			return;
		// Handle input for horizontal movement
		int direction = GetHorizontalInput();

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
		var maxSpeed = Speed;
		if (isAttacking && IsOnFloor()) {
			maxSpeed = SpeedWhileAttacking;
		}
		velocity.X = Mathf.Clamp(velocity.X, -maxSpeed, maxSpeed);
	}

	private void ApplyGravity(ref Vector2 velocity, double delta)
	{
		if (isLedgeGrabbing || isClimbing)
			return;
		velocity.Y += Gravity * (float)delta;
	}

	private bool isLedgeGrabbing = false;

	private bool ApplyLedgeGrab(ref Vector2 velocity, double delta)
	{
		// test if we can begin ledge grabbing
		if (!isLedgeGrabbing && !IsOnFloor() && Velocity.Y > 0)
		{
			int playerDirection = GetHorizontalInput();
			// TODO, channel?
			Vector2 ledgeDetectorCollision = ledgeDetector.GetCollisionPoint();
			float collisionOffsetX = ledgeDetectorCollision.X - GlobalPosition.X;
			int directionOfLedge = Math.Sign(collisionOffsetX);

			if (ledgeDetector.IsColliding() && directionOfLedge == playerDirection) //&& !freeSpaceChecker.IsColliding())
			{
				isLedgeGrabbing = true;
				velocity = Vector2.Zero;

				DebugPoint.Create(ledgeDetectorCollision, GetTree().Root);

				GD.Print("Begin ledge grab");
				// Snap to grab position
				Vector2 collisionOffset = ledgeDetectorCollision - ledgeDetector.GlobalPosition;
				GlobalPosition = GlobalPosition + collisionOffset;
			}
		}
		// test to exit ledge grab
		else if (isLedgeGrabbing && Input.IsActionJustPressed(InputDown))
		{
			isLedgeGrabbing = false;
			velocity.Y = 100;

			GD.Print("End ledge grab");
		}

		return isLedgeGrabbing;
	}

	private int GetHorizontalInput()
	{
		int direction = 0;
		if (Input.IsActionPressed(InputRight))
		{
			direction += 1;
		}
		else if (Input.IsActionPressed(InputLeft))
		{
			direction += -1;
		}
		return direction;
	}

	private void ApplyPlayerOrientation(ref Vector2 velocity)
	{
		int direction = Math.Sign(velocity.X);
		if (direction == 0)
			return;
		body.Scale = new Vector2(direction * originalBodyScale.X, originalBodyScale.Y);
	}

	[Export]
	private float climbUpSpeed = -50f;

	[Export]
	private float climbDownSpeed = 200f;

	private bool isClimbing = false;

	private void ApplyClimb(ref Vector2 velocity, double delta)
	{
		// no ladder climbing while ledge grabbing
		if (isLedgeGrabbing)
			return;

		if (!isClimbing && Input.IsActionPressed(InputUp) && ladderDetector.IsColliding())
		{
			GD.Print("Climbing: start");
			isClimbing = true;
		}

		if (isClimbing && !ladderDetector.IsColliding())
		{
			GD.Print("Climbing: stop");
			isClimbing = false;
		}

		if (!isClimbing)
			return;

		if (Input.IsActionPressed(InputUp))
		{
			velocity.Y = climbUpSpeed;
		}
		else if (Input.IsActionPressed(InputDown))
		{
			velocity.Y = climbDownSpeed;
		}
		else
		{
			velocity.Y = 0;
		}
	}

	private static string ScenePath = "res://scripts/game/Player.tscn";

	public static Player Create()
	{
		return NodeUtils.CreateFromScene<Player>(ScenePath);
	}

}
