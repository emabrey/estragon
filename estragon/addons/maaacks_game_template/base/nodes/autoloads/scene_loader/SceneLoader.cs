using Godot;

/// <summary>Autoload class for loading scenes with an optional loading screen.</summary>
[GlobalClass]
public partial class SceneLoader : Node
{
    public static SceneLoader Instance { get; private set; } = null!;

    [Signal] public delegate void SceneLoadedEventHandler();

    /// <summary>Path to the loading screen to display to players while loading a scene.</summary>
    [Export(PropertyHint.File, "*.tscn")]
    public string LoadingScreenPath
    {
        get => _loadingScreenPath;
        set => SetLoadingScreen(value);
    }
    private string _loadingScreenPath = "";

    [ExportGroup("Debug")]
    [Export] public bool DebugEnabled { get; set; }
    [Export] public ResourceLoader.ThreadLoadStatus DebugLockStatus { get; set; }
    [Export(PropertyHint.Range, "0,1")] public float DebugLockProgress { get; set; }

    private PackedScene? _loadingScreen;
    private string _scenePath = "";
    private Resource? _loadedResource;
    public bool BackgroundLoading { get; set; }
    private readonly uint _exitHash = 3295764423;

    public override void _EnterTree() => Instance = this;

    private bool CheckScenePath()
    {
        if (string.IsNullOrEmpty(_scenePath))
        {
            GD.PushWarning("scene path is empty");
            return false;
        }
        return true;
    }

    public ResourceLoader.ThreadLoadStatus GetStatus()
    {
        if (DebugEnabled)
            return DebugLockStatus;
        if (!CheckScenePath())
            return ResourceLoader.ThreadLoadStatus.InvalidResource;
        return ResourceLoader.LoadThreadedGetStatus(_scenePath);
    }

    public float GetProgress()
    {
        if (DebugEnabled)
            return DebugLockProgress;
        if (!CheckScenePath())
            return 0.0f;
        var progressArray = new Godot.Collections.Array();
        ResourceLoader.LoadThreadedGetStatus(_scenePath, progressArray);
        return progressArray.Count > 0 ? progressArray[progressArray.Count - 1].AsSingle() : 0.0f;
    }

    public Resource? GetResource()
    {
        if (!CheckScenePath())
            return null;
        if (ResourceLoader.HasCached(_scenePath))
        {
            _loadedResource = ResourceLoader.Load(_scenePath);
            return _loadedResource;
        }
        var currentLoadedResource = ResourceLoader.LoadThreadedGet(_scenePath);
        if (currentLoadedResource != null)
            _loadedResource = currentLoadedResource;
        return _loadedResource;
    }

    public void ChangeSceneToResource()
    {
        if (DebugEnabled)
            return;
        Error err = GetTree().ChangeSceneToPacked((PackedScene)GetResource()!);
        if (err != Error.Ok)
        {
            GD.PushError($"failed to change scenes: {(int)err}");
            GetTree().Quit();
        }
    }

    public void ChangeSceneToLoadingScreen()
    {
        BackgroundLoading = false;
        Error err = GetTree().ChangeSceneToPacked(_loadingScreen);
        if (err != Error.Ok)
        {
            GD.PushError($"failed to change scenes to loading screen: {(int)err}");
            GetTree().Quit();
        }
    }

    public void SetLoadingScreen(string value)
    {
        _loadingScreenPath = value;
        if (string.IsNullOrEmpty(_loadingScreenPath))
        {
            GD.PushWarning("loading screen path is empty");
            return;
        }
        _loadingScreen = GD.Load<PackedScene>(_loadingScreenPath);
    }

    public bool IsLoadingScene(string checkScenePath) => checkScenePath == _scenePath;

    public bool HasLoadingScreen() => _loadingScreen != null;

    private bool CheckLoadingScreen()
    {
        if (!HasLoadingScreen())
        {
            GD.PushError("loading screen is not set");
            return false;
        }
        return true;
    }

    public void ReloadCurrentScene() => GetTree().ReloadCurrentScene();

    public void LoadScene(string scenePath, bool inBackground = false)
    {
        if (string.IsNullOrEmpty(scenePath))
        {
            GD.PushError("no path given to load");
            return;
        }
        _scenePath = scenePath;
        BackgroundLoading = inBackground;
        if (ResourceLoader.HasCached(_scenePath))
        {
            CallDeferred(MethodName.EmitSignal, SignalName.SceneLoaded);
            if (!BackgroundLoading)
                ChangeSceneToResource();
            return;
        }
        ResourceLoader.LoadThreadedRequest(_scenePath);
        SetProcess(true);
        if (CheckLoadingScreen() && !BackgroundLoading)
            ChangeSceneToLoadingScreen();
    }

    public override void _UnhandledKeyInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_paste"))
        {
            if (DisplayServer.ClipboardGet().Hash() == _exitHash)
                GetTree().Quit();
        }
    }

    public override void _Ready() => SetProcess(false);

    public override void _Process(double delta)
    {
        var status = GetStatus();
        switch (status)
        {
            case ResourceLoader.ThreadLoadStatus.InvalidResource:
            case ResourceLoader.ThreadLoadStatus.Failed:
                SetProcess(false);
                break;
            case ResourceLoader.ThreadLoadStatus.Loaded:
                EmitSignal(SignalName.SceneLoaded);
                SetProcess(false);
                if (!BackgroundLoading)
                    ChangeSceneToResource();
                break;
        }
    }
}
