using Godot;

/// <summary>Node for opening a pause menu when detecting a 'ui_cancel' event.</summary>
public partial class PauseMenuController : Node
{
    [Export] public PackedScene PauseMenuPacked { get; set; } = null!;
    [Export] public Viewport FocusedViewport { get; set; } = null!;

    private Node _pauseMenu = null!;

    public async void Pause()
    {
        if (_pauseMenu.Get("visible").AsBool())
            return;
        FocusedViewport ??= GetViewport();
        var initialFocusControl = FocusedViewport.GuiGetFocusOwner();
        _pauseMenu.Call("show");
        if (_pauseMenu is CanvasLayer)
            await ToSignal(_pauseMenu, "visibility_changed");
        else
            await ToSignal(_pauseMenu, "hidden");
        if (IsInsideTree() && initialFocusControl != null)
            initialFocusControl.GrabFocus();
    }

    // If pause menu should take precedence, override _Input() instead.
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
            Pause();
    }

    public override void _Ready()
    {
        _pauseMenu = PauseMenuPacked.Instantiate();
        _pauseMenu.Set("visible", false);
        GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, _pauseMenu);
    }
}
