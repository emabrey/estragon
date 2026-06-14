using System.Collections.Generic;
using Godot;

/// <summary>Controller for music playback across scenes.</summary>
[GlobalClass]
public partial class MusicController : Node
{
    private const string BlendBusPrefix = "Blend";
    private const int MaxDepth = 16;
    private const float MinimumVolumeDb = -80;

    [Export] public StringName AudioBus { get; set; } = "Music";

    [ExportGroup("Blending")]
    [Export]
    public float FadeOutDuration
    {
        get => _fadeOutDuration;
        set => _fadeOutDuration = value < 0 ? 0 : value;
    }
    private float _fadeOutDuration;

    [Export]
    public float FadeInDuration
    {
        get => _fadeInDuration;
        set => _fadeInDuration = value < 0 ? 0 : value;
    }
    private float _fadeInDuration;

    [Export] public bool EmptyStreamsStopPlayer { get; set; } = true;

    private AudioStreamPlayer _musicStreamPlayer;
    private StringName _blendAudioBus;
    private int _blendAudioBusIdx;

    private readonly Dictionary<AudioStreamPlayer, Callable> _treeExitingCallables = new();

    public Tween FadeOut(float duration = 0.0f)
    {
        if (Mathf.IsZeroApprox(duration))
            return null;
        _musicStreamPlayer.Bus = AudioBus;
        var tween = CreateTween();
        tween.TweenProperty(_musicStreamPlayer, "volume_db", MinimumVolumeDb, duration);
        return tween;
    }

    private void SetSubAudioVolumeDb(float subVolumeDb)
        => AudioServer.SetBusVolumeDb(_blendAudioBusIdx, subVolumeDb);

    public Tween FadeIn(float duration = 0.0f)
    {
        if (Mathf.IsZeroApprox(duration))
            return null;
        _musicStreamPlayer.Bus = _blendAudioBus;
        AudioServer.SetBusVolumeDb(_blendAudioBusIdx, MinimumVolumeDb);
        var tween = CreateTween();
        tween.TweenMethod(Callable.From<float>(SetSubAudioVolumeDb), MinimumVolumeDb, 0.0f, duration);
        return tween;
    }

    public Tween BlendTo(float targetVolumeDb, float duration = 0.0f)
    {
        if (!Mathf.IsZeroApprox(duration))
        {
            var tween = CreateTween();
            tween.TweenProperty(_musicStreamPlayer, "volume_db", targetVolumeDb, duration);
            return tween;
        }
        _musicStreamPlayer.VolumeDb = targetVolumeDb;
        return null;
    }

    public void Stop()
    {
        if (!GodotObject.IsInstanceValid(_musicStreamPlayer))
            return;
        _musicStreamPlayer.Stop();
    }

    public void Play(float playbackPosition = 0.0f)
    {
        if (!GodotObject.IsInstanceValid(_musicStreamPlayer))
            return;
        if (Mathf.IsZeroApprox(playbackPosition) && !_musicStreamPlayer.Playing)
            _musicStreamPlayer.Play();
        else
            _musicStreamPlayer.Play(playbackPosition);
    }

    private async void FadeOutAndFree()
    {
        if (!GodotObject.IsInstanceValid(_musicStreamPlayer))
            return;
        var streamPlayer = _musicStreamPlayer;
        var tween = FadeOut(FadeOutDuration);
        if (tween != null)
            await ToSignal(tween, Tween.SignalName.Finished);
        streamPlayer.QueueFree();
    }

    private void PlayAndFadeIn()
    {
        Play();
        FadeIn(FadeInDuration);
    }

    private bool IsMatchingStream(AudioStreamPlayer streamPlayer)
    {
        if (streamPlayer.Bus != AudioBus)
            return false;
        if (!GodotObject.IsInstanceValid(_musicStreamPlayer))
            return false;
        return _musicStreamPlayer.Stream == streamPlayer.Stream;
    }

    private void ConnectStreamOnTreeExiting(AudioStreamPlayer streamPlayer)
    {
        if (!_treeExitingCallables.ContainsKey(streamPlayer))
        {
            Callable cb = Callable.From(() => OnRemovedMusicPlayer(streamPlayer));
            _treeExitingCallables[streamPlayer] = cb;
            streamPlayer.Connect(Node.SignalName.TreeExiting, cb);
        }
    }

    private void BlendAndRemoveStreamPlayer(AudioStreamPlayer streamPlayer)
    {
        float playbackPosition = (float)(_musicStreamPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix());
        var oldStreamPlayer = _musicStreamPlayer;
        _musicStreamPlayer = streamPlayer;
        _musicStreamPlayer.Bus = _blendAudioBus;
        Play(playbackPosition);
        oldStreamPlayer.Stop();
        oldStreamPlayer.QueueFree();
        ConnectStreamOnTreeExiting(_musicStreamPlayer);
    }

    private void BlendAndConnectStreamPlayer(AudioStreamPlayer streamPlayer)
    {
        streamPlayer.Bus = _blendAudioBus;
        FadeOutAndFree();
        _musicStreamPlayer = streamPlayer;
        PlayAndFadeIn();
        ConnectStreamOnTreeExiting(_musicStreamPlayer);
    }

    public void PlayStreamPlayer(AudioStreamPlayer streamPlayer)
    {
        if (streamPlayer == _musicStreamPlayer)
            return;
        if (streamPlayer.Stream == null && !EmptyStreamsStopPlayer)
            return;
        if (IsMatchingStream(streamPlayer))
            BlendAndRemoveStreamPlayer(streamPlayer);
        else
            BlendAndConnectStreamPlayer(streamPlayer);
    }

    public AudioStreamPlayer GetStreamPlayer(AudioStream audioStream)
    {
        var streamPlayer = new AudioStreamPlayer
        {
            Stream = audioStream,
            Bus = AudioBus
        };
        AddChild(streamPlayer);
        return streamPlayer;
    }

    public AudioStreamPlayer PlayStream(AudioStream audioStream)
    {
        var streamPlayer = GetStreamPlayer(audioStream);
        streamPlayer.CallDeferred(AudioStreamPlayer.MethodName.Play, 0.0f);
        PlayStreamPlayer(streamPlayer);
        return streamPlayer;
    }

    private void CloneMusicPlayer(AudioStreamPlayer streamPlayer)
    {
        float playbackPosition = (float)(streamPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix());
        var audioStream = streamPlayer.Stream;
        _musicStreamPlayer = GetStreamPlayer(audioStream);
        _musicStreamPlayer.VolumeDb = streamPlayer.VolumeDb;
        _musicStreamPlayer.MaxPolyphony = streamPlayer.MaxPolyphony;
        _musicStreamPlayer.PitchScale = streamPlayer.PitchScale;
        _musicStreamPlayer.CallDeferred(AudioStreamPlayer.MethodName.Play, playbackPosition);
    }

    private void ReparentMusicPlayer(AudioStreamPlayer streamPlayer)
    {
        float playbackPosition = (float)(streamPlayer.GetPlaybackPosition() + AudioServer.GetTimeSinceLastMix());
        streamPlayer.Owner = null;
        streamPlayer.CallDeferred(Node.MethodName.Reparent, this, true);
        streamPlayer.CallDeferred(AudioStreamPlayer.MethodName.Play, playbackPosition);
    }

    private bool NodeMatchesChecks(Node node)
        => node is AudioStreamPlayer streamPlayer && streamPlayer.Autoplay && streamPlayer.Bus == AudioBus;

    private void OnRemovedMusicPlayer(Node node)
    {
        if (_musicStreamPlayer == node)
        {
            if (node.Owner == null)
                CloneMusicPlayer((AudioStreamPlayer)node);
            else
                ReparentMusicPlayer((AudioStreamPlayer)node);
            if (node is AudioStreamPlayer player && _treeExitingCallables.TryGetValue(player, out var cb)
                && node.IsConnected(Node.SignalName.TreeExiting, cb))
            {
                node.Disconnect(Node.SignalName.TreeExiting, cb);
                _treeExitingCallables.Remove(player);
            }
        }
    }

    private void OnAddedMusicPlayer(Node node)
    {
        if (node == _musicStreamPlayer)
            return;
        if (!NodeMatchesChecks(node))
            return;
        PlayStreamPlayer((AudioStreamPlayer)node);
    }

    public override void _EnterTree()
    {
        AudioServer.AddBus();
        _blendAudioBusIdx = AudioServer.BusCount - 1;
        _blendAudioBus = AppSettings.SystemBusNamePrefix + BlendBusPrefix + AudioBus;
        AudioServer.SetBusSend(_blendAudioBusIdx, AudioBus);
        AudioServer.SetBusName(_blendAudioBusIdx, _blendAudioBus);
        GetTree().NodeAdded += OnAddedMusicPlayer;
    }

    public override void _ExitTree()
    {
        GetTree().NodeAdded -= OnAddedMusicPlayer;
    }
}
