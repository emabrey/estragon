using Godot;

/// <summary>Interface for a single configuration file through <see cref="ConfigFile"/>.</summary>
public static class PlayerConfig
{
    public const string ConfigFileLocation = "user://player_config.cfg";

    public static ConfigFile? ConfigFile;

    private static void SaveConfigFile()
    {
        Error saveError = ConfigFile!.Save(ConfigFileLocation);
        if (saveError != Error.Ok)
            GD.PushError($"save config file failed with error {(int)saveError}");
    }

    public static void LoadConfigFile()
    {
        if (ConfigFile != null)
            return;
        ConfigFile = new ConfigFile();
        Error loadError = ConfigFile.Load(ConfigFileLocation);
        if (loadError != Error.Ok)
        {
            Error saveError = ConfigFile.Save(ConfigFileLocation);
            if (saveError != Error.Ok)
                GD.PushError($"save config file failed with error {(int)saveError}");
        }
    }

    public static void SetConfig(string section, string key, Variant value)
    {
        LoadConfigFile();
        ConfigFile!.SetValue(section, key, value);
        SaveConfigFile();
    }

    public static Variant GetConfig(string section, string key, Variant @default = default)
    {
        LoadConfigFile();
        return ConfigFile!.GetValue(section, key, @default);
    }

    public static bool HasSection(string section)
    {
        LoadConfigFile();
        return ConfigFile!.HasSection(section);
    }

    public static bool HasSectionKey(string section, string key)
    {
        LoadConfigFile();
        return ConfigFile!.HasSectionKey(section, key);
    }

    public static void EraseSection(string section)
    {
        if (HasSection(section))
        {
            ConfigFile!.EraseSection(section);
            SaveConfigFile();
        }
    }

    public static void EraseSectionKey(string section, string key)
    {
        if (HasSectionKey(section, key))
        {
            ConfigFile!.EraseSectionKey(section, key);
            SaveConfigFile();
        }
    }

    public static string[] GetSectionKeys(string section)
    {
        LoadConfigFile();
        if (ConfigFile!.HasSection(section))
            return ConfigFile!.GetSectionKeys(section);
        return System.Array.Empty<string>();
    }
}
