using Godot;

/// <summary>Main menu extension that adds options and animates the title and menu fading in.</summary>
public partial class MainMenuWithAnimations : MainMenu
{
    [Export] public PackedScene LevelSelectPackedScene { get; set; }
    [Export] public bool ConfirmNewGame { get; set; } = true;

    private AnimationNodeStateMachinePlayback _animationStateMachine;

    private Control ContinueGameButton => GetNode<Control>("%ContinueGameButton");
    private Control LevelSelectButton => GetNode<Control>("%LevelSelectButton");
    private WindowContainer NewGameConfirmation => GetNode<WindowContainer>("%NewGameConfirmation");

    public override void LoadGameScene()
    {
        GameState.StartGame();
        base.LoadGameScene();
    }

    public override void NewGame()
    {
        if (ConfirmNewGame && ContinueGameButton.Visible)
        {
            NewGameConfirmation.Show();
        }
        else
        {
            GameState.Reset();
            LoadGameScene();
        }
    }

    public void IntroDone() => _animationStateMachine.Travel("OpenMainMenu");

    private bool IsInIntro() => _animationStateMachine.GetCurrentNode() == "Intro";

    private bool EventSkipsIntro(InputEvent @event)
        => @event.IsActionReleased("ui_accept")
            || @event.IsActionReleased("ui_select")
            || @event.IsActionReleased("ui_cancel")
            || EventIsMouseButtonReleased(@event);

    protected override Node OpenSubMenu(PackedScene menu)
    {
        _animationStateMachine.Travel("OpenSubMenu");
        return base.OpenSubMenu(menu);
    }

    protected override void CloseSubMenu()
    {
        base.CloseSubMenu();
        _animationStateMachine.Travel("OpenMainMenu");
    }

    public override void _Input(InputEvent @event)
    {
        if (IsInIntro() && EventSkipsIntro(@event))
        {
            IntroDone();
            return;
        }
        base._Input(@event);
    }

    private void ShowLevelSelectIfSet()
    {
        if (LevelSelectPackedScene == null)
            return;
        if (GameState.GetLevelsReached() <= 1)
            return;
        LevelSelectButton.Show();
    }

    private void ShowContinueIfSet()
    {
        if (string.IsNullOrEmpty(GameState.GetCurrentLevelPath()))
            return;
        ContinueGameButton.Show();
    }

    public override void _Ready()
    {
        base._Ready();
        ShowLevelSelectIfSet();
        ShowContinueIfSet();
        _animationStateMachine = (AnimationNodeStateMachinePlayback)GetNode<AnimationTree>("MenuAnimationTree").Get("parameters/playback");
    }

    private void _on_continue_game_button_pressed()
    {
        GameState.ContinueGame();
        LoadGameScene();
    }

    private void _on_level_select_button_pressed()
    {
        var levelSelectScene = OpenSubMenu(LevelSelectPackedScene);
        if (levelSelectScene.HasSignal("level_selected"))
            levelSelectScene.Connect("level_selected", Callable.From(LoadGameScene));
    }

    private void _on_new_game_confirmation_confirmed()
    {
        GameState.Reset();
        LoadGameScene();
    }
}
