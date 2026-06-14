using Godot;

public partial class LevelSelectMenu : Control
{
    [Signal] public delegate void LevelSelectedEventHandler();

    private ItemList LevelButtonsContainer => GetNode<ItemList>("%LevelButtonsContainer");

    private readonly Godot.Collections.Array<string> _levelPaths = new();

    public override void _Ready() => AddLevelsToContainer();

    /// <summary>A fresh level list is propagated into the ItemList, and the file names are cleaned.</summary>
    public void AddLevelsToContainer()
    {
        LevelButtonsContainer.Clear();
        _levelPaths.Clear();
        var gameState = GameState.GetOrCreateState();
        foreach (Variant key in gameState.LevelStates.Keys)
        {
            string filePath = key.AsString();
            string fileName = filePath.GetFile();        // e.g., "level_1.tscn"
            fileName = fileName.TrimSuffix(".tscn");      // Remove the ".tscn" extension
            fileName = fileName.Replace("_", " ");        // Replace underscores with spaces
            fileName = fileName.Capitalize();             // Convert to proper case
            LevelButtonsContainer.AddItem(fileName);
            _levelPaths.Add(filePath);
        }
    }

    private void _on_level_buttons_container_item_activated(int index)
    {
        GameState.SetCheckpointLevelPath(_levelPaths[index]);
        EmitSignal(SignalName.LevelSelected);
    }
}
