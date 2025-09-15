using Godot;
using System;

namespace godotstage.Maps
{
    public partial class IsometricTestMap : Node2D
    {
        private ColorRect background;
        private Node2D tileContainer;
        private Node2D collisionContainer;
        
        // Isometric tile settings
        private const int TileWidth = 160;
        private const int TileHeight = 80;
        private const int MapWidth = 20;
        private const int MapHeight = 15;
        
        private Random random = new Random();

        public override void _Ready()
        {
            // Get references to child nodes
            background = GetNode<ColorRect>("Background");
            tileContainer = GetNode<Node2D>("TileContainer");
            collisionContainer = GetNode<Node2D>("CollisionContainer");
            
            // Generate random background color
            GenerateRandomBackground();
            
            // Create isometric tile grid
            CreateIsometricTiles();
            
            // Create collision boundaries
            CreateCollisionBoundaries();
            
            GD.Print("Isometric test map initialized with random background and collision boundaries");
        }

        private void GenerateRandomBackground()
        {
            // Generate random pastel colors for a pleasant background
            float r = (float)(random.NextDouble() * 0.3 + 0.2); // 0.2 to 0.5
            float g = (float)(random.NextDouble() * 0.3 + 0.2); // 0.2 to 0.5
            float b = (float)(random.NextDouble() * 0.3 + 0.4); // 0.4 to 0.7 (more blue bias)
            
            Color randomColor = new Color(r, g, b, 1.0f);
            background.Color = randomColor;
            
            GD.Print($"Generated random background color: R={r:F2}, G={g:F2}, B={b:F2}");
        }

        private void CreateIsometricTiles()
        {
            // Center the tile grid on screen
            Vector2 startPos = new Vector2(960, 200); // Center horizontally, start near top
            
            for (int x = 0; x < MapWidth; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    CreateIsometricTile(x, y, startPos);
                }
            }
        }

        private void CreateIsometricTile(int gridX, int gridY, Vector2 startPos)
        {
            // Calculate isometric position
            Vector2 isoPos = CartesianToIsometric(gridX, gridY);
            Vector2 worldPos = startPos + isoPos;
            
            // Create tile as a Polygon2D for diamond shape
            Polygon2D tile = new Polygon2D();
            
            // Create diamond shape points
            Vector2[] points = new Vector2[]
            {
                new Vector2(0, -TileHeight / 2),      // Top
                new Vector2(TileWidth / 2, 0),        // Right
                new Vector2(0, TileHeight / 2),       // Bottom
                new Vector2(-TileWidth / 2, 0)        // Left
            };
            
            tile.Polygon = points;
            tile.Position = worldPos;
            
            // Generate tile color based on position for variety
            float colorVariation = (float)((gridX + gridY) % 3) / 3.0f;
            Color tileColor = new Color(
                0.4f + colorVariation * 0.3f,
                0.6f + colorVariation * 0.2f,
                0.3f + colorVariation * 0.4f,
                1.0f
            );
            tile.Color = tileColor;
            
            // Add outline
            Line2D outline = new Line2D();
            outline.AddPoint(points[0]);
            outline.AddPoint(points[1]);
            outline.AddPoint(points[2]);
            outline.AddPoint(points[3]);
            outline.AddPoint(points[0]); // Close the shape
            outline.DefaultColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            outline.Width = 1.0f;
            
            tile.AddChild(outline);
            tileContainer.AddChild(tile);
        }

        private Vector2 CartesianToIsometric(int x, int y)
        {
            // Convert cartesian coordinates to isometric
            float isoX = (x - y) * (TileWidth / 2);
            float isoY = (x + y) * (TileHeight / 2);
            return new Vector2(isoX, isoY);
        }

        private void CreateCollisionBoundaries()
        {
            Vector2 startPos = new Vector2(960, 200);
            
            // Calculate the diamond boundary points of the isometric map
            Vector2 topPoint = startPos + CartesianToIsometric(0, 0);
            Vector2 rightPoint = startPos + CartesianToIsometric(MapWidth - 1, 0);
            Vector2 bottomPoint = startPos + CartesianToIsometric(MapWidth - 1, MapHeight - 1);
            Vector2 leftPoint = startPos + CartesianToIsometric(0, MapHeight - 1);
            
            // Extend boundaries slightly outward for better collision
            float margin = 30.0f;
            topPoint.Y -= margin;
            rightPoint.X += margin;
            bottomPoint.Y += margin;
            leftPoint.X -= margin;
            
            // Create collision walls for each side of the diamond
            CreateCollisionWall(topPoint, rightPoint, "TopRight");
            CreateCollisionWall(rightPoint, bottomPoint, "RightBottom");
            CreateCollisionWall(bottomPoint, leftPoint, "BottomLeft");
            CreateCollisionWall(leftPoint, topPoint, "LeftTop");
            
            GD.Print($"Created collision boundaries: Top{topPoint}, Right{rightPoint}, Bottom{bottomPoint}, Left{leftPoint}");
        }

        private void CreateCollisionWall(Vector2 start, Vector2 end, string wallName)
        {
            // Create StaticBody2D for collision
            StaticBody2D wall = new StaticBody2D();
            wall.Name = wallName;
            
            // Create CollisionShape2D
            CollisionShape2D collisionShape = new CollisionShape2D();
            
            // Create RectangleShape2D for the wall
            RectangleShape2D shape = new RectangleShape2D();
            
            // Calculate wall properties
            Vector2 direction = (end - start).Normalized();
            float length = start.DistanceTo(end);
            float thickness = 20.0f; // Wall thickness
            
            shape.Size = new Vector2(length, thickness);
            collisionShape.Shape = shape;
            
            // Position and rotate the wall
            Vector2 center = (start + end) / 2;
            wall.Position = center;
            
            // Rotate to align with the wall direction
            float angle = Mathf.Atan2(direction.Y, direction.X);
            wall.Rotation = angle;
            
            // Add collision shape to wall
            wall.AddChild(collisionShape);
            
            // Optional: Add visual representation for debugging
            CreateWallVisual(wall, length, thickness);
            
            // Add wall to collision container
            collisionContainer.AddChild(wall);
        }

        private void CreateWallVisual(StaticBody2D wall, float length, float thickness)
        {
            // Create a visual representation of the collision wall (for debugging)
            ColorRect visual = new ColorRect();
            visual.Size = new Vector2(length, thickness);
            visual.Position = new Vector2(-length / 2, -thickness / 2);
            visual.Color = new Color(1.0f, 0.0f, 0.0f, 0.3f); // Semi-transparent red
            visual.MouseFilter = Control.MouseFilterEnum.Ignore;
            
            wall.AddChild(visual);
        }

        // Method to regenerate the map with new random colors
        public void RegenerateMap()
        {
            // Clear existing tiles
            foreach (Node child in tileContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            // Clear existing collision boundaries
            foreach (Node child in collisionContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            // Generate new random background and tiles
            GenerateRandomBackground();
            CreateIsometricTiles();
            CreateCollisionBoundaries();
        }

        // Input handling for regeneration (press R to regenerate)
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.R)
                {
                    RegenerateMap();
                    GD.Print("Map regenerated with new random colors!");
                }
            }
        }
    }
}
