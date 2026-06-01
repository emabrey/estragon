using Godot;
using System.Diagnostics;

[GlobalClass]
public partial class AutoFocusingControl : Control
{
    private bool _focusGrabbed = false;

    public void CheckGrab(Control focusedControl)
    {
        Debug.Assert(focusedControl != null, "Don't pass null values; this method expects a Control");
        if (!_focusGrabbed && focusedControl.GetViewport().GuiGetFocusOwner() == null)
        {
            _focusGrabbed = true;
            focusedControl.GrabFocus();
        }
    }
}
