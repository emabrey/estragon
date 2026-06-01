using Godot;

public partial class StartupCreditStudio : Panel
{
    private void _on_tree_entered()
    {
        float timeShown = 3.0f;

        if (OS.IsDebugBuild())
            timeShown = 3.0f;

        GetTree().CreateTimer(timeShown).Timeout += SwapNextScene;
    }

    private void SwapNextScene()
    {
        GameSceneManager.SwapSceneWithinTree("MainMenu", GetTree());
    }
}
