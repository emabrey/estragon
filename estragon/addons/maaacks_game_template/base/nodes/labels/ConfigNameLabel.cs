using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Displays the value of `application/config/name`, set in project settings.</summary>
[Tool]
public partial class ConfigNameLabel : Label
{
    private const string NoNameString = "Title";

    [Export] public bool AutoUpdate { get; set; } = true;

    public void UpdateNameLabel()
    {
        string configName = ProjectSettings.GetSetting("application/config/name", NoNameString).AsString();
        if (string.IsNullOrEmpty(configName))
            configName = NoNameString;
        Text = configName;
    }

    public override void _Ready()
    {
        if (AutoUpdate)
            UpdateNameLabel();
    }
}
