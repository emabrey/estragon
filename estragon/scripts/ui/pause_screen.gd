extends CanvasLayer


## Game wide support for a Pause screen which pauses the currently loaded scene
## within the GameSceneManager
class_name PauseScreen

## Emit this signal to toggle the pause state
signal toggle_game_paused

## Emitted when game is about to pause or unpause. The boolean value
## specifies whether we are pausing, if true, or unpausing, if false.
@warning_ignore("unused_signal")
signal game_pause_state_changing(is_paused :bool)

## If enabled then pause actions will be ignored even when the toggle_game_paused
## event is fired
static var disable_pause := true

func _ready():
	hide() # Ensure the pause screen starts hidden
	toggle_game_paused.connect(_handle_toggle_game_pause)

func _input(event: InputEvent) -> void:
	if !disable_pause && event.is_action_pressed("ui_pause"):
		toggle_game_paused.emit()
	pass

## Handle the toggle_game_paused event by toggling pause state
func _handle_toggle_game_pause():
	if visible:
		_unpause()
	else:
		_pause()

## Pauses the tree and displays a pause screen
func _pause():
	game_pause_state_changing.emit(true)
	get_tree().paused = true
	show()
	pass

## Unpauses the tree and hides the pause screen
func _unpause():
	game_pause_state_changing.emit(false)
	get_tree().paused = false
	hide()
	pass
