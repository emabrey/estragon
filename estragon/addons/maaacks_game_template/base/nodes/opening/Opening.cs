using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Scene for displaying opening logos, placards, or other images before a game.</summary>
public partial class Opening : Control
{
    [Export(PropertyHint.File, "*.tscn")] public string NextScenePath { get; set; } = "";
    [Export] public Godot.Collections.Array<Texture2D> Images { get; set; } = new();

    [ExportGroup("Animation")]
    [Export] public float FadeInTime { get; set; } = 0.2f;
    [Export] public float FadeOutTime { get; set; } = 0.2f;
    [Export] public float VisibleTime { get; set; } = 1.6f;

    [ExportGroup("Transition")]
    [Export] public float StartDelay { get; set; } = 0.5f;
    [Export] public float EndDelay { get; set; } = 0.5f;
    [Export] public bool ShowLoadingScreen { get; set; }

    private Tween? _tween;
    private int _nextImageIndex;

    private Container ImagesContainer => GetNode<Container>("%ImagesContainer");

    public string GetNextScenePath()
    {
        if (string.IsNullOrEmpty(NextScenePath))
            return AppConfig.Instance.MainMenuScenePath;
        return NextScenePath;
    }

    private void OnSceneLoaded() => SceneLoader.Instance.ChangeSceneToResource();

    private void LoadNextScene()
    {
        var status = SceneLoader.Instance.GetStatus();
        if (status == ResourceLoader.ThreadLoadStatus.Loaded)
            OnSceneLoaded();
        else if (ShowLoadingScreen)
            SceneLoader.Instance.ChangeSceneToLoadingScreen();
        else
            SceneLoader.Instance.Connect(SceneLoader.SignalName.SceneLoaded, Callable.From(OnSceneLoaded), (uint)ConnectFlags.OneShot);
    }

    private void AddTexturesToContainer(Godot.Collections.Array<Texture2D> textures)
    {
        foreach (Texture2D texture in textures)
        {
            var textureRect = new TextureRect
            {
                Texture = texture,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                Modulate = new Color(1, 1, 1, 0)
            };
            ImagesContainer.CallDeferred(Node.MethodName.AddChild, textureRect);
        }
    }

    private bool EventSkipsImage(InputEvent @event)
        => @event.IsActionReleased("ui_accept") || @event.IsActionReleased("ui_select");

    private bool EventSkipsIntro(InputEvent @event)
        => @event.IsActionReleased("ui_cancel");

    private bool EventIsMouseButtonReleased(InputEvent @event)
        => @event is InputEventMouseButton mouseButton && !mouseButton.IsPressed();

    public override void _UnhandledInput(InputEvent @event)
    {
        if (EventSkipsIntro(@event))
            LoadNextScene();
        else if (EventSkipsImage(@event))
            ShowNextImage(false);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (EventIsMouseButtonReleased(@event))
            ShowNextImage(false);
    }

    private async void TransitionOut()
    {
        await ToSignal(GetTree().CreateTimer(EndDelay), SceneTreeTimer.SignalName.Timeout);
        LoadNextScene();
    }

    private async void TransitionIn()
    {
        await ToSignal(GetTree().CreateTimer(StartDelay), SceneTreeTimer.SignalName.Timeout);
        if (_nextImageIndex == 0)
            ShowNextImage();
    }

    private async void WaitAndFadeOut(Control textureRect)
    {
        int compareNextIndex = _nextImageIndex;
        await ToSignal(GetTree().CreateTimer(VisibleTime, false), SceneTreeTimer.SignalName.Timeout);
        if (compareNextIndex != _nextImageIndex)
            return;
        _tween = CreateTween();
        _tween.TweenProperty(textureRect, "modulate:a", 0.0f, FadeOutTime);
        await ToSignal(_tween, Tween.SignalName.Finished);
        Callable.From(() => ShowNextImage()).CallDeferred();
    }

    private void HidePreviousImage()
    {
        if (_tween != null && _tween.IsRunning())
            _tween.Stop();
        if (ImagesContainer.GetChildCount() == 0)
            return;
        if (ImagesContainer.GetChild(_nextImageIndex - 1) is CanvasItem currentImage)
        {
            Color c = currentImage.Modulate;
            c.A = 0.0f;
            currentImage.Modulate = c;
        }
    }

    private async void ShowNextImage(bool animated = true)
    {
        HidePreviousImage();
        if (_nextImageIndex >= ImagesContainer.GetChildCount())
        {
            if (animated)
                TransitionOut();
            else
                LoadNextScene();
            return;
        }
        var textureRect = ImagesContainer.GetChild<Control>(_nextImageIndex);
        if (animated)
        {
            _tween = CreateTween();
            _tween.TweenProperty(textureRect, "modulate:a", 1.0f, FadeInTime);
            await ToSignal(_tween, Tween.SignalName.Finished);
        }
        else
        {
            Color c = textureRect.Modulate;
            c.A = 1.0f;
            textureRect.Modulate = c;
        }
        _nextImageIndex += 1;
        WaitAndFadeOut(textureRect);
    }

    public override void _Ready()
    {
        SceneLoader.Instance.LoadScene(GetNextScenePath(), true);
        AddTexturesToContainer(Images);
        TransitionIn();
    }
}
