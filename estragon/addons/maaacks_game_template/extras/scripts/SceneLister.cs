using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Helper class for listing all the scenes in a directory.</summary>
[Tool]
[GlobalClass]
public partial class SceneLister : Node
{
    [Export] public Godot.Collections.Array<string> Files { get; set; } = new();

    [Export]
    public string Directory
    {
        get => _directory;
        set
        {
            _directory = value;
            RefreshFiles();
        }
    }
    private string _directory = "";

    private void RefreshFiles()
    {
        if (!IsInsideTree() || string.IsNullOrEmpty(_directory))
            return;
        var dirAccess = DirAccess.Open(_directory);
        if (dirAccess != null)
        {
            Files.Clear();
            foreach (string file in dirAccess.GetFiles())
            {
                if (!file.EndsWith(".tscn"))
                    continue;
                Files.Add(_directory + "/" + file);
            }
        }
    }
}
