using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Helper class for listing all the scenes in a directory.</summary>
[Tool]
[GlobalClass]
public partial class FileLister : Node
{
    [Export]
    public bool RefreshFilesAction
    {
        get => false;
        set
        {
            if (value && Engine.IsEditorHint())
                RefreshFiles();
        }
    }

    [Export] public Godot.Collections.Array<string> Files { get; set; } = new();

    [Export]
    public Godot.Collections.Array<string> Directories
    {
        get => _directories;
        set
        {
            _directories = value;
            RefreshFiles();
        }
    }
    private Godot.Collections.Array<string> _directories = new();

    [ExportGroup("Constraints")]
    [Export] public string Search { get; set; } = "";
    [Export] public string Filter { get; set; } = "";
    [ExportSubgroup("Advanced Search")]
    [Export] public string BeginsWith { get; set; } = "";
    [Export] public string EndsWith { get; set; } = "";
    [Export] public string NotBeginsWith { get; set; } = "";
    [Export] public string NotEndsWith { get; set; } = "";

    protected void RefreshFiles()
    {
        if (!IsInsideTree())
            return;
        Files.Clear();
        foreach (string directory in _directories)
        {
            var dirAccess = DirAccess.Open(directory);
            if (dirAccess != null)
            {
                foreach (string file in dirAccess.GetFiles())
                {
                    if (!string.IsNullOrEmpty(Search) && !file.Contains(Search))
                        continue;
                    if (!string.IsNullOrEmpty(Filter) && file.Contains(Filter))
                        continue;
                    if (!string.IsNullOrEmpty(BeginsWith) && !file.StartsWith(BeginsWith))
                        continue;
                    if (!string.IsNullOrEmpty(EndsWith) && !file.EndsWith(EndsWith))
                        continue;
                    if (!string.IsNullOrEmpty(NotBeginsWith) && file.StartsWith(NotBeginsWith))
                        continue;
                    if (!string.IsNullOrEmpty(NotEndsWith) && file.EndsWith(NotEndsWith))
                        continue;
                    Files.Add(directory + "/" + file);
                }
            }
        }
    }
}
