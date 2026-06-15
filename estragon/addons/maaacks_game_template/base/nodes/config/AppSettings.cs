using System.Collections.Generic;
using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Interface to read/write general application settings through <see cref="PlayerConfig"/>.</summary>
public static class AppSettings
{
    public const string InputSection = "InputSettings";
    public const string AudioSection = "AudioSettings";
    public const string VideoSection = "VideoSettings";
    public const string GameSection = "GameSettings";
    public const string ApplicationSection = "ApplicationSettings";
    public const string CustomSection = "CustomSettings";

    public const string Fullscreen = "Fullscreen";
    public const string ScreenResolution = "ScreenResolution";
    public const string VSync = "V-Sync";
    public const string MuteSetting = "Mute";
    public const int MasterBusIndex = 0;
    public const string SystemBusNamePrefix = "_";

    // Input
    public static Godot.Collections.Dictionary DefaultActionEvents { get; set; } = new();
    public static readonly List<float> InitialBusVolumes = new();

    public static Godot.Collections.Array GetConfigInputEvents(string actionName, Variant @default = default)
        => PlayerConfig.GetConfig(InputSection, actionName, @default).AsGodotArray();

    public static void SetConfigInputEvents(string actionName, Godot.Collections.Array inputs)
        => PlayerConfig.SetConfig(InputSection, actionName, inputs);

    private static void ClearConfigInputEvents()
        => PlayerConfig.EraseSection(InputSection);

    public static void RemoveActionInputEvent(string actionName, InputEvent inputEvent)
    {
        InputMap.ActionEraseEvent(actionName, inputEvent);
        var actionEvents = InputMap.ActionGetEvents(actionName);
        var configEvents = GetConfigInputEvents(actionName, (Variant)actionEvents);
        int index = configEvents.IndexOf(inputEvent);
        if (index >= 0)
            configEvents.RemoveAt(index);
        SetConfigInputEvents(actionName, configEvents);
    }

    public static void SetInputFromConfig(string actionName)
    {
        var actionEvents = InputMap.ActionGetEvents(actionName);
        // The GDScript original returned early when the config returned the default
        // (i.e. nothing saved). Checking the section key is the clearer equivalent.
        if (!PlayerConfig.HasSectionKey(InputSection, actionName))
            return;
        var configEvents = GetConfigInputEvents(actionName, (Variant)actionEvents);
        if (configEvents.Count == 0)
        {
            PlayerConfig.EraseSectionKey(InputSection, actionName);
            return;
        }
        InputMap.ActionEraseEvents(actionName);
        foreach (Variant configEvent in configEvents)
        {
            if (!actionEvents.Contains(configEvent.As<InputEvent>()))
                InputMap.ActionAddEvent(actionName, configEvent.As<InputEvent>());
        }
    }

    private static Godot.Collections.Array<StringName> GetActionNamesInternal()
        => InputMap.GetActions();

    private static Godot.Collections.Array<StringName> GetCustomActionNames()
    {
        var result = new Godot.Collections.Array<StringName>();
        foreach (StringName actionName in InputMap.GetActions())
        {
            string name = actionName.ToString();
            if (!name.StartsWith("ui_") && !name.StartsWith("spatial_editor"))
                result.Add(actionName);
        }
        return result;
    }

    public static Godot.Collections.Array<StringName> GetActionNames(bool builtInActions = false)
        => builtInActions ? GetActionNamesInternal() : GetCustomActionNames();

    public static void ResetToDefaultInputs()
    {
        ClearConfigInputEvents();
        foreach (Variant key in DefaultActionEvents.Keys)
        {
            StringName actionName = key.AsStringName();
            InputMap.ActionEraseEvents(actionName);
            foreach (Variant inputEvent in DefaultActionEvents[key].AsGodotArray())
                InputMap.ActionAddEvent(actionName, inputEvent.As<InputEvent>());
        }
    }

    public static void SetDefaultInputs()
    {
        foreach (StringName actionName in GetActionNamesInternal())
            DefaultActionEvents[actionName] = InputMap.ActionGetEvents(actionName);
    }

    public static void SetInputsFromConfig()
    {
        foreach (StringName actionName in GetActionNamesInternal())
            SetInputFromConfig(actionName);
    }

    // Audio
    public static float GetBusVolume(int busIndex)
    {
        float initialLinear = 1.0f;
        if (InitialBusVolumes.Count > busIndex)
            initialLinear = InitialBusVolumes[busIndex];
        float linear = (float)Mathf.DbToLinear(AudioServer.GetBusVolumeDb(busIndex));
        linear /= initialLinear;
        return linear;
    }

    public static void SetBusVolume(int busIndex, float linear)
    {
        float initialLinear = 1.0f;
        if (InitialBusVolumes.Count > busIndex)
            initialLinear = InitialBusVolumes[busIndex];
        linear *= initialLinear;
        AudioServer.SetBusVolumeDb(busIndex, (float)Mathf.LinearToDb(linear));
    }

    public static bool IsMuted()
        => AudioServer.IsBusMute(MasterBusIndex);

    public static void SetMute(bool muteFlag)
        => AudioServer.SetBusMute(MasterBusIndex, muteFlag);

    public static string GetAudioBusName(int busIter)
        => AudioServer.GetBusName(busIter);

    public static void SetAudioFromConfig()
    {
        for (int busIter = 0; busIter < AudioServer.BusCount; busIter++)
        {
            string busKey = GetAudioBusName(busIter).ToPascalCase();
            float busVolume = GetBusVolume(busIter);
            InitialBusVolumes.Add(busVolume);
            busVolume = PlayerConfig.GetConfig(AudioSection, busKey, busVolume).AsSingle();
            if (float.IsNaN(busVolume))
            {
                busVolume = 1.0f;
                PlayerConfig.SetConfig(AudioSection, busKey, busVolume);
            }
            SetBusVolume(busIter, busVolume);
        }
        bool muteAudioFlag = IsMuted();
        muteAudioFlag = PlayerConfig.GetConfig(AudioSection, MuteSetting, muteAudioFlag).AsBool();
        SetMute(muteAudioFlag);
    }

    // Video
    public static void SetFullscreenEnabled(bool value, Window window)
        => window.Mode = value ? Window.ModeEnum.ExclusiveFullscreen : Window.ModeEnum.Windowed;

    public static void SetResolution(Vector2I value, Window window, bool updateConfig = true)
    {
        if (value.X == 0 || value.Y == 0)
            return;
        window.Size = value;
        if (updateConfig)
            PlayerConfig.SetConfig(VideoSection, ScreenResolution, value);
    }

    public static bool IsFullscreen(Window window)
        => window.Mode == Window.ModeEnum.ExclusiveFullscreen || window.Mode == Window.ModeEnum.Fullscreen;

    public static Vector2I GetResolution(Window window)
        => PlayerConfig.GetConfig(VideoSection, ScreenResolution, window.Size).AsVector2I();

    private static void OnWindowSizeChanged(Window window)
        => PlayerConfig.SetConfig(VideoSection, ScreenResolution, window.Size);

    private static bool SetFullscreenFromConfig(Window window)
    {
        bool fullscreenEnabled = IsFullscreen(window);
        fullscreenEnabled = PlayerConfig.GetConfig(VideoSection, Fullscreen, fullscreenEnabled).AsBool();
        SetFullscreenEnabled(fullscreenEnabled, window);
        return fullscreenEnabled;
    }

    public static void SetVsync(DisplayServer.VSyncMode vsyncMode, Window? window = null)
    {
        int windowId = 0;
        if (window != null)
            windowId = (int)window.GetWindowId();
        DisplayServer.WindowSetVsyncMode(vsyncMode, windowId);
    }

    public static DisplayServer.VSyncMode GetVsync(Window? window = null)
    {
        int windowId = 0;
        if (window != null)
            windowId = (int)window.GetWindowId();
        return DisplayServer.WindowGetVsyncMode(windowId);
    }

    private static DisplayServer.VSyncMode SetVSyncFromConfig(Window window)
    {
        var vsync = GetVsync(window);
        vsync = (DisplayServer.VSyncMode)PlayerConfig.GetConfig(VideoSection, VSync, (int)vsync).AsInt32();
        SetVsync(vsync);
        return vsync;
    }

    public static void SetVideoFromConfig(Window window)
    {
        window.SizeChanged += () => OnWindowSizeChanged(window);
        bool fullscreenEnabled = SetFullscreenFromConfig(window);
        if (!(fullscreenEnabled || OS.HasFeature("web")))
        {
            Vector2I currentResolution = GetResolution(window);
            SetResolution(currentResolution, window);
        }
        SetVSyncFromConfig(window);
    }

    // All
    public static void SetFromConfig()
    {
        SetDefaultInputs();
        SetInputsFromConfig();
        SetAudioFromConfig();
    }

    public static void SetFromConfigAndWindow(Window window)
    {
        SetFromConfig();
        SetVideoFromConfig(window);
    }
}
