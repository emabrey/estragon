extends SoundEffectButton

func _on_toggled() -> void:
	GameSceneManager.swap_scene_within_tree("MainMenu", get_tree())
