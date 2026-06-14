using Godot;

/// <summary>Applies UI page up and page down inputs to tab switching.</summary>
public partial class PaginatedTabContainer : TabContainer
{
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsVisibleInTree())
            return;
        if (@event.IsActionPressed("ui_page_down"))
        {
            CurrentTab = (CurrentTab + 1) % GetTabCount();
        }
        else if (@event.IsActionPressed("ui_page_up"))
        {
            if (CurrentTab == 0)
                CurrentTab = GetTabCount() - 1;
            else
                CurrentTab = CurrentTab - 1;
        }
    }
}
