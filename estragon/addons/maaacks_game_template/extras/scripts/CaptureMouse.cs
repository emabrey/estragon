using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Control node that captures the mouse for games that require it.</summary>
public partial class CaptureMouse : Control
{
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton && Input.MouseMode != Input.MouseModeEnum.Captured)
            Input.MouseMode = Input.MouseModeEnum.Captured;
    }
}
