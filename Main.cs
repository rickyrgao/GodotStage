using Godot;
using godotstage.Actors;

// Debug settings - easy to toggle all debug features
public static class DebugSettings
{
    public const bool ENABLE_DEBUG_FEATURES = true;
}

public partial class Main : Node2D
{
	private CharacterBody2D player;
	private const int FROG_COUNT = 2;
	private int frogCounter = 0;

	// Damage system constants
	private const float SHOOT_DAMAGE_RADIUS = 150.0f;
	private const int SHOOT_DAMAGE_AMOUNT = 25;

	// Debug manager
	private DebugManager debugManager;

	public override void _Ready()
	{
		// Get Player node
		player = GetNode<CharacterBody2D>("Player");

		if (player != null)
		{
			// Position player in center of isometric map (960, 450)
			player.Position = new Vector2(960, 450);
			GD.Print($"Player positioned at: {player.Position}");
			GD.Print("Player loaded with built-in camera system");
		}
		else
		{
			GD.PrintErr("Player node not found");
		}

		// Spawn frogs
		SpawnMultipleFrogs();

		// Setup debug features
		if (DebugSettings.ENABLE_DEBUG_FEATURES)
		{
			debugManager = new DebugManager();
			AddChild(debugManager);
		}
	}

	public override void _Process(double delta)
	{
		// Check for add_frog input
		if (Input.IsActionJustPressed("add_frog"))
		{
			SpawnSingleFrog();
		}

		// Check for shoot input and apply damage
		if (Input.IsActionJustPressed("shoot") && player != null)
		{
			ApplyShootDamage();
			if (DebugSettings.ENABLE_DEBUG_FEATURES)
			{
				debugManager.ShowDamageArea();
			}
		}
	}

	private void SpawnMultipleFrogs()
	{
		PackedScene frogScene = (PackedScene)ResourceLoader.Load("res://Actors/Frog/Frog.tscn");
		RandomNumberGenerator rng = new RandomNumberGenerator();
		rng.Randomize();

		// Get the isometric map boundaries for proper spawning
		var map = GetNode<Node2D>("IsometricTestMap");

		for (int i = 0; i < FROG_COUNT; i++)
		{
			Node2D frogInstance = (Node2D)frogScene.Instantiate();

			// Spawn frogs within the isometric playable area (diamond shape)
			Vector2 spawnPos = GetRandomPositionInPlayableArea(rng);
			frogInstance.Position = spawnPos;

			AddChild(frogInstance);
			frogCounter++;
			GD.Print($"Spawned frog {frogCounter} at position: {frogInstance.Position}");
		}

		GD.Print($"Spawned {FROG_COUNT} frogs within the isometric playable area!");
	}

	private void SpawnSingleFrog()
	{
		PackedScene frogScene = (PackedScene)ResourceLoader.Load("res://Actors/Frog/Frog.tscn");
		RandomNumberGenerator rng = new RandomNumberGenerator();
		rng.Randomize();

		Node2D frogInstance = (Node2D)frogScene.Instantiate();

		// Spawn frog within the isometric playable area (diamond shape)
		Vector2 spawnPos = GetRandomPositionInPlayableArea(rng);
		frogInstance.Position = spawnPos;

		AddChild(frogInstance);
		frogCounter++;
		GD.Print($"Spawned extra frog {frogCounter} at position: {frogInstance.Position}");
	}

	private Vector2 GetRandomPositionInPlayableArea(RandomNumberGenerator rng)
	{
		// Isometric map parameters (matching IsometricTestMap.cs)
		const int TileWidth = 160;
		const int TileHeight = 80;
		const int MapWidth = 20;
		const int MapHeight = 15;
		Vector2 mapCenter = new Vector2(960, 200);

		// Generate random grid coordinates within the map bounds
		int gridX = rng.RandiRange(0, MapWidth - 1);
		int gridY = rng.RandiRange(0, MapHeight - 1);

		// Convert to isometric world position
		float isoX = (gridX - gridY) * (TileWidth / 2.0f);
		float isoY = (gridX + gridY) * (TileHeight / 2.0f);

		// Add some random offset within the tile for more natural distribution
		float offsetX = rng.RandfRange(-TileWidth / 4.0f, TileWidth / 4.0f);
		float offsetY = rng.RandfRange(-TileHeight / 4.0f, TileHeight / 4.0f);

		return mapCenter + new Vector2(isoX + offsetX, isoY + offsetY);
	}

	private void ApplyShootDamage()
	{
		if (player == null) return;

		// Get all frog nodes
		var frogs = GetTree().GetNodesInGroup("frogs");

		int damagedFrogs = 0;
		foreach (Node node in frogs)
		{
			if (node is Frog frog && frog.Visible)
			{
				float distance = player.Position.DistanceTo(frog.Position);
				if (distance <= SHOOT_DAMAGE_RADIUS)
				{
					// Calculate damage based on distance - closer = more damage
					float distanceRatio = 1.0f - (distance / SHOOT_DAMAGE_RADIUS);
					int actualDamage = (int)(SHOOT_DAMAGE_AMOUNT * distanceRatio);
					actualDamage = Mathf.Max(1, actualDamage);

					int oldHealth = frog.GetCurrentHealth();
					frog.TakeDamage(actualDamage);
					int newHealth = frog.GetCurrentHealth();
					damagedFrogs++;

					if (DebugSettings.ENABLE_DEBUG_FEATURES)
					{
						// Show on-screen log for each individual damage instance
						debugManager.LogDamage(GetFrogNumber(frog), actualDamage, newHealth, distance);
					}
				}
			}
		}

		if (DebugSettings.ENABLE_DEBUG_FEATURES)
		{
			debugManager.LogShootSummary(damagedFrogs);
		}
	}

	private int GetFrogNumber(Frog frog)
	{
		// Get all frogs and find the index of this one
		var frogs = GetTree().GetNodesInGroup("frogs");
		for (int i = 0; i < frogs.Count; i++)
		{
			if (frogs[i] == frog)
			{
				return i + 1; // 1-based numbering
			}
		}
		return 0; // Fallback
	}
}
