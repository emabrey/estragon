using Godot;

namespace estragon.addons.maaacks_game_template;

public partial class MiniOptionsMenu : Control
{
    private Control MuteControl => GetNode<Control>("%MuteControl");
    private Control FullscreenControl => GetNode<Control>("%FullscreenControl");

    /// <summary>Scene for adjusting the volume of the audio busses.</summary>
    [Export] public PackedScene AudioControlScene { get; set; } = null!;
    /// <summary>Optional names of audio busses that should be ignored.</summary>
    [Export] public Godot.Collections.Array<string> HideBusses { get; set; } = new();

    private void OnBusChanged(float busValue, int busIter)
        => AppSettings.SetBusVolume(busIter, busValue);

    private void AddAudioControl(string busName, float busValue, int busIter)
    {
        if (AudioControlScene == null || HideBusses.Contains(busName) || busName.StartsWith(AppSettings.SystemBusNamePrefix))
            return;
        var audioControl = AudioControlScene.Instantiate();
        GetNode("%AudioControlContainer").CallDeferred(Node.MethodName.AddChild, audioControl);
        if (audioControl is OptionControl optionControl)
        {
            optionControl.OptionSection = OptionControl.OptionSections.Audio;
            optionControl.OptionName = busName;
            optionControl.Set("value", busValue);
            optionControl.Connect(OptionControl.SignalName.SettingChanged,
                Callable.From<Variant>(v => OnBusChanged(v.AsSingle(), busIter)));
        }
    }

    private void AddAudioBusControls()
    {
        for (int busIter = 0; busIter < AudioServer.BusCount; busIter++)
        {
            string busName = AppSettings.GetAudioBusName(busIter);
            float linear = AppSettings.GetBusVolume(busIter);
            AddAudioControl(busName, linear, busIter);
        }
    }

    private void UpdateUi()
    {
        AddAudioBusControls();
        MuteControl.Set("value", AppSettings.IsMuted());
        FullscreenControl.Set("value", AppSettings.IsFullscreen(GetWindow()));
    }

    private void SyncWithConfig() => UpdateUi();

    public override void _Ready() => SyncWithConfig();

    private void _on_mute_control_setting_changed(bool value)
        => AppSettings.SetMute(value);

    private void _on_fullscreen_control_setting_changed(bool value)
        => AppSettings.SetFullscreenEnabled(value, GetWindow());
}
