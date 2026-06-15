using Godot;

namespace estragon.addons.maaacks_game_template;

[Tool]
[GlobalClass]
public partial class WindowContainer : PanelContainer
{
    [Signal] public delegate void ClosedEventHandler();
    [Signal] public delegate void OpenedEventHandler();

    [Export] public bool UiCancelCloses { get; set; } = true;

    [ExportGroup("Content")]
    [Export] public bool UpdateContent { get; set; }

    [Export(PropertyHint.MultilineText)]
    public string Text
    {
        get => _text;
        set
        {
            _text = value;
            if (UpdateContent && IsInsideTree())
                DescriptionLabel.Text = _text;
        }
    }
    private string _text = "";

    [Export]
    public string CloseButtonText
    {
        get => _closeButtonText;
        set
        {
            _closeButtonText = value;
            if (UpdateContent && IsInsideTree())
                CloseButton.Text = _closeButtonText;
        }
    }
    private string _closeButtonText = "Close";

    [ExportSubgroup("Title")]
    [Export]
    public string Title
    {
        get => _title;
        set
        {
            _title = value;
            if (UpdateContent && IsInsideTree())
                TitleLabel.Text = _title;
        }
    }
    private string _title = "Menu";

    [Export(PropertyHint.Range, "0,1000,1")]
    public int TitleFontSize
    {
        get => _titleFontSize;
        set
        {
            _titleFontSize = value;
            if (UpdateContent && IsInsideTree())
                TitleLabel.Set("theme_override_font_sizes/font_size", _titleFontSize);
        }
    }
    private int _titleFontSize = 16;

    [Export]
    public bool TitleVisible
    {
        get => _titleVisible;
        set
        {
            _titleVisible = value;
            if (UpdateContent && IsInsideTree())
                TitleMargin.Visible = _titleVisible;
        }
    }
    private bool _titleVisible = true;

    protected Container ContentContainer => GetNode<Container>("%ContentContainer");
    protected Label TitleLabel => GetNode<Label>("%TitleLabel");
    protected MarginContainer TitleMargin => GetNode<MarginContainer>("%TitleMargin");
    protected RichTextLabel DescriptionLabel => GetNode<RichTextLabel>("%DescriptionLabel");
    protected Button CloseButton => GetNode<Button>("%CloseButton");
    protected BoxContainer MenuButtons => GetNode<BoxContainer>("%MenuButtons");

    public override void _Ready()
    {
        UpdateContent = UpdateContent;
        Text = _text;
        CloseButtonText = _closeButtonText;
        Title = _title;
        TitleFontSize = _titleFontSize;
        TitleVisible = _titleVisible;
    }

    public virtual void Close()
    {
        if (!Visible)
            return;
        Hide();
        EmitSignal(SignalName.Closed);
    }

    protected virtual void HandleCancelInput() => Close();

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Visible && @event.IsActionReleased("ui_cancel") && UiCancelCloses)
        {
            HandleCancelInput();
            GetViewport().SetInputAsHandled();
        }
    }

    private void _on_close_button_pressed() => Close();

    public new virtual void Show()
    {
        base.Show();
        EmitSignal(SignalName.Opened);
    }

    public override void _ExitTree()
    {
        if (Engine.IsEditorHint())
            return;
        Close();
    }
}
