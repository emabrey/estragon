using Godot;
using System.Diagnostics;

[GlobalClass]
public partial class ManagedPackedScene : Resource
{
    public enum TRANSITION
    {
        NONE,
        FADE,
        FADE_WHITE,
        WIPE_VERTICAL,
        WIPE_HORIZONTAL
    }

    [Export] public PackedScene TargetScene { get; set; } = new PackedScene();
    [Export] public TRANSITION Transition { get; set; } = TRANSITION.NONE;

    private Node? _loadedScene;

    public void Cleanup()
    {
        if (_loadedScene != null && !_loadedScene.IsQueuedForDeletion())
            _loadedScene.QueueFree();
    }

    public Node GetLoadedScene()
    {
        Debug.Assert(TargetScene != null, "Target scene must be configured first");
        Debug.Assert(TargetScene.CanInstantiate(), "Target scene must be instantiable");
        if (_loadedScene == null)
            _loadedScene = TargetScene.Instantiate();
        return _loadedScene;
    }
}
