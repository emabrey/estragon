using Godot;

[Tool]
public partial class AudioInputOptionControl : ListOptionControl
{
    private void SetInputDevice()
    {
        Variant currentSetting = GetSetting(_defaultValue);
        if (currentSetting.VariantType == Variant.Type.Bool)
            currentSetting = (StringName)"Default";
        AudioServer.InputDevice = GetSetting(_defaultValue).AsString();
    }

    private void AddMicrophoneAudioStream()
    {
        var instance = new AudioStreamPlayer
        {
            Stream = new AudioStreamMicrophone(),
            Autoplay = true
        };
        CallDeferred(Node.MethodName.AddChild, instance);
        instance.Ready += SetInputDevice;
    }

    public override void _Ready()
    {
        if (ProjectSettings.GetSetting("audio/driver/enable_input", false).AsBool())
        {
            Show();
            if (string.IsNullOrEmpty(AudioServer.InputDevice))
                AddMicrophoneAudioStream();
            else
                SetInputDevice();
            if (!Engine.IsEditorHint())
            {
                var values = new Godot.Collections.Array();
                foreach (string device in AudioServer.GetInputDeviceList())
                    values.Add(device);
                OptionValues = values;
            }
        }
        else
        {
            Hide();
        }
        base._Ready();
    }

    protected override void OnSettingChanged(Variant value)
    {
        if (value.AsInt32() >= OptionValues.Count)
            return;
        AudioServer.InputDevice = OptionValues[value.AsInt32()].AsString();
        base.OnSettingChanged(value);
    }

    protected override string ValueTitleMap(Variant value)
    {
        if (value.VariantType == Variant.Type.String)
            return value.AsString();
        return base.ValueTitleMap(value);
    }
}
