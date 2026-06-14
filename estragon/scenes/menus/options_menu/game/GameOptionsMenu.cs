using Godot;

public partial class GameOptionsMenu : Control
{
    private void _on_ResetGameControl_reset_confirmed() => GameState.Reset();
}
