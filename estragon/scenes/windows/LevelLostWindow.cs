using Godot;
using estragon.addons.maaacks_game_template;

namespace estragon.scenes.windows;

[Tool]
public partial class LevelLostWindow : OverlaidWindow
{
    [Signal] public delegate void RestartPressedEventHandler();
    [Signal] public delegate void MainMenuPressedEventHandler();

    public override void _Ready()
    {
        if (OS.HasFeature("web"))
            GetNode<Control>("%ExitButton").Hide();
    }

    private void _on_exit_button_pressed() => GetNode<WindowContainer>("%ExitConfirmation").Show();

    private void _on_main_menu_button_pressed() => GetNode<WindowContainer>("%MainMenuConfirmation").Show();

    private void _on_close_button_pressed()
    {
        EmitSignal(SignalName.RestartPressed);
        Close();
    }

    private void _on_main_menu_confirmation_confirmed()
    {
        EmitSignal(SignalName.MainMenuPressed);
        Close();
    }

    private void _on_exit_confirmation_confirmed() => GetTree().Quit();
}
