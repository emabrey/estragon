using Godot;

public partial class StartButton : SoundEffectButton
{
    public override void _Process(double delta)
    {
        new AutoFocusingControl().CheckGrab(this);
    }

    private void _on_toggled(bool value)
    {
        GameSceneManager.SwapSceneWithinTree("MainGame", GetTree());
    }
}
