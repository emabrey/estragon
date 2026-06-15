using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Node that captures UI focus when switching menus.</summary>
public partial class CaptureFocus : Control
{
    [Export] public int SearchDepth { get; set; } = 1;
    [Export] public bool Enabled { get; set; }
    [Export] public bool NullFocusEnabled { get; set; } = true;
    [Export] public bool JoypadEnabled { get; set; } = true;
    [Export] public bool MouseHiddenEnabled { get; set; } = true;

    [Export]
    public bool Lock
    {
        get => _lock;
        set
        {
            bool valueChanged = _lock != value;
            _lock = value;
            if (valueChanged && !_lock)
                UpdateFocus();
        }
    }
    private bool _lock;

    private bool FocusFirstSearch(Control controlNode, int levels = 1)
    {
        if (controlNode == null || !controlNode.IsVisibleInTree())
            return false;
        if (controlNode.FocusMode == FocusModeEnum.All)
        {
            controlNode.GrabFocus();
            if (controlNode is ItemList itemList)
                itemList.Select(0);
            return true;
        }
        if (levels < 1)
            return false;
        foreach (Node child in controlNode.GetChildren())
        {
            if (child is Control childControl && FocusFirstSearch(childControl, levels - 1))
                return true;
        }
        return false;
    }

    public void FocusFirst() => FocusFirstSearch(this, SearchDepth);

    public void UpdateFocus()
    {
        if (_lock)
            return;
        if (IsVisibleAndShouldCapture())
            FocusFirst();
    }

    private bool ShouldCaptureFocus()
    {
        return Enabled
            || (GetViewport().GuiGetFocusOwner() == null && NullFocusEnabled)
            || (Input.GetConnectedJoypads().Count > 0 && JoypadEnabled)
            || (Input.MouseMode != Input.MouseModeEnum.Visible && Input.MouseMode != Input.MouseModeEnum.Confined && MouseHiddenEnabled);
    }

    private bool IsVisibleAndShouldCapture() => IsVisibleInTree() && ShouldCaptureFocus();

    private void OnVisibilityChanged() => CallDeferred(MethodName.UpdateFocus);

    public override void _Ready()
    {
        if (IsInsideTree())
        {
            UpdateFocus();
            VisibilityChanged += OnVisibilityChanged;
        }
    }
}
