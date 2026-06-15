using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Loads scenes into a container.</summary>
[Tool]
[GlobalClass]
public partial class LevelLoader : Node
{
    [Signal] public delegate void LevelLoadStartedEventHandler();
    [Signal] public delegate void LevelLoadedEventHandler();
    [Signal] public delegate void LevelReadyEventHandler();

    [Export] public Node LevelContainer { get; set; } = null!;
    [Export] public LoadingScreen LevelLoadingScreen { get; set; } = null!;

    [ExportGroup("Debugging")]
    [Export] public Node? CurrentLevel { get; set; }

    public string CurrentLevelPath { get; private set; } = "";
    public bool IsLoading { get; private set; }

    private Node AttachLevel(Resource levelResource)
    {
        System.Diagnostics.Debug.Assert(LevelContainer != null, "level_container is null");
        var instance = ((PackedScene)levelResource).Instantiate();
        LevelContainer.CallDeferred(Node.MethodName.AddChild, instance);
        return instance;
    }

    public async void LoadLevel(string levelPath)
    {
        if (IsLoading)
            return;
        if (GodotObject.IsInstanceValid(CurrentLevel))
        {
            CurrentLevel.QueueFree();
            await ToSignal(CurrentLevel, Node.SignalName.TreeExited);
            CurrentLevel = null;
        }
        CurrentLevelPath = levelPath;
        IsLoading = true;
        SceneLoader.Instance.LoadScene(CurrentLevelPath, true);
        LevelLoadingScreen?.Reset();
        EmitSignal(SignalName.LevelLoadStarted);
        await ToSignal(SceneLoader.Instance, SceneLoader.SignalName.SceneLoaded);
        IsLoading = false;
        CurrentLevel = AttachLevel(SceneLoader.Instance.GetResource()!);
        LevelLoadingScreen?.Close();
        EmitSignal(SignalName.LevelLoaded);
        await ToSignal(CurrentLevel!, Node.SignalName.Ready);
        EmitSignal(SignalName.LevelReady);
    }
}
