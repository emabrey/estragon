using Godot;

[GlobalClass]
public partial class LevelState : Resource
{
    [Export] public Color Color { get; set; }
    [Export] public bool TutorialRead { get; set; }
}
