using Godot;

[GlobalClass]
public partial class AppConfig : Node
{
    public static AppConfig Instance { get; private set; } = null!;

    [ExportGroup("Scenes")]
    [Export(PropertyHint.File, "*.tscn")] public string MainMenuScenePath { get; set; } = "";
    [Export(PropertyHint.File, "*.tscn")] public string GameScenePath { get; set; } = "";
    [Export(PropertyHint.File, "*.tscn")] public string EndingScenePath { get; set; } = "";

    public override void _EnterTree() => Instance = this;

    public override void _Ready()
    {
        GlobalState.Open();
        AppSettings.SetFromConfigAndWindow(GetWindow());
    }
}
