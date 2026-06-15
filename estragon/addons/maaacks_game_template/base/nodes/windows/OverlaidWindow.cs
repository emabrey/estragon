using Godot;

namespace estragon.addons.maaacks_game_template;

[Tool]
[GlobalClass]
public partial class OverlaidWindow : WindowContainer
{
    [Export]
    public bool PausesGame
    {
        get => _pausesGame;
        set
        {
            _pausesGame = value;
            ProcessMode = _pausesGame ? ProcessModeEnum.Always : ProcessModeEnum.Inherit;
        }
    }
    private bool _pausesGame;

    [Export] public bool MakesMouseVisible { get; set; } = true;
    [Export] public bool Exclusive { get; set; } = true;
    [Export] public Color ExclusiveBackgroundColor { get; set; }

    private bool _initialPauseState;
    private Input.MouseModeEnum _initialMouseMode;
    private Control? _initialFocusControl;
    private readonly Godot.Collections.Dictionary _initialNodeFocusModes = new();
    protected SceneTree _sceneTree = null!;
    private ColorRect? _exclusiveControlNode;
    private bool _visibilityConnected;

    private void SetFocusNone(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child == this)
                continue;
            if (child is Control control)
            {
                _initialNodeFocusModes[control] = (int)control.FocusMode;
                control.FocusMode = FocusModeEnum.None;
            }
            SetFocusNone(child);
        }
    }

    private void SetFocusInitial()
    {
        foreach (Variant key in _initialNodeFocusModes.Keys)
        {
            var node = key.As<Control>();
            if (GodotObject.IsInstanceValid(node))
                node.FocusMode = (FocusModeEnum)_initialNodeFocusModes[key].AsInt32();
        }
        _initialNodeFocusModes.Clear();
    }

    public override void Close()
    {
        if (!Visible)
            return;
        _sceneTree.Paused = _initialPauseState;
        Input.MouseMode = _initialMouseMode;
        SetFocusInitial();
        if (GodotObject.IsInstanceValid(_initialFocusControl) && _initialFocusControl.IsInsideTree())
            _initialFocusControl.GrabFocus();
        _exclusiveControlNode?.QueueFree();
        base.Close();
    }

    private async void OverlaidWindowSetup()
    {
        if (_sceneTree != null)
            _initialPauseState = _sceneTree.Paused;
        _initialMouseMode = Input.MouseMode;
        _initialFocusControl = GetViewport().GuiGetFocusOwner();
        _initialFocusControl?.ReleaseFocus();
        if (Engine.IsEditorHint())
            return;
        _sceneTree!.Paused = PausesGame || _initialPauseState;
        if (MakesMouseVisible)
            Input.MouseMode = Input.MouseModeEnum.Visible;
        if (Exclusive)
        {
            SetFocusNone(GetTree().CurrentScene);
            _exclusiveControlNode = new ColorRect
            {
                Name = Name + "ExclusiveControl",
                Color = ExclusiveBackgroundColor
            };
            _exclusiveControlNode.SetAnchorsPreset(LayoutPreset.FullRect);
            CallDeferred(Node.MethodName.AddSibling, _exclusiveControlNode, false);
            await ToSignal(_exclusiveControlNode, CanvasItem.SignalName.Draw);
            GetParent().MoveChild(_exclusiveControlNode, GetIndex());
        }
    }

    protected virtual void OnVisibilityChanged()
    {
        if (IsVisibleInTree())
            OverlaidWindowSetup();
    }

    public override void _EnterTree()
    {
        _sceneTree = GetTree();
        if (!_visibilityConnected)
        {
            VisibilityChanged += OnVisibilityChanged;
            _visibilityConnected = true;
        }
        OnVisibilityChanged();
    }
}
