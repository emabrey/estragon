extends SoundEffectButton

func _process(_delta: float) -> void:
	AutoFocusingControl.new().check_grab(self)

func _on_toggled(_value :bool) -> void:
	GameSceneManager.swap_scene_within_tree("MainGame", get_tree())
