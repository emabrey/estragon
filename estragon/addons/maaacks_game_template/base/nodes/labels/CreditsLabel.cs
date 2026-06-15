using Godot;

namespace estragon.addons.maaacks_game_template;

/// <summary>Script for parsing an attribution file in markdown format.</summary>
[Tool]
public partial class CreditsLabel : RichTextLabel
{
    private const string HeadingStringReplacement = "$1[font_size=%d]$2[/font_size]";
    private const string BoldHeadingStringReplacement = "$1[b][font_size=%d]$2[/font_size][/b]";

    [Export(PropertyHint.File, "*.md")] public string AttributionFilePath { get; set; } = "";
    [Export] public bool AutoUpdate { get; set; } = true;

    [ExportGroup("Font Sizes")]
    [Export] public int H1FontSize { get; set; }
    [Export] public int H2FontSize { get; set; }
    [Export] public int H3FontSize { get; set; }
    [Export] public int H4FontSize { get; set; }
    [Export] public int H5FontSize { get; set; }
    [Export] public int H6FontSize { get; set; }
    [Export] public bool BoldHeadings { get; set; }

    [ExportGroup("Image Sizes")]
    [Export] public int MaxImageWidth { get; set; }
    [Export] public int MaxImageHeight { get; set; }

    [ExportGroup("Extra Options")]
    [Export] public bool DisableImages { get; set; }
    [Export] public bool DisableUrls { get; set; }
    [Export] public bool DisableOpeningLinks { get; set; }

    public string LoadFile(string filePath)
    {
        string fileString = FileAccess.GetFileAsString(filePath);
        if (string.IsNullOrEmpty(fileString))
        {
            GD.PushWarning($"File open error: {FileAccess.GetOpenError()}");
            return "";
        }
        return fileString;
    }

    public string RegexReplaceImgs(string credits)
    {
        var regex = new RegEx();
        string matchString = "!\\[([^\\]]*)\\]\\(([^\\)]*)\\)";
        string replaceString = "";
        if (!DisableImages)
        {
            replaceString = "res://$2[/img]";
            if (MaxImageWidth != 0)
            {
                if (MaxImageHeight != 0)
                    replaceString = $"[img={MaxImageWidth}x{MaxImageHeight}]" + replaceString;
                else
                    replaceString = $"[img={MaxImageWidth}]" + replaceString;
            }
            else
            {
                replaceString = "[img]" + replaceString;
            }
        }
        regex.Compile(matchString);
        return regex.Sub(credits, replaceString, true);
    }

    public string RegexReplaceUrls(string credits)
    {
        var regex = new RegEx();
        string matchString = "\\[([^\\]]*)\\]\\(([^\\)]*)\\)";
        string replaceString = "$1";
        if (!DisableUrls)
            replaceString = "[url=$2]$1[/url]";
        regex.Compile(matchString);
        return regex.Sub(credits, replaceString, true);
    }

    public string RegexReplaceTitles(string credits)
    {
        int iter = 0;
        int[] headingFontSizes = { H1FontSize, H2FontSize, H3FontSize, H4FontSize, H5FontSize, H6FontSize };
        foreach (int headingFontSize in headingFontSizes)
        {
            iter += 1;
            var regex = new RegEx();
            string matchString = $"([^#]|^)#{{{iter}}}\\s([^\n]*)";
            string replaceString = HeadingStringReplacement.Replace("%d", headingFontSize.ToString());
            if (BoldHeadings)
                replaceString = BoldHeadingStringReplacement.Replace("%d", headingFontSize.ToString());
            regex.Compile(matchString);
            credits = regex.Sub(credits, replaceString, true);
        }
        return credits;
    }

    private void UpdateTextFromFile()
    {
        string fileText = LoadFile(AttributionFilePath);
        if (fileText == "")
            return;
        int endOfFirstLine = fileText.Find("\n") + 1;
        fileText = fileText.Substring(endOfFirstLine); // Trims first line "ATTRIBUTION"
        fileText = RegexReplaceImgs(fileText);
        fileText = RegexReplaceUrls(fileText);
        fileText = RegexReplaceTitles(fileText);
        Text = fileText;
    }

    public void SetFilePath(string filePath)
    {
        AttributionFilePath = filePath;
        UpdateTextFromFile();
    }

    private void OnMetaClicked(Variant meta)
    {
        string metaString = meta.AsString();
        if (metaString.StartsWith("https://") && !DisableOpeningLinks)
            OS.ShellOpen(metaString);
    }

    public override void _Ready()
    {
        MetaClicked += OnMetaClicked;
        if (!AutoUpdate)
            return;
        SetFilePath(AttributionFilePath);
    }
}
