using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class InspectorMethodScanner : EditorWindow
{
    private class MethodInfo
    {
        public string name;
        public string path;
        public int line;
        public int codeReferences;
        public int inspectorReferences;
    }

    private readonly List<MethodInfo> methods = new();
    private Vector2 scroll;
    private string methodName = "";
    private string filter = "";
    private bool showAll;

    [MenuItem("Tools/Inspector Method Scanner")]
    private static void OpenWindow()
    {
        GetWindow<InspectorMethodScanner>("Method Scanner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Unity Method Scanner", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        methodName = EditorGUILayout.TextField("Method Name", methodName);

        if (GUILayout.Button("Search Inspector References"))
        {
            if (string.IsNullOrWhiteSpace(methodName))
            {
                Debug.LogWarning("Enter a method name.");
                return;
            }

            SearchInspector(methodName.Trim());
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Scan All Methods"))
            ScanAllMethods();

        if (methods.Count == 0)
            return;

        EditorGUILayout.LabelField($"Methods found: {methods.Count}");
        EditorGUILayout.LabelField($"No references found: {methods.Count(m => m.codeReferences == 0 && m.inspectorReferences == 0)}");

        filter = EditorGUILayout.TextField("Filter", filter);
        showAll = EditorGUILayout.Toggle("Show all methods", showAll);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (MethodInfo method in methods)
        {
            bool unused = method.codeReferences == 0 && method.inspectorReferences == 0;

            if (!showAll && !unused)
                continue;

            if (!string.IsNullOrWhiteSpace(filter) && !method.name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                continue;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField(method.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{method.path}:{method.line}");
            EditorGUILayout.LabelField($"Code references: {method.codeReferences} | Inspector references: {method.inspectorReferences}");

            if (unused)
                EditorGUILayout.HelpBox("No references found. Verify before deleting.", MessageType.Warning);

            if (GUILayout.Button("Open Script"))
            {
                MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(method.path);
                if (script != null)
                    AssetDatabase.OpenAsset(script, method.line);
            }

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    private void SearchInspector(string name)
    {
        int count = 0;
        string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories);

        foreach (string file in files)
        {
            if (!file.EndsWith(".unity") && !file.EndsWith(".prefab") && !file.EndsWith(".asset"))
                continue;

            try
            {
                string content = File.ReadAllText(file);
                if (!Regex.IsMatch(content, @"m_MethodName:\s*" + Regex.Escape(name) + @"\s*(?:\r?$)", RegexOptions.Multiline))
                    continue;

                string path = ToAssetPath(file);
                Debug.Log($"Inspector reference to '{name}': {path}", AssetDatabase.LoadMainAssetAtPath(path));
                count++;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not read {file}: {e.Message}");
            }
        }

        Debug.Log($"Search completed. Found {count} matching asset file(s) for '{name}'.");
    }

    private void ScanAllMethods()
    {
        methods.Clear();

        string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);
        var sources = new Dictionary<string, string>();

        foreach (string file in files)
        {
            try
            {
                sources[ToAssetPath(file)] = File.ReadAllText(file);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not read {file}: {e.Message}");
            }
        }

        string allCode = string.Join("\n", sources.Values);
        string allAssets = "";

        string[] assets = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".unity") || f.EndsWith(".prefab") || f.EndsWith(".asset"))
            .ToArray();

        foreach (string file in assets)
        {
            try
            {
                allAssets += "\n" + File.ReadAllText(file);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Could not read {file}: {e.Message}");
            }
        }

        Regex declarationRegex = new Regex(
            @"(?m)^[ \t]*(?:(?:public|private|protected|internal|static|virtual|override|abstract|async|extern|new|sealed|unsafe|partial)\s+)*(?:[\w<>,\[\].?]+\s+)+(?<name>@?\w+)\s*\([^;{}\r\n]*\)\s*(?:where\s+[^{]+)?\s*\{",
            RegexOptions.Compiled);

        var declarations = new List<MethodInfo>();

        foreach (var source in sources)
        {
            foreach (Match match in declarationRegex.Matches(source.Value))
            {
                string name = match.Groups["name"].Value.TrimStart('@');

                if (name == "if" || name == "for" || name == "foreach" || name == "while" || name == "switch" || name == "catch" || name == "using" || name == "lock")
                    continue;

                int line = source.Value.Take(match.Index).Count(c => c == '\n') + 1;

                declarations.Add(new MethodInfo
                {
                    name = name,
                    path = source.Key,
                    line = line
                });
            }
        }

        var declarationCounts = declarations
            .GroupBy(m => m.name)
            .ToDictionary(g => g.Key, g => g.Count());

        foreach (MethodInfo method in declarations)
        {
            string pattern = @"(?<![\w])" + Regex.Escape(method.name) + @"(?![\w])";
            int occurrences = Regex.Matches(allCode, pattern).Count;

            method.codeReferences = Math.Max(0, occurrences - declarationCounts[method.name]);
            method.inspectorReferences = Regex.Matches(
                allAssets,
                @"m_MethodName:\s*" + Regex.Escape(method.name) + @"\s*(?:\r?$)",
                RegexOptions.Multiline).Count;
        }

        methods.AddRange(declarations
            .OrderBy(m => m.codeReferences + m.inspectorReferences)
            .ThenBy(m => m.name));

        Debug.Log($"Scan complete. Found {methods.Count} method declarations.");
    }

    private static string ToAssetPath(string fullPath)
    {
        return "Assets" + fullPath.Substring(Application.dataPath.Length).Replace('\\', '/');
    }
}