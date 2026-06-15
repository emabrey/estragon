using Godot;

namespace estragon.addons.maaacks_game_template;

[Tool]
[GlobalClass]
public partial class ListOptionControl : OptionControl
{
    [Export] public bool LockTitles { get; set; }

    [Export]
    public Godot.Collections.Array OptionValues
    {
        get => _optionValues;
        set
        {
            _optionValues = value;
            OnOptionValuesChanged();
        }
    }
    private Godot.Collections.Array _optionValues = new();

    [Export]
    public Godot.Collections.Array<string> OptionTitles
    {
        get => _optionTitles;
        set
        {
            _optionTitles = value;
            if (IsInsideTree())
                SetOptionList(_optionTitles);
        }
    }
    private Godot.Collections.Array<string> _optionTitles = new();

    protected Godot.Collections.Array CustomOptionValues = new();

    private void OnOptionValuesChanged()
    {
        if (_optionValues.Count == 0)
            return;
        CustomOptionValues = _optionValues.Duplicate();
        var firstValue = CustomOptionValues[0];
        PropertyType = firstValue.VariantType;
        SetTitlesFromValues();
    }

    protected override void OnSettingChanged(Variant value)
    {
        int index = value.AsInt32();
        if (index < CustomOptionValues.Count && index >= 0)
            base.OnSettingChanged(CustomOptionValues[index]);
    }

    private void SetTitlesFromValues()
    {
        if (LockTitles)
            return;
        var mappedTitles = new Godot.Collections.Array<string>();
        foreach (Variant optionValue in CustomOptionValues)
            mappedTitles.Add(ValueTitleMap(optionValue));
        OptionTitles = mappedTitles;
    }

    protected virtual string ValueTitleMap(Variant value) => value.ToString();

    private Variant MatchValueToOther(Variant value, Variant other)
    {
        // Primarily for when the editor saves floats as ints instead
        if (value.VariantType == Variant.Type.Int && other.VariantType == Variant.Type.Float)
            return (float)value.AsInt64();
        if (value.VariantType == Variant.Type.Float && other.VariantType == Variant.Type.Int)
            return (int)Mathf.Round(value.AsDouble());
        return value;
    }

    private void RefreshOptionValues(Variant value)
    {
        if (_optionValues.Count == 0)
            return;
        if (value.VariantType == Variant.Type.Nil)
            return;
        CustomOptionValues = _optionValues.Duplicate();
        value = MatchValueToOther(value, CustomOptionValues[0]);
        if (!CustomOptionValues.Contains(value) && value.VariantType == PropertyType)
        {
            CustomOptionValues.Add(value);
            CustomOptionValues.Sort();
        }
        SetTitlesFromValues();
        if (!_optionValues.Contains(value))
            DisableOption(CustomOptionValues.IndexOf(value));
    }

    public override void SetValue(Variant value)
    {
        RefreshOptionValues(value);
        int index = CustomOptionValues.IndexOf(value);
        base.SetValue(index);
    }

    private void SetOptionList(Godot.Collections.Array<string> optionTitlesList)
    {
        var optionButton = GetNode<OptionButton>("%OptionButton");
        optionButton.Clear();
        foreach (string optionTitle in optionTitlesList)
            optionButton.AddItem(optionTitle);
    }

    public void DisableOption(int optionIndex, bool disabled = true)
        => GetNode<OptionButton>("%OptionButton").SetItemDisabled(optionIndex, disabled);

    public override void _Ready()
    {
        LockTitles = LockTitles;
        OptionTitles = _optionTitles;
        OptionValues = _optionValues;
        base._Ready();
    }
}
