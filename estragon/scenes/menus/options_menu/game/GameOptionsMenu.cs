using Godot;
using estragon.scripts;

namespace estragon.scenes.menus.options_menu.game;

public partial class GameOptionsMenu : Control
{
    private void _on_ResetGameControl_reset_confirmed() => GameState.Reset();
}
