extends Label


func _on_tree_entered() -> void:
	self.text = ProjectSettings.get_setting("application/config/version")
	pass # Replace with function body.
