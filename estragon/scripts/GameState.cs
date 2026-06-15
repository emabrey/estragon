using Godot;

namespace estragon.scripts;

[GlobalClass]
public partial class GameState : Resource
{
    public const string StateName = "GameState";

    [Export] public Godot.Collections.Dictionary LevelStates { get; set; } = new();
    [Export] public string CurrentLevelPath { get; set; } = "";
    [Export] public string CheckpointLevelPath { get; set; } = "";
    [Export] public int TotalGamesPlayed { get; set; }
    [Export] public int PlayTime { get; set; }
    [Export] public int TotalTime { get; set; }

    public static LevelState? GetLevelState(string levelStateKey)
    {
        if (!HasGameState())
            return null;
        var gameState = GetOrCreateState()!;
        if (string.IsNullOrEmpty(levelStateKey))
            return null;
        if (gameState.LevelStates.ContainsKey(levelStateKey))
            return gameState.LevelStates[levelStateKey].As<LevelState>();
        var newLevelState = new LevelState();
        gameState.LevelStates[levelStateKey] = newLevelState;
        GlobalState.Save();
        return newLevelState;
    }

    public static bool HasGameState() => GlobalState.HasState(StateName);

    public static GameState? GetOrCreateState() => GlobalState.GetOrCreateState<GameState>(StateName);


    public static string GetCurrentLevelPath()
    {
        if (!HasGameState())
            return "";
        return GetOrCreateState()!.CurrentLevelPath;
    }

    public static string GetCheckpointLevelPath()
    {
        if (!HasGameState())
            return "";
        return GetOrCreateState()!.CheckpointLevelPath;
    }

    public static int GetLevelsReached()
    {
        if (!HasGameState())
            return 0;
        return GetOrCreateState()!.LevelStates.Count;
    }

    public static void SetCheckpointLevelPath(string levelPath)
    {
        var gameState = GetOrCreateState()!;
        gameState.CheckpointLevelPath = levelPath;
        GetLevelState(levelPath);
        GlobalState.Save();
    }

    public static void SetCurrentLevelPath(string levelPath)
    {
        var gameState = GetOrCreateState()!;
        gameState.CurrentLevelPath = levelPath;
        GlobalState.Save();
    }

    public static void StartGame()
    {
        var gameState = GetOrCreateState()!;
        gameState.TotalGamesPlayed += 1;
        GlobalState.Save();
    }

    public static void ContinueGame()
    {
        var gameState = GetOrCreateState()!;
        gameState.CurrentLevelPath = gameState.CheckpointLevelPath;
        GlobalState.Save();
    }

    public static void Reset()
    {
        var gameState = GetOrCreateState()!;
        gameState.LevelStates.Clear();
        gameState.CurrentLevelPath = "";
        gameState.CheckpointLevelPath = "";
        gameState.PlayTime = 0;
        gameState.TotalTime = 0;
        GlobalState.Save();
    }
}
