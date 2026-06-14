using Godot;

/// <summary>Base menu scene that links to a game scene, an options menu, and credits.</summary>
[GlobalClass]
public partial class MainMenu : Control
{
    [Signal] public delegate void SubMenuOpenedEventHandler();
    [Signal] public delegate void SubMenuClosedEventHandler();
    [Signal] public delegate void GameStartedEventHandler();
    [Signal] public delegate void GameExitedEventHandler();

    [Export(PropertyHint.File, "*.tscn")] public string GameScenePath { get; set; } = "";
    [Export] public PackedScene OptionsPackedScene { get; set; }
    [Export] public PackedScene CreditsPackedScene { get; set; }
    [Export] public bool ConfirmExit { get; set; } = true;

    [ExportGroup("Extra Settings")]
    [Export] public bool SignalGameStart { get; set; }
    [Export] public bool SignalGameExit { get; set; }

    protected Control SubMenu;

    protected Control MenuContainer => GetNode<Control>("%MenuContainer");
    protected CaptureFocus MenuButtonsBoxContainer => GetNode<CaptureFocus>("%MenuButtonsBoxContainer");
    protected Control NewGameButton => GetNode<Control>("%NewGameButton");
    protected Control OptionsButton => GetNode<Control>("%OptionsButton");
    protected Control CreditsButton => GetNode<Control>("%CreditsButton");
    protected Control ExitButton => GetNode<Control>("%ExitButton");
    protected Control ExitConfirmation => GetNode<Control>("%ExitConfirmation");

    public string GetGameScenePath()
    {
        if (string.IsNullOrEmpty(GameScenePath))
            return AppConfig.Instance.GameScenePath;
        return GameScenePath;
    }

    public virtual void LoadGameScene()
    {
        if (SignalGameStart)
        {
            SceneLoader.Instance.LoadScene(GetGameScenePath(), true);
            EmitSignal(SignalName.GameStarted);
        }
        else
        {
            SceneLoader.Instance.LoadScene(GetGameScenePath());
        }
    }

    public virtual void NewGame() => LoadGameScene();

    public void TryExitGame()
    {
        if (ConfirmExit && !ExitConfirmation.Visible)
            ExitConfirmation.Call("show");
        else
            ExitGame();
    }

    public virtual void ExitGame()
    {
        if (OS.HasFeature("web"))
            return;
        if (SignalGameExit)
            EmitSignal(SignalName.GameExited);
        else
            GetTree().Quit();
    }

    protected virtual Node OpenSubMenu(PackedScene menu)
    {
        SubMenu = (Control)menu.Instantiate();
        AddChild(SubMenu);
        MenuContainer.Hide();
        SubMenu.Connect("hidden", Callable.From(CloseSubMenu), (uint)ConnectFlags.OneShot);
        SubMenu.Connect(Node.SignalName.TreeExiting, Callable.From(CloseSubMenu), (uint)ConnectFlags.OneShot);
        EmitSignal(SignalName.SubMenuOpened);
        return SubMenu;
    }

    protected virtual void CloseSubMenu()
    {
        if (SubMenu == null)
            return;
        SubMenu.QueueFree();
        SubMenu = null;
        MenuContainer.Show();
        EmitSignal(SignalName.SubMenuClosed);
    }

    protected bool EventIsMouseButtonReleased(InputEvent @event)
        => @event is InputEventMouseButton mouseButton && !mouseButton.IsPressed();

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionReleased("ui_cancel"))
        {
            if (SubMenu != null)
                CloseSubMenu();
            else
                TryExitGame();
        }
        if (@event.IsActionReleased("ui_accept") && GetViewport().GuiGetFocusOwner() == null)
            MenuButtonsBoxContainer.FocusFirst();
    }

    private void HideExitForWeb()
    {
        if (OS.HasFeature("web"))
            ExitButton.Hide();
    }

    private void HideNewGameIfUnset()
    {
        if (string.IsNullOrEmpty(GetGameScenePath()))
            NewGameButton.Hide();
    }

    private void HideOptionsIfUnset()
    {
        if (OptionsPackedScene == null)
            OptionsButton.Hide();
    }

    private void HideCreditsIfUnset()
    {
        if (CreditsPackedScene == null)
            CreditsButton.Hide();
    }

    public override void _Ready()
    {
        HideExitForWeb();
        HideOptionsIfUnset();
        HideCreditsIfUnset();
        HideNewGameIfUnset();
    }

    private void _on_new_game_button_pressed() => NewGame();

    private void _on_options_button_pressed() => OpenSubMenu(OptionsPackedScene);

    private void _on_credits_button_pressed() => OpenSubMenu(CreditsPackedScene);

    private void _on_exit_button_pressed() => TryExitGame();

    private void _on_exit_confirmation_confirmed() => ExitGame();
}
