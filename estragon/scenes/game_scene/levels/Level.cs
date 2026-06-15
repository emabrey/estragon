using Godot;
using estragon.addons.maaacks_game_template;
using estragon.scripts;
using estragon.scenes.game_scene;

namespace estragon.scenes.game_scene.levels;

public partial class Level : Node
{
    [Signal] public delegate void LevelLostEventHandler();
    [Signal] public delegate void LevelWonEventHandler(string levelPath);
    [Signal] public delegate void LevelChangedEventHandler(string levelPath);

    /// <summary>Optional path to the next level if using an open world level system.</summary>
    [Export(PropertyHint.File, "*.tscn")] public string NextLevelPath { get; set; } = "";

    private LevelState? _levelState;

    private void _on_lose_button_pressed() => EmitSignal(SignalName.LevelLost);

    private void _on_win_button_pressed() => EmitSignal(SignalName.LevelWon, NextLevelPath);

    public void OpenTutorials()
    {
        GetNode<TutorialManager>("%TutorialManager").OpenTutorials();
        if (_levelState == null) return;
        _levelState.TutorialRead = true;
        GlobalState.Save();
    }

    public override void _Ready()
    {
        _levelState = GameState.GetLevelState(SceneFilePath);
        if (_levelState == null) return;
        GetNode<ColorPickerButton>("%ColorPickerButton").Color = _levelState.Color;
        GetNode<ColorRect>("%BackgroundColor").Color = _levelState.Color;
        if (!_levelState.TutorialRead)
            OpenTutorials();
    }

    private void _on_color_picker_button_color_changed(Color color)
    {
        GetNode<ColorRect>("%BackgroundColor").Color = color;
        if (_levelState == null) return;
        _levelState.Color = color;
        GlobalState.Save();
    }

    private void _on_tutorial_button_pressed() => OpenTutorials();
}
