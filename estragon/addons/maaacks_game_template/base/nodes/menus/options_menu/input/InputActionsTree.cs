using System.Collections.Generic;
using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Scene to list the input actions out in a tree format.</summary>
[Tool]
[GlobalClass]
public partial class InputActionsTree : Tree
{
    [Signal] public delegate void AlreadyAssignedEventHandler(string actionName, string inputName);
    [Signal] public delegate void MinimumReachedEventHandler(string actionName);
    [Signal] public delegate void AddButtonClickedEventHandler(string actionName);
    [Signal] public delegate void RemoveButtonClickedEventHandler(string actionName, string inputName);

    [Export]
    public Godot.Collections.Array<StringName> InputActionNames
    {
        get => _inputActionNames;
        set
        {
            bool valueChanged = !_inputActionNames.RecursiveEqual(value);
            _inputActionNames = value;
            if (valueChanged)
                RefreshReadableActionNames();
        }
    }
    private Godot.Collections.Array<StringName> _inputActionNames = new();

    [Export]
    public Godot.Collections.Array<string> ReadableActionNames
    {
        get => _readableActionNames;
        set
        {
            bool valueChanged = !_readableActionNames.RecursiveEqual(value);
            _readableActionNames = value;
            if (valueChanged)
            {
                var newActionNameMap = new Godot.Collections.Dictionary();
                for (int iter = 0; iter < _inputActionNames.Count; iter++)
                    newActionNameMap[_inputActionNames[iter]] = _readableActionNames[iter];
                ActionNameMap = newActionNameMap;
            }
        }
    }
    private Godot.Collections.Array<string> _readableActionNames = new();

    [Export]
    public bool CapitalizeActionNames
    {
        get => _capitalizeActionNames;
        set
        {
            _capitalizeActionNames = value;
            RefreshReadableActionNames();
        }
    }
    private bool _capitalizeActionNames = true;

    [Export] public bool ShowAllActions { get; set; } = true;

    [ExportGroup("Icons")]
    [Export] public Texture2D AddButtonTexture { get; set; } = null!;
    [Export] public Texture2D RemoveButtonTexture { get; set; } = null!;
    [Export] public InputIconMapper InputIconMapper { get; set; } = null!;

    [ExportGroup("Built-in Actions")]
    [Export] public bool ShowBuiltInActions { get; set; }
    [Export] public bool CatchBuiltInDuplicateInputs { get; set; }
    [Export] public Godot.Collections.Dictionary BuiltInActionNameMap { get; set; } = InputEventHelper.BuiltInActionNameMap;

    [ExportGroup("Debug")]
    [Export] public Godot.Collections.Dictionary ActionNameMap { get; set; } = new();

    private readonly Dictionary<TreeItem, string> _treeItemAddMap = new();
    private readonly Dictionary<TreeItem, InputEvent> _treeItemRemoveMap = new();
    private readonly Dictionary<TreeItem, string> _treeItemActionMap = new();
    private readonly Dictionary<string, string> _assignedInputEvents = new();
    private string _editingActionName = "";
    public TreeItem? EditingItem { get; private set; }
    private string _lastInputReadableName = null!;

    private void RefreshReadableActionNames()
    {
        var newReadableActionNames = new Godot.Collections.Array<string>();
        foreach (StringName actionName in _inputActionNames)
        {
            string name = actionName.ToString();
            if (_capitalizeActionNames)
                name = name.Capitalize();
            newReadableActionNames.Add(name);
        }
        ReadableActionNames = newReadableActionNames;
    }

    private void StartTree()
    {
        Clear();
        CreateItem();
    }

    private void AddInputEventAsTreeItem(string actionName, InputEvent inputEvent, TreeItem parentItem)
    {
        TreeItem inputTreeItem = CreateItem(parentItem);
        Texture2D? icon = InputIconMapper?.GetIcon(inputEvent);
        if (icon != null)
            inputTreeItem.SetIcon(0, icon);
        inputTreeItem.SetText(0, InputEventHelper.GetText(inputEvent));
        if (RemoveButtonTexture != null)
            inputTreeItem.AddButton(0, RemoveButtonTexture, -1, false, "Remove");
        _treeItemRemoveMap[inputTreeItem] = inputEvent;
        _treeItemActionMap[inputTreeItem] = actionName;
    }

    private void AddActionAsTreeItem(string readableName, string actionName, Godot.Collections.Array<InputEvent> inputEvents)
    {
        TreeItem rootTreeItem = GetRoot();
        TreeItem actionTreeItem = CreateItem(rootTreeItem);
        actionTreeItem.SetText(0, readableName);
        _treeItemAddMap[actionTreeItem] = actionName;
        if (AddButtonTexture != null)
            actionTreeItem.AddButton(0, AddButtonTexture, -1, false, "Add");
        foreach (InputEvent inputEvent in inputEvents)
            AddInputEventAsTreeItem(actionName, inputEvent, actionTreeItem);
    }

    private Godot.Collections.Array<StringName> GetAllActionNames(bool includeBuiltIn = false)
    {
        var actionNames = _inputActionNames.Duplicate();
        if (includeBuiltIn)
        {
            foreach (Variant key in BuiltInActionNameMap.Keys)
            {
                StringName actionName = key.AsStringName();
                if (!actionNames.Contains(actionName))
                    actionNames.Add(actionName);
            }
        }
        if (ShowAllActions)
        {
            foreach (StringName actionName in AppSettings.GetActionNames(includeBuiltIn))
            {
                if (!actionNames.Contains(actionName))
                    actionNames.Add(actionName);
            }
        }
        return actionNames;
    }

    private string GetActionReadableName(StringName actionName)
    {
        string readableName;
        if (ActionNameMap.ContainsKey(actionName))
            readableName = ActionNameMap[actionName].AsString();
        else if (BuiltInActionNameMap.ContainsKey(actionName))
            readableName = BuiltInActionNameMap[actionName].AsString();
        else
        {
            readableName = actionName.ToString();
            if (_capitalizeActionNames)
                readableName = readableName.Capitalize();
            ActionNameMap[actionName] = readableName;
        }
        return readableName;
    }

    private void BuildUiTree()
    {
        StartTree();
        foreach (StringName actionName in GetAllActionNames(ShowBuiltInActions))
        {
            var inputEvents = InputMap.ActionGetEvents(actionName);
            if (inputEvents.Count < 1)
                continue;
            string readableName = GetActionReadableName(actionName);
            AddActionAsTreeItem(readableName, actionName, inputEvents);
        }
    }

    private void AssignInputEvent(InputEvent inputEvent, string actionName)
        => _assignedInputEvents[InputEventHelper.GetText(inputEvent)] = actionName;

    private void AssignInputEventToAction(InputEvent inputEvent, string actionName)
    {
        AssignInputEvent(inputEvent, actionName);
        InputMap.ActionAddEvent(actionName, inputEvent);
        var actionEvents = InputMap.ActionGetEvents(actionName);
        AppSettings.SetConfigInputEvents(actionName, (Godot.Collections.Array)actionEvents);
        AddInputEventAsTreeItem(actionName, inputEvent, EditingItem!);
    }

    private bool CanRemoveInputEvent(string actionName)
        => InputMap.ActionGetEvents(actionName).Count > 1;

    private void RemoveInputEvent(InputEvent inputEvent)
        => _assignedInputEvents.Remove(InputEventHelper.GetText(inputEvent));

    private void RemoveInputEventFromAction(InputEvent inputEvent, string actionName)
    {
        RemoveInputEvent(inputEvent);
        AppSettings.RemoveActionInputEvent(actionName, inputEvent);
    }

    private void BuildAssignedInputEvents()
    {
        _assignedInputEvents.Clear();
        foreach (StringName actionName in GetAllActionNames(ShowBuiltInActions && CatchBuiltInDuplicateInputs))
        {
            foreach (InputEvent inputEvent in InputMap.ActionGetEvents(actionName))
                AssignInputEvent(inputEvent, actionName);
        }
    }

    private string GetActionForInputEvent(InputEvent inputEvent)
    {
        if (_assignedInputEvents.TryGetValue(InputEventHelper.GetText(inputEvent), out string? action))
            return action;
        return "";
    }

    public void AddActionEvent(string lastInputText, InputEvent? lastInputEvent)
    {
        _lastInputReadableName = lastInputText;
        if (lastInputEvent != null)
        {
            string assignedAction = GetActionForInputEvent(lastInputEvent);
            if (!string.IsNullOrEmpty(assignedAction))
            {
                string readableActionName = Tr(GetActionReadableName(assignedAction));
                EmitSignal(SignalName.AlreadyAssigned, readableActionName, _lastInputReadableName);
            }
            else
            {
                AssignInputEventToAction(lastInputEvent, _editingActionName);
            }
        }
        _editingActionName = "";
    }

    public void RemoveActionEvent(TreeItem item)
    {
        if (!_treeItemRemoveMap.ContainsKey(item))
            return;
        string actionName = _treeItemActionMap[item];
        InputEvent inputEvent = _treeItemRemoveMap[item];
        if (!CanRemoveInputEvent(actionName))
        {
            string readableActionName = GetActionReadableName(actionName);
            EmitSignal(SignalName.MinimumReached, readableActionName);
            return;
        }
        RemoveInputEventFromAction(inputEvent, actionName);
        TreeItem parentTreeItem = item.GetParent();
        parentTreeItem.RemoveChild(item);
    }

    public void Reset()
    {
        AppSettings.ResetToDefaultInputs();
        BuildAssignedInputEvents();
        BuildUiTree();
    }

    private void AddItem(TreeItem item)
    {
        EditingItem = item;
        _editingActionName = _treeItemAddMap[item];
        string readableActionName = Tr(GetActionReadableName(_editingActionName));
        EmitSignal(SignalName.AddButtonClicked, readableActionName);
    }

    private void RemoveItem(TreeItem item)
    {
        EditingItem = item;
        _editingActionName = _treeItemActionMap[item];
        string readableActionName = Tr(GetActionReadableName(_editingActionName));
        string itemText = item.GetText(0);
        EmitSignal(SignalName.RemoveButtonClicked, readableActionName, itemText);
    }

    private void CheckItemActions(TreeItem item)
    {
        if (_treeItemAddMap.ContainsKey(item))
            AddItem(item);
        else if (_treeItemRemoveMap.ContainsKey(item))
            RemoveItem(item);
    }

    private void OnButtonClicked(TreeItem item, long column, long id, long mouseButtonIndex)
        => CheckItemActions(item);

    private void OnItemActivated()
    {
        TreeItem item = GetSelected();
        CheckItemActions(item);
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;
        BuildAssignedInputEvents();
        Callable.From(BuildUiTree).CallDeferred();
        ButtonClicked += OnButtonClicked;
        ItemActivated += OnItemActivated;
        if (InputIconMapper != null)
            InputIconMapper.JoypadDeviceChanged += BuildUiTree;
    }
}
