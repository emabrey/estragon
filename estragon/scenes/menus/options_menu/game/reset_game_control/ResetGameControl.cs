using Godot;

public partial class ResetGameControl : HBoxContainer
{
    private const string ResetString = "Reset Game:";
    private const string ConfirmString = "Confirm Reset:";

    [Signal] public delegate void ResetConfirmedEventHandler();

    private void _on_cancel_button_pressed()
    {
        GetNode<Control>("%CancelButton").Hide();
        GetNode<Control>("%ConfirmButton").Hide();
        GetNode<Control>("%ResetButton").Show();
        GetNode<Control>("%ResetButton").GrabFocus();
        GetNode<Label>("%ResetLabel").Text = ResetString;
    }

    private void _on_reset_button_pressed()
    {
        GetNode<Control>("%CancelButton").Show();
        GetNode<Control>("%ConfirmButton").Show();
        GetNode<Control>("%CancelButton").GrabFocus();
        GetNode<Control>("%ResetButton").Hide();
        GetNode<Label>("%ResetLabel").Text = ConfirmString;
    }

    private void _on_confirm_button_pressed()
    {
        EmitSignal(SignalName.ResetConfirmed);
        GetTree().Paused = false;
        SceneLoader.Instance.ReloadCurrentScene();
    }
}
