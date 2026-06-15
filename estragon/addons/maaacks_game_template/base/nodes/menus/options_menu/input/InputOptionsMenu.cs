using Godot;

namespace estragon.addons.maaacks_game_template;

[Tool]
public partial class InputOptionsMenu : Control
{
    private const string AlreadyAssignedText = "{key} already assigned to {action}.";
    private const string OneInputMinimumText = "%s must have at least one key or button assigned.";
    private const string KeyDeletionText = "Are you sure you want to remove {key} from {action}?";

    [Export(PropertyHint.Enum, "List,Tree")]
    public int RemappingMode
    {
        get => _remappingMode;
        set
        {
            _remappingMode = value;
            if (IsInsideTree())
            {
                switch (_remappingMode)
                {
                    case 0:
                        InputActionsList.Show();
                        InputActionsTree.Hide();
                        break;
                    case 1:
                        InputActionsList.Hide();
                        InputActionsTree.Show();
                        break;
                }
            }
        }
    }
    private int _remappingMode;

    private string _assignmentPlaceholderText = null!;
    private string _lastInputReadableName = null!;

    private InputActionsList InputActionsList => GetNode<InputActionsList>("%InputActionsList");
    private InputActionsTree InputActionsTree => GetNode<InputActionsTree>("%InputActionsTree");
    private KeyAssignmentWindow KeyAssignmentWindow => GetNode<KeyAssignmentWindow>("KeyAssignmentWindow");
    private WindowContainer ResetConfirmation => GetNode<WindowContainer>("ResetConfirmation");
    private WindowContainer KeyDeletionConfirmation => GetNode<WindowContainer>("KeyDeletionConfirmation");
    private WindowContainer AlreadyAssignedMessage => GetNode<WindowContainer>("AlreadyAssignedMessage");
    private WindowContainer OneInputMinimumMessage => GetNode<WindowContainer>("OneInputMinimumMessage");

    public override void _Ready()
    {
        _assignmentPlaceholderText = KeyAssignmentWindow.Text;
        RemappingMode = _remappingMode;
    }

    private void AddActionEvent()
    {
        var lastInputEvent = KeyAssignmentWindow.LastInputEvent;
        _lastInputReadableName = KeyAssignmentWindow.LastInputText ?? "";
        switch (_remappingMode)
        {
            case 0:
                InputActionsList.AddActionEvent(_lastInputReadableName, lastInputEvent);
                break;
            case 1:
                InputActionsTree.AddActionEvent(_lastInputReadableName, lastInputEvent);
                break;
        }
    }

    private void RemoveActionEvent(TreeItem item) => InputActionsTree.RemoveActionEvent(item);

    private void _on_reset_button_pressed() => ResetConfirmation.Show();

    private void _on_key_deletion_confirmation_confirmed()
    {
        var editingItem = InputActionsTree.EditingItem;
        if (GodotObject.IsInstanceValid(editingItem))
            RemoveActionEvent(editingItem!);
    }

    private void _on_key_assignment_window_confirmed() => AddActionEvent();

    private void OpenKeyAssignmentWindow(string actionName, string? readableInputName = null)
    {
        readableInputName ??= _assignmentPlaceholderText;
        var window = KeyAssignmentWindow;
        window.Title = Tr("Assign Key for {action}").Replace("{action}", actionName);
        window.Text = readableInputName;
        window.ConfirmButton.Disabled = true;
        window.Show();
    }

    private void _on_input_actions_tree_add_button_clicked(string actionName)
        => OpenKeyAssignmentWindow(actionName);

    private void _on_input_actions_tree_remove_button_clicked(string actionName, string inputName)
    {
        var window = KeyDeletionConfirmation;
        window.Title = Tr("Remove Key for {action}").Replace("{action}", actionName);
        window.Text = Tr(KeyDeletionText).Replace("{key}", inputName).Replace("{action}", actionName);
        window.Show();
    }

    private void PopupAlreadyAssigned(string actionName, string inputName)
    {
        var window = AlreadyAssignedMessage;
        window.Text = Tr(AlreadyAssignedText).Replace("{key}", inputName).Replace("{action}", actionName);
        window.Show();
    }

    private void PopupMinimumReached(string actionName)
    {
        var window = OneInputMinimumMessage;
        window.Text = OneInputMinimumText.Replace("%s", actionName);
        window.Show();
    }

    private void _on_input_actions_tree_already_assigned(string actionName, string inputName)
        => Callable.From(() => PopupAlreadyAssigned(actionName, inputName)).CallDeferred();

    private void _on_input_actions_tree_minimum_reached(string actionName)
        => Callable.From(() => PopupMinimumReached(actionName)).CallDeferred();

    private void _on_input_actions_list_already_assigned(string actionName, string inputName)
        => Callable.From(() => PopupAlreadyAssigned(actionName, inputName)).CallDeferred();

    private void _on_input_actions_list_minimum_reached(string actionName)
        => Callable.From(() => PopupMinimumReached(actionName)).CallDeferred();

    private void _on_input_actions_list_button_clicked(string actionName, string readableInputName)
        => OpenKeyAssignmentWindow(actionName, readableInputName);

    private void _on_reset_confirmation_confirmed()
    {
        switch (_remappingMode)
        {
            case 0:
                InputActionsList.Reset();
                break;
            case 1:
                InputActionsTree.Reset();
                break;
        }
    }
}
