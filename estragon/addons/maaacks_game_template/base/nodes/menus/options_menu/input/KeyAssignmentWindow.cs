using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Scene to confirm a new input for an action name.</summary>
[Tool]
public partial class KeyAssignmentWindow : ConfirmationOverlaidWindow
{
    private const string ListeningText = "Listening for input...";
    private const string FocusHereText = "Focus here to assign inputs.";
    private const string ConfirmInputText = "Press again to confirm...";
    private const string NoInputText = "None";

    public enum InputConfirmation { Single, Double, OkButton }

    [Export] public InputConfirmation Confirmation { get; set; } = InputConfirmation.Single;

    public InputEvent? LastInputEvent { get; private set; }
    public string? LastInputText { get; private set; }
    private bool _listening;
    private bool _confirming;

    private Label InputLabel => GetNode<Label>("%InputLabel");
    private TextEdit InputTextEdit => GetNode<TextEdit>("%InputTextEdit");
    private Timer DelayTimer => GetNode<Timer>("%DelayTimer");

    private void RecordInputEvent(InputEvent @event)
    {
        LastInputText = InputEventHelper.GetText(@event);
        if (string.IsNullOrEmpty(LastInputText))
            return;
        LastInputEvent = @event;
        InputLabel.Text = LastInputText;
        ConfirmButton.Disabled = false;
    }

    private bool IsRecordableInput(InputEvent @event)
    {
        return @event != null
            && (@event is InputEventKey
                || @event is InputEventMouseButton
                || @event is InputEventJoypadButton
                || (@event is InputEventJoypadMotion motion && Mathf.Abs(motion.AxisValue) > 0.5f))
            && @event.IsPressed();
    }

    private void StartListening()
    {
        InputTextEdit.PlaceholderText = ListeningText;
        _listening = true;
        DelayTimer.Start();
    }

    private void StopListening()
    {
        InputTextEdit.PlaceholderText = FocusHereText;
        _listening = false;
        _confirming = false;
    }

    private void _on_input_text_edit_focus_entered() => Callable.From(StartListening).CallDeferred();

    private void _on_input_text_edit_focus_exited() => StopListening();

    private void FocusOnOk() => ConfirmButton.GrabFocus();

    public override void _Ready()
    {
        ConfirmButton.FocusNeighborTop = "../../../BodyMargin/VBoxContainer/InputTextEdit";
        CloseButton.FocusNeighborTop = "../../../BodyMargin/VBoxContainer/InputTextEdit";
        base._Ready();
    }

    private bool InputMatchesLast(InputEvent @event)
        => LastInputText == InputEventHelper.GetText(@event);

    private bool IsMouseInput(InputEvent @event) => @event is InputEventMouse;

    private bool InputConfirmsChoice(InputEvent @event)
        => _confirming && !IsMouseInput(@event) && InputMatchesLast(@event);

    private bool ShouldProcessInputEvent(InputEvent @event)
        => _listening && IsRecordableInput(@event) && DelayTimer.IsStopped();

    private bool ShouldConfirmInputEvent(InputEvent @event) => !IsMouseInput(@event);

    private void ConfirmChoice()
    {
        EmitSignal(SignalName.Confirmed);
        Close();
    }

    private void ProcessInputEvent(InputEvent @event)
    {
        if (!ShouldProcessInputEvent(@event))
            return;
        if (InputConfirmsChoice(@event))
        {
            _confirming = false;
            if (Confirmation == InputConfirmation.Double)
                ConfirmChoice();
            else
                Callable.From(FocusOnOk).CallDeferred();
            return;
        }
        RecordInputEvent(@event);
        if (Confirmation == InputConfirmation.Single)
            ConfirmChoice();
        if (ShouldConfirmInputEvent(@event))
        {
            _confirming = true;
            DelayTimer.Start();
            InputTextEdit.PlaceholderText = ConfirmInputText;
        }
    }

    private void _on_input_text_edit_gui_input(InputEvent @event)
    {
        InputTextEdit.SetDeferred("text", "");
        ProcessInputEvent(@event);
    }

    protected override void OnVisibilityChanged()
    {
        base.OnVisibilityChanged();
        if (Visible)
        {
            if (!string.IsNullOrEmpty(Text.Trim()))
                InputLabel.Text = Text;
            else
                InputLabel.Text = NoInputText;
            InputTextEdit.GrabFocus();
        }
    }
}
