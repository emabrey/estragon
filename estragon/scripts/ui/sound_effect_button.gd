extends Button
class_name SoundEffectButton

@onready var effect_music_player = GameSceneManager.get_effect_player(get_tree())

# Plays sound when button is hovered
func _on_mouse_entered() -> void:
	effect_music_player.stream = load("res://assets/audio/effects/HOVER.mp3")
	effect_music_player.play_random_pitch()
	pass # Replace with function body.

# Plays sound when button is clicked on
func _on_button_down() -> void:
	effect_music_player.stream = load("res://assets/audio/effects/CLICK.mp3")
	effect_music_player.play_random_pitch()
