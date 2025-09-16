using Godot;

public partial class Main : Node2D
{
	private CharacterBody2D player;
	private const int FROG_COUNT = 2;
	private int frogCounter = 0;

	public override void _Ready()
	{
		// 获取 Player 节点
		player = GetNode<CharacterBody2D>("Player");

		if (player != null)
		{
			// 确保玩家在等距地图中央 (960, 450)
			player.Position = new Vector2(960, 450);
			GD.Print($"Player positioned at: {player.Position}");

			// 新的 Player 场景已经自带启用的 Camera2D，无需额外设置
			GD.Print("Player loaded with built-in camera system");
		}
		else
		{
			GD.PrintErr("Player node not found");
		}

		// Spawn frogs
		SpawnMultipleFrogs();
	}

	public override void _Process(double delta)
	{
		// Check for add_frog input
		if (Input.IsActionJustPressed("add_frog"))
		{
			SpawnSingleFrog();
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
}
