using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class InspectorMethodScanner : EditorWindow
{
    private string methodName = "";

    [MenuItem("Tools/Inspector Method Scanner")]
    private static void OpenWindow()
    {
        GetWindow<InspectorMethodScanner>("Method Scanner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Search Unity Inspector References", EditorStyles.boldLabel);
        methodName = EditorGUILayout.TextField("Method Name", methodName);

        if (GUILayout.Button("Search Assets"))
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                Debug.LogWarning("Enter a method name.");
                return;
            }

            int count = 0;
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                if (!file.EndsWith(".unity") && !file.EndsWith(".prefab") && !file.EndsWith(".asset"))
                    continue;

                try
                {
                    string content = File.ReadAllText(file);
                    if (!Regex.IsMatch(content, @"m_MethodName:\s*" + Regex.Escape(methodName) + @"\s*(?:\r?$)", RegexOptions.Multiline))
                        continue;

                    string path = "Assets" + file.Substring(Application.dataPath.Length).Replace('\\', '/');
                    Debug.Log($"Inspector reference to '{methodName}': {path}", AssetDatabase.LoadMainAssetAtPath(path));
                    count++;
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"Could not read {file}: {e.Message}");
                }
            }

            Debug.Log($"Search completed. Found {count} matching asset file(s) for '{methodName}'.");
        }
    }
}