using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;

public class EditorIcons : EditorWindow
{
    public static EditorIcons Instance;

    [MenuItem("Tools/Editor Icons %e", priority = -1001)]
    public static void EditorIconsOpen()
    {
        Instance = (EditorIcons)GetWindow(typeof(EditorIcons), true);
        Instance.Show();
        Instance.titleContent = new GUIContent("Editor Icons");
        Instance.minSize = new Vector2(640, 380);
    }

    List<GUIContent> icons = new List<GUIContent>();
    AssetBundle EditorBundle;

    private AssetBundle GetEditorAssetBundle()
    {
        var editorGUIUtility = typeof(EditorGUIUtility);
        var getEditorAssetBundle = editorGUIUtility.GetMethod(
            "GetEditorAssetBundle",
            BindingFlags.NonPublic | BindingFlags.Static);

        return (AssetBundle)getEditorAssetBundle.Invoke(null, new object[] { });
    }

    private void OnEnable()
    {
        if (EditorBundle == null)
        {
            EditorBundle = GetEditorAssetBundle();
        }
        foreach (Texture x in EditorBundle.LoadAllAssets<Texture>())
        {
            GUIContent c = new GUIContent(x);
            c.tooltip = "Width: "+x.width+"\nHeight: "+x.height;
            c.text = x.name;
            icons.Add(c);
        }
        start = 1;
        end = 100 % icons.Count;
    }

    private void OnDisable()
    {
        icons.Clear();
    }

    Vector2 scrollPosition;

    float start = 0;
    float end = 0;
    float size = 64;

    private void OnGUI()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        for (int i = (int)start; i < (int)end; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(icons[i],GUILayout.MinHeight(32), GUILayout.MinWidth(32), GUILayout.Height(size), GUILayout.Width(size));
            GUILayout.EndHorizontal();
        }
        GUILayout.EndScrollView();
        EditorGUILayout.MinMaxSlider(ref start, ref end, 1, icons.Count);
        EditorGUILayout.HelpBox("Viewing "+ (int)start + " - "+((int)end - (int)start)+" - " + (int)end, MessageType.Info, true);

        size = EditorGUILayout.Slider(size, 1, 1024);
        EditorGUILayout.HelpBox("Size " + size, MessageType.Info, true);
    }
}