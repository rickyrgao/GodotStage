using Godot;

namespace godotstage.Actors
{
    public partial class Frog : CharacterBody2D
    {
        [Export] public float JumpForce = 750.0f;
        [Export] public float Gravity = 2000.0f;
        [Export] public float MinJumpDelay = 1.0f;
        [Export] public float MaxJumpDelay = 3.0f;
        [Export] public float MoveSpeed = 250.0f;
        [Export] public float PlayerDetectionRadius = 200.0f;
        [Export] public float PlayingAreaMargin = 125.0f;

        // Health system
        public const int MAX_HEALTH = 100;
        private int currentHealth = MAX_HEALTH;

        private float jumpTimer = 0.0f;
        private float nextJumpTime = 0.0f;
        private bool isGrounded = false;
        private Vector2 screenSize;
        private Color frogColor;
        private float radius = 20.0f;
        private CharacterBody2D player;
        private bool isFleeing = false;
        private Main mainNode;

        public override void _Ready()
        {
            // Set random color for the frog
            RandomNumberGenerator rng = new RandomNumberGenerator();
            rng.Randomize();
            frogColor = new Color(rng.Randf(), rng.Randf(), rng.Randf());

            // Set initial jump time
            nextJumpTime = rng.RandfRange(MinJumpDelay, MaxJumpDelay);

            // Get screen size for boundary checking
            screenSize = GetViewportRect().Size;

            // Find player reference
            player = GetNode<CharacterBody2D>("/root/Main/Player");

            // Get main node reference for debug features
            mainNode = GetNode<Main>("/root/Main");

            // Add to frogs group for damage system
            AddToGroup("frogs");

            GD.Print($"Frog spawned at position: {Position}");
        }

        public override void _PhysicsProcess(double delta)
        {
            // Check player proximity and flee if too close
            if (player != null)
            {
                Vector2 distanceToPlayer = player.Position - Position;
                float distance = distanceToPlayer.Length();

                if (distance < PlayerDetectionRadius && isGrounded && !isFleeing)
                {
                    FleeFromPlayer(distanceToPlayer);
                }
            }

            // Apply gravity
            if (!isGrounded)
            {
                Velocity = new Vector2(Velocity.X, Velocity.Y + Gravity * (float)delta);
            }

            // Handle jumping timer (only if not fleeing)
            if (!isFleeing)
            {
                jumpTimer += (float)delta;
                if (jumpTimer >= nextJumpTime && isGrounded)
                {
                    Jump();
                }
            }

            // Move and slide
            MoveAndSlide();

            // Check if grounded (simple ground detection for isometric map)
            if (IsOnFloor() || IsOnGround())
            {
                isGrounded = true;
                Velocity = new Vector2(Velocity.X, 0);
                // Reset fleeing state when grounded
                isFleeing = false;
            }
            else
            {
                isGrounded = false;
            }

            // Keep frog within playing area bounds
            KeepWithinBounds();

            // Redraw the frog
            QueueRedraw();
        }

        private void Jump()
        {
            RandomNumberGenerator rng = new RandomNumberGenerator();
            rng.Randomize();

            // Random horizontal direction
            float horizontalForce = rng.RandfRange(-MoveSpeed, MoveSpeed);
            Velocity = new Vector2(horizontalForce, -JumpForce);

            // Reset jump timer
            jumpTimer = 0.0f;
            nextJumpTime = rng.RandfRange(MinJumpDelay, MaxJumpDelay);

            isGrounded = false;

            GD.Print($"Frog jumped with velocity: {Velocity}");
        }

        private void FleeFromPlayer(Vector2 distanceToPlayer)
        {
            // Calculate direction away from player
            Vector2 fleeDirection = -distanceToPlayer.Normalized();

            // Add some randomness to the flee direction
            RandomNumberGenerator rng = new RandomNumberGenerator();
            rng.Randomize();
            fleeDirection = fleeDirection.Rotated(rng.RandfRange(-Mathf.Pi / 4, Mathf.Pi / 4));

            // Jump away with higher force
            float fleeForce = JumpForce * 1.2f; // Slightly stronger jump when fleeing
            float fleeSpeed = MoveSpeed * 1.5f; // Faster horizontal movement when fleeing

            Velocity = new Vector2(fleeDirection.X * fleeSpeed, -fleeForce);

            isFleeing = true;
            isGrounded = false;
            jumpTimer = 0.0f;

            GD.Print($"Frog fleeing from player! Direction: {fleeDirection}, Velocity: {Velocity}");
        }

        private void KeepWithinBounds()
        {
            // Check if frog is within the isometric playable area (diamond shape)
            if (!IsWithinPlayableArea())
            {
                // Find the nearest point within the playable area
                Vector2 clampedPosition = ClampToPlayableArea(Position);

                // If we're significantly outside, teleport back
                if (Position.DistanceTo(clampedPosition) > radius * 2)
                {
                    Position = clampedPosition;
                    Velocity = Vector2.Zero;
                }
                else
                {
                    // Bounce off the boundary
                    Vector2 normal = GetBoundaryNormal(Position);
                    Velocity = Velocity.Bounce(normal);

                    // If we bounced while fleeing, reset fleeing state
                    if (isFleeing)
                    {
                        isFleeing = false;
                        jumpTimer = 0.0f;
                        nextJumpTime = 0.5f; // Short delay before next normal jump
                    }
                }
            }
        }

        private bool IsWithinPlayableArea()
        {
            // Isometric map parameters (matching IsometricTestMap.cs)
            const int TileWidth = 160;
            const int TileHeight = 80;
            const int MapWidth = 20;
            const int MapHeight = 15;
            Vector2 mapCenter = new Vector2(960, 200);

            // Convert world position to isometric grid coordinates
            Vector2 relativePos = Position - mapCenter;

            // Convert from isometric back to cartesian coordinates
            float cartX = (relativePos.X / (TileWidth / 2.0f) + relativePos.Y / (TileHeight / 2.0f)) / 2.0f;
            float cartY = (relativePos.Y / (TileHeight / 2.0f) - relativePos.X / (TileWidth / 2.0f)) / 2.0f;

            // Check if within grid bounds (with small margin)
            float margin = 0.3f; // Allow slight overflow
            return cartX >= -margin && cartX < MapWidth - 1 + margin &&
                   cartY >= -margin && cartY < MapHeight - 1 + margin;
        }

        private Vector2 ClampToPlayableArea(Vector2 position)
        {
            // Isometric map parameters (matching IsometricTestMap.cs)
            const int TileWidth = 160;
            const int TileHeight = 80;
            const int MapWidth = 20;
            const int MapHeight = 15;
            Vector2 mapCenter = new Vector2(960, 200);

            // Convert world position to isometric grid coordinates
            Vector2 relativePos = position - mapCenter;

            // Convert from isometric back to cartesian coordinates
            float cartX = (relativePos.X / (TileWidth / 2.0f) + relativePos.Y / (TileHeight / 2.0f)) / 2.0f;
            float cartY = (relativePos.Y / (TileHeight / 2.0f) - relativePos.X / (TileWidth / 2.0f)) / 2.0f;

            // Clamp to grid bounds
            cartX = Mathf.Clamp(cartX, 0, MapWidth - 1);
            cartY = Mathf.Clamp(cartY, 0, MapHeight - 1);

            // Convert back to isometric world position
            float isoX = (cartX - cartY) * (TileWidth / 2.0f);
            float isoY = (cartX + cartY) * (TileHeight / 2.0f);

            return mapCenter + new Vector2(isoX, isoY);
        }

        private Vector2 GetBoundaryNormal(Vector2 position)
        {
            // Isometric map parameters (matching IsometricTestMap.cs)
            const int TileWidth = 160;
            const int TileHeight = 80;
            const int MapWidth = 20;
            const int MapHeight = 15;
            Vector2 mapCenter = new Vector2(960, 200);

            // Convert world position to isometric grid coordinates
            Vector2 relativePos = position - mapCenter;

            // Convert from isometric back to cartesian coordinates
            float cartX = (relativePos.X / (TileWidth / 2.0f) + relativePos.Y / (TileHeight / 2.0f)) / 2.0f;
            float cartY = (relativePos.Y / (TileHeight / 2.0f) - relativePos.X / (TileWidth / 2.0f)) / 2.0f;

            // Determine which boundary we're closest to and return appropriate normal
            Vector2 normal = Vector2.Zero;

            if (cartX < 0)
                normal = new Vector2(-1, 1).Normalized(); // Left-top boundary
            else if (cartX >= MapWidth - 1)
                normal = new Vector2(1, 1).Normalized(); // Right-bottom boundary
            else if (cartY < 0)
                normal = new Vector2(-1, -1).Normalized(); // Left-bottom boundary
            else if (cartY >= MapHeight - 1)
                normal = new Vector2(1, -1).Normalized(); // Right-top boundary

            return normal;
        }

        private bool IsOnGround()
        {
            // Isometric map parameters (matching IsometricTestMap.cs)
            const int TileHeight = 80;
            const int MapHeight = 15;
            Vector2 mapCenter = new Vector2(960, 200);

            // Calculate the ground level for the current X position in isometric space
            Vector2 relativePos = Position - mapCenter;
            float cartX = (relativePos.X / 80.0f + relativePos.Y / 40.0f) / 2.0f; // Simplified conversion

            // The ground level follows the isometric diamond pattern
            // For a position, the ground Y is higher as we move toward the edges
            float groundOffset = Mathf.Abs(cartX - (MapHeight - 1) / 2.0f) * TileHeight / 2.0f;
            float groundLevel = mapCenter.Y + (MapHeight - 1) * TileHeight / 2.0f + groundOffset;

            return Position.Y >= groundLevel - radius;
        }

        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                currentHealth = 0;
                // Frog dies - could add death animation/effect here
                GD.Print($"Frog died at position: {Position}");
                // For now, just hide the frog
                Visible = false;
            }
            QueueRedraw(); // Update the health display
        }

        public int GetCurrentHealth()
        {
            return currentHealth;
        }

        public override void _Draw()
        {
            // Draw the frog as a colored circle
            DrawCircle(Vector2.Zero, radius, frogColor);

            // Add a simple face (scaled proportionally with the larger size)
            DrawCircle(new Vector2(-7.5f, -5), 3.75f, Colors.Black); // Left eye
            DrawCircle(new Vector2(7.5f, -5), 3.75f, Colors.Black);  // Right eye
            DrawCircle(new Vector2(0, 5), 2.5f, Colors.Black);       // Nose

            // Draw health text above the frog (only if alive and debug enabled)
            if (currentHealth > 0 && DebugSettings.ENABLE_DEBUG_FEATURES)
            {
                var font = new SystemFont();
                font.FontNames = new string[] { "Arial" };
                DrawString(font, new Vector2(-20, -35), currentHealth.ToString(), HorizontalAlignment.Center, -1, 16, Colors.White);
                DrawString(font, new Vector2(-20, -37), currentHealth.ToString(), HorizontalAlignment.Center, -1, 16, Colors.Black); // Shadow
            }
        }
    }
}
