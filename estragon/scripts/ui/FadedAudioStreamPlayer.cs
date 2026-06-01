using Godot;
using System.Diagnostics;

[GlobalClass]
public partial class FadedAudioStreamPlayer : AudioStreamPlayer
{
    private const float FadeDuration = 1.25f;
    private const float MaxPitchShift = 1.02f;
    private const float MaxVolume = 0.75f;

    public void PlayFaded(float fromPosition = 0.0f)
    {
        if (!Playing)
        {
            var tween = GetTree().CreateTween();
            tween.SetEase(Tween.EaseType.In);
            tween.TweenProperty(this, "volume_linear", MaxVolume, FadeDuration);
            Play(fromPosition);
        }
        else
        {
            GD.PrintErr("Call stop before resuming playback");
        }
    }

    public void PlayFadedLoop(float fromPosition = 0.0f)
    {
        Finished += () => PlayFaded();
        PlayFaded(fromPosition);
    }

    public void StopFaded()
    {
        var tween = GetTree().CreateTween();
        tween.SetEase(Tween.EaseType.Out);
        tween.TweenProperty(this, "volume_linear", 0, FadeDuration);
        Stop();
    }

    public void PlayRandomPitch(float fromPosition = 0.0f)
    {
        var random = new RandomNumberGenerator();
        random.Randomize();
        float rndPitch = random.RandfRange(1, MaxPitchShift);
        var effect = new AudioEffectPitchShift();
        int busIndex = AudioServer.GetBusIndex(Bus);
        effect.PitchScale = rndPitch;
        if (AudioServer.GetBusEffectCount(busIndex) != 0)
            _ResetEffects();
        AudioServer.AddBusEffect(busIndex, effect);
        Play(fromPosition);
    }

    private void _ResetEffects()
    {
        int busId = AudioServer.GetBusIndex(Bus);
        for (int i = 0; i < AudioServer.GetBusEffectCount(busId); i++)
            AudioServer.RemoveBusEffect(busId, i);
        Debug.Assert(AudioServer.GetBusEffectCount(busId) == 0, "Reset effects not working correctly");
    }
}
