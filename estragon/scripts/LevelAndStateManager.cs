using Godot;

public partial class LevelAndStateManager : LevelManager
{
    public override void SetCurrentLevelPath(string value)
    {
        base.SetCurrentLevelPath(value);
        GameState.SetCurrentLevelPath(value);
    }

    public override void SetCheckpointLevelPath(string value)
    {
        base.SetCheckpointLevelPath(value);
        GameState.SetCheckpointLevelPath(value);
    }

    public override string GetCheckpointLevelPath()
    {
        string stateLevelPath = GameState.GetCheckpointLevelPath();
        if (!string.IsNullOrEmpty(stateLevelPath))
            return stateLevelPath;
        return base.GetCheckpointLevelPath();
    }
}
