using Godot;

public partial class WinLoseManager : Node
{
    [Export(PropertyHint.File, "*.tscn")] public string MainMenuScenePath { get; set; } = "";
    [Export(PropertyHint.File, "*.tscn")] public string EndingScenePath { get; set; } = "";
    [Export] public PackedScene GameWonScene { get; set; }
    [Export] public PackedScene GameLostScene { get; set; }

    public bool HasLostGame { get; private set; }
    public bool HasWonGame { get; private set; }

    private void TryConnectingSignalToNode(Node node, StringName signalName, Callable callable)
    {
        if (node.HasSignal(signalName) && !node.IsConnected(signalName, callable))
            node.Connect(signalName, callable);
    }

    public string GetMainMenuScenePath()
    {
        if (string.IsNullOrEmpty(MainMenuScenePath))
            return AppConfig.Instance.MainMenuScenePath;
        return MainMenuScenePath;
    }

    private void LoadMainMenu() => SceneLoader.Instance.LoadScene(GetMainMenuScenePath());

    public string GetEndingScenePath()
    {
        if (string.IsNullOrEmpty(EndingScenePath))
            return AppConfig.Instance.EndingScenePath;
        return EndingScenePath;
    }

    private void LoadEnding()
    {
        if (string.IsNullOrEmpty(GetEndingScenePath()))
            LoadMainMenu();
        else
            SceneLoader.Instance.LoadScene(GetEndingScenePath());
    }

    private void LoadLoseScreenOrReload()
    {
        if (GameLostScene != null)
        {
            var instance = GameLostScene.Instantiate();
            GetTree().CurrentScene.AddChild(instance);
            TryConnectingSignalToNode(instance, "restart_pressed", Callable.From(ReloadLevel));
            TryConnectingSignalToNode(instance, "main_menu_pressed", Callable.From(LoadMainMenu));
        }
        else
        {
            ReloadLevel();
        }
    }

    private void ReloadLevel() => SceneLoader.Instance.ReloadCurrentScene();

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

    public void GameLost()
    {
        if (HasWonGame || HasLostGame)
            return;
        HasLostGame = true;
        LoadLoseScreenOrReload();
    }

    public void GameWon()
    {
        if (HasWonGame || HasLostGame)
            return;
        HasWonGame = true;
        LoadWinScreenOrEnding();
    }
}
