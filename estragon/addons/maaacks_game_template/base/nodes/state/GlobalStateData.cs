using Godot;

[GlobalClass]
public partial class GlobalStateData : Resource
{
    [Export] public string FirstVersionOpened { get; set; } = "";
    [Export] public string LastVersionOpened { get; set; } = "";
    [Export] public long LastUnixTimeOpened { get; set; }
    [Export] public Godot.Collections.Dictionary States { get; set; } = new();

    // The original GDScript instantiated a state Resource from a script path via
    // `script.new()`. C# script-defined types are not in ClassDB and CSharpScript
    // has no New(), so the idiomatic replacement is a generic factory: the caller
    // names the concrete state type, and we reuse the saved instance when its type
    // still matches.
    public T GetOrCreateState<T>(string keyName) where T : Resource, new()
    {
        if (States.ContainsKey(keyName) && States[keyName].As<Resource>() is T saved)
            return saved;
        var newState = new T();
        States[keyName] = newState;
        return newState;
    }

    public bool HasState(string keyName) => States.ContainsKey(keyName);
}
