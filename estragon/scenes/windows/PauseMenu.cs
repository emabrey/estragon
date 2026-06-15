using System.Threading.Tasks;
using Godot;
using estragon.addons.maaacks_game_template;

namespace estragon.scenes.windows;

[Tool]
public partial class PauseMenu : OverlaidWindow
{
    [Export] public PackedScene OptionsMenuScene { get; set; } = null!;
    [Export(PropertyHint.File, "*.tscn")] public string MainMenuScenePath { get; set; } = "";
    [Export] public NodePath RestartConfirmationNodePath { get; set; } = null!;
    [Export] public NodePath MainMenuConfirmationNodePath { get; set; } = null!;
    [Export] public NodePath ExitConfirmationNodePath { get; set; } = null!;
    [Export] public NodePath MenuContainerNodePath { get; set; } = "..";

    private ConfirmationOverlaidWindow _restartConfirmation = null!;
    private ConfirmationOverlaidWindow _mainMenuConfirmation = null!;
    private ConfirmationOverlaidWindow _exitConfirmation = null!;
    private Node _menuContainer = null!;
    private Control OptionsButton => GetNode<Control>("%OptionsButton");
    private Control MainMenuButton => GetNode<Control>("%MainMenuButton");
    private Control ExitButton => GetNode<Control>("%ExitButton");

    private Node? _openWindow;
    private bool _ignoreFirstCancel;

    public string GetMainMenuScenePath()
        => string.IsNullOrEmpty(MainMenuScenePath) ? AppConfig.Instance.MainMenuScenePath : MainMenuScenePath;

    private void CloseWindow()
    {
        if (_openWindow != null)
        {
            if (_openWindow is WindowContainer wc)
                wc.Close();
            else if (_openWindow is CanvasItem ci)
                ci.Hide();
            _openWindow = null;
        }
    }

    private void DisableFocus()
    {
        foreach (Node child in GetNode("%MenuButtons").GetChildren())
        {
            if (child is Control control)
                control.FocusMode = FocusModeEnum.None;
        }
    }

    private void EnableFocus()
    {
        foreach (Node child in GetNode("%MenuButtons").GetChildren())
        {
            if (child is Control control)
                control.FocusMode = FocusModeEnum.All;
        }
    }

    private void LoadScene(string scenePath)
    {
        _sceneTree.Paused = false;
        SceneLoader.Instance.LoadScene(scenePath);
    }

    private async Task ShowWindow(Control window)
    {
        Callable.From(DisableFocus).CallDeferred();
        if (window is WindowContainer wc)
            wc.Show();
        else
            window.Show();
        _openWindow = window;
        await ToSignal(window, CanvasItem.SignalName.Hidden);
        _openWindow = null;
        Callable.From(EnableFocus).CallDeferred();
    }

    private async void LoadAndShowMenu(PackedScene scene)
    {
        var windowInstance = (Control)scene.Instantiate();
        windowInstance.Visible = false;
        _menuContainer.CallDeferred(Node.MethodName.AddChild, windowInstance);
        await ShowWindow(windowInstance);
        windowInstance.QueueFree();
    }

    protected override void HandleCancelInput()
    {
        if (_ignoreFirstCancel)
        {
            _ignoreFirstCancel = false;
            return;
        }
        if (_openWindow != null)
            CloseWindow();
        else
            base.HandleCancelInput();
    }

    public override void Show()
    {
        base.Show();
        if (Input.IsActionPressed("ui_cancel"))
            _ignoreFirstCancel = true;
    }

    private void RefreshExitButton() => ExitButton.Visible = !OS.HasFeature("web");

    private void RefreshOptionsButton() => OptionsButton.Visible = OptionsMenuScene != null;

    private void RefreshMainMenuButton() => MainMenuButton.Visible = !string.IsNullOrEmpty(GetMainMenuScenePath());

    public override void _Ready()
    {
        _restartConfirmation = GetNode<ConfirmationOverlaidWindow>(RestartConfirmationNodePath);
        _mainMenuConfirmation = GetNode<ConfirmationOverlaidWindow>(MainMenuConfirmationNodePath);
        _exitConfirmation = GetNode<ConfirmationOverlaidWindow>(ExitConfirmationNodePath);
        _menuContainer = GetNode(MenuContainerNodePath);
        RefreshExitButton();
        RefreshOptionsButton();
        RefreshMainMenuButton();
        _restartConfirmation.Confirmed += _on_restart_confirmation_confirmed;
        _mainMenuConfirmation.Confirmed += _on_main_menu_confirmation_confirmed;
        _exitConfirmation.Confirmed += _on_exit_confirmation_confirmed;
    }

    private void _on_restart_button_pressed() => _ = ShowWindow(_restartConfirmation);

    private void _on_options_button_pressed() => LoadAndShowMenu(OptionsMenuScene);

    private void _on_main_menu_button_pressed() => _ = ShowWindow(_mainMenuConfirmation);

    private void _on_exit_button_pressed() => _ = ShowWindow(_exitConfirmation);

    private void _on_restart_confirmation_confirmed()
    {
        SceneLoader.Instance.ReloadCurrentScene();
        Close();
    }

    private void _on_main_menu_confirmation_confirmed() => LoadScene(GetMainMenuScenePath());

    private void _on_exit_confirmation_confirmed() => GetTree().Quit();
}
