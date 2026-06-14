using Godot;

public partial class VideoOptionsMenu : Control
{
    private OptionControl ResolutionControl => GetNode<OptionControl>("%ResolutionControl");
    private OptionControl FullscreenControl => GetNode<OptionControl>("%FullscreenControl");
    private OptionControl VSyncControl => GetNode<OptionControl>("%VSyncControl");

    private void PreselectResolution(Window window)
        => ResolutionControl.Set("value", window.Size);

    private void UpdateResolutionOptionsEnabled(Window window)
    {
        var resolutionControl = ResolutionControl;
        if (OS.HasFeature("web"))
        {
            resolutionControl.Editable = false;
            resolutionControl.TooltipText = "Disabled for web";
        }
        else if (AppSettings.IsFullscreen(window))
        {
            resolutionControl.Editable = false;
            resolutionControl.TooltipText = "Disabled for fullscreen";
        }
        else
        {
            resolutionControl.Editable = true;
            resolutionControl.TooltipText = "Select a screen size";
        }
    }

    private void UpdateUi(Window window)
    {
        FullscreenControl.Set("value", AppSettings.IsFullscreen(window));
        PreselectResolution(window);
        VSyncControl.Set("value", (int)AppSettings.GetVsync(window));
        UpdateResolutionOptionsEnabled(window);
    }

    public override void _Ready()
    {
        Window window = GetWindow();
        UpdateUi(window);
        window.SizeChanged += () => PreselectResolution(window);
    }

    private void _on_fullscreen_control_setting_changed(Variant value)
    {
        Window window = GetWindow();
        AppSettings.SetFullscreenEnabled(value.AsBool(), window);
        UpdateResolutionOptionsEnabled(window);
    }

    private void _on_resolution_control_setting_changed(Variant value)
        => AppSettings.SetResolution(value.AsVector2I(), GetWindow(), false);

    private void _on_v_sync_control_setting_changed(Variant value)
        => AppSettings.SetVsync((DisplayServer.VSyncMode)value.AsInt32(), GetWindow());
}
