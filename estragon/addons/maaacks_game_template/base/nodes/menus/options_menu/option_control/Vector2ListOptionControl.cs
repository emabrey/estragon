using Godot;

namespace estragon.addons.maaacks_game_template;

[Tool]
[GlobalClass]
public partial class Vector2ListOptionControl : ListOptionControl
{
    protected override string ValueTitleMap(Variant value)
    {
        if (value.VariantType == Variant.Type.Vector2)
        {
            var v = value.AsVector2();
            return $"{(int)v.X} x {(int)v.Y}";
        }
        if (value.VariantType == Variant.Type.Vector2I)
        {
            var v = value.AsVector2I();
            return $"{v.X} x {v.Y}";
        }
        return base.ValueTitleMap(value);
    }
}
