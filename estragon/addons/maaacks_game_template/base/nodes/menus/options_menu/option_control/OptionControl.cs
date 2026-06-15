using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Generic scene for editing a value of the <see cref="PlayerConfig"/>.</summary>
[Tool]
[GlobalClass]
public partial class OptionControl : Control
{
    [Signal] public delegate void SettingChangedEventHandler(Variant value);

    public enum OptionSections
    {
        None,
        Input,
        Audio,
        Video,
        Game,
        Application,
        Custom,
    }

    private static string OptionSectionName(OptionSections section) => section switch
    {
        OptionSections.None => "",
        OptionSections.Input => AppSettings.InputSection,
        OptionSections.Audio => AppSettings.AudioSection,
        OptionSections.Video => AppSettings.VideoSection,
        OptionSections.Game => AppSettings.GameSection,
        OptionSections.Application => AppSettings.ApplicationSection,
        OptionSections.Custom => AppSettings.CustomSection,
        _ => "",
    };

    [Export] public bool LockConfigNames { get; set; }

    [Export]
    public string OptionName
    {
        get => _optionName;
        set
        {
            bool updateConfig = _optionName.ToPascalCase() == Key && !LockConfigNames;
            _optionName = value;
            if (IsInsideTree())
                GetNode<Label>("%OptionLabel").Text = $"{_optionName}{LabelSuffix}";
            if (updateConfig)
                Key = _optionName.ToPascalCase();
        }
    }
    private string _optionName = "";

    [Export]
    public OptionSections OptionSection
    {
        get => _optionSection;
        set
        {
            bool updateConfig = OptionSectionName(_optionSection) == Section && !LockConfigNames;
            _optionSection = value;
            if (updateConfig)
                Section = OptionSectionName(_optionSection);
        }
    }
    private OptionSections _optionSection;

    [ExportGroup("Config Names")]
    [Export] public string Key { get; set; } = "";
    [Export] public string Section { get; set; } = "";
    [ExportGroup("Format")]
    [Export] public string LabelSuffix { get; set; } = " :";
    [ExportGroup("Properties")]

    [Export]
    public bool Editable
    {
        get => _editable;
        set => SetEditable(value);
    }
    private bool _editable = true;

    [Export] public Variant.Type PropertyType { get; set; } = Variant.Type.Bool;

    protected Variant _defaultValue;
    private readonly Godot.Collections.Array _connectedNodes = new();

    protected virtual void OnSettingChanged(Variant value)
    {
        if (Engine.IsEditorHint())
            return;
        PlayerConfig.SetConfig(Section, Key, value);
        EmitSignal(SignalName.SettingChanged, value);
    }

    protected Variant GetSetting(Variant @default = default)
        => PlayerConfig.GetConfig(Section, Key, @default);

    private void ConnectOptionInputs(Node node)
    {
        if (_connectedNodes.Contains(node))
            return;
        if (node is Button)
        {
            if (node is OptionButton optionButton)
                optionButton.ItemSelected += index => OnSettingChanged(index);
            else if (node is ColorPickerButton colorPickerButton)
                colorPickerButton.ColorChanged += color => OnSettingChanged(color);
            else
                ((Button)node).Toggled += pressed => OnSettingChanged(pressed);
            _connectedNodes.Add(node);
        }
        if (node is Range range)
        {
            range.ValueChanged += value => OnSettingChanged(value);
            _connectedNodes.Add(node);
        }
        if (node is LineEdit lineEdit)
        {
            lineEdit.TextChanged += text => OnSettingChanged(text);
            _connectedNodes.Add(node);
        }
        else if (node is TextEdit textEdit)
        {
            textEdit.TextChanged += () => OnSettingChanged(textEdit.Text);
            _connectedNodes.Add(node);
        }
    }

    public virtual void SetValue(Variant value)
    {
        if (value.VariantType == Variant.Type.Nil)
            return;
        foreach (Node node in GetChildren())
        {
            if (node is Button)
            {
                if (node is OptionButton optionButton)
                    optionButton.Select(value.AsInt32());
                else if (node is ColorPickerButton colorPickerButton)
                    colorPickerButton.Color = value.AsColor();
                else
                    ((Button)node).ButtonPressed = value.AsBool();
            }
            if (node is Range range)
                range.Value = value.AsDouble();
            if (node is LineEdit lineEdit)
                lineEdit.Text = value.ToString();
            else if (node is TextEdit textEdit)
                textEdit.Text = value.ToString();
        }
    }

    public void SetEditable(bool value = true)
    {
        _editable = value;
        foreach (Node node in GetChildren())
        {
            if (node is Button button)
                button.Disabled = !_editable;
            if (node is Slider slider)
                slider.Editable = _editable;
            else if (node is SpinBox spinBox)
                spinBox.Editable = _editable;
            else if (node is LineEdit lineEdit)
                lineEdit.Editable = _editable;
            else if (node is TextEdit textEdit)
                textEdit.Editable = _editable;
        }
    }

    public override void _Ready()
    {
        LockConfigNames = LockConfigNames;
        OptionSection = _optionSection;
        OptionName = _optionName;
        PropertyType = PropertyType;
        SetValue(GetSetting(_defaultValue));
        foreach (Node child in GetChildren())
            ConnectOptionInputs(child);
        ChildEnteredTree += ConnectOptionInputs;
    }

    public override bool _Set(StringName property, Variant value)
    {
        if (property == "value")
        {
            SetValue(value);
            return true;
        }
        if (property == "default_value")
        {
            _defaultValue = value;
            return true;
        }
        return false;
    }

    public override Variant _Get(StringName property)
    {
        if (property == "default_value")
            return _defaultValue;
        return default;
    }

    public override Godot.Collections.Array<Godot.Collections.Dictionary> _GetPropertyList()
    {
        return new Godot.Collections.Array<Godot.Collections.Dictionary>
        {
            new Godot.Collections.Dictionary
            {
                { "name", "value" }, { "type", (int)PropertyType }, { "usage", (int)PropertyUsageFlags.None }
            },
            new Godot.Collections.Dictionary
            {
                { "name", "default_value" }, { "type", (int)PropertyType }
            },
        };
    }
}
