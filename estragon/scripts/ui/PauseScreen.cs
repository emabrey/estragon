using Godot;

[GlobalClass]
public partial class PauseScreen : CanvasLayer
{
    [Signal] public delegate void ToggleGamePausedEventHandler();
    [Signal] public delegate void GamePauseStateChangingEventHandler(bool isPaused);

    public static bool DisablePause { get; set; } = true;

    public override void _Ready()
    {
        Hide();
        ToggleGamePaused += _HandleToggleGamePause;
    }

    public override void _Input(InputEvent @event)
    {
        if (!DisablePause && @event.IsActionPressed("ui_pause"))
            EmitSignal(SignalName.ToggleGamePaused);
    }

    private void _HandleToggleGamePause()
    {
        if (Visible)
            _Unpause();
        else
            _Pause();
    }

    private void _Pause()
    {
        EmitSignal(SignalName.GamePauseStateChanging, true);
        GetTree().Paused = true;
        Show();
    }

    private void _Unpause()
    {
        EmitSignal(SignalName.GamePauseStateChanging, false);
        GetTree().Paused = false;
        Hide();
    }
}
