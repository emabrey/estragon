using Godot;

public partial class CreditsButton : SoundEffectButton
{
    private void _on_toggled(bool value)
    {
        GameSceneManager.SwapSceneWithinTree("StartupCreditsStudio", GetTree());
    }
}
