using Godot;
using estragon.addons.maaacks_game_template;
using estragon.scripts;

namespace estragon.scenes.menus.main_menu;

/// <summary>Main menu extension that adds options. Adds a 'Continue' button if a game is in progress.</summary>
public partial class MainMenuWithOptions : MainMenu
{
    [Export] public PackedScene LevelSelectPackedScene { get; set; } = null!;
    [Export] public bool ConfirmNewGame { get; set; } = true;

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

    private void AddLevelSelectIfSet()
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
        AddLevelSelectIfSet();
        ShowContinueIfSet();
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
