using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Scene for adjusting the volume of the audio busses.</summary>
public partial class AudioOptionsMenu : Control
{
    [Export] public PackedScene AudioControlScene { get; set; } = null!;
    [Export] public Godot.Collections.Array<string> HideBusses { get; set; } = new();

    private Control MuteControl => GetNode<Control>("%MuteControl");

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
    }

    public override void _Ready() => UpdateUi();

    private void _on_mute_control_setting_changed(bool value)
        => AppSettings.SetMute(value);
}
