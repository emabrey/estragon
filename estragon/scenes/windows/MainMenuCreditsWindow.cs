using Godot;
using estragon.addons.maaacks_game_template;

namespace estragon.scenes.windows;

[Tool]
public partial class MainMenuCreditsWindow : OverlaidWindowContainer
{
    public override void _Ready()
    {
        base._Ready();
        if (Instance != null && Instance.HasSignal("end_reached"))
            Instance.Connect("end_reached", Callable.From(Close));
    }
}
