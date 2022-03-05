using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Compilation;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class GameBuilderSettings
{
    public string buildName = "build";
    public string buildDir = "";
    public string buildPath = "";
    public BuildTarget buildTarget = BuildTarget.StandaloneWindows64;

    public bool compressWithLz4 = false;
    public bool compressWithLz4HC = false;

    /* DEVELOPMENT BUILD */
    public bool development = false;
    public bool enableDeepProfilingSupport = false;
    public bool connectWithProfiler = false;
    /* DEVELOPMENT BUILD END */

    public bool useAssetBundleManifest = false;
    public string AssetBundleManifestPath = "";
    public bool removeCrashHandler = false;
    public bool compile2cpp = false;
    public bool generateBatchmodeBat = false;
    public bool generateDebugBat = false;
}

public class GameBuilder : EditorWindow
{
    public static GameBuilder Instance;

    public GameBuilderSettings settings = new GameBuilderSettings();

    [MenuItem("Game/Build", priority = 2056)]
    static void ShowWindow()
    {
        Instance = (GameBuilder)GetWindow(typeof(GameBuilder), true);
        Instance.Show();
        Instance.titleContent = new GUIContent("Game Builder");
        Instance.minSize = new Vector2(640, 380);
        CompilationPipeline.compilationStarted += delegate { Save(Instance.settings); };
    }

    Vector2 scrollPosition = Vector2.zero;

    private void OnGUI()
    {
        GUILayout.BeginVertical();

        /* SCROLL START */
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);

        /* BUILD NAME */
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Build Name");
        settings.buildName = EditorGUILayout.TextField(settings.buildName);
        GUILayout.EndHorizontal();

        /* BUILD FOLDER */
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Build Folder");
        settings.buildDir = EditorGUILayout.TextField(settings.buildDir);
        if (GUILayout.Button("Select Dir"))
        {
            settings.buildDir = EditorUtility.OpenFolderPanel("Select build folder", Environment.CurrentDirectory, "build");
            settings.buildPath = settings.buildDir + "/" + settings.buildName + ".exe";
        }
        GUILayout.EndHorizontal();

        /* EXE PATH */
        GUILayout.BeginHorizontal("box");
        GUILayout.Label("Exe path: " + settings.buildPath);
        GUILayout.EndHorizontal();

        /* BUILD TARGET/OPTIONS */
        GUILayout.BeginVertical("box");
        settings.buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("Build Target:", settings.buildTarget);

        if (!settings.compressWithLz4HC)
        {
            settings.compressWithLz4 = GUILayout.Toggle(settings.compressWithLz4, "Compress with Lz4");
        }
        if (!settings.compressWithLz4)
        {
            settings.compressWithLz4HC = GUILayout.Toggle(settings.compressWithLz4HC, "Compress with Lz4 HC");
        }

        /* DEVELOPMENT BUILD */
        settings.development = GUILayout.Toggle(settings.development, "Development");

        if (settings.development)
        {
            GUILayout.BeginVertical("box");
            EditorGUILayout.HelpBox("Use these parameters only for debugging!", MessageType.Warning, true);

            settings.enableDeepProfilingSupport = GUILayout.Toggle(settings.enableDeepProfilingSupport, "Deep Profiling");
            settings.connectWithProfiler = GUILayout.Toggle(settings.connectWithProfiler, "Connect to Profiller");
            GUILayout.EndVertical();
        }
        /* DEVELOPMENT BUILD END */

        settings.removeCrashHandler = GUILayout.Toggle(settings.removeCrashHandler, new GUIContent(removeCrashHandlerTitle, removeCrashHandlerTooltip));

        settings.compile2cpp = GUILayout.Toggle(settings.compile2cpp, "Compile code to C++");

        if (settings.compile2cpp)
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.IL2CPP);
        }
        else
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        }

        settings.useAssetBundleManifest = GUILayout.Toggle(settings.useAssetBundleManifest, new GUIContent(AssetBundleManifestTitle, AssetBundleManifestTooltip));

        if (settings.useAssetBundleManifest)
        {
            GUILayout.BeginHorizontal("box");
            settings.AssetBundleManifestPath = GUILayout.TextField(settings.AssetBundleManifestPath);
            if (GUILayout.Button("Select Manifest"))
            {
                settings.AssetBundleManifestPath = EditorUtility.OpenFilePanelWithFilters("Select asset bundle manifest", settings.buildDir, new string[] { "Manifest File", "manifest", "All files", "*" });
            }
            GUILayout.EndHorizontal();
        }

        settings.generateBatchmodeBat = GUILayout.Toggle(settings.generateBatchmodeBat, "Generate bat for run in batchmode");
        settings.generateDebugBat = GUILayout.Toggle(settings.generateDebugBat, "Generate bat for run debug");

        GUILayout.EndVertical();

        /* SCROLL END */
        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        if (GUILayout.Button("Build"))
        {
            StartBuild(settings);
        }
    }

    public static void StartBuild()
    {
        GameBuilderSettings settings = new GameBuilderSettings();
        Load(settings);
        StartBuild(settings);
    }

    public static void StartBuild(GameBuilderSettings settings)
    {
        BuildPlayerOptions options = new BuildPlayerOptions();
        options.locationPathName = settings.buildPath;
        options.target = settings.buildTarget;

        if (settings.compressWithLz4)
        {
            options.options |= BuildOptions.CompressWithLz4;
        }
        if (settings.compressWithLz4HC)
        {
            options.options |= BuildOptions.CompressWithLz4HC;
        }

        /* DEVELOPMENT BUILD */
        if (settings.development)
        {
            options.options |= BuildOptions.Development;

            if (settings.connectWithProfiler)
            {
                options.options |= BuildOptions.ConnectWithProfiler;
            }
            if (settings.enableDeepProfilingSupport)
            {
                options.options |= BuildOptions.EnableDeepProfilingSupport;
            }
        }
        /* DEVELOPMENT BUILD END */

        if (settings.useAssetBundleManifest)
        {
            options.assetBundleManifestPath = settings.AssetBundleManifestPath;
        }

        options.scenes = new string[] { EditorBuildSettings.scenes[0].path };

        DateTime time = DateTime.Now;
        PlayerSettings.bundleVersion = "build_" + time.Day + "" + time.Month + "" + time.Year + "" + time.Hour;

        BuildReport report = BuildPipeline.BuildPlayer(options);

        /* BUILD SUCCESS */
        if (report.summary.result == BuildResult.Succeeded)
        {
            string p = Path.Combine(settings.buildDir, "Data");

            if (Directory.Exists(p))
            {
                DeleteDirectory(p);
            }

            if (Directory.Exists(Path.Combine(settings.buildDir, settings.buildName + "_Data")))
            {
                Directory.Move(Path.Combine(settings.buildDir, settings.buildName + "_Data"), p);
            }

            //remove crashHandler if checked
            if (settings.removeCrashHandler)
            {
                if (File.Exists(Path.Combine(settings.buildDir, "UnityCrashHandler64.exe")))
                {
                    File.Delete(Path.Combine(settings.buildDir, "UnityCrashHandler64.exe"));
                }
                if (File.Exists(Path.Combine(settings.buildDir, "UnityCrashHandler32.exe")))
                {
                    File.Delete(Path.Combine(settings.buildDir, "UnityCrashHandler32.exe"));
                }
            }

            if (settings.generateBatchmodeBat)
            {
                string commandline1 = "@echo off\necho Starting engine...\nstart " + settings.buildName + ".exe -batchmode -nographics -logFile engine.log";
                File.WriteAllText(settings.buildDir + "/batchmode.bat", commandline1);

                string commandline2 = "@echo off\necho Starting engine...\nstart " + settings.buildName + ".exe -logFile engine.log";
                File.WriteAllText(settings.buildDir + "/debug.bat", commandline2);
            }

            BuildFile[] files = report.files;
            PackedAssets[] assets = report.packedAssets;
            int total_size = (int)report.summary.totalSize;
            TimeSpan build_time = report.summary.totalTime;

            using (StringWriter sw = new StringWriter())
            {
                sw.WriteLine("Build version (engine/game): " + Application.unityVersion + "/" + Application.version + " (Platform: " + (report.summary.platform + ")"));
                sw.WriteLine("Build date: " + report.summary.buildEndedAt);
                sw.WriteLine("Build GUID: " + report.summary.guid);
                sw.WriteLine("Total files: " + files.Length);
                sw.WriteLine("Total build size: " + (total_size / 1024) / 1024 + " MB");
                sw.WriteLine("\nFiles used by build:");

                for (int i = 0; i < files.Length; i++)
                {
                    BuildFile _f = files[i];

                    if (_f.role == "DebugInfo" || _f.role == "MonoConfig")
                        continue;

                    sw.WriteLine("[" + _f.role + "]  " + Path.GetFileName(_f.path) + "  [" + _f.size / 1024 + " kb]");
                }

                sw.WriteLine("\nAssets used by build:");

                for (int i = 0; i < assets.Length; i++)
                {
                    PackedAssets _f = assets[i];

                    sw.WriteLine("======= " + _f.shortPath + " =======  (Files " + _f.contents.Length + ")");
                    for (int j = 0; j < _f.contents.Length; j++)
                    {
                        sw.WriteLine("  - [" + _f.contents[j].type.Name + "]  " + _f.contents[j].sourceAssetPath + "  [" + _f.contents[j].packedSize + " kb]");
                    }
                }

                sw.Close();
                sw.Flush();
                File.WriteAllText(Path.Combine(settings.buildDir, settings.buildName + ".buildreport"), sw.ToString());
                Save(settings);
            }


            int dialog_result = EditorUtility.DisplayDialogComplex("Build complete!", "Path: " + report.summary.outputPath + "\n"
                    + "Files: " + files.Length + "\n"
                    + "Total Size: " + (total_size / 1024) / 1024 + " MB\n"
                    + "Building time: " + (int)build_time.TotalHours + ":" + (int)build_time.TotalMinutes + ":" + (int)build_time.TotalSeconds, "Open build directory", "Run game", "Close");

            if (dialog_result == 0)
            {
                EditorUtility.RevealInFinder(report.summary.outputPath);
            }
            if (dialog_result == 1)
            {
                Process.Start(settings.buildPath);
            }
        }
        else if (report.summary.result == BuildResult.Failed)
        {
            EditorUtility.DisplayDialog("Building error!", "Error on building... See in console", "Close");
        }
    }

    const string AssetBundleManifestTitle = "Use Asset Bundle Manifest";
    const string AssetBundleManifestTooltip = "In build will not include files related to bundles that are specified in the manifest";

    const string removeCrashHandlerTitle = "Remove CrashHandler";
    const string removeCrashHandlerTooltip = "After the build is complete, the roof handler will be removed from the game folder";



    private void OnEnable()
    {
        Load(settings);
    }

    private void OnDestroy()
    {
        Save(settings);
    }

    /* SETTINGS SAVE/LOAD */
    public const string GameBuilderSettingsFileHeader = "GameBuilderSettings";
    public const string GameBuilderSettingsFile = "GameBuilderSettings.bin";

    private static void Load(GameBuilderSettings settings)
    {
        if (File.Exists(Path.Combine(Environment.CurrentDirectory, GameBuilderSettingsFile)))
        {
            try
            {
                using (FileStream fs = new FileStream(Path.Combine(Environment.CurrentDirectory, GameBuilderSettingsFile), FileMode.Open))
                {
                    using (BinaryReader br = new BinaryReader(fs))
                    {
                        string fileHeader = new string(br.ReadChars(GameBuilderSettingsFileHeader.Length));
                        if (fileHeader == GameBuilderSettingsFileHeader)
                        {
                            settings.buildName = br.ReadString();
                            settings.buildDir = br.ReadString();
                            settings.buildPath = br.ReadString();
                            settings.buildTarget = (BuildTarget)br.ReadInt32();

                            /* DEVELOPMENT BUILD */
                            settings.development = br.ReadBoolean();
                            settings.compressWithLz4 = br.ReadBoolean();
                            settings.compressWithLz4HC = br.ReadBoolean();
                            settings.enableDeepProfilingSupport = br.ReadBoolean();
                            settings.connectWithProfiler = br.ReadBoolean();
                            /* DEVELOPMENT BUILD END */

                            settings.useAssetBundleManifest = br.ReadBoolean();
                            settings.AssetBundleManifestPath = br.ReadString();
                            settings.removeCrashHandler = br.ReadBoolean();
                            settings.compile2cpp = br.ReadBoolean();
                            settings.generateBatchmodeBat = br.ReadBoolean();
                            settings.generateDebugBat = br.ReadBoolean();
                        }
                        else
                        {
                            Debug.LogError("[GameBuilder] Settings file invalid! (" + fileHeader + " != " + GameBuilderSettingsFileHeader + ")");
                        }
                    }
                    fs.Close();
                }
            }
            catch
            {
                return;
            }
        }
        else
        {
            Save(settings);
        }
    }

    private static void Save(GameBuilderSettings settings)
    {
        using (FileStream fs = new FileStream(Path.Combine(Environment.CurrentDirectory, GameBuilderSettingsFile), FileMode.OpenOrCreate))
        {
            using (BinaryWriter bw = new BinaryWriter(fs))
            {
                bw.Write(GameBuilderSettingsFileHeader.ToCharArray());
                bw.Write(settings.buildName);
                bw.Write(settings.buildDir);
                bw.Write(settings.buildPath);
                bw.Write((int)settings.buildTarget);

                /* DEVELOPMENT BUILD */
                bw.Write(settings.development);
                bw.Write(settings.compressWithLz4);
                bw.Write(settings.compressWithLz4HC);
                bw.Write(settings.enableDeepProfilingSupport);
                bw.Write(settings.connectWithProfiler);
                /* DEVELOPMENT BUILD END */

                bw.Write(settings.useAssetBundleManifest);
                bw.Write(settings.AssetBundleManifestPath);
                bw.Write(settings.removeCrashHandler);
                bw.Write(settings.compile2cpp);
                bw.Write(settings.generateBatchmodeBat);
                bw.Write(settings.generateDebugBat);
            }
            fs.Close();
        }
    }


    public static void DeleteDirectory(string target_dir)
    {
        string[] files = Directory.GetFiles(target_dir);
        string[] dirs = Directory.GetDirectories(target_dir);

        foreach (string file in files)
        {
            File.SetAttributes(file, FileAttributes.Normal);
            File.Delete(file);
        }

        foreach (string dir in dirs)
        {
            DeleteDirectory(dir);
        }

        Directory.Delete(target_dir, false);
    }
}
