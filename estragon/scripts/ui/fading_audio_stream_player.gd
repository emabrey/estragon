extends AudioStreamPlayer
class_name FadedAudioStreamPlayer

# How many seconds a fade will last, i.e. duration
const FADE_DURATION = 1.25

# Maximum amount of pitch shifting to prevent audio fatigue
#1.02 = 2% upward shift
const MAX_PITCH_SHIFT = 1.02

# Max volume to allow when fading in an audiostream
const MAX_VOLUME = 0.75

# Plays the audio stream with fade-in at beginning
func play_faded(from_position := 0.0) -> void:
	if not playing:
		var tween = get_tree().create_tween()
		tween.set_ease(Tween.EASE_IN)
		tween.tween_property(self, "volume_linear", MAX_VOLUME, FADE_DURATION)
		super.play(from_position)
	else:
		print_debug("Call stop before resuming playback")

# Plays the audio stream with fade-in at beginning while also looping
func play_faded_loop(from_position := 0.0):
		finished.connect(play_faded)
		play_faded(from_position)
	
# Stops the audio stream with fade-out at the end
func stop_faded() -> void:
	var tween = get_tree().create_tween()
	tween.set_ease(Tween.EASE_OUT)
	tween.tween_property(self, "volume_linear", 0, FADE_DURATION)
	super.stop()

# Plays with random pitch variability (MAX_PITCH_SHIFT controls max allowed
# pitch shift). This is useful to prevent audio fatigue on effect noises
func play_random_pitch(from_position := 0.0):
	var random = RandomNumberGenerator.new()
	random.randomize()
	var rnd_pitch := random.randf_range(1,MAX_PITCH_SHIFT)
	var effect = AudioEffectPitchShift.new()
	var bus_index = AudioServer.get_bus_index(bus)
	effect.pitch_scale = rnd_pitch
	if !AudioServer.get_bus_effect_count(bus_index) == 0:
		_resetEffects()
	AudioServer.add_bus_effect(bus_index, effect)
	play(from_position)
	pass

# Removes all the effects on the current bus
func _resetEffects():
	var bus_id = AudioServer.get_bus_index(bus)
	for effect_index in range(AudioServer.get_bus_effect_count(bus_id)):
		AudioServer.remove_bus_effect(bus_id,effect_index)
	assert(AudioServer.get_bus_effect_count(bus_id) == 0, "Reset effects not working correctly")
	pass
