using Godot;
using System.Collections.Generic;

// Debug Manager - handles all debug visualization features
public partial class DebugManager : Node
{
    // UI for damage logs
    private CanvasLayer damageLogCanvas;
    private Control damageLogContainer;
    private const int MAX_DAMAGE_LOGS = 5;
    private List<Label> damageLogLabels = new List<Label>();
    private float logDisplayTime = 3.0f; // Show logs for 3 seconds

    // Damage area visualization
    private CanvasLayer damageAreaCanvas;
    private bool showDamageArea = false;
    private float damageAreaDisplayTime = 0.5f; // Show damage area for 0.5 seconds
    private float damageAreaTimer = 0.0f;

    // Damage system constants
    private const float SHOOT_DAMAGE_RADIUS = 150.0f;

    private CharacterBody2D player;

    public override void _Ready()
    {
        // Get player reference
        player = GetNode<CharacterBody2D>("/root/Main/Player");

        // Setup damage log UI
        SetupDamageLogUI();

        // Setup damage area overlay canvas
        SetupDamageAreaCanvas();
    }

    public override void _Process(double delta)
    {
        // Update damage area visualization timer
        if (showDamageArea)
        {
            damageAreaTimer -= (float)delta;
            if (damageAreaTimer <= 0)
            {
                showDamageArea = false;
                // Trigger redraw of the canvas layer
                damageAreaCanvas?.GetChild<Control>(0)?.QueueRedraw();
            }
        }
    }

    private void SetupDamageLogUI()
    {
        // Create a CanvasLayer for fixed-position damage logs
        damageLogCanvas = new CanvasLayer();
        damageLogCanvas.Layer = 50; // Lower than damage area (100) but still high
        AddChild(damageLogCanvas);

        // Create container for damage logs (fixed screen position)
        damageLogContainer = new Control();
        damageLogContainer.Position = new Vector2(20, 20); // Fixed screen position
        damageLogContainer.Size = new Vector2(400, 200);
        damageLogCanvas.AddChild(damageLogContainer);

        // Create log labels
        for (int i = 0; i < MAX_DAMAGE_LOGS; i++)
        {
            var label = new Label();
            label.Position = new Vector2(0, i * 25);
            label.Size = new Vector2(400, 24);
            label.AddThemeFontSizeOverride("font_size", 16);
            label.AddThemeColorOverride("font_color", Colors.Yellow);
            label.Visible = false;
            damageLogLabels.Add(label);
            damageLogContainer.AddChild(label);
        }
    }

    private void SetupDamageAreaCanvas()
    {
        // Create a CanvasLayer that draws on top of all world elements (including logs)
        damageAreaCanvas = new CanvasLayer();
        damageAreaCanvas.Layer = 110; // Higher than log canvas (50) to ensure it's on top
        AddChild(damageAreaCanvas);

        // Create a Control node to handle the drawing (CanvasLayers work with Controls)
        var damageAreaControl = new Control();
        damageAreaControl.Size = new Vector2(1920, 1080); // Match viewport size
        damageAreaControl.MouseFilter = Control.MouseFilterEnum.Ignore;

        // Connect to the draw signal instead of using _Draw
        damageAreaControl.Draw += () => {
            if (showDamageArea && player != null)
            {
				// Since camera follows player, player is at center of screen
				Vector2 playerScreenPos = GetViewport().GetVisibleRect().Size / 2;

                Color outlineColor = new Color(1.0f, 0.8f, 0.0f, 1.0f); // Bright orange outline
                Color innerRingColor = new Color(1.0f, 0.6f, 0.0f, 0.8f); // Lighter orange inner ring

                // Draw outer ring (main damage area)
                for (int i = 0; i < 64; i++) // More segments for smoother circle
                {
                    float angle1 = (i / 64.0f) * Mathf.Tau;
                    float angle2 = ((i + 1) / 64.0f) * Mathf.Tau;

                    Vector2 point1 = playerScreenPos + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * SHOOT_DAMAGE_RADIUS;
                    Vector2 point2 = playerScreenPos + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * SHOOT_DAMAGE_RADIUS;

                    damageAreaControl.DrawLine(point1, point2, outlineColor, 3.0f);
                }

                // Draw inner ring for better visibility
                float innerRadius = SHOOT_DAMAGE_RADIUS * 0.8f;
                for (int i = 0; i < 32; i++)
                {
                    float angle1 = (i / 32.0f) * Mathf.Tau;
                    float angle2 = ((i + 1) / 32.0f) * Mathf.Tau;

                    Vector2 point1 = playerScreenPos + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * innerRadius;
                    Vector2 point2 = playerScreenPos + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * innerRadius;

                    damageAreaControl.DrawLine(point1, point2, innerRingColor, 2.0f);
                }

                // Draw radial lines for better depth perception
                for (int i = 0; i < 8; i++)
                {
                    float angle = (i / 8.0f) * Mathf.Tau;
                    Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                    Vector2 startPoint = playerScreenPos + direction * (SHOOT_DAMAGE_RADIUS * 0.3f);
                    Vector2 endPoint = playerScreenPos + direction * SHOOT_DAMAGE_RADIUS;

                    damageAreaControl.DrawLine(startPoint, endPoint, outlineColor, 1.5f);
                }
            }
        };

        damageAreaCanvas.AddChild(damageAreaControl);
    }

    public void ShowDamageLog(string message)
    {
        // Find the next available position (first invisible label from the top)
        int targetIndex = -1;
        for (int i = 0; i < MAX_DAMAGE_LOGS; i++)
        {
            if (!damageLogLabels[i].Visible)
            {
                targetIndex = i;
                break;
            }
        }

        // If all labels are visible, shift all down and use the last position
        if (targetIndex == -1)
        {
            // Shift all labels down (older messages move to higher indices)
            for (int i = MAX_DAMAGE_LOGS - 1; i > 0; i--)
            {
                damageLogLabels[i].Text = damageLogLabels[i - 1].Text;
                damageLogLabels[i].Position = new Vector2(0, i * 25);
                damageLogLabels[i].Visible = true;
            }
            targetIndex = 0; // Use the top position for the new message
        }

        // Set new message at the target position
        damageLogLabels[targetIndex].Text = message;
        damageLogLabels[targetIndex].Position = new Vector2(0, targetIndex * 25);
        damageLogLabels[targetIndex].Visible = true;

        // Set timer to hide this specific label
        int labelIndex = targetIndex;
        var timer = GetTree().CreateTimer(logDisplayTime);
        timer.Timeout += () => {
            if (damageLogLabels[labelIndex].Visible)
            {
                damageLogLabels[labelIndex].Visible = false;
                // Shift remaining labels up to fill the gap
                for (int i = labelIndex; i < MAX_DAMAGE_LOGS - 1; i++)
                {
                    if (damageLogLabels[i + 1].Visible)
                    {
                        damageLogLabels[i].Text = damageLogLabels[i + 1].Text;
                        damageLogLabels[i].Position = new Vector2(0, i * 25);
                        damageLogLabels[i].Visible = true;
                    }
                    else
                    {
                        damageLogLabels[i].Visible = false;
                    }
                }
            }
        };
    }

    public void ShowDamageArea()
    {
        showDamageArea = true;
        damageAreaTimer = damageAreaDisplayTime;
        // Trigger redraw of the canvas layer
        damageAreaCanvas?.GetChild<Control>(0)?.QueueRedraw();
    }

    public void LogDamage(int frogNumber, int damage, int newHealth, float distance)
    {
        ShowDamageLog($"Shot Frog #{frogNumber} with {damage} dmg Health {newHealth}");
        GD.Print($"Damaged frog at distance {distance:F1} - Damage: {damage}, Health: {newHealth}");
    }

    public void LogShootSummary(int damagedFrogs)
    {
        GD.Print($"Shoot damaged {damagedFrogs} frogs within {SHOOT_DAMAGE_RADIUS} radius");
    }
}
