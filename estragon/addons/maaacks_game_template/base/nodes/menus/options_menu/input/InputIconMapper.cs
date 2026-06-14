using System.Collections.Generic;
using Godot;

[Tool]
[GlobalClass]
public partial class InputIconMapper : FileLister
{
    [Signal] public delegate void JoypadDeviceChangedEventHandler();

    private static readonly Dictionary<string, string> CommonReplaceStrings = new()
    {
        ["L 1"] = "Left Shoulder",
        ["R 1"] = "Right Shoulder",
        ["L 2"] = "Left Trigger",
        ["R 2"] = "Right Trigger",
        ["Lt"] = "Left Trigger",
        ["Rt"] = "Right Trigger",
        ["Lb"] = "Left Shoulder",
        ["Rb"] = "Right Shoulder",
    };

    [Export] public Godot.Collections.Array<string> PrioritizedStrings { get; set; } = new();
    [Export] public Godot.Collections.Dictionary ReplaceStrings { get; set; } = new();
    [Export] public Godot.Collections.Array<string> FilteredStrings { get; set; } = new();
    [Export] public bool AddStickDirections { get; set; }
    [Export] public string InitialJoypadDevice { get; set; } = InputEventHelper.DeviceGeneric;

    [Export]
    public bool MatchIconsToInputsAction
    {
        get => false;
        set
        {
            if (value && Engine.IsEditorHint())
                MatchIconsToInputs();
        }
    }

    [Export] public Godot.Collections.Dictionary MatchingIcons { get; set; } = new();

    [ExportGroup("Debug")]
    [Export] public Godot.Collections.Dictionary AllIcons { get; set; } = new();

    private string _lastJoypadDevice;

    private bool IsEndOfWord(string fullString, string what)
    {
        int stringEndPosition = fullString.Find(what) + what.Length;
        bool endOfWord = false;
        if (stringEndPosition + 1 < fullString.Length)
        {
            string nextCharacter = fullString.Substr(stringEndPosition, 1);
            endOfWord = nextCharacter == " ";
        }
        return fullString.EndsWith(what) || endOfWord;
    }

    private string GetStandardJoyName(string joyName)
    {
        var allReplaceStrings = new Dictionary<string, string>(CommonReplaceStrings);
        foreach (Variant key in ReplaceStrings.Keys)
            allReplaceStrings[key.AsString()] = ReplaceStrings[key].AsString();
        foreach (var pair in allReplaceStrings)
        {
            string what = pair.Key;
            if (joyName.Contains(what) && IsEndOfWord(joyName, what))
            {
                int position = joyName.Find(what);
                joyName = joyName.Remove(position, what.Length);
                joyName = joyName.Insert(position, pair.Value);
            }
        }
        var combinedJoystickName = new List<string>();
        foreach (string part in joyName.Split(' '))
        {
            if (FilteredStrings.Contains(part.ToLower()))
                continue;
            if (!string.IsNullOrEmpty(part))
                combinedJoystickName.Add(part);
        }
        joyName = string.Join(" ", combinedJoystickName);
        joyName = joyName.Trim();
        return joyName;
    }

    private void MatchIconToFile(string file)
    {
        string matchingString = file.GetFile().GetBaseName();
        var icon = ResourceLoader.Load(file) as Texture2D;
        if (icon == null)
            return;
        AllIcons[matchingString] = icon;
        matchingString = matchingString.Capitalize();
        matchingString = GetStandardJoyName(matchingString);
        matchingString = matchingString.Trim();
        if (AddStickDirections && matchingString.EndsWith("Stick"))
        {
            MatchingIcons[matchingString + " Up"] = icon;
            MatchingIcons[matchingString + " Down"] = icon;
            MatchingIcons[matchingString + " Left"] = icon;
            MatchingIcons[matchingString + " Right"] = icon;
            return;
        }
        if (MatchingIcons.ContainsKey(matchingString))
            return;
        MatchingIcons[matchingString] = icon;
    }

    private List<string> PrioritizedFiles()
    {
        var priorityLevels = new Dictionary<string, int>();
        var prioritizedFiles = new List<string>();
        foreach (string prioritizedString in PrioritizedStrings)
        {
            foreach (string file in Files)
            {
                if (file.Contains(prioritizedString, System.StringComparison.OrdinalIgnoreCase))
                {
                    if (priorityLevels.ContainsKey(file))
                        priorityLevels[file] += 1;
                    else
                        priorityLevels[file] = 1;
                }
            }
        }
        var priorityFileMap = new Dictionary<int, List<string>>();
        int maxPriorityLevel = 0;
        foreach (var pair in priorityLevels)
        {
            int priorityLevel = pair.Value;
            maxPriorityLevel = Mathf.Max(priorityLevel, maxPriorityLevel);
            if (priorityFileMap.ContainsKey(priorityLevel))
                priorityFileMap[priorityLevel].Add(pair.Key);
            else
                priorityFileMap[priorityLevel] = new List<string> { pair.Key };
        }
        while (maxPriorityLevel > 0)
        {
            if (priorityFileMap.TryGetValue(maxPriorityLevel, out var filesAtLevel))
            {
                foreach (string priorityFile in filesAtLevel)
                    prioritizedFiles.Add(priorityFile);
            }
            maxPriorityLevel -= 1;
        }
        return prioritizedFiles;
    }

    private void MatchIconsToInputs()
    {
        MatchingIcons.Clear();
        AllIcons.Clear();
        foreach (string prioritizedFile in PrioritizedFiles())
            MatchIconToFile(prioritizedFile);
        foreach (string file in Files)
            MatchIconToFile(file);
    }

    public Texture2D GetIcon(InputEvent inputEvent)
    {
        string specificText = InputEventHelper.GetDeviceSpecificText(inputEvent, _lastJoypadDevice);
        if (MatchingIcons.ContainsKey(specificText))
            return MatchingIcons[specificText].As<Texture2D>();
        return null;
    }

    private void AssignJoypad0ToLast()
    {
        if (_lastJoypadDevice != InitialJoypadDevice)
            return;
        var connectedJoypads = Input.GetConnectedJoypads();
        if (connectedJoypads.Count == 0)
            return;
        _lastJoypadDevice = InputEventHelper.GetDeviceNameById(connectedJoypads[0]);
    }

    public override void _Input(InputEvent @event)
    {
        string deviceName = InputEventHelper.GetDeviceName(@event);
        if (deviceName != InputEventHelper.DeviceGeneric && deviceName != _lastJoypadDevice)
        {
            _lastJoypadDevice = deviceName;
            EmitSignal(SignalName.JoypadDeviceChanged);
        }
    }

    public override void _Ready()
    {
        _lastJoypadDevice = InitialJoypadDevice;
        AssignJoypad0ToLast();
        if (Files.Count == 0)
            RefreshFiles();
        if (MatchingIcons.Count == 0)
            MatchIconsToInputs();
    }
}
