using Godot;

namespace estragon.addons.maaacks_game_template;

[Tool]
[GlobalClass]
public partial class ConfirmationOverlaidWindow : OverlaidWindow
{
    [Signal] public delegate void ConfirmedEventHandler();

    public Button ConfirmButton => GetNode<Button>("%ConfirmButton");

    [Export]
    public string ConfirmButtonText
    {
        get => _confirmButtonText;
        set
        {
            _confirmButtonText = value;
            if (UpdateContent && IsInsideTree())
                ConfirmButton.Text = _confirmButtonText;
        }
    }
    private string _confirmButtonText = "Confirm";

    public void Confirm()
    {
        EmitSignal(SignalName.Confirmed);
        Close();
    }

    private void _on_confirm_button_pressed() => Confirm();
}
