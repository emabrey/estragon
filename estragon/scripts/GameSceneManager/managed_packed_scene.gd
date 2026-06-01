extends Resource
class_name ManagedPackedScene

enum TRANSITION{
	NONE,
	FADE,
	FADE_WHITE,
	WIPE_VERTICAL,
	WIPE_HORIZONTAL
}

@export var targetScene :PackedScene = PackedScene.new()
var loadedScene : Node = targetScene.instantiate() if targetScene.can_instantiate() else null
@export var transition := TRANSITION.NONE

## Removes the loaded scene from memory
func cleanup():
	if loadedScene && !loadedScene.is_queued_for_deletion():
			loadedScene.queue_free()

## Return the loaded scene.
## Checks if the scene was previously instantiated and if not, instantiates it.
func get_loaded_scene():
	assert(targetScene!=null, "Target scene must be configured first")
	assert(targetScene.can_instantiate(), "Target scene must be instantiable")
	if !loadedScene:
		loadedScene = targetScene.instantiate()

	return loadedScene
