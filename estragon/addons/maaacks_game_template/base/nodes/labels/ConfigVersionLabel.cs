using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Displays the value of `application/config/version`, set in project settings.</summary>
[Tool]
public partial class ConfigVersionLabel : Label
{
    private const string NoVersionString = "0.0.0";

    [Export] public string VersionPrefix { get; set; } = "v";

    public void UpdateVersionLabel()
    {
        string configVersion = ProjectSettings.GetSetting("application/config/version", NoVersionString).AsString();
        if (string.IsNullOrEmpty(configVersion))
            configVersion = NoVersionString;
        Text = VersionPrefix + configVersion;
    }

    public override void _Ready() => UpdateVersionLabel();
}
