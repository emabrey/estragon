extends Panel

func _on_tree_entered() -> void:
	var time_shown = 3.0

	if OS.is_debug_build():
		time_shown = 3.0

	get_tree().create_timer(time_shown).timeout.connect(swap_next_scene)
	pass # Replace with function body.

func swap_next_scene():
	GameSceneManager.swap_scene_within_tree("MainMenu", get_tree())
	return
