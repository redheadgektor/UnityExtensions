using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Pipeline;
using UnityEditor.Build.Pipeline.Interfaces;
using UnityEditor.Compilation;
using UnityEngine;
using Object = UnityEngine.Object;

class AssetBundleBuilder : EditorWindow
{
    static AssetBundleBuilder Instance;

    [MenuItem("Game/Asset Bundle Builder", priority = 2056)]
    static void ShowWindow()
    {
        Instance = (AssetBundleBuilder)GetWindow(typeof(AssetBundleBuilder), true);
        Instance.Show();
        Instance.titleContent = new GUIContent("Asset Bundle Builder");
        Instance.minSize = new Vector2(640, 380);
        CompilationPipeline.compilationStarted += delegate { };
    }

    ContentDatabase container = new ContentDatabase();
    List<BundleInfo> Bundles => container.Bundles;
    bool IncludeDepedencies
    {
        get
        {
            return container.IncludeDepedencies;
        }
        set
        {
            container.IncludeDepedencies = value;
        }
    }

    BundleBuildFlags BuildOptions
    {
        get
        {
            return container.BuildOptions;
        }
        set
        {
            container.BuildOptions = value;
        }
    }

    Vector2 MainScroll = Vector2.zero;
    Vector2 BundlesScroll = Vector2.zero;
    Vector2 AssetsScroll = Vector2.zero;
    BundleInfo SelectedBundle;

    private void OnGUI()
    {
        MainScroll = GUILayout.BeginScrollView(MainScroll);
        DrawUI();
        GUILayout.EndScrollView();
    }

    void DropAreaGUI()
    {
        Event evt = Event.current;
        Rect drop_area = GUILayoutUtility.GetLastRect();

        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!drop_area.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object asset in DragAndDrop.objectReferences)
                    {
                        ProcessAsset(SelectedBundle, asset);
                    }
                }
                break;
        }
    }

    void ReogranizeAssets()
    {
        List<AssetInfo> allAssets = new List<AssetInfo>();

        foreach (var b in Bundles)
        {
            allAssets.AddRange(b.assets);
        }

        Bundles.Clear();

        for (int i = 0; i < allAssets.Count; i++)
        {
            BundleInfo bundle = FindOrCreateBundle(AssetTypeToString(AssetDatabase.GetMainAssetTypeAtPath(allAssets[i].path)));
            if (!bundle.ContainsAsset(allAssets[i].guid))
            {
                bundle.assets.Add(allAssets[i]);
            }
        }
    }

    void BundleColumn()
    {
        GUILayout.BeginVertical("box", GUILayout.Width(300));
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Bundle Format");
        bundleFormat = EditorGUILayout.DelayedTextField(bundleFormat);
        GUILayout.EndHorizontal();

        GUILayout.BeginVertical("box", GUILayout.Width(300));
        BundlesScroll = GUILayout.BeginScrollView(BundlesScroll);
        for (int i = 0; i < Bundles.Count; i++)
        {
            if (SelectedBundle == Bundles[i])
            {
                GUI.color = SelectedBundleColor;
            }

            GUILayout.BeginHorizontal("box");

            //name and assets count
            GUILayout.BeginVertical("box");
            Bundles[i].name = EditorGUILayout.TextField(Bundles[i].name);
            GUILayout.Label($"Assets: {Bundles[i].assets.Count}");
            GUILayout.EndVertical();
            GUI.color = Color.white;

            GUILayout.BeginVertical("box");
            if (SelectedBundle == Bundles[i])
            {
                GUI.color = Color.green;
            }
            //actions
            if (GUILayout.Button($"View"))
            {
                if (SelectedBundle != Bundles[i])
                {
                    SelectedBundle = Bundles[i];
                }
                else if (SelectedBundle == Bundles[i])
                {
                    SelectedBundle = null;
                }
            }

            GUI.color = Color.white;

            if (GUILayout.Button("Delete"))
            {
                if (Bundles[i] == SelectedBundle)
                {
                    SelectedBundle = null;
                }
                Bundles.RemoveAt(i);
            }

            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("Create bundle"))
        {
            Bundles.Add(new BundleInfo($"New Bundle {(Bundles.Count + 1)}"));
        }
        GUILayout.EndHorizontal();
        if (GUILayout.Button("Reogranize assets (by type)"))
        {
            ReogranizeAssets();
        }
        GUILayout.BeginHorizontal("box");
        if (GUILayout.Button("Build Selected"))
        {
            BuildBundles(true);
        }
        if (GUILayout.Button("Build All"))
        {
            BuildBundles(false);
        }
        GUILayout.EndHorizontal();
        BuildOptions = (BundleBuildFlags)EditorGUILayout.EnumFlagsField(BuildOptions);
        if (BuildOptions.HasFlag(BundleBuildFlags.Uncompressed))
        {
            BuildOptions &= ~BundleBuildFlags.LZMA;
            BuildOptions &= ~BundleBuildFlags.LZ4;
        }
        if (BuildOptions.HasFlag(BundleBuildFlags.LZMA))
        {
            BuildOptions &= ~BundleBuildFlags.Uncompressed;
            BuildOptions &= ~BundleBuildFlags.LZ4;
        }
        if (BuildOptions.HasFlag(BundleBuildFlags.LZ4))
        {
            BuildOptions &= ~BundleBuildFlags.Uncompressed;
            BuildOptions &= ~BundleBuildFlags.LZMA;
        }
        GUILayout.EndVertical();
        GUILayout.EndVertical();
        GUILayout.EndVertical();
    }

    void AssetsColumn()
    {
        GUILayout.BeginVertical("box");
        if (SelectedBundle != null)
        {
            AssetsScroll = GUILayout.BeginScrollView(AssetsScroll);
            if (SelectedBundle.assets.Count > 0)
            {
                for (int i = 0; i < SelectedBundle.assets.Count; i++)
                {
                    if (SelectedBundle.assets[i].isMissing)
                    {
                        GUI.color = MissingAssetColor;
                        GUI.tooltip = "This asset is missing!";
                    }
                    GUILayout.BeginHorizontal("box");
                    if (SelectedBundle.assets[i].icon)
                    {
                        GUILayout.Box(SelectedBundle.assets[i].icon, GUILayout.Width(64), GUILayout.Height(64));
                    }
                    else
                    {
                        GUILayout.Box("No icon", GUILayout.Width(64), GUILayout.Height(64));
                    }
                    GUILayout.BeginVertical("box");
                    GUILayout.Label($"{SelectedBundle.assets[i].name}");
                    GUILayout.Label(SelectedBundle.assets[i].path);
                    GUILayout.Label(SelectedBundle.assets[i].guid);
                    GUILayout.EndVertical();
                    GUILayout.BeginVertical("box");
                    const float btn_scale = 64;
                    GUI.enabled = !SelectedBundle.assets[i].isMissing;
                    GUI.color = Color.white;
                    if (GUILayout.Button("Focus", GUILayout.Width(btn_scale))) { EditorGUIUtility.PingObject(AssetDatabase.LoadMainAssetAtPath(SelectedBundle.assets[i].path)); }
                    GUI.enabled = true;
                    if (GUILayout.Button("Delete", GUILayout.Width(btn_scale))) { SelectedBundle.assets.RemoveAt(i); }
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Asset's not found!\nDrag&Drop assets", MessageType.Warning, true);
            }
            GUILayout.EndScrollView();

            GUI.color = DragDropColor;
            GUILayout.BeginHorizontal("box", GUILayout.Height(96));
            GUILayout.Label("Drag&Drop Assets");
            IncludeDepedencies = EditorGUILayout.ToggleLeft("Include depedencies", IncludeDepedencies);
            GUILayout.EndHorizontal();
            DropAreaGUI();
            GUI.color = Color.white;
        }
        else
        {
            EditorGUILayout.HelpBox("Select bundle to view assets", MessageType.Info, true);
        }
        GUILayout.EndVertical();
    }

    BundleInfo FindOrCreateBundle(string bundleName)
    {
        for (int i = 0; i < Bundles.Count; i++)
        {
            if (Bundles[i].name == bundleName)
            {
                return Bundles[i];
            }
        }

        BundleInfo info = new BundleInfo(bundleName);
        Bundles.Add(info);
        return info;
    }

    string AssetTypeToString(Type type)
    {
        if (type == typeof(SceneAsset)) { return "scenes"; }
        if (type.IsSubclassOf(typeof(ScriptableObject)) || type == typeof(PhysicMaterial)) { return "scriptable"; }
        if (type.IsSubclassOf(typeof(Texture)) || type == typeof(Sprite)) { return "textures"; }
        if (type == typeof(GameObject)) { return "common"; }
        if (type == typeof(Font)) { return "fonts"; }
        if (type == typeof(Shader)) { return "shaders"; }
        if (type == typeof(Material)) { return "materials"; }
        if (type == typeof(AudioClip)) { return "audio"; }
        if (type == typeof(AnimationClip)) { return "animations"; }
        return "unknown";
    }

    bool CanIncludeAsset(Object asset, out bool isSceneAsset)
    {
        isSceneAsset = asset.GetType() == typeof(SceneAsset);
        bool supported_type =
            asset.GetType().IsSubclassOf(typeof(ScriptableObject)) ||
            asset.GetType().IsSubclassOf(typeof(Texture)) ||
            asset.GetType() == typeof(GameObject) ||
            asset.GetType() == typeof(Sprite) ||
            asset.GetType() == typeof(AnimationClip) ||
            asset.GetType() == typeof(SceneAsset) ||
            asset.GetType() == typeof(AudioClip) ||
            asset.GetType() == typeof(Shader) ||
            asset.GetType() == typeof(Font) ||
            asset.GetType() == typeof(Material) ||
            asset.GetType() == typeof(PhysicMaterial);

        if (!supported_type) { return false; }

        string path = AssetDatabase.GetAssetPath(asset);

        if (/*!path.StartsWith("assets", StringComparison.OrdinalIgnoreCase) || */
            path.ToLower().Contains("editor/") ||
            path.ToLower().Contains("/editor") ||
            path.ToLower().Contains("/resources") ||
            path.ToLower().Contains("resources/")
            )
        {
            return false;
        }

        return true;
    }

    bool IsContainsInOtherBundles(string guid, out BundleInfo contained_in, BundleInfo ignoreBundle)
    {
        contained_in = null;
        for (int i = 0; i < Bundles.Count; i++)
        {
            if (ignoreBundle == Bundles[i])
            {
                continue;
            }
            foreach (var asset in Bundles[i].assets)
            {
                if (asset.guid == guid)
                {
                    contained_in = Bundles[i];
                    return true;
                }
            }
        }

        return false;
    }

    void ProcessAsset(BundleInfo bundle, Object asset, bool is_depedency = false)
    {
        if (CanIncludeAsset(asset, out bool isSceneAsset))
        {
            string path = AssetDatabase.GetAssetPath(asset);
            string guid = AssetDatabase.AssetPathToGUID(path);

            if (!bundle.ContainsAsset(guid))
            {
                if (!IsContainsInOtherBundles(guid, out BundleInfo contained_in, bundle))
                {
                    bundle.assets.Add(new AssetInfo(asset.name, path, guid));
                }
                else if (!is_depedency)
                {
                    ShowNotification(new GUIContent($"{asset.name} already added in {contained_in.name}"), 2);
                }
            }

            if (IncludeDepedencies && !is_depedency)
            {
                if (!isSceneAsset)
                {
                    string[] depedencies = AssetDatabase.GetDependencies(path);

                    for (int i = 0; i < depedencies.Length; i++)
                    {
                        if (depedencies[i] != AssetDatabase.GetAssetPath(asset))
                        {
                            ProcessAsset(bundle, AssetDatabase.LoadMainAssetAtPath(depedencies[i]), true);
                        }
                    }
                }
                else
                {
                    BundleInfo scene_shared = FindOrCreateBundle($"{asset.name}_shared");
                    string[] depedencies = AssetDatabase.GetDependencies(path);
                    int oldCount = scene_shared.assets.Count;
                    for (int i = 0; i < depedencies.Length; i++)
                    {
                        if (depedencies[i] != AssetDatabase.GetAssetPath(asset))
                        {
                            ProcessAsset(scene_shared, AssetDatabase.LoadMainAssetAtPath(depedencies[i]), true);
                        }
                    }

                    int count = scene_shared.assets.Count - oldCount;

                    if (count > 0)
                    {
                        ShowNotification(new GUIContent($"{scene_shared.assets.Count - oldCount} depedencies moved to {scene_shared.name}"), 2);
                    }
                }
            }
        }
    }

    static string bundleBuildPath = Application.streamingAssetsPath;
    string bundleFormat = ".bundle";

    void BuildBundles(bool onlySelected)
    {
        if (!Directory.Exists(bundleBuildPath))
        {
            Directory.CreateDirectory(bundleBuildPath);
        }

        if (!onlySelected)
        {
            var buildParams = new BundleBuildParameters(BuildTarget.StandaloneWindows, BuildTargetGroup.Standalone, bundleBuildPath);

            if (BuildOptions.HasFlag(BundleBuildFlags.LZMA)) { buildParams.BundleCompression = BuildCompression.LZMA; }
            if (BuildOptions.HasFlag(BundleBuildFlags.LZ4)) { buildParams.BundleCompression = BuildCompression.LZ4; }
            if (BuildOptions.HasFlag(BundleBuildFlags.Uncompressed)) { buildParams.BundleCompression = BuildCompression.Uncompressed; }
            if (BuildOptions.HasFlag(BundleBuildFlags.AppendHash)) { buildParams.AppendHash = true; }
            if (BuildOptions.HasFlag(BundleBuildFlags.StripUnityVersion)) { buildParams.ContentBuildFlags |= UnityEditor.Build.Content.ContentBuildFlags.StripUnityVersion; }
            AssetBundleBuild[] map = new AssetBundleBuild[Bundles.Count];

            for (int i = 0; i < map.Length; i++)
            {
                map[i].assetBundleName = $"{Bundles[i].name}{bundleFormat}";
                map[i].assetNames = new string[Bundles[i].assets.Count];
                map[i].addressableNames = new string[Bundles[i].assets.Count];
                for (int j = 0; j < map[i].assetNames.Length; j++)
                {
                    map[i].assetNames[j] = Bundles[i].assets[j].path;
                    map[i].addressableNames[j] = Bundles[i].assets[j].name;
                }
            }


            //BuildPipeline.BuildAssetBundles(bundleBuildPath, map, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);

            BundleBuildContent bbc = new BundleBuildContent(map);
            IBundleBuildResults results;
            ReturnCode code = ContentPipeline.BuildAssetBundles(buildParams, bbc, out results);
            if (code != ReturnCode.Success)
            {
                ShowNotification(new GUIContent($"Build failed {code}"), 3);
            }
            else
            {

                File.WriteAllText(Path.Combine(bundleBuildPath, "Manifest.json"), JsonUtility.ToJson(new Manifest(results.BundleInfos), true));

                StringBuilder builder = new StringBuilder();
                builder.Append("ManifestFileVersion: 0\n");
                builder.Append("CRC: 0\n");
                builder.Append("AssetBundleManifest:\n");
                if (results.BundleInfos.Count > 0)
                {

                    builder.Append("  AssetBundleInfos:\n");
                    int infoCount = 0;
                    foreach (var details in results.BundleInfos)
                    {
                        builder.AppendFormat("    Info_{0}:\n", infoCount++);
                        builder.AppendFormat("      Name: {0}\n", details.Key);
                        int dependencyCount = 0;
                        if (details.Value.Dependencies != null && details.Value.Dependencies.Length > 0)
                        {
                            builder.Append("      Dependencies:\n");
                            foreach (var dependency in details.Value.Dependencies)
                                builder.AppendFormat("        Dependency_{0}: {1}\n", dependencyCount++, dependency);
                        }
                        else
                            builder.Append("      Dependencies: {}\n");
                    }
                }
                else
                {
                    builder.Append("  AssetBundleInfos: {}\n");
                }

                File.WriteAllText(Path.Combine(bundleBuildPath, "Bundles.manifest"), builder.ToString());
            }
            
        }
        else
        {
            AssetBundleBuild map = new AssetBundleBuild();
            map.assetBundleName = $"{SelectedBundle.name}{bundleFormat}";
            map.assetNames = new string[SelectedBundle.assets.Count];
            map.addressableNames = new string[SelectedBundle.assets.Count];
            for (int j = 0; j < map.assetNames.Length; j++)
            {
                map.assetNames[j] = SelectedBundle.assets[j].path;
                map.addressableNames[j] = SelectedBundle.assets[j].name;
            }

            
            BundleBuildContent bbc = new BundleBuildContent(new AssetBundleBuild[] { map });
            var buildParams = new BundleBuildParameters(BuildTarget.StandaloneWindows, BuildTargetGroup.Standalone, bundleBuildPath);
            IBundleBuildResults results;
            ReturnCode code = ContentPipeline.BuildAssetBundles(buildParams, bbc, out results);
            if (code != ReturnCode.Success)
            {
                ShowNotification(new GUIContent($"Build failed {code}"), 3);
            }
            
            //BuildPipeline.BuildAssetBundles(bundleBuildPath, new AssetBundleBuild[] { map }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        }
    }

    private void OnEnable()
    {
        EditorApplication.update += EditorUpdate;
        Load();
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
        Save();
    }

    Color MissingAssetColor = new Color();
    bool MissingAssetFlicker = false;
    float MissingAssetTime = 0;

    Color SelectedBundleColor = new Color();
    bool SelectedBundleFlicker = false;
    float SelectedBundleTime = 0;

    Color DragDropColor = new Color();
    bool DragDropFlicker = false;
    float DragDropTime = 0;

    int AssetIndex = 0;
    int EditorFrame = 0;
    private void EditorUpdate()
    {
        MissingAssetColor = Color.Lerp(MissingAssetColor, MissingAssetFlicker ? new Color(255 / 168f, 0, 0) : Color.white, 1f / 120);
        SelectedBundleColor = Color.Lerp(SelectedBundleColor, SelectedBundleFlicker ? new Color(255 / 168f, 255 / 100f, 255 / 50f) : Color.white, 1f / 120);
        DragDropColor = Color.Lerp(DragDropColor, DragDropFlicker ? new Color(255 / 100f, 255 / 100f, 255 / 50f) : Color.white, 1f / 520);

        if (Time.realtimeSinceStartup - DragDropTime > 1f)
        {
            DragDropFlicker = !DragDropFlicker;
            DragDropTime = Time.realtimeSinceStartup;
        }

        if (Time.realtimeSinceStartup - MissingAssetTime > 1)
        {
            MissingAssetFlicker = !MissingAssetFlicker;
            MissingAssetTime = Time.realtimeSinceStartup;
        }

        if (Time.realtimeSinceStartup - SelectedBundleTime > 0.2f)
        {
            SelectedBundleFlicker = !SelectedBundleFlicker;
            SelectedBundleTime = Time.realtimeSinceStartup;
        }


        if (SelectedBundle != null)
        {
            if (SelectedBundle.assets.Count > 0)
            {
                AssetInfo info = SelectedBundle.assets[AssetIndex % SelectedBundle.assets.Count];

                if (info != null)
                {
                    info.isMissing = AssetDatabase.AssetPathToGUID(info.path).Length == 0;

                    if (!info.icon && !info.isMissing)
                    {
                        Type type = AssetDatabase.GetMainAssetTypeAtPath(info.path);

                        if (type == typeof(GameObject) || type == typeof(Material) || type == typeof(AudioClip))
                        {
                            Object asset = AssetDatabase.LoadMainAssetAtPath(info.path);
                            info.icon = AssetPreview.GetAssetPreview(asset);

                            if (!info.icon && !AssetPreview.IsLoadingAssetPreview(asset.GetInstanceID()))
                            {
                                info.icon = AssetPreview.GetMiniTypeThumbnail(type);
                            }
                        }
                        else
                        {
                            info.icon = AssetPreview.GetMiniTypeThumbnail(type);
                        }
                    }
                }
            }

            AssetIndex++;
        }

        if (EditorFrame % 20 == 0)
        {
            Repaint();
        }

        EditorFrame++;
    }

    private void DrawUI()
    {
        GUILayout.BeginHorizontal();
        BundleColumn();
        AssetsColumn();
        GUILayout.EndHorizontal();
    }

    public const string BundleInfoFileName = "Assets.json";
    public static string BundlesInfoFile = Path.Combine(bundleBuildPath, BundleInfoFileName);
    private void Load()
    {
        if (File.Exists(BundlesInfoFile))
        {
            container = JsonUtility.FromJson<ContentDatabase>(File.ReadAllText(BundlesInfoFile));
        }
        else
        {
            Save();
        }
    }

    private void Save()
    {
        if (!Directory.Exists(Application.streamingAssetsPath))
        {
            Directory.CreateDirectory(Application.streamingAssetsPath);
        }
        File.WriteAllText(BundlesInfoFile, JsonUtility.ToJson(container, true));
    }
}
