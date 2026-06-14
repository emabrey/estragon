using Godot;

[Tool]
public partial class ScrollableCredits : Control
{
    private RichTextLabel CreditsLabel => GetNode<RichTextLabel>("%CreditsLabel");

    [Export] public float InputScrollSpeed { get; set; } = 10.0f;

    private float _lineNumber;

    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            CreditsLabel.ScrollToLine(0);
            CreditsLabel.GrabFocus();
        }
    }

    public override void _Ready() => VisibilityChanged += OnVisibilityChanged;

    public override void _Process(double delta)
    {
        if (Engine.IsEditorHint() || !Visible)
            return;
        float inputAxis = Input.GetAxis("ui_up", "ui_down");
        if (Mathf.Abs(inputAxis) > 0.5f)
        {
            _lineNumber += inputAxis * (float)delta * InputScrollSpeed;
            int maxLines = CreditsLabel.GetLineCount() - CreditsLabel.GetVisibleLineCount();
            if (_lineNumber < 0)
                _lineNumber = 0;
            if (_lineNumber > maxLines)
                _lineNumber = maxLines;
            CreditsLabel.ScrollToLine((int)Mathf.Round(_lineNumber));
        }
    }
}
