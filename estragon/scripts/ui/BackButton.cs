using Godot;

[GlobalClass]
public partial class BackButton : SoundEffectButton
{
    private void _on_pressed()
    {
        GameSceneManager.SwapSceneWithinTree("MainMenu", GetTree());
    }
}
