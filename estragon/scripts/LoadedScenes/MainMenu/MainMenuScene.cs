using Godot;

public partial class MainMenuScene : Node2D
{
    private void _on_tree_entered()
    {
        var musicPlayer = GameSceneManager.GetMusicPlayer(GetTree());
        if (!musicPlayer.Playing)
        {
            musicPlayer.Stream = GD.Load<AudioStream>("res://assets/audio/music/main-menu.mp3");
            musicPlayer.PlayFadedLoop();
        }
    }
}
