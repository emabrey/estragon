extends Node2D

## This class manages swaps and transitions between game scenes, and provides audioplayers for
## music and effects to prevent sound dropping out when a scene is swapped
class_name GameSceneManager

## Emitted when a scene swap occurs
signal scene_swapped

## The user provided dictionary of scenes; this is set via the editor
## The initial dummy values are used for integration testing
@export var scenes : Dictionary[StringName, ManagedPackedScene] = {
	"MainGame" : ManagedPackedScene.new(),
	"StartupCreditsStudio" : ManagedPackedScene.new()
}

## The animation player which plays transition related animations
@onready var animation_player := %TransitionMatte/AnimationPlayer

## The scene within the scenes dictionary which will be loaded first, without any in transition
var current_scene_alias = "StartupCreditsStudio"

## Convenience method for swapping the current scene
static func swap_scene_within_tree(sceneAlias :StringName, tree := SceneTree.new()) -> SceneTree:
	var manager = tree.current_scene.get_node("%GameSceneManager")
	assert(manager!=null, "The scene tree does not contain GameSceneManager!")
	manager._swap_scene(sceneAlias)
	return tree

## Convenience method for acquiring the AudioPlayer for playing music
static func get_music_player(tree := SceneTree.new()) -> FadedAudioStreamPlayer:
	return tree.current_scene.get_node("%MusicPlayer")

## Convenience method for acquiring the AudioPlayer for playing effects
static func get_effect_player(tree := SceneTree.new()) -> FadedAudioStreamPlayer:
	return tree.current_scene.get_node("%EffectPlayer")

## Called when game is first starting up, we wait a moment for window
## layout before continuing
func _ready() -> void:
	var time_before_credits = 0.35

	await get_tree().create_timer(time_before_credits).timeout
	_swap_scene(current_scene_alias, false)

## Ensure we delete loaded scenes to prevent memory leak
func _notification(what):
	if what == NOTIFICATION_PREDELETE:
		_cleanup()

## Plays the in or out transition associated with a specific scene
func _handle_transition(alias : StringName, reverse = false):
	#Switches direction of animation in or out by referencing the appropriate function
	var animFunc :Callable = animation_player.play_backwards if reverse else animation_player.play
	animation_player.play("RESET")
	match scenes[alias].transition:
		ManagedPackedScene.TRANSITION.FADE:
			animFunc.call("fade_black")
		ManagedPackedScene.TRANSITION.FADE_WHITE:
			animFunc.call("fade_white")
		ManagedPackedScene.TRANSITION.WIPE_VERTICAL:
			animFunc.call("vertical_wipe_in")
		ManagedPackedScene.TRANSITION.WIPE_HORIZONTAL:
			animFunc.call("horizontal_wipe_in")
		ManagedPackedScene.TRANSITION.NONE:
			animation_player.play("RESET")
			return
		_:
			assert(false, "Unknown Transition Alias: " + alias)

	await animation_player.animation_finished

## Swaps the current child scene for the new specific scene associated with
## the given alias This method assumes that after transition out is played, we are
## free to modify the tree without the user seeing it
#
# Transition out is played
# Remove the current child of SceneParent
# Add new child of SceneParent
# Transition in is played
func _swap_scene(new_alias : StringName, allow_transitions = true):
		assert(scenes.has(current_scene_alias), "You did not add " + current_scene_alias + " to the scene manager")
		assert(scenes.has(new_alias), "You did not add " + new_alias + " to the scene manager")

		if allow_transitions:
			_handle_transition(current_scene_alias, false)
			await animation_player.animation_finished

		# Scene parent holds the inner child scene
		# We need to remove the old child scene
		for child in $SceneParent.get_children():
			$SceneParent.remove_child(child)

		$SceneParent.add_child(scenes[new_alias].get_loaded_scene())
		scene_swapped.emit()

		if allow_transitions:
			_handle_transition(new_alias, true)
			await animation_player.animation_finished

		#Sets the child scene as a child of SceneParent
		#Keep track of the currently loaded scene for future out transition
		current_scene_alias = new_alias

## Remove in-memory child scene representations
func _cleanup():
	for scene :ManagedPackedScene in scenes.values():
		scene.cleanup()
