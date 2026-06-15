using Godot;
using estragon.addons.maaacks_game_template;

namespace estragon.scenes.windows;

public partial class PauseMenuLayer : CanvasLayer
{
    private WindowContainer PauseMenu => GetNode<WindowContainer>("%PauseMenu");

    private void _on_pause_menu_hidden() => Hide();

    private void OnVisibilityChanged()
    {
        if (Visible)
            PauseMenu.Show();
    }

    public override void _Ready() => VisibilityChanged += OnVisibilityChanged;
}
