using System.Collections.Generic;
using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Scene to list the input actions out as buttons in a grid format.</summary>
[Tool]
[GlobalClass]
public partial class InputActionsList : Container
{
    private const string EmptyInputActionString = " ";

    [Signal] public delegate void AlreadyAssignedEventHandler(string actionName, string inputName);
    [Signal] public delegate void MinimumReachedEventHandler(string actionName);
    [Signal] public delegate void ButtonClickedEventHandler(string actionName, string readableInputName);

    [Export]
    public bool Vertical
    {
        get => _vertical;
        set
        {
            _vertical = value;
            if (IsInsideTree())
                GetNode<BoxContainer>("%ParentBoxContainer").Vertical = _vertical;
        }
    }
    private bool _vertical = true;

    [Export(PropertyHint.Range, "1,5")] public int ActionGroups { get; set; } = 2;
    [Export] public Godot.Collections.Array<string> ActionGroupNames { get; set; } = new();

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
    [Export] public Vector2 ButtonMinimumSize { get; set; }

    [ExportGroup("Icons")]
    [Export] public InputIconMapper InputIconMapper { get; set; } = null!;
    [Export] public bool ExpandIcon { get; set; }

    [ExportGroup("Built-in Actions")]
    [Export] public bool ShowBuiltInActions { get; set; }
    [Export] public bool CatchBuiltInDuplicateInputs { get; set; }
    [Export] public Godot.Collections.Dictionary BuiltInActionNameMap { get; set; } = InputEventHelper.BuiltInActionNameMap;

    [ExportGroup("Debug")]
    [Export] public Godot.Collections.Dictionary ActionNameMap { get; set; } = new();

    private readonly Dictionary<string, Button> _actionButtonMap = new();
    private readonly Dictionary<Button, string> _buttonReadableInputMap = new();
    private readonly Dictionary<string, string> _assignedInputEvents = new();
    private string _editingActionName = "";
    private int _editingActionGroup;
    private string _lastInputReadableName = null!;

    private BoxContainer ParentBoxContainer => GetNode<BoxContainer>("%ParentBoxContainer");
    private BoxContainer ActionBoxContainer => GetNode<BoxContainer>("%ActionBoxContainer");

    private static string KeyString(string actionName, int actionGroup) => $"{actionName}:{actionGroup}";

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

    private void ClearList()
    {
        foreach (Node child in ParentBoxContainer.GetChildren())
        {
            if (child == ActionBoxContainer)
                continue;
            child.QueueFree();
        }
    }

    private void ReplaceAction(string actionName, string readableInputName = "")
    {
        string readableActionName = Tr(GetActionReadableName(actionName));
        EmitSignal(SignalName.ButtonClicked, readableActionName, readableInputName);
    }

    private void OnButtonPressed(string actionName, int actionGroup)
    {
        _editingActionName = actionName;
        _editingActionGroup = actionGroup;
        Button? button = GetButtonByAction(actionName, actionGroup);
        string readableInputName = "";
        if (button != null && _buttonReadableInputMap.TryGetValue(button, out string? mapped))
            readableInputName = mapped;
        ReplaceAction(actionName, readableInputName);
    }

    private BoxContainer NewActionBox()
    {
        var newActionBox = (BoxContainer)ActionBoxContainer.Duplicate();
        newActionBox.Visible = true;
        newActionBox.Vertical = !_vertical;
        return newActionBox;
    }

    private void ApplyButtonSizeFlags(Control control)
    {
        if (ButtonMinimumSize.X > 0)
        {
            control.CustomMinimumSize = new Vector2(ButtonMinimumSize.X, control.CustomMinimumSize.Y);
            control.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        }
        else
        {
            control.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        }
        if (ButtonMinimumSize.Y > 0)
        {
            control.CustomMinimumSize = new Vector2(control.CustomMinimumSize.X, ButtonMinimumSize.Y);
            control.SizeFlagsVertical = SizeFlags.ShrinkCenter;
        }
        else
        {
            control.SizeFlagsVertical = SizeFlags.ExpandFill;
        }
    }

    private void AddHeader()
    {
        if (ActionGroupNames.Count == 0)
            return;
        var newActionBox = NewActionBox();
        for (int groupIter = 0; groupIter < ActionGroups; groupIter++)
        {
            string groupName = "";
            if (groupIter < ActionGroupNames.Count)
                groupName = ActionGroupNames[groupIter];
            var newLabel = new Label();
            ApplyButtonSizeFlags(newLabel);
            newLabel.HorizontalAlignment = HorizontalAlignment.Center;
            newLabel.VerticalAlignment = VerticalAlignment.Center;
            newLabel.Text = groupName;
            newActionBox.AddChild(newLabel);
        }
        ParentBoxContainer.AddChild(newActionBox);
    }

    private void AddToActionButtonMap(string actionName, int actionGroup, Button buttonNode)
        => _actionButtonMap[KeyString(actionName, actionGroup)] = buttonNode;

    private Button? GetButtonByAction(string actionName, int actionGroup)
    {
        if (_actionButtonMap.TryGetValue(KeyString(actionName, actionGroup), out Button? button))
            return button;
        return null;
    }

    private void UpdateNextButtonDisabledState(string actionName, int actionGroup, bool disabled = false)
    {
        Button? button = GetButtonByAction(actionName, actionGroup + 1);
        if (button != null)
            button.Disabled = disabled;
    }

    private void UpdateAssignedInputsAndButton(string actionName, int actionGroup, InputEvent inputEvent)
    {
        string newReadableInputName = InputEventHelper.GetText(inputEvent);
        Button? button = GetButtonByAction(actionName, actionGroup);
        if (button == null)
            return;
        Texture2D? icon = InputIconMapper?.GetIcon(inputEvent);
        button.Icon = icon;
        button.Text = button.Icon == null ? newReadableInputName : "";
        if (_buttonReadableInputMap.TryGetValue(button, out string? oldReadableInputName))
            _assignedInputEvents.Remove(oldReadableInputName);
        _buttonReadableInputMap[button] = newReadableInputName;
        _assignedInputEvents[newReadableInputName] = actionName;
    }

    private void ClearButton(string actionName, int actionGroup)
    {
        Button? button = GetButtonByAction(actionName, actionGroup);
        if (button == null)
            return;
        button.Icon = null;
        button.Text = EmptyInputActionString;
        if (_buttonReadableInputMap.TryGetValue(button, out string? oldReadableInputName))
            _assignedInputEvents.Remove(oldReadableInputName);
        _buttonReadableInputMap[button] = EmptyInputActionString;
    }

    private Button AddNewButton(Variant content, Control container, bool disabled = false)
    {
        var newButton = new Button();
        ApplyButtonSizeFlags(newButton);
        newButton.IconAlignment = HorizontalAlignment.Center;
        newButton.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        newButton.ExpandIcon = ExpandIcon;
        if (content.VariantType == Variant.Type.Object && content.As<Texture2D>() != null)
            newButton.Icon = content.As<Texture2D>();
        else if (content.VariantType == Variant.Type.String)
            newButton.Text = content.AsString();
        newButton.Disabled = disabled;
        container.AddChild(newButton);
        return newButton;
    }

    private void ConnectButtonAndAddToMaps(Button button, string inputName, string actionName, int groupIter)
    {
        button.Pressed += () => OnButtonPressed(actionName, groupIter);
        _buttonReadableInputMap[button] = inputName;
        AddToActionButtonMap(actionName, groupIter, button);
    }

    private void AddActionOptions(string actionName, string readableActionName, Godot.Collections.Array<InputEvent> inputEvents)
    {
        var newActionBox = (BoxContainer)ActionBoxContainer.Duplicate();
        newActionBox.Visible = true;
        newActionBox.Vertical = !_vertical;
        newActionBox.GetChild<Label>(0).Text = readableActionName;
        for (int groupIter = 0; groupIter < ActionGroups; groupIter++)
        {
            InputEvent? inputEvent = null;
            if (groupIter < inputEvents.Count)
                inputEvent = inputEvents[groupIter];
            string text = InputEventHelper.GetText(inputEvent!);
            bool isDisabled = groupIter > inputEvents.Count;
            if (string.IsNullOrEmpty(text))
                text = EmptyInputActionString;
            Texture2D? icon = InputIconMapper?.GetIcon(inputEvent!);
            Variant content = icon != null ? icon : text;
            Button button = AddNewButton(content, newActionBox, isDisabled);
            ConnectButtonAndAddToMaps(button, text, actionName, groupIter);
        }
        ParentBoxContainer.AddChild(newActionBox);
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

    private void BuildUiList()
    {
        ClearList();
        AddHeader();
        foreach (StringName actionName in GetAllActionNames(ShowBuiltInActions))
        {
            var inputEvents = InputMap.ActionGetEvents(actionName);
            if (inputEvents.Count < 1)
                continue;
            string readableName = GetActionReadableName(actionName);
            AddActionOptions(actionName, readableName, inputEvents);
        }
    }

    private void AssignInputEvent(InputEvent inputEvent, string actionName)
        => _assignedInputEvents[InputEventHelper.GetText(inputEvent)] = actionName;

    private void AssignInputEventToActionGroup(InputEvent inputEvent, string actionName, int actionGroup)
    {
        AssignInputEvent(inputEvent, actionName);
        var actionEvents = InputMap.ActionGetEvents(actionName);
        actionEvents.Resize(actionEvents.Count + 1);
        actionEvents[actionGroup] = inputEvent;
        InputMap.ActionEraseEvents(actionName);
        var finalActionEvents = new Godot.Collections.Array<InputEvent>();
        foreach (InputEvent inputActionEvent in actionEvents)
        {
            if (inputActionEvent == null)
                continue;
            finalActionEvents.Add(inputActionEvent);
            InputMap.ActionAddEvent(actionName, inputActionEvent);
        }
        AppSettings.SetConfigInputEvents(actionName, (Godot.Collections.Array)finalActionEvents);
        actionGroup = Mathf.Min(actionGroup, finalActionEvents.Count - 1);
        UpdateAssignedInputsAndButton(actionName, actionGroup, inputEvent);
        UpdateNextButtonDisabledState(actionName, actionGroup);
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
                AssignInputEventToActionGroup(lastInputEvent, _editingActionName, _editingActionGroup);
            }
        }
        _editingActionName = "";
    }

    private void RefreshUiListButtonContent()
    {
        foreach (StringName actionName in GetAllActionNames(ShowBuiltInActions))
        {
            var inputEvents = InputMap.ActionGetEvents(actionName);
            if (inputEvents.Count < 1)
                continue;
            int groupIter = 0;
            foreach (InputEvent inputEvent in inputEvents)
            {
                UpdateAssignedInputsAndButton(actionName, groupIter, inputEvent);
                UpdateNextButtonDisabledState(actionName, groupIter);
                groupIter += 1;
            }
            while (groupIter < ActionGroups)
            {
                ClearButton(actionName, groupIter);
                UpdateNextButtonDisabledState(actionName, groupIter, true);
                groupIter += 1;
            }
        }
    }

    private void SetActionBoxContainerSize()
    {
        ActionBoxContainer.SizeFlagsHorizontal = ButtonMinimumSize.X > 0 ? SizeFlags.ShrinkCenter : SizeFlags.ExpandFill;
        ActionBoxContainer.SizeFlagsVertical = ButtonMinimumSize.Y > 0 ? SizeFlags.ShrinkCenter : SizeFlags.ExpandFill;
    }

    public void Reset()
    {
        AppSettings.ResetToDefaultInputs();
        BuildAssignedInputEvents();
        RefreshUiListButtonContent();
    }

    public override void _Ready()
    {
        if (Engine.IsEditorHint())
            return;
        Vertical = _vertical;
        SetActionBoxContainerSize();
        BuildAssignedInputEvents();
        Callable.From(BuildUiList).CallDeferred();
        if (InputIconMapper != null)
            InputIconMapper.JoypadDeviceChanged += RefreshUiListButtonContent;
    }
}
