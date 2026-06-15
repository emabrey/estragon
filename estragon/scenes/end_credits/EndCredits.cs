using Godot;
using estragon.addons.maaacks_game_template;
using estragon.scenes.credits;

namespace estragon.scenes.end_credits;

[Tool]
public partial class EndCredits : ScrollingCredits
{
    [Export(PropertyHint.File, "*.tscn")] public string MainMenuScenePath { get; set; } = "";
    [Export] public bool ForceMouseModeVisible { get; set; }

    private Control EndMessagePanel => GetNode<Control>("%EndMessagePanel");
    private Control ExitButton => GetNode<Control>("%ExitButton");
    private Control MenuButton => GetNode<Control>("%MenuButton");
    private MouseFilterEnum _initMouseFilter;

    public string GetMainMenuScenePath()
        => string.IsNullOrEmpty(MainMenuScenePath) ? AppConfig.Instance.MainMenuScenePath : MainMenuScenePath;

    protected override void EndReached_()
    {
        EndMessagePanel.Show();
        MouseFilter = MouseFilterEnum.Stop;
        if (ForceMouseModeVisible)
            Input.MouseMode = Input.MouseModeEnum.Visible;
        base.EndReached_();
    }

    public void LoadMainMenu() => SceneLoader.Instance.LoadScene(GetMainMenuScenePath());

    public void ExitGame()
    {
        if (OS.HasFeature("web"))
            LoadMainMenu();
        GetTree().Quit();
    }

    protected override void OnVisibilityChanged()
    {
        if (Visible)
        {
            EndMessagePanel.Hide();
            MouseFilter = _initMouseFilter;
        }
        base.OnVisibilityChanged();
    }

    public override void _Ready()
    {
        _initMouseFilter = MouseFilter;
        if (string.IsNullOrEmpty(GetMainMenuScenePath()))
            MenuButton.Hide();
        if (OS.HasFeature("web"))
            ExitButton.Hide();
        EndMessagePanel.Hide();
        base._Ready();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionReleased("ui_cancel"))
        {
            if (!EndMessagePanel.Visible)
                EndReached_();
            else
                ExitGame();
        }
    }

    private void _on_exit_button_pressed() => ExitGame();

    private void _on_menu_button_pressed() => LoadMainMenu();
}
