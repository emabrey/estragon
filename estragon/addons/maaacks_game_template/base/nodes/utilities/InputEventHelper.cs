using System.Collections.Generic;
using Godot;

/// <summary>Helper class for organizing constants related to <see cref="InputEvent"/>.</summary>
public static class InputEventHelper
{
    public const string DeviceKeyboard = "Keyboard";
    public const string DeviceMouse = "Mouse";
    public const string DeviceXboxController = "Xbox";
    public const string DeviceSwitchController = "Switch";
    public const string DeviceSwitchJoyconLeftController = "Switch Left Joycon";
    public const string DeviceSwitchJoyconRightController = "Switch Right Joycon";
    public const string DeviceSwitchJoyconCombinedController = "Switch Combined Joycons";
    public const string DevicePlaystationController = "Playstation";
    public const string DeviceSteamdeckController = "Steamdeck";
    public const string DeviceGeneric = "Generic";

    public const string JoystickLeftName = "Left Stick";
    public const string JoystickRightName = "Right Stick";
    public const string DPadName = "Dpad";

    public static readonly string[] MouseButtons =
        { "None", "Left", "Right", "Middle", "Scroll Up", "Scroll Down", "Wheel Left", "Wheel Right" };

    public static readonly Dictionary<string, string[]> JoypadButtonNameMap = new()
    {
        [DeviceGeneric] = new[] { "Trigger A", "Trigger B", "Trigger C", "", "", "", "", "Left Stick Press", "Right Stick Press", "Left Shoulder", "Right Shoulder", "Up", "Down", "Left", "Right" },
        [DeviceXboxController] = new[] { "A", "B", "X", "Y", "View", "Home", "Menu", "Left Stick Press", "Right Stick Press", "Left Shoulder", "Right Shoulder", "Up", "Down", "Left", "Right", "Share" },
        [DeviceSwitchController] = new[] { "B", "A", "Y", "X", "Minus", "", "Plus", "Left Stick Press", "Right Stick Press", "Left Shoulder", "Right Shoulder", "Up", "Down", "Left", "Right", "Capture" },
        [DevicePlaystationController] = new[] { "Cross", "Circle", "Square", "Triangle", "Select", "PS", "Options", "Left Stick Press", "Right Stick Press", "Left Shoulder", "Right Shoulder", "Up", "Down", "Left", "Right", "Microphone" },
        [DeviceSteamdeckController] = new[] { "A", "B", "X", "Y", "View", "", "Options", "Left Stick Press", "Right Stick Press", "Left Shoulder", "Right Shoulder", "Up", "Down", "Left", "Right" },
    };

    public static readonly Dictionary<string, string[]> SdlDeviceNames = new()
    {
        [DeviceXboxController] = new[] { "XInput", "XBox" },
        [DevicePlaystationController] = new[] { "Sony", "PS5", "PS4", "Nacon" },
        [DeviceSteamdeckController] = new[] { "Steam" },
        [DeviceSwitchController] = new[] { "Switch" },
        [DeviceSwitchJoyconLeftController] = new[] { "Joy-Con (L)", "Left Joy-Con" },
        [DeviceSwitchJoyconRightController] = new[] { "Joy-Con (R)", "Right Joy-Con" },
        [DeviceSwitchJoyconCombinedController] = new[] { "Joy-Con (L/R)", "Combined Joy-Cons" },
    };

    public static readonly Dictionary<JoyButton, string> JoyButtonNames = new()
    {
        [JoyButton.A] = "Button A",
        [JoyButton.B] = "Button B",
        [JoyButton.X] = "Button X",
        [JoyButton.Y] = "Button Y",
        [JoyButton.LeftShoulder] = "Left Shoulder",
        [JoyButton.RightShoulder] = "Right Shoulder",
        [JoyButton.LeftStick] = "Left Stick",
        [JoyButton.RightStick] = "Right Stick",
        [JoyButton.Start] = "Button Start",
        [JoyButton.Guide] = "Button Guide",
        [JoyButton.Back] = "Button Back",
        [JoyButton.DpadUp] = DPadName + " Up",
        [JoyButton.DpadDown] = DPadName + " Down",
        [JoyButton.DpadLeft] = DPadName + " Left",
        [JoyButton.DpadRight] = DPadName + " Right",
        [JoyButton.Misc1] = "Misc",
    };

    public static readonly Dictionary<JoyButton, string> JoypadDpadNames = new()
    {
        [JoyButton.DpadUp] = DPadName + " Up",
        [JoyButton.DpadDown] = DPadName + " Down",
        [JoyButton.DpadLeft] = DPadName + " Left",
        [JoyButton.DpadRight] = DPadName + " Right",
    };

    public static readonly Dictionary<JoyAxis, string> JoyAxisNames = new()
    {
        [JoyAxis.TriggerLeft] = "Left Trigger",
        [JoyAxis.TriggerRight] = "Right Trigger",
    };

    public static readonly Godot.Collections.Dictionary BuiltInActionNameMap = new()
    {
        ["ui_accept"] = "Accept",
        ["ui_select"] = "Select",
        ["ui_cancel"] = "Cancel",
        ["ui_focus_next"] = "Focus Next",
        ["ui_focus_prev"] = "Focus Prev",
        ["ui_left"] = "Left (UI)",
        ["ui_right"] = "Right (UI)",
        ["ui_up"] = "Up (UI)",
        ["ui_down"] = "Down (UI)",
        ["ui_page_up"] = "Page Up",
        ["ui_page_down"] = "Page Down",
        ["ui_home"] = "Home",
        ["ui_end"] = "End",
        ["ui_cut"] = "Cut",
        ["ui_copy"] = "Copy",
        ["ui_paste"] = "Paste",
        ["ui_undo"] = "Undo",
        ["ui_redo"] = "Redo",
    };

    public static bool HasJoypad() => Input.GetConnectedJoypads().Count > 0;

    public static bool IsJoypadEvent(InputEvent @event)
        => @event is InputEventJoypadButton or InputEventJoypadMotion;

    public static bool IsMouseEvent(InputEvent @event)
        => @event is InputEventMouseButton or InputEventMouseMotion;

    public static string GetDeviceNameById(int deviceId)
    {
        if (deviceId >= 0)
        {
            string deviceName = Input.GetJoyName(deviceId);
            foreach (var pair in SdlDeviceNames)
            {
                foreach (string keyword in pair.Value)
                {
                    if (deviceName.Contains(keyword, System.StringComparison.OrdinalIgnoreCase))
                        return pair.Key;
                }
            }
        }
        return DeviceGeneric;
    }

    public static string GetDeviceName(InputEvent @event)
    {
        if (@event is InputEventJoypadButton or InputEventJoypadMotion)
        {
            if (@event.Device == -1)
                return DeviceGeneric;
            return GetDeviceNameById(@event.Device);
        }
        return DeviceGeneric;
    }

    private static bool DisplayServerSupportsKeycodeFromPhysical()
        => OS.HasFeature("windows") || OS.HasFeature("macos") || OS.HasFeature("linux");

    public static string GetText(InputEvent @event)
    {
        if (@event == null)
            return "";
        if (@event is InputEventJoypadButton joypadButton)
        {
            if (JoyButtonNames.TryGetValue(joypadButton.ButtonIndex, out string? buttonName))
                return buttonName;
        }
        else if (@event is InputEventJoypadMotion joypadMotion)
        {
            string fullString = "";
            string directionString = "";
            bool isRightOrDown = joypadMotion.AxisValue > 0.0f;
            if (JoyAxisNames.TryGetValue(joypadMotion.Axis, out string? axisName))
                return axisName;
            switch (joypadMotion.Axis)
            {
                case JoyAxis.LeftX:
                    fullString = JoystickLeftName;
                    directionString = isRightOrDown ? "Right" : "Left";
                    break;
                case JoyAxis.LeftY:
                    fullString = JoystickLeftName;
                    directionString = isRightOrDown ? "Down" : "Up";
                    break;
                case JoyAxis.RightX:
                    fullString = JoystickRightName;
                    directionString = isRightOrDown ? "Right" : "Left";
                    break;
                case JoyAxis.RightY:
                    fullString = JoystickRightName;
                    directionString = isRightOrDown ? "Down" : "Up";
                    break;
            }
            fullString += " " + directionString;
            return fullString;
        }
        else if (@event is InputEventKey keyEvent)
        {
            Key keycode = keyEvent.GetPhysicalKeycode();
            if (keycode != Key.None)
                keycode = keyEvent.GetPhysicalKeycodeWithModifiers();
            else
                keycode = keyEvent.GetKeycodeWithModifiers();
            if (DisplayServerSupportsKeycodeFromPhysical())
                keycode = DisplayServer.KeyboardGetKeycodeFromPhysical(keycode);
            return OS.GetKeycodeString(keycode);
        }
        return @event.AsText();
    }

    public static string GetDeviceSpecificText(InputEvent @event, string deviceName = "")
    {
        if (string.IsNullOrEmpty(deviceName))
            deviceName = GetDeviceName(@event);
        if (@event is InputEventJoypadButton joypadButton)
        {
            string joypadButtonName = "";
            if (JoypadDpadNames.TryGetValue(joypadButton.ButtonIndex, out string? dpadName))
                joypadButtonName = dpadName;
            else if ((int)joypadButton.ButtonIndex < JoypadButtonNameMap[deviceName].Length)
                joypadButtonName = JoypadButtonNameMap[deviceName][(int)joypadButton.ButtonIndex];
            return $"{deviceName} {joypadButtonName}";
        }
        if (@event is InputEventJoypadMotion)
            return $"{deviceName} {GetText(@event)}";
        if (@event is InputEventMouseButton mouseButton)
        {
            if ((int)mouseButton.ButtonIndex < MouseButtons.Length)
            {
                string mouseButtonName = MouseButtons[(int)mouseButton.ButtonIndex];
                return $"{DeviceMouse} {mouseButtonName}";
            }
        }
        return GetText(@event).Capitalize();
    }
}
