using Godot;
using System.Diagnostics;
using System.Threading.Tasks;

[GlobalClass]
public partial class GameSceneManager : Node2D
{
    [Signal] public delegate void SceneSwappedEventHandler();

    [Export]
    public Godot.Collections.Dictionary<StringName, ManagedPackedScene> Scenes { get; set; } = new()
    {
        { "MainGame", new ManagedPackedScene() },
        { "StartupCreditsStudio", new ManagedPackedScene() }
    };

    private AnimationPlayer _animationPlayer = null!;
    private StringName _currentSceneAlias = "StartupCreditsStudio";

    public static SceneTree SwapSceneWithinTree(StringName sceneAlias, SceneTree tree)
    {
        var manager = tree.CurrentScene.GetNode<GameSceneManager>("%GameSceneManager");
        Debug.Assert(manager != null, "The scene tree does not contain GameSceneManager!");
        _ = manager._SwapScene(sceneAlias);
        return tree;
    }

    public static FadedAudioStreamPlayer GetMusicPlayer(SceneTree tree)
        => tree.CurrentScene.GetNode<FadedAudioStreamPlayer>("%MusicPlayer");

    public static FadedAudioStreamPlayer GetEffectPlayer(SceneTree tree)
        => tree.CurrentScene.GetNode<FadedAudioStreamPlayer>("%EffectPlayer");

    public override async void _Ready()
    {
        _animationPlayer = GetNode<AnimationPlayer>("%TransitionMatte/AnimationPlayer");
        float timeBeforeCredits = 0.35f;
        await ToSignal(GetTree().CreateTimer(timeBeforeCredits), SceneTreeTimer.SignalName.Timeout);
        await _SwapScene(_currentSceneAlias, false);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
            _Cleanup();
    }

    private async Task _HandleTransition(StringName alias, bool reverse = false)
    {
        System.Action<string> animFunc = reverse
            ? name => _animationPlayer.PlayBackwards(name)
            : name => _animationPlayer.Play(name);

        _animationPlayer.Play("RESET");
        var transition = Scenes[alias].Transition;
        switch (transition)
        {
            case ManagedPackedScene.TRANSITION.FADE:
                animFunc("fade_black");
                break;
            case ManagedPackedScene.TRANSITION.FADE_WHITE:
                animFunc("fade_white");
                break;
            case ManagedPackedScene.TRANSITION.WIPE_VERTICAL:
                animFunc("vertical_wipe_in");
                break;
            case ManagedPackedScene.TRANSITION.WIPE_HORIZONTAL:
                animFunc("horizontal_wipe_in");
                break;
            case ManagedPackedScene.TRANSITION.NONE:
                _animationPlayer.Play("RESET");
                return;
            default:
                Debug.Assert(false, "Unknown Transition Alias: " + alias);
                break;
        }
        await ToSignal(_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
    }

    private async Task _SwapScene(StringName newAlias, bool allowTransitions = true)
    {
        Debug.Assert(Scenes.ContainsKey(_currentSceneAlias), "You did not add " + _currentSceneAlias + " to the scene manager");
        Debug.Assert(Scenes.ContainsKey(newAlias), "You did not add " + newAlias + " to the scene manager");

        if (allowTransitions)
        {
            _ = _HandleTransition(_currentSceneAlias, false);
            await ToSignal(_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        }

        var sceneParent = GetNode<Node>("SceneParent");
        foreach (Node child in sceneParent.GetChildren())
            sceneParent.RemoveChild(child);

        sceneParent.AddChild(Scenes[newAlias].GetLoadedScene());
        EmitSignal(SignalName.SceneSwapped);

        if (allowTransitions)
        {
            _ = _HandleTransition(newAlias, true);
            await ToSignal(_animationPlayer, AnimationPlayer.SignalName.AnimationFinished);
        }

        _currentSceneAlias = newAlias;
    }

    private void _Cleanup()
    {
        foreach (ManagedPackedScene scene in Scenes.Values)
            scene.Cleanup();
    }
}
