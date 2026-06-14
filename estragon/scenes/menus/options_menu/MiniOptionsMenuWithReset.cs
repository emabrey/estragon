using Godot;

public partial class MiniOptionsMenuWithReset : MiniOptionsMenu
{
    private void _on_reset_game_control_reset_confirmed() => GameState.Reset();
}
