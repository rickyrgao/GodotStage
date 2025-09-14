# GodotStage

## Prerequisites

Before running this project, make sure you have the following installed:

1. **Godot Engine 4.4+** with .NET support
   - Download from: https://godotengine.org/download
   - Make sure to download the ".NET" version, not the standard version
   
2. **.NET 9.0 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/9.0
   - Required for C# compilation and IntelliSense support

3. **Spine Runtime** (included)
   - The project includes the Spine-Godot runtime in the `bin/` directory
   - No additional setup required for Spine functionality

## Project Structure

```
GodotStage/
├── Actors/
│   └── Player/           # Player character scene and scripts
├── Content/
│   └── Spine/
│       └── Player/       # Spine animation assets (spineboy)
├── Maps/                 # Isometric map scenes
├── Systems/              # Core game systems (Input, etc.)
├── bin/                  # Spine-Godot runtime binaries
├── Main.cs               # Main game scene controller
├── Main.tscn             # Main scene file
└── project.godot         # Godot project configuration
```

## How to Run

### Method 1: Using Godot Editor (Recommended)

1. **Open the Project**:
   - Launch Godot Engine
   - Click "Import" and navigate to the project folder
   - Select the `project.godot` file
   - Click "Import & Edit"

2. **Build the Project**:
   - In the Godot editor, go to `Project` → `Tools` → `C#` → `Build Project`
   - Wait for the build to complete (you should see "Build succeeded" in the output)

3. **Run the Game**:
   - Press `F5` or click the "Play" button in the top-right
   - Select `Main.tscn` as the main scene when prompted
   - The game will start with the player character in the center of the screen

### Method 2: Command Line Build

1. **Navigate to Project Directory**:
   ```bash
   cd path/to/GodotStage
   ```

2. **Build the Project**:
   ```bash
   dotnet build
   ```

3. **Run with Godot**:
   ```bash
   godot --headless --export-debug "Windows Desktop" ./build/
   # Or simply open in editor:
   godot project.godot
   ```


## Development Setup


## Troubleshooting

### Common Issues

1. **"Build failed" errors**:
   - Ensure .NET 9.0 SDK is installed
   - Try `Project` → `Tools` → `C#` → `Reload Project`
   - Check the output panel for specific error messages

2. **Spine animations not working**:
   - Verify the Spine runtime files are in the `bin/` directory
   - Check that `.gdextension` files are properly imported

3. **Player not moving**:
   - Ensure the InputSystem is properly loaded (check AutoLoad settings)
   - Verify the Player scene is correctly instantiated in Main.tscn

4. **Missing assemblies**:
   - Clean and rebuild the project: `Project` → `Tools` → `C#` → `Clean Build`
   - Restart Godot and rebuild

### Performance Tips

- Use `Project` → `Project Settings` → `Rendering` to adjust quality settings
- Enable V-Sync for smoother gameplay
- Consider using Godot's built-in profiler for performance analysis

## Additional Resources

- [Godot Documentation](https://docs.godotengine.org/)
- [Spine-Godot Runtime](https://github.com/EsotericSoftware/spine-runtimes/tree/4.2/spine-godot)
- [C# in Godot Guide](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/)