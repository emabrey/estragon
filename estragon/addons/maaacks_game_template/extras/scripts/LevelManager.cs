using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Manage level changes in games.</summary>
[GlobalClass]
public partial class LevelManager : Node
{
    [Export] public LevelLoader LevelLoader { get; set; } = null!;
    [Export(PropertyHint.File)] public string StartingLevelPath { get; set; } = "";
    [Export] public SceneLister SceneLister { get; set; } = null!;
    [Export] public bool AutoLoad { get; set; } = true;

    [ExportGroup("Scenes")]
    [Export(PropertyHint.File, "*.tscn")] public string MainMenuScenePath { get; set; } = "";
    [Export(PropertyHint.File, "*.tscn")] public string EndingScenePath { get; set; } = "";
    [Export] public PackedScene? GameWonScene { get; set; }
    [Export] public PackedScene? LevelLostScene { get; set; }
    [Export] public PackedScene? LevelWonScene { get; set; }

    public Node? CurrentLevel { get; set; }

    private string _currentLevelPath = "";
    public string CurrentLevelPath
    {
        get => _currentLevelPath;
        set => SetCurrentLevelPath(value);
    }

    private string _checkpointLevelPath = "";
    public string CheckpointLevelPath
    {
        get => _checkpointLevelPath;
        set => SetCheckpointLevelPath(value);
    }

    public virtual void SetCurrentLevelPath(string value) => _currentLevelPath = value;

    public virtual void SetCheckpointLevelPath(string value) => _checkpointLevelPath = value;

    private void TryConnectingSignalToNode(Node node, StringName signalName, Callable callable)
    {
        if (node.HasSignal(signalName) && !node.IsConnected(signalName, callable))
            node.Connect(signalName, callable);
    }

    private void TryConnectingSignalToLevel(StringName signalName, Callable callable)
        => TryConnectingSignalToNode(CurrentLevel!, signalName, callable);

    public string GetMainMenuScenePath()
    {
        if (string.IsNullOrEmpty(MainMenuScenePath))
            return AppConfig.Instance.MainMenuScenePath;
        return MainMenuScenePath;
    }

    private void LoadMainMenu() => SceneLoader.Instance.LoadScene(GetMainMenuScenePath());

    private int FindInSceneLister(string levelPath)
    {
        if (SceneLister == null)
            return -1;
        levelPath = ResourceUid.EnsurePath(levelPath);
        return SceneLister.Files.IndexOf(levelPath);
    }

    public bool IsOnLastLevel()
    {
        int currentLevelId = FindInSceneLister(CurrentLevelPath);
        return currentLevelId > -1 && currentLevelId == SceneLister.Files.Count - 1;
    }

    public string GetRelativeLevelPath(int offset = 1)
    {
        int currentLevelId = FindInSceneLister(CurrentLevelPath);
        if (currentLevelId > -1)
        {
            if (currentLevelId >= Mathf.Max(0, -offset) && currentLevelId < SceneLister.Files.Count - Mathf.Max(0, offset))
            {
                currentLevelId += offset;
                return SceneLister.Files[currentLevelId];
            }
        }
        return "";
    }

    public string GetNextLevelPath() => GetRelativeLevelPath(1);

    public string GetPrevLevelPath() => GetRelativeLevelPath(-1);

    public string GetEndingScenePath()
    {
        if (string.IsNullOrEmpty(EndingScenePath))
            return AppConfig.Instance.EndingScenePath;
        return EndingScenePath;
    }

    private void LoadEnding()
    {
        if (!string.IsNullOrEmpty(GetEndingScenePath()))
            SceneLoader.Instance.LoadScene(GetEndingScenePath());
        else
            LoadMainMenu();
    }

    private void OnLevelLost()
    {
        if (LevelLostScene != null)
        {
            var instance = LevelLostScene.Instantiate();
            GetTree().CurrentScene.AddChild(instance);
            TryConnectingSignalToNode(instance, "restart_pressed", Callable.From(ReloadLevel));
            TryConnectingSignalToNode(instance, "main_menu_pressed", Callable.From(LoadMainMenu));
        }
        else
        {
            ReloadLevel();
        }
    }

    public virtual string GetCheckpointLevelPath()
    {
        if (string.IsNullOrEmpty(_checkpointLevelPath))
        {
            if (SceneLister != null)
                return SceneLister.Files.Count > 0 ? SceneLister.Files[0] : "";
            if (!string.IsNullOrEmpty(StartingLevelPath))
                return StartingLevelPath;
        }
        return _checkpointLevelPath;
    }

    public void LoadLevel(string levelPath)
    {
        CurrentLevelPath = levelPath;
        LevelLoader.LoadLevel(levelPath);
    }

    private void LoadCheckpointLevel() => LoadLevel(GetCheckpointLevelPath());

    private void ReloadLevel() => LoadLevel(CurrentLevelPath);

    private void LoadWinScreenOrEnding()
    {
        if (GameWonScene != null)
        {
            var instance = GameWonScene.Instantiate();
            GetTree().CurrentScene.AddChild(instance);
            TryConnectingSignalToNode(instance, "continue_pressed", Callable.From(LoadEnding));
            TryConnectingSignalToNode(instance, "restart_pressed", Callable.From(ReloadLevel));
            TryConnectingSignalToNode(instance, "main_menu_pressed", Callable.From(LoadMainMenu));
        }
        else
        {
            LoadEnding();
        }
    }

    private void LoadLevelWonScreenOrCheckpoint()
    {
        if (LevelWonScene != null)
        {
            var instance = LevelWonScene.Instantiate();
            GetTree().CurrentScene.AddChild(instance);
            TryConnectingSignalToNode(instance, "continue_pressed", Callable.From(LoadCheckpointLevel));
            TryConnectingSignalToNode(instance, "restart_pressed", Callable.From(ReloadLevel));
            TryConnectingSignalToNode(instance, "main_menu_pressed", Callable.From(LoadMainMenu));
        }
        else
        {
            LoadCheckpointLevel();
        }
    }

    private void OnLevelWon(string nextLevelPath = "")
    {
        if (string.IsNullOrEmpty(nextLevelPath))
            nextLevelPath = GetNextLevelPath();
        if (string.IsNullOrEmpty(nextLevelPath))
        {
            LoadWinScreenOrEnding();
        }
        else
        {
            CheckpointLevelPath = nextLevelPath;
            LoadLevelWonScreenOrCheckpoint();
        }
    }

    private void OnLevelChanged(string nextLevelPath)
    {
        CheckpointLevelPath = nextLevelPath;
        LoadCheckpointLevel();
    }

    private void ConnectLevelSignals()
    {
        TryConnectingSignalToLevel("level_lost", Callable.From(OnLevelLost));
        TryConnectingSignalToLevel("level_won", Callable.From<string>(OnLevelWon));
        TryConnectingSignalToLevel("level_changed", Callable.From<string>(OnLevelChanged));
    }

    private async void OnLevelLoaderLevelLoaded()
    {
        CurrentLevel = LevelLoader.CurrentLevel;
        await ToSignal(CurrentLevel!, Node.SignalName.Ready);
        ConnectLevelSignals();
    }

    private void OnLevelLoaderLevelLoadStarted() { }

    private void OnLevelLoaderLevelReady() { }

    private void AutoLoadLevel()
    {
        if (AutoLoad)
            LoadCheckpointLevel();
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;
        LevelLoader.LevelLoaded += OnLevelLoaderLevelLoaded;
        LevelLoader.LevelReady += OnLevelLoaderLevelReady;
        LevelLoader.LevelLoadStarted += OnLevelLoaderLevelLoadStarted;
        AutoLoadLevel();
    }
}
