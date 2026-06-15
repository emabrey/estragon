using Godot;
using estragon.addons.maaacks_game_template;

namespace estragon.scenes.game_scene;

public partial class InputDisplayLabel : Label
{
    private Godot.Collections.Array<StringName> _actionNames = null!;

    public override void _Ready() => _actionNames = AppSettings.GetActionNames();

    private string GetInputsAsString()
    {
        string allInputs = "";
        bool isFirst = true;
        foreach (StringName actionName in _actionNames)
        {
            if (Input.IsActionPressed(actionName))
            {
                if (isFirst)
                {
                    isFirst = false;
                    allInputs += actionName.ToString();
                }
                else
                {
                    allInputs += " + " + actionName;
                }
            }
        }
        return allInputs;
    }

    public override void _Process(double delta)
    {
        if (Input.IsAnythingPressed())
            Text = GetInputsAsString();
        else
            Text = "";
    }
}
