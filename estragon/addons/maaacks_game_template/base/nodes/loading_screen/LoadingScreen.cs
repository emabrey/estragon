using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Scene for displaying the progress of a loading scene to the player.</summary>
[GlobalClass]
public partial class LoadingScreen : CanvasLayer
{
    private const string StalledOnWeb = "\nIf running in a browser, try clicking out of the window, \nand then click back into the window. It might unstick.\nLasty, you may try refreshing the page.\n\n";

    protected enum StallStage { Started, Waiting, StillWaiting, GiveUp }

    [Export(PropertyHint.Range, "5,60,0.5,or_greater")] public float StateChangeDelay { get; set; } = 15.0f;

    [ExportGroup("State Messages")]
    [ExportSubgroup("In Progress")]
    [Export] public string InProgress { get; set; } = "Loading...";
    [Export] public string InProgressWaiting { get; set; } = "Still Loading...";
    [Export] public string InProgressStillWaiting { get; set; } = "Still Loading... (%d seconds)";
    [ExportSubgroup("Completed")]
    [Export] public string Complete { get; set; } = "Loading Complete!";
    [Export] public string CompleteWaiting { get; set; } = "Any Moment Now...";
    [Export] public string CompleteStillWaiting { get; set; } = "Any Moment Now... (%d seconds)";

    private StallStage _stallStage = StallStage.Started;
    protected bool _sceneLoadingComplete;

    protected float SceneLoadingProgress
    {
        get => _sceneLoadingProgress;
        set
        {
            bool valueChanged = !Mathf.IsEqualApprox(_sceneLoadingProgress, value);
            _sceneLoadingProgress = value;
            if (valueChanged)
            {
                UpdateTotalLoadingProgress();
                ResetLoadingStage();
            }
        }
    }
    private float _sceneLoadingProgress;

    protected float TotalLoadingProgress
    {
        get => _totalLoadingProgress;
        set
        {
            _totalLoadingProgress = value;
            GetNode<Range>("%ProgressBar").Value = _totalLoadingProgress;
        }
    }
    private float _totalLoadingProgress;

    private ulong _loadingStartTime;

    protected virtual void UpdateTotalLoadingProgress() => TotalLoadingProgress = SceneLoadingProgress;

    protected void ResetLoadingStage()
    {
        _stallStage = StallStage.Started;
        GetNode<Timer>("%LoadingTimer").Start(StateChangeDelay);
    }

    private void ResetLoadingStartTime() => _loadingStartTime = Time.GetTicksMsec();

    private int GetSecondsWaiting() => (int)((Time.GetTicksMsec() - _loadingStartTime) / 1000.0);

    private void UpdateSceneLoadingProgress()
    {
        float newProgress = SceneLoader.Instance.GetProgress();
        if (newProgress > SceneLoadingProgress)
            SceneLoadingProgress = newProgress;
    }

    protected virtual void SetSceneLoadingComplete()
    {
        SceneLoadingProgress = 1.0f;
        _sceneLoadingComplete = true;
    }

    private void ResetSceneLoadingProgress()
    {
        SceneLoadingProgress = 0.0f;
        _sceneLoadingComplete = false;
    }

    private void ShowLoadingStalledErrorMessage()
    {
        var stalledMessage = GetNode<AcceptDialog>("%StalledMessage");
        if (stalledMessage.Visible)
            return;
        if (SceneLoadingProgress == 0)
            stalledMessage.DialogText = "Stalled at start. You may try waiting or restarting.\n";
        else
            stalledMessage.DialogText = $"Stalled at {(int)(SceneLoadingProgress * 100.0)}%. You may try waiting or restarting.\n";
        if (OS.HasFeature("web"))
            stalledMessage.DialogText += StalledOnWeb;
        stalledMessage.Popup();
    }

    private void ShowSceneSwitchingErrorMessage()
    {
        var errorMessage = GetNode<AcceptDialog>("%ErrorMessage");
        if (errorMessage.Visible)
            return;
        errorMessage.DialogText = "Loading Error: Failed to switch scenes.";
        errorMessage.Popup();
    }

    private void HidePopups()
    {
        GetNode<AcceptDialog>("%ErrorMessage").Hide();
        GetNode<AcceptDialog>("%StalledMessage").Hide();
    }

    public string GetProgressMessage()
    {
        string progressMessage = "";
        switch (_stallStage)
        {
            case StallStage.Started:
                progressMessage = _sceneLoadingComplete ? Complete : InProgress;
                break;
            case StallStage.Waiting:
                progressMessage = _sceneLoadingComplete ? CompleteWaiting : InProgressWaiting;
                break;
            case StallStage.StillWaiting:
            case StallStage.GiveUp:
                progressMessage = _sceneLoadingComplete ? CompleteStillWaiting : InProgressStillWaiting;
                break;
        }
        if (progressMessage.Contains("%d"))
            progressMessage = progressMessage.Replace("%d", GetSecondsWaiting().ToString());
        return progressMessage;
    }

    private void UpdateProgressMessaging()
    {
        GetNode<Label>("%ProgressLabel").Text = GetProgressMessage();
        if (_stallStage == StallStage.GiveUp)
        {
            if (_sceneLoadingComplete)
                ShowSceneSwitchingErrorMessage();
            else
                ShowLoadingStalledErrorMessage();
        }
        else
        {
            HidePopups();
        }
    }

    public override void _Process(double delta)
    {
        var status = SceneLoader.Instance.GetStatus();
        switch (status)
        {
            case ResourceLoader.ThreadLoadStatus.InProgress:
                UpdateSceneLoadingProgress();
                UpdateProgressMessaging();
                break;
            case ResourceLoader.ThreadLoadStatus.Loaded:
                SetSceneLoadingComplete();
                UpdateProgressMessaging();
                break;
            case ResourceLoader.ThreadLoadStatus.Failed:
                var errorMessage = GetNode<AcceptDialog>("%ErrorMessage");
                errorMessage.DialogText = $"Loading Error: {(int)status}";
                errorMessage.Popup();
                SetProcess(false);
                break;
            case ResourceLoader.ThreadLoadStatus.InvalidResource:
                HidePopups();
                SetProcess(false);
                break;
        }
    }

    private void _on_loading_timer_timeout()
    {
        switch (_stallStage)
        {
            case StallStage.Started:
                _stallStage = StallStage.Waiting;
                GetNode<Timer>("%LoadingTimer").Start(StateChangeDelay);
                break;
            case StallStage.Waiting:
                _stallStage = StallStage.StillWaiting;
                GetNode<Timer>("%LoadingTimer").Start(StateChangeDelay);
                break;
            case StallStage.StillWaiting:
                _stallStage = StallStage.GiveUp;
                break;
        }
    }

    private void ReloadMainSceneOrQuit()
    {
        Error err = GetTree().ChangeSceneToFile(ProjectSettings.GetSetting("application/run/main_scene").AsString());
        if (err != Error.Ok)
        {
            GD.PushError($"failed to load main scene: {(int)err}");
            GetTree().Quit();
        }
    }

    private void _on_error_message_confirmed() => ReloadMainSceneOrQuit();

    private void _on_confirmation_dialog_canceled() => ReloadMainSceneOrQuit();

    private void _on_confirmation_dialog_confirmed() => ResetLoadingStage();

    public void Reset()
    {
        Show();
        ResetLoadingStage();
        ResetSceneLoadingProgress();
        ResetLoadingStartTime();
        HidePopups();
        SetProcess(true);
    }

    public void Close()
    {
        SetProcess(false);
        HidePopups();
        Hide();
    }
}
