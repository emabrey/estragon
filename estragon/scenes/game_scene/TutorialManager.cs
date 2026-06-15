using Godot;

namespace estragon.scenes.game_scene;

/// <summary>A script to add into a level or game scene to display tutorial windows.</summary>
public partial class TutorialManager : Node
{
    [Export] public Godot.Collections.Array<PackedScene> TutorialScenes { get; set; } = new();
    [Export] public bool AutoOpen { get; set; }
    [Export] public float AutoOpenDelay { get; set; } = 0.25f;

    public async void OpenTutorials()
    {
        var initialFocusControl = GetViewport().GuiGetFocusOwner();
        foreach (PackedScene tutorialScene in TutorialScenes)
        {
            var tutorialMenu = tutorialScene.Instantiate() as Control;
            if (tutorialMenu == null)
            {
                GD.PushWarning($"tutorial failed to open {tutorialScene}");
                return;
            }
            GetTree().CurrentScene.CallDeferred(Node.MethodName.AddChild, tutorialMenu);
            if (tutorialMenu.HasSignal("closed"))
                await ToSignal(tutorialMenu, "closed");
            else
                await ToSignal(tutorialMenu, Node.SignalName.TreeExited);
            if (IsInsideTree() && initialFocusControl != null)
                initialFocusControl.GrabFocus();
        }
    }

    public override async void _Ready()
    {
        if (AutoOpen)
        {
            if (AutoOpenDelay > 0.0f)
                await ToSignal(GetTree().CreateTimer(AutoOpenDelay, false), SceneTreeTimer.SignalName.Timeout);
            Callable.From(OpenTutorials).CallDeferred();
        }
    }
}
