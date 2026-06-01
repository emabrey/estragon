extends Node2D

# Start music on scene load to tree
func _on_tree_entered() -> void:
	var music_player = GameSceneManager.get_music_player(get_tree())
	if !music_player.playing:
		music_player.stream = load("res://assets/audio/music/main-menu.mp3")
		music_player.play_faded_loop()
