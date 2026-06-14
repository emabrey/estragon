using System.Collections.Generic;
using Godot;

/// <summary>Controller for managing all UI sounds in a scene from one place.</summary>
[GlobalClass]
public partial class UISoundController : Node
{
    private const int MaxDepth = 16;

    [Export] public NodePath RootPath { get; set; } = "..";
    [Export] public StringName AudioBus { get; set; } = "SFX";

    [Export]
    public bool Persistent
    {
        get => _persistent;
        set
        {
            _persistent = value;
            UpdatePersistentSignals();
        }
    }
    private bool _persistent = true;

    [ExportGroup("Button Sounds")]
    [Export] public AudioStream? ButtonHovered { get; set; }
    [Export] public AudioStream? ButtonFocused { get; set; }
    [Export] public AudioStream? ButtonPressed { get; set; }

    [ExportGroup("TabBar Sounds")]
    [Export] public AudioStream? TabHovered { get; set; }
    [Export] public AudioStream? TabChanged { get; set; }
    [Export] public AudioStream? TabSelected { get; set; }

    [ExportGroup("Slider Sounds")]
    [Export] public AudioStream? SliderHovered { get; set; }
    [Export] public AudioStream? SliderFocused { get; set; }
    [Export] public AudioStream? SliderDragStarted { get; set; }
    [Export] public AudioStream? SliderDragEnded { get; set; }

    [ExportGroup("LineEdit Sounds")]
    [Export] public AudioStream? LineHovered { get; set; }
    [Export] public AudioStream? LineFocused { get; set; }
    [Export] public AudioStream? LineTextChanged { get; set; }
    [Export] public AudioStream? LineTextSubmitted { get; set; }
    [Export] public AudioStream? LineTextChangeRejected { get; set; }

    [ExportGroup("ItemList Sounds")]
    [Export] public AudioStream? ItemListSelected { get; set; }
    [Export] public AudioStream? ItemListActivated { get; set; }

    [ExportGroup("Tree Sounds")]
    [Export] public AudioStream? TreeItemSelected { get; set; }
    [Export] public AudioStream? TreeItemActivated { get; set; }
    [Export] public AudioStream? TreeButtonClicked { get; set; }

    private Node _rootNode = null!;

    private AudioStreamPlayer? _buttonHoveredPlayer;
    private AudioStreamPlayer? _buttonFocusedPlayer;
    private AudioStreamPlayer? _buttonPressedPlayer;
    private AudioStreamPlayer? _tabHoveredPlayer;
    private AudioStreamPlayer? _tabChangedPlayer;
    private AudioStreamPlayer? _tabSelectedPlayer;
    private AudioStreamPlayer? _sliderHoveredPlayer;
    private AudioStreamPlayer? _sliderFocusedPlayer;
    private AudioStreamPlayer? _sliderDragStartedPlayer;
    private AudioStreamPlayer? _sliderDragEndedPlayer;
    private AudioStreamPlayer? _lineHoveredPlayer;
    private AudioStreamPlayer? _lineFocusedPlayer;
    private AudioStreamPlayer? _lineTextChangedPlayer;
    private AudioStreamPlayer? _lineTextSubmittedPlayer;
    private AudioStreamPlayer? _lineTextChangeRejectedPlayer;
    private AudioStreamPlayer? _itemListActivatedPlayer;
    private AudioStreamPlayer? _itemListSelectedPlayer;
    private AudioStreamPlayer? _treeItemActivatedPlayer;
    private AudioStreamPlayer? _treeItemSelectedPlayer;
    private AudioStreamPlayer? _treeButtonClickedPlayer;

    private readonly HashSet<(ulong, string)> _connected = new();

    private void UpdatePersistentSignals()
    {
        if (!IsInsideTree())
            return;
        var tree = GetTree();
        if (_persistent)
            tree.NodeAdded += ConnectUiSounds;
        else
            tree.NodeAdded -= ConnectUiSounds;
    }

    private AudioStreamPlayer? BuildStreamPlayer(AudioStream? stream, string streamName = "")
    {
        AudioStreamPlayer? streamPlayer = null;
        if (stream != null)
        {
            streamPlayer = new AudioStreamPlayer
            {
                Stream = stream,
                Bus = AudioBus,
                Name = streamName + "AudioStreamPlayer"
            };
            AddChild(streamPlayer);
        }
        return streamPlayer;
    }

    private void BuildAllStreamPlayers()
    {
        _buttonHoveredPlayer = BuildStreamPlayer(ButtonHovered, "ButtonHovered");
        _buttonFocusedPlayer = BuildStreamPlayer(ButtonFocused, "ButtonFocused");
        _buttonPressedPlayer = BuildStreamPlayer(ButtonPressed, "ButtonClicked");
        _tabHoveredPlayer = BuildStreamPlayer(TabHovered, "TabHovered");
        _tabChangedPlayer = BuildStreamPlayer(TabChanged, "TabChanged");
        _tabSelectedPlayer = BuildStreamPlayer(TabSelected, "TabSelected");
        _sliderHoveredPlayer = BuildStreamPlayer(SliderHovered, "SliderHovered");
        _sliderFocusedPlayer = BuildStreamPlayer(SliderFocused, "SliderFocused");
        _sliderDragStartedPlayer = BuildStreamPlayer(SliderDragStarted, "SliderDragStarted");
        _sliderDragEndedPlayer = BuildStreamPlayer(SliderDragEnded, "SliderDragEnded");
        _lineHoveredPlayer = BuildStreamPlayer(LineHovered, "LineHovered");
        _lineFocusedPlayer = BuildStreamPlayer(LineFocused, "LineFocused");
        _lineTextChangedPlayer = BuildStreamPlayer(LineTextChanged, "LineTextChanged");
        _lineTextSubmittedPlayer = BuildStreamPlayer(LineTextSubmitted, "LineTextSubmitted");
        _lineTextChangeRejectedPlayer = BuildStreamPlayer(LineTextChangeRejected, "LineTextChangeRejected");
        _itemListActivatedPlayer = BuildStreamPlayer(ItemListActivated, "ItemActivated");
        _itemListSelectedPlayer = BuildStreamPlayer(ItemListSelected, "ItemSelected");
        _treeItemActivatedPlayer = BuildStreamPlayer(TreeItemActivated, "TreeItemActivated");
        _treeItemSelectedPlayer = BuildStreamPlayer(TreeItemSelected, "TreeItemSelected");
        _treeButtonClickedPlayer = BuildStreamPlayer(TreeButtonClicked, "TreeButtonClicked");
    }

    private void PlayStream(AudioStreamPlayer? streamPlayer)
    {
        if (streamPlayer == null || !streamPlayer.IsInsideTree())
            return;
        streamPlayer.Play();
    }

    private void ConnectSound(Node node, AudioStreamPlayer? streamPlayer, StringName signalName, Callable callable)
    {
        if (streamPlayer == null)
            return;
        if (!_connected.Add((node.GetInstanceId(), signalName.ToString())))
            return;
        node.Connect(signalName, callable);
    }

    public void ConnectUiSounds(Node node)
    {
        if (node is Button)
        {
            ConnectSound(node, _buttonHoveredPlayer, "mouse_entered", Callable.From(() => PlayStream(_buttonHoveredPlayer)));
            ConnectSound(node, _buttonFocusedPlayer, "focus_entered", Callable.From(() => PlayStream(_buttonFocusedPlayer)));
            ConnectSound(node, _buttonPressedPlayer, "pressed", Callable.From(() => PlayStream(_buttonPressedPlayer)));
        }
        else if (node is TabBar)
        {
            ConnectSound(node, _tabHoveredPlayer, "tab_hovered", Callable.From<long>(_ => PlayStream(_tabHoveredPlayer)));
            ConnectSound(node, _tabChangedPlayer, "tab_changed", Callable.From<long>(_ => PlayStream(_tabChangedPlayer)));
            ConnectSound(node, _tabSelectedPlayer, "tab_selected", Callable.From<long>(_ => PlayStream(_tabSelectedPlayer)));
        }
        else if (node is Slider)
        {
            ConnectSound(node, _sliderHoveredPlayer, "mouse_entered", Callable.From(() => PlayStream(_sliderHoveredPlayer)));
            ConnectSound(node, _sliderFocusedPlayer, "focus_entered", Callable.From(() => PlayStream(_sliderFocusedPlayer)));
            ConnectSound(node, _sliderDragStartedPlayer, "drag_started", Callable.From(() => PlayStream(_sliderDragStartedPlayer)));
            ConnectSound(node, _sliderDragEndedPlayer, "drag_ended", Callable.From<bool>(_ => PlayStream(_sliderDragEndedPlayer)));
        }
        else if (node is LineEdit)
        {
            ConnectSound(node, _lineHoveredPlayer, "mouse_entered", Callable.From(() => PlayStream(_lineHoveredPlayer)));
            ConnectSound(node, _lineFocusedPlayer, "focus_entered", Callable.From(() => PlayStream(_lineFocusedPlayer)));
            ConnectSound(node, _lineTextChangedPlayer, "text_changed", Callable.From<string>(_ => PlayStream(_lineTextChangedPlayer)));
            ConnectSound(node, _lineTextSubmittedPlayer, "text_submitted", Callable.From<string>(_ => PlayStream(_lineTextSubmittedPlayer)));
            ConnectSound(node, _lineTextChangeRejectedPlayer, "text_change_rejected", Callable.From<string>(_ => PlayStream(_lineTextChangeRejectedPlayer)));
        }
        else if (node is ItemList)
        {
            ConnectSound(node, _itemListActivatedPlayer, "item_activated", Callable.From<long>(_ => PlayStream(_itemListActivatedPlayer)));
            ConnectSound(node, _itemListSelectedPlayer, "item_selected", Callable.From<long>(_ => PlayStream(_itemListSelectedPlayer)));
        }
        else if (node is Tree)
        {
            ConnectSound(node, _treeItemActivatedPlayer, "item_activated", Callable.From(() => PlayStream(_treeItemActivatedPlayer)));
            ConnectSound(node, _treeItemSelectedPlayer, "item_selected", Callable.From(() => PlayStream(_treeItemSelectedPlayer)));
            ConnectSound(node, _treeButtonClickedPlayer, "button_clicked", Callable.From<TreeItem, long, long, long>((_, _, _, _) => PlayStream(_treeButtonClickedPlayer)));
        }
    }

    private void RecursiveConnectUiSounds(Node currentNode, int currentDepth = 0)
    {
        if (currentDepth >= MaxDepth)
            return;
        foreach (Node node in currentNode.GetChildren())
        {
            ConnectUiSounds(node);
            RecursiveConnectUiSounds(node, currentDepth + 1);
        }
    }

    public override void _Ready()
    {
        _rootNode = GetNode(RootPath);
        BuildAllStreamPlayers();
        RecursiveConnectUiSounds(_rootNode);
        Persistent = _persistent;
    }

    public override void _ExitTree()
    {
        GetTree().NodeAdded -= ConnectUiSounds;
    }
}
