using Godot;

[Tool]
public partial class ScrollingCredits : Control
{
    [Signal] public delegate void EndReachedEventHandler();

    [Export] public float AutoScrollSpeed { get; set; } = 60.0f;
    [Export] public float InputScrollSpeed { get; set; } = 400.0f;
    [Export] public float ScrollRestartDelay { get; set; } = 1.5f;
    [Export] public bool ScrollPaused { get; set; }

    private readonly Timer _timer = new();
    private float _currentScrollPosition;

    protected Control HeaderSpace => GetNode<Control>("%HeaderSpace");
    protected Control FooterSpace => GetNode<Control>("%FooterSpace");
    protected Control CreditsLabel => GetNode<Control>("%CreditsLabel");
    protected ScrollContainer ScrollContainer => GetNode<ScrollContainer>("%ScrollContainer");

    public void SetHeaderAndFooter()
    {
        HeaderSpace.CustomMinimumSize = new Vector2(HeaderSpace.CustomMinimumSize.X, Size.Y);
        FooterSpace.CustomMinimumSize = new Vector2(FooterSpace.CustomMinimumSize.X, Size.Y);
        CreditsLabel.CustomMinimumSize = new Vector2(Size.X, CreditsLabel.CustomMinimumSize.Y);
    }

    private void OnResized()
    {
        SetHeaderAndFooter();
        _currentScrollPosition = ScrollContainer.ScrollVertical;
    }

    protected virtual void EndReached_()
    {
        ScrollPaused = true;
        EmitSignal(SignalName.EndReached);
    }

    public bool IsEndReached()
    {
        float endOfCreditsVertical = CreditsLabel.Size.Y + HeaderSpace.Size.Y;
        return ScrollContainer.ScrollVertical > endOfCreditsVertical;
    }

    private void CheckEndReached()
    {
        if (!IsEndReached())
            return;
        EndReached_();
    }

    private void ScrollTheContainer(float amount)
    {
        if (!Visible || ScrollPaused)
            return;
        _currentScrollPosition += amount;
        ScrollContainer.ScrollVertical = (int)Mathf.Round(_currentScrollPosition);
        CheckEndReached();
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton)
        {
            ScrollPaused = true;
            StartScrollRestartTimer();
        }
        CheckEndReached();
    }

    private void OnScrollStarted()
    {
        ScrollPaused = true;
        StartScrollRestartTimer();
    }

    private void StartScrollRestartTimer() => _timer.Start(ScrollRestartDelay);

    private void OnScrollRestartTimerTimeout()
    {
        _currentScrollPosition = ScrollContainer.ScrollVertical;
        ScrollPaused = false;
    }

    protected virtual void OnVisibilityChanged()
    {
        if (Visible)
        {
            ScrollContainer.ScrollVertical = 0;
            _currentScrollPosition = ScrollContainer.ScrollVertical;
            ScrollPaused = false;
        }
    }

    public override void _Ready()
    {
        ScrollContainer.Connect("scroll_started", Callable.From(OnScrollStarted));
        GuiInput += OnGuiInput;
        Resized += OnResized;
        VisibilityChanged += OnVisibilityChanged;
        _timer.Timeout += OnScrollRestartTimerTimeout;
        SetHeaderAndFooter();
        AddChild(_timer);
        ScrollPaused = false;
    }

    public override void _Process(double delta)
    {
        float inputAxis = Input.GetAxis("ui_up", "ui_down");
        if (inputAxis != 0)
            ScrollTheContainer(inputAxis * InputScrollSpeed * (float)delta);
        else
            ScrollTheContainer(AutoScrollSpeed * (float)delta);
    }

    public override void _ExitTree()
    {
        _currentScrollPosition = ScrollContainer.ScrollVertical;
    }
}
