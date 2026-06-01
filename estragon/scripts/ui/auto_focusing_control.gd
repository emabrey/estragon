extends Control
class_name AutoFocusingControl

## This class is used to grab focus on a provided control if no control
## in the provided control's viewport is in focus

var focus_grabbed = false

func check_grab(focused_control : Control):
	assert(focused_control != null, "Don't pass null values; this method expects a Control")
	if !focus_grabbed && focused_control.get_viewport().gui_get_focus_owner() == null:
		focus_grabbed = true
		focused_control.grab_focus()
