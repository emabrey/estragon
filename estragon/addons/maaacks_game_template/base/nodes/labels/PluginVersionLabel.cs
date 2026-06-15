using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Displays the value of `version` from the config file of the specified plugin.</summary>
[Tool]
public partial class PluginVersionLabel : Label
{
    private const string NoVersionString = "0.0.0";

    [Export] public string PluginDirectory { get; set; } = "";
    [Export] public string VersionPrefix { get; set; } = "v";

    private string GetPluginVersion()
    {
        if (!string.IsNullOrEmpty(PluginDirectory))
        {
            foreach (string enabledPlugin in ProjectSettings.GetSetting("editor_plugins/enabled").AsStringArray())
            {
                if (enabledPlugin.Contains(PluginDirectory))
                {
                    var config = new ConfigFile();
                    Error error = config.Load(enabledPlugin);
                    if (error != Error.Ok)
                        break;
                    return config.GetValue("plugin", "version", NoVersionString).AsString();
                }
            }
        }
        return "";
    }

    public void UpdateVersionLabel()
    {
        string pluginVersion = GetPluginVersion();
        if (string.IsNullOrEmpty(pluginVersion))
            pluginVersion = NoVersionString;
        Text = VersionPrefix + pluginVersion;
    }

    public override void _Ready() => UpdateVersionLabel();
}
