using Godot;
using System.Collections.Generic;

namespace godotstage.Systems
{
	public partial class InputSystem : Node
	{
		public override void _Ready()
		{
			RegisterInputActions();

			// Optional: Write current mapping back to project settings in editor for visibility.
			// if (Engine.IsEditorHint())
			// {
			//     InputMap.SaveToProjectSettings();
			//     ProjectSettings.Save();
			//     GD.Print("Input actions saved to project.godot");
			// }
		}

		private void RegisterInputActions()
		{
			// Action → Definition of a group of "key events"
			// WASD use PhysicalKeycode, direction keys use Keycode
			var inputActions = new Dictionary<string, List<InputEvent>>
			{
				{
					"move_up",
					new List<InputEvent>
					{
						new InputEventKey { PhysicalKeycode = Key.W },
						new InputEventKey { Keycode = Key.Up }
					}
				},
				{
					"move_down",
					new List<InputEvent>
					{
						new InputEventKey { PhysicalKeycode = Key.S },
						new InputEventKey { Keycode = Key.Down }
					}
				},
				{
					"move_left",
					new List<InputEvent>
					{
						new InputEventKey { PhysicalKeycode = Key.A },
						new InputEventKey { Keycode = Key.Left }
					}
				},
				{
					"move_right",
					new List<InputEvent>
					{
						new InputEventKey { PhysicalKeycode = Key.D },
						new InputEventKey { Keycode = Key.Right }
					}
				},
				{
					"shoot",
					new List<InputEvent>
					{
						new InputEventMouseButton { ButtonIndex = MouseButton.Left }
					}
				}
			};

			// Optional: Add gamepad mappings for actions that need joystick support (example)
			// AddGamepadBindings(inputActions);

			foreach (var kv in inputActions)
			{
				var actionName = kv.Key;
				var events = kv.Value;

				if (!InputMap.HasAction(actionName))
				{
					// Optional: Set deadzone for actions that need joystick support (default 0.5 if not needed)
					InputMap.AddAction(actionName /*, deadzone: 0.5f */);
					GD.Print($"Added input action: {actionName}");
				}

				var existing = InputMap.ActionGetEvents(actionName);

				foreach (var ev in events)
				{
					if (!HasEquivalentEvent(existing, ev))
					{
						InputMap.ActionAddEvent(actionName, ev);
						GD.Print($"Added binding to '{actionName}': {DescribeEvent(ev)}");
					}
				}
			}

			GD.Print("Input system initialization completed.");
		}

		private static bool HasEquivalentEvent(Godot.Collections.Array<InputEvent> existing, InputEvent target)
		{
			foreach (var e in existing)
			{
				if (e is InputEventKey ek && target is InputEventKey tk)
				{
					// Compare both dimensions: Keycode & PhysicalKeycode (if one is equal, it is considered existing)
					if ((ek.Keycode != Key.None && ek.Keycode == tk.Keycode) ||
						(ek.PhysicalKeycode != Key.None && ek.PhysicalKeycode == tk.PhysicalKeycode))
						return true;
				}
				else if (e is InputEventMouseButton em && target is InputEventMouseButton tm)
				{
					// Compare mouse button events
					if (em.ButtonIndex == tm.ButtonIndex)
						return true;
				}
				else if (e.GetType() == target.GetType())
				{
					// Other event types (joystick/mouse etc.) can be extended according to need
					if (DescribeEvent(e) == DescribeEvent(target))
						return true;
				}
			}
			return false;
		}

		private static string DescribeEvent(InputEvent ev)
		{
			return ev switch
			{
				InputEventKey k => $"Key(code={k.Keycode}, phys={k.PhysicalKeycode})",
				InputEventMouseButton mb => $"MouseBtn(button={mb.ButtonIndex})",
				InputEventJoypadButton jb => $"JoyBtn(device={jb.Device}, button={(int)jb.ButtonIndex})",
				InputEventJoypadMotion jm => $"JoyAxis(device={jm.Device}, axis={(int)jm.Axis}, value={jm.AxisValue:0.00})",
				_ => ev.GetType().Name
			};
		}

		// Example: If joystick support is needed, open call and adjust as needed
		private static void AddGamepadBindings(Dictionary<string, List<InputEvent>> map)
		{
			// D-pad
			map["move_up"].Add(new InputEventJoypadButton { ButtonIndex = JoyButton.DpadUp });
			map["move_down"].Add(new InputEventJoypadButton { ButtonIndex = JoyButton.DpadDown });
			map["move_left"].Add(new InputEventJoypadButton { ButtonIndex = JoyButton.DpadLeft });
			map["move_right"].Add(new InputEventJoypadButton { ButtonIndex = JoyButton.DpadRight });

			// Left joystick (example: check strength in code based on axis threshold, not directly binding to action here)
			// Also can create actions for "move_axis_x/y", use GetActionStrength or read Axis directly in game.
		}
	}
}
