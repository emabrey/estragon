using Godot;

public partial class SettingsButton : SoundEffectButton
{
    private void _on_toggled(bool value)
    {
        GameSceneManager.SwapSceneWithinTree("Settings", GetTree());
    }
}
