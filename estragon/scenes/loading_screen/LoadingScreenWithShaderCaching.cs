using System.Collections.Generic;
using Godot;

/// <summary>Loading Screen extension that pre-loads shaders before opening the next scene.</summary>
public partial class LoadingScreenWithShaderCaching : LoadingScreen
{
    [Export(PropertyHint.Dir)] public string SpatialShaderMaterialDir { get; set; } = "";
    [Export(PropertyHint.File, "*.tscn")] public string CacheShadersScene { get; set; } = "";
    [Export] public Mesh Mesh { get; set; }

    [ExportGroup("Advanced")]
    [Export] public Godot.Collections.Array<string> MatchingExtensions { get; set; } = new() { ".tres", ".material", ".res" };
    [Export] public Godot.Collections.Array<string> IgnoreSubfolders { get; set; } = new() { ".", ".." };
    [Export] public float ShaderDelayTimer { get; set; } = 0.1f;

    private bool _loadingShaderCache;

    private float _cachingProgress;
    private float CachingProgress
    {
        get => _cachingProgress;
        set
        {
            if (value <= _cachingProgress)
                return;
            _cachingProgress = value;
            UpdateTotalLoadingProgress();
            ResetLoadingStage();
        }
    }

    public bool CanLoadShaderCache()
        => !string.IsNullOrEmpty(SpatialShaderMaterialDir)
            && !string.IsNullOrEmpty(CacheShadersScene)
            && SceneLoader.Instance.IsLoadingScene(CacheShadersScene);

    protected override void UpdateTotalLoadingProgress()
    {
        float partialTotal = SceneLoadingProgress;
        if (CanLoadShaderCache())
        {
            partialTotal += _cachingProgress;
            partialTotal /= 2;
        }
        TotalLoadingProgress = partialTotal;
    }

    protected override void SetSceneLoadingComplete()
    {
        base.SetSceneLoadingComplete();
        if (CanLoadShaderCache() && !_loadingShaderCache)
        {
            _loadingShaderCache = true;
            ShowAllDrawPassesOnce();
        }
        if (CanLoadShaderCache() && _cachingProgress < 1.0f)
            return;
        SceneLoader.Instance.BackgroundLoading = false;
        SceneLoader.Instance.SetProcess(true);
    }

    private async void ShowAllDrawPassesOnce()
    {
        var allMaterials = TraverseFolders(SpatialShaderMaterialDir);
        int totalMaterialCount = allMaterials.Count;
        int cachedMaterialCount = 0;
        foreach (string materialPath in allMaterials)
        {
            LoadMaterial(materialPath);
            cachedMaterialCount += 1;
            CachingProgress = (float)cachedMaterialCount / totalMaterialCount;
            if (ShaderDelayTimer > 0)
                await ToSignal(GetTree().CreateTimer(ShaderDelayTimer), SceneTreeTimer.SignalName.Timeout);
        }
    }

    private List<string> TraverseFolders(string dirPath)
    {
        var materialList = new List<string>();
        if (!dirPath.EndsWith("/"))
            dirPath += "/";
        var dir = DirAccess.Open(dirPath);
        if (dir == null)
        {
            GD.PushError("failed to access the path ", dirPath);
            return materialList;
        }
        if (dir.ListDirBegin() != Error.Ok)
        {
            GD.PushError("failed to access the path ", dirPath);
            return materialList;
        }
        string fileName = dir.GetNext();
        while (fileName != "")
        {
            if (!dir.CurrentIsDir())
            {
                bool matches = false;
                foreach (string extension in MatchingExtensions)
                {
                    if (fileName.EndsWith(extension))
                    {
                        matches = true;
                        break;
                    }
                }
                if (matches)
                    materialList.Add(dirPath + fileName);
            }
            else
            {
                string subfolderName = fileName;
                if (!IgnoreSubfolders.Contains(subfolderName))
                    materialList.AddRange(TraverseFolders(dirPath + subfolderName));
            }
            fileName = dir.GetNext();
        }
        return materialList;
    }

    private void LoadMaterial(string path)
    {
        var materialShower = new MeshInstance3D { Mesh = Mesh };
        var material = ResourceLoader.Load(path) as Material;
        materialShower.SetSurfaceOverrideMaterial(0, material);
        GetNode("%SpatialShaderTypeCaches").AddChild(materialShower);
    }

    public override void _Ready() => SceneLoader.Instance.BackgroundLoading = true;
}
