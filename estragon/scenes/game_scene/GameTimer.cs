using Godot;

public partial class GameTimer : Node
{
    private int _playTime;
    private int _totalTime;

    private void AddTimers()
    {
        var playTimer = new Timer { OneShot = false, ProcessMode = ProcessModeEnum.Pausable };
        playTimer.Timeout += () => _playTime += 1;
        AddChild(playTimer);
        playTimer.Start(1);

        var totalTimer = new Timer { OneShot = false, ProcessMode = ProcessModeEnum.Always };
        totalTimer.Timeout += () => _totalTime += 1;
        AddChild(totalTimer);
        totalTimer.Start(1);
    }

    public override void _EnterTree() => AddTimers();

    public override void _ExitTree()
    {
        var gameState = GameState.GetOrCreateState();
        gameState.PlayTime += _playTime;
        gameState.TotalTime += _totalTime;
        GlobalState.Save();
    }
}
