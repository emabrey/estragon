using Godot;

/// <summary>Script to apply the anti-aliasing setting from PlayerConfig to a SubViewport.</summary>
public partial class ConfigurableSubViewport : SubViewport
{
    [Export] public StringName AntiAliasingKey { get; set; } = "Anti-aliasing";
    [Export] public StringName VideoSection { get; set; } = AppSettings.VideoSection;

    public override void _Ready()
    {
        int antiAliasing = PlayerConfig.GetConfig(VideoSection, AntiAliasingKey, (int)Viewport.Msaa.Disabled).AsInt32();
        Msaa2D = (Viewport.Msaa)antiAliasing;
        Msaa3D = (Viewport.Msaa)antiAliasing;
    }
}
