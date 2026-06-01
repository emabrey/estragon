using Godot;

public partial class GameVersion : Label
{
    private void _on_tree_entered()
    {
        Text = (string)ProjectSettings.GetSetting("application/config/version");
    }
}
