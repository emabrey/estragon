using Godot;

[Tool]
[GlobalClass]
public partial class OverlaidWindowContainer : OverlaidWindow
{
    public Node Instance { get; set; }

    protected Container SceneContainer => GetNode<Container>("%SceneContainer");

    [Export]
    public PackedScene PackedScene
    {
        get => _packedScene;
        set
        {
            _packedScene = value;
            if (IsInsideTree())
            {
                foreach (Node child in SceneContainer.GetChildren())
                    child.QueueFree();
                if (_packedScene != null)
                {
                    Instance = _packedScene.Instantiate();
                    SceneContainer.AddChild(Instance);
                }
            }
        }
    }
    private PackedScene _packedScene;

    public override void _Ready()
    {
        PackedScene = _packedScene;
    }
}
