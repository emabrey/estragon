using Godot;
using System;
using System.Diagnostics;

[GlobalClass]
public partial class ManagedPackedScene : Resource
{
    public enum SceneTransition
    {
        None,
        FadeToBlack,
        FadeToWhite,
        WipeVertical,
        WipeHorizontal
    }

    [Export]
    private PackedScene targetScene = new PackedScene();

    private Node? loadedScene = null;

    public Node? getLoadedScene()
    {
        Debug.Assert(targetScene != null, "Target scene must be configured first");
        Debug.Assert(targetScene.CanInstantiate(), "Target scene must be instantiable");

        return loadedScene ??= targetScene.Instantiate();
    }
}
