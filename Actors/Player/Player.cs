using Godot;

namespace godotstage.Actors.Player
{
	public partial class Player : CharacterBody2D
	{
		[Export] public float Speed = 550.0f;
		
	private AnimatedSprite2D animatedSprite;
	private Node2D spineSprite;
	private Vector2 lastDirection = Vector2.Zero;
	private bool isMoving = false;
	private bool isShooting = false;
	private float shootAnimationDuration = 0.4f; // Duration of shoot animation in seconds
	private float shootTimer = 0.0f;

		public override void _Ready()
		{
			// Get AnimatedSprite2D node (keep for backup)
			animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
			
			// Get SpineSprite node
			spineSprite = GetNode<Node2D>("SpineSprite");
			
			// Set initial Spine animation
			if (spineSprite != null)
			{
				// Use Call method to call Spine method
				var animationState = spineSprite.Call("get_animation_state");
				if (!animationState.Equals(default(Variant)))
				{
					animationState.AsGodotObject().Call("set_animation", "idle", true, 0);
					GD.Print("SpineSprite found and idle animation set");
				}

				// Set initial scale to maintain large size
				spineSprite.Scale = new Vector2(0.25f, 0.25f);
				GD.Print("SpineSprite scale set to large size");
			}
			else
			{
				GD.PrintErr("SpineSprite not found!");
			}
			
			// Hide AnimatedSprite2D, use Spine animation
			if (animatedSprite != null)
			{
				animatedSprite.Visible = false;
			}
			
			// Check if input action is registered
			CheckInputActions();
			
			GD.Print("Player ready - WASD to move with Spine animations");
		}

	private void CheckInputActions()
	{
		string[] requiredActions = { "move_up", "move_down", "move_left", "move_right", "shoot" };
		
		GD.Print("=== Checking Input Actions ===");
		foreach (string action in requiredActions)
		{
			if (InputMap.HasAction(action))
			{
				var events = InputMap.ActionGetEvents(action);
				GD.Print($"✓ Action '{action}' registered with {events.Count} events");
				foreach (InputEvent evt in events)
				{
					if (evt is InputEventKey keyEvent)
					{
						GD.Print($"  - Key: {keyEvent.Keycode}");
					}
					else if (evt is InputEventMouseButton mouseEvent)
					{
						GD.Print($"  - Mouse Button: {mouseEvent.ButtonIndex}");
					}
				}
			}
			else
			{
				GD.PrintErr($"✗ Action '{action}' NOT registered!");
			}
		}
		GD.Print("=== End Input Check ===");
	}

	public override void _PhysicsProcess(double delta)
	{
		HandleInput();
		HandleMovement();
		HandleShootTimer(delta);
		HandleAnimation();
	}

	private void HandleInput()
	{
		Vector2 direction = Vector2.Zero;

		// Check WASD input
		if (Godot.Input.IsActionPressed("move_up"))
			direction.Y -= 1;
		if (Godot.Input.IsActionPressed("move_down"))
			direction.Y += 1;
		if (Godot.Input.IsActionPressed("move_left"))
			direction.X -= 1;
		if (Godot.Input.IsActionPressed("move_right"))
			direction.X += 1;

		// Check shoot input
		if (Godot.Input.IsActionJustPressed("shoot") && !isShooting)
		{
			StartShootAnimation();
		}

		// Normalize direction vector, ensure diagonal movement speed is consistent
		if (direction != Vector2.Zero)
		{
			direction = direction.Normalized();
			isMoving = true;
			lastDirection = direction;
		}
		else
		{
			isMoving = false;
		}

		// Set speed
		Velocity = direction * Speed;
	}

	private void HandleMovement()
	{
		// Use Godot 4's physics movement
		MoveAndSlide();
	}


	private void StartShootAnimation()
	{
		if (spineSprite == null) return;

		var animationState = spineSprite.Call("get_animation_state");
		if (animationState.Equals(default(Variant))) return;

		// Start shoot animation
		animationState.AsGodotObject().Call("set_animation", "shoot", false, 0);
		
		isShooting = true;
		shootTimer = shootAnimationDuration;
		
		GD.Print("Spine animation: shoot");
	}




	private void HandleShootTimer(double delta)
	{
		if (isShooting)
		{
			shootTimer -= (float)delta;
			if (shootTimer <= 0.0f)
			{
				isShooting = false;
				shootTimer = 0.0f;
				GD.Print("Shoot animation finished");
			}
		}
	}

	private void HandleAnimation()
	{
		if (spineSprite == null) return;

		var animationState = spineSprite.Call("get_animation_state");
		if (animationState.Equals(default(Variant))) return;

		// Get current animation information
		var currentTrack = animationState.AsGodotObject().Call("get_current", 0);
		string currentAnimationName = "";
		
		if (!currentTrack.Equals(default(Variant)))
		{
			var animation = currentTrack.AsGodotObject().Call("get_animation");
			if (!animation.Equals(default(Variant)))
			{
				currentAnimationName = animation.AsGodotObject().Call("get_name").AsString();
			}
		}

		// If shooting, don't change animation until shoot is finished
		if (isShooting)
		{
			// Handle sprite flipping during shoot animation
			if (lastDirection.X > 0)
			{
				spineSprite.Scale = new Vector2(0.25f, 0.25f); // Face right, normal scale
			}
			else if (lastDirection.X < 0)
			{
				spineSprite.Scale = new Vector2(-0.25f, 0.25f); // Face left, horizontal flip
			}
			return;
		}
		
		// Switch Spine animation based on movement state
		if (isMoving)
		{
			// Check if current animation is not run, avoid duplicate setting
			if (currentAnimationName != "run")
			{
				animationState.AsGodotObject().Call("set_animation", "run", true, 0);
				GD.Print("Spine animation: run");
			}

			// Flip sprite based on horizontal movement direction
			if (lastDirection.X > 0)
			{
				spineSprite.Scale = new Vector2(0.25f, 0.25f); // Face right, normal scale
			}
			else if (lastDirection.X < 0)
			{
				spineSprite.Scale = new Vector2(-0.25f, 0.25f); // Face left, horizontal flip
			}
		}
		else
		{
			// Check if current animation is not idle, avoid duplicate setting
			if (currentAnimationName != "idle")
			{
				animationState.AsGodotObject().Call("set_animation", "idle", true, 0);
				GD.Print("Spine animation: idle");
			}

			// Ensure proper scale for idle state (maintain large size)
			if (lastDirection.X >= 0)
			{
				spineSprite.Scale = new Vector2(0.25f, 0.25f); // Face right, normal scale
			}
			else
			{
				spineSprite.Scale = new Vector2(-0.25f, 0.25f); // Face left, horizontal flip
			}
		}
	}

		// Optional: Add debug information
		public override void _Input(InputEvent @event)
		{
			if (@event is InputEventKey keyEvent && keyEvent.Pressed)
			{
				if (keyEvent.Keycode == Key.Space)
				{
					GD.Print($"Player Position: {Position}");
					GD.Print($"Player Velocity: {Velocity}");
					GD.Print($"Is Moving: {isMoving}");
					
					if (spineSprite != null)
					{
						var animationState = spineSprite.Call("get_animation_state");
						string currentAnimation = "None";
						
						if (!animationState.Equals(default(Variant)))
						{
							var currentTrack = animationState.AsGodotObject().Call("get_current", 0);
							if (!currentTrack.Equals(default(Variant)))
							{
								var animation = currentTrack.AsGodotObject().Call("get_animation");
								if (!animation.Equals(default(Variant)))
								{
									currentAnimation = animation.AsGodotObject().Call("get_name").AsString();
								}
							}
						}
						
						GD.Print($"Current Spine Animation: {currentAnimation}");
						GD.Print($"Spine Scale: {spineSprite.Scale}");
					}
					else
					{
						GD.Print("SpineSprite is null");
					}
				}
			}
		}
	}
}
