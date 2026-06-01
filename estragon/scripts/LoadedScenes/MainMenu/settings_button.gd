extends SoundEffectButton

func _on_toggled(_value: bool) -> void:
	GameSceneManager.swap_scene_within_tree("Settings", get_tree())
