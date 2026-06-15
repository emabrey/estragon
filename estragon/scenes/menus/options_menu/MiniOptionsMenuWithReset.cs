using Godot;
using estragon.addons.maaacks_game_template;
using estragon.scripts;

namespace estragon.scenes.menus.options_menu;

public partial class MiniOptionsMenuWithReset : MiniOptionsMenu
{
    private void _on_reset_game_control_reset_confirmed() => GameState.Reset();
}
