using Godot;

public static class GlobalState
{
    public const string SaveStatePath = "user://global_state.tres";
    public const string NoVersionName = "0.0.0";

    public static GlobalStateData? Current { get; set; }
    public static string? CurrentVersion { get; set; }

    private static void LogOpened()
    {
        if (Current != null)
            Current.LastUnixTimeOpened = (long)Time.GetUnixTimeFromSystem();
    }

    private static void LogVersion()
    {
        if (Current == null)
            return;
        CurrentVersion = ProjectSettings.GetSetting("application/config/version", NoVersionName).AsString();
        if (string.IsNullOrEmpty(CurrentVersion))
            CurrentVersion = NoVersionName;
        if (string.IsNullOrEmpty(Current.FirstVersionOpened))
            Current.FirstVersionOpened = CurrentVersion;
        Current.LastVersionOpened = CurrentVersion;
    }

    private static void LoadCurrentState()
    {
        if (FileAccess.FileExists(SaveStatePath))
            Current = ResourceLoader.Load<GlobalStateData>(SaveStatePath);
        Current ??= new GlobalStateData();
    }

    public static void Open()
    {
        LoadCurrentState();
        LogOpened();
        LogVersion();
        Save();
    }

    public static void Save()
    {
        if (Current != null)
            ResourceSaver.Save(Current, SaveStatePath);
    }

    public static bool HasState(string stateKey)
        => Current != null && Current.HasState(stateKey);

    public static T? GetOrCreateState<T>(string stateKey) where T : Resource, new()
        => Current?.GetOrCreateState<T>(stateKey);

    public static void Reset()
    {
        if (Current == null)
            return;
        Current.States.Clear();
        Save();
    }
}
