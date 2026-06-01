using Godot;

[GlobalClass]
public partial class SoundEffectButton : Button
{
    private FadedAudioStreamPlayer _effectMusicPlayer = null!;

    public override void _Ready()
    {
        _effectMusicPlayer = GameSceneManager.GetEffectPlayer(GetTree());
    }

    private void _on_mouse_entered()
    {
        _effectMusicPlayer.Stream = GD.Load<AudioStream>("res://assets/audio/effects/HOVER.mp3");
        _effectMusicPlayer.PlayRandomPitch();
    }

    private void _on_button_down()
    {
        _effectMusicPlayer.Stream = GD.Load<AudioStream>("res://assets/audio/effects/CLICK.mp3");
        _effectMusicPlayer.PlayRandomPitch();
    }
}
