#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class InspectorMethodScanner : EditorWindow
{
    private class MethodEntry
    {
        public string name;
        public string path;
        public int line;
        public bool isPrivate;
        public int codeReferences;
        public int inspectorReferences;

        public bool Unused => codeReferences == 0 && inspectorReferences == 0;
    }

    private const long MaxAssetBytes = 50L * 1024 * 1024;

    private static readonly HashSet<string> InspectorExtensions = new HashSet<string>
    {
        ".unity", ".prefab", ".asset", ".anim"
    };

    private static readonly HashSet<string> Modifiers = new HashSet<string>
    {
        "public", "private", "protected", "internal", "static", "virtual", "override",
        "abstract", "async", "extern", "sealed", "unsafe", "partial", "new"
    };

    private static readonly HashSet<string> RejectedTypeTokens = new HashSet<string>
    {
        "return", "else", "throw", "yield", "await", "case", "goto", "new", "in", "is", "as"
    };

    private static readonly HashSet<string> NotMethodNames = new HashSet<string>
    {
        "if", "for", "foreach", "while", "switch", "catch", "using", "lock", "fixed",
        "return", "nameof", "typeof", "sizeof", "default"
    };

    private static readonly HashSet<string> UnityMessages = new HashSet<string>
    {
        "Awake", "Start", "Update", "FixedUpdate", "LateUpdate", "OnEnable", "OnDisable", "OnDestroy",
        "OnGUI", "OnValidate", "Reset", "OnApplicationQuit", "OnApplicationFocus", "OnApplicationPause",
        "OnTriggerEnter", "OnTriggerStay", "OnTriggerExit", "OnTriggerEnter2D", "OnTriggerStay2D", "OnTriggerExit2D",
        "OnCollisionEnter", "OnCollisionStay", "OnCollisionExit", "OnCollisionEnter2D", "OnCollisionStay2D", "OnCollisionExit2D",
        "OnMouseDown", "OnMouseUp", "OnMouseEnter", "OnMouseExit", "OnMouseOver", "OnMouseDrag", "OnMouseUpAsButton",
        "OnBecameVisible", "OnBecameInvisible", "OnDrawGizmos", "OnDrawGizmosSelected",
        "OnAnimatorMove", "OnAnimatorIK", "OnParticleCollision", "OnParticleTrigger",
        "OnPreCull", "OnPreRender", "OnPostRender", "OnRenderObject", "OnWillRenderObject", "OnRenderImage",
        "OnTransformChildrenChanged", "OnTransformParentChanged", "OnBeforeTransformParentChanged",
        "OnRectTransformDimensionsChange", "OnCanvasGroupChanged", "OnAudioFilterRead",
        "OnJointBreak", "OnControllerColliderHit", "OnBeforeSerialize", "OnAfterDeserialize",
        "OnPointerClick", "OnPointerDown", "OnPointerUp", "OnPointerEnter", "OnPointerExit",
        "OnBeginDrag", "OnDrag", "OnEndDrag", "OnDrop", "OnScroll", "OnSelect", "OnDeselect",
        "OnSubmit", "OnCancel", "OnMove", "OnUpdateSelected", "OnInitializePotentialDrag",
        "OnSceneGUI", "OnInspectorGUI", "OnWizardCreate", "OnWizardUpdate", "OnHierarchyChange",
        "OnProjectChange", "OnSelectionChange", "OnFocus", "OnLostFocus", "OnInspectorUpdate",
        "CreateGUI", "Main"
    };

    private static readonly string[] EntryPointAttributes =
    {
        "ContextMenu", "RuntimeInitializeOnLoadMethod", "MenuItem", "InitializeOnLoadMethod",
        "DidReloadScripts", "PostProcessBuild", "PostProcessScene", "OnOpenAsset",
        "Test", "UnityTest", "SetUp", "TearDown", "OneTimeSetUp", "OneTimeTearDown"
    };

    private static readonly Regex TokenRegex = new Regex(@"[A-Za-z_]\w*", RegexOptions.Compiled);

    private static readonly Regex DeclarationRegex = new Regex(
        @"(?m)^[ \t]*(?<mods>(?:(?:public|private|protected|internal|static|virtual|override|abstract|async|extern|sealed|unsafe|partial|new)[ \t]+)*)" +
        @"(?<type>(?:[\w<>,\[\].?]+[ \t]+)+)" +
        @"(?<name>@?\w+)[ \t]*(?:<[\w, ]+>)?[ \t]*" +
        @"\((?:[^;{}()]|\([^()]*\))*\)\s*(?:where\b[^{=;]*)?\s*(?:\{|=>)",
        RegexOptions.Compiled);

    private readonly List<MethodEntry> methods = new List<MethodEntry>();
    private Vector2 scroll;
    private int unusedCount;

    [SerializeField] private string methodName = "";
    [SerializeField] private string filter = "";
    [SerializeField] private bool showAll;
    [SerializeField] private string excludedFolders = "Plugins;TextMesh Pro;ThirdParty";

    [MenuItem("Tools/Inspector Method Scanner")]
    private static void OpenWindow()
    {
        GetWindow<InspectorMethodScanner>("Method Scanner");
    }

    private void OnGUI()
    {
        GUILayout.Label("Unity Method Scanner", EditorStyles.boldLabel);

        excludedFolders = EditorGUILayout.TextField(
            new GUIContent("Excluded folders", "Folder names separated by ';' (matched anywhere under Assets)"),
            excludedFolders);

        EditorGUILayout.Space();

        methodName = EditorGUILayout.TextField("Method Name", methodName);

        if (GUILayout.Button("Search Inspector References"))
        {
            if (string.IsNullOrWhiteSpace(methodName))
                Debug.LogWarning("Enter a method name.");
            else
                SearchInspector(methodName.Trim());
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Scan All Methods"))
            ScanAllMethods();

        if (methods.Count == 0)
            return;

        EditorGUILayout.LabelField($"Methods found: {methods.Count}");
        EditorGUILayout.LabelField($"No references found: {unusedCount}");

        filter = EditorGUILayout.TextField("Filter", filter);
        showAll = EditorGUILayout.Toggle("Show all methods", showAll);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (MethodEntry method in methods)
        {
            bool unused = method.Unused;

            if (!showAll && !unused)
                continue;

            if (!string.IsNullOrWhiteSpace(filter) &&
                method.name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) < 0)
                continue;

            EditorGUILayout.BeginVertical("box");

            EditorGUILayout.LabelField(method.name, EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{method.path}:{method.line}");
            EditorGUILayout.LabelField(
                $"Code references: {method.codeReferences}" +
                (method.isPrivate ? " (same file, private)" : " (whole project)") +
                $" | Inspector references: {method.inspectorReferences}");

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
        var foundFiles = new HashSet<string>();

        bool completed = ForEachInspectorMethod((file, method) =>
        {
            if (method != name || !foundFiles.Add(file))
                return;

            string path = ToAssetPath(file);
            Debug.Log($"Inspector reference to '{name}': {path}", AssetDatabase.LoadMainAssetAtPath(path));
        }, "Searching inspector references");

        if (completed)
            Debug.Log($"Search completed. Found {foundFiles.Count} matching asset file(s) for '{name}'.");
        else
            Debug.Log("Search cancelled.");
    }

    private bool ForEachInspectorMethod(Action<string, string> onMethod, string progressTitle)
    {
        string[] excluded = ParseExcludedFolders();

        string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories)
            .Where(f => InspectorExtensions.Contains(Path.GetExtension(f).ToLowerInvariant()))
            .Where(f => !IsExcluded(ToAssetPath(f), excluded))
            .ToArray();

        try
        {
            for (int i = 0; i < files.Length; i++)
            {
                if (i % 10 == 0 &&
                    EditorUtility.DisplayCancelableProgressBar(progressTitle, ToAssetPath(files[i]), (float)i / files.Length))
                    return false;

                try
                {
                    if (new FileInfo(files[i]).Length > MaxAssetBytes)
                        continue;

                    using (StreamReader reader = new StreamReader(files[i]))
                    {
                        string first = reader.ReadLine();

                        if (first == null || !first.StartsWith("%YAML", StringComparison.Ordinal))
                            continue;

                        string line;

                        while ((line = reader.ReadLine()) != null)
                        {
                            if (TryParseMethodName(line, out string name))
                                onMethod(files[i], name);
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not read {files[i]}: {e.Message}");
                }
            }
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }

        return true;
    }

    private static bool TryParseMethodName(string line, out string name)
    {
        const string persistentKey = "m_MethodName:";
        const string animationKey = "functionName:";

        name = null;

        int index = line.IndexOf(persistentKey, StringComparison.Ordinal);
        int keyLength = persistentKey.Length;

        if (index < 0)
        {
            index = line.IndexOf(animationKey, StringComparison.Ordinal);
            keyLength = animationKey.Length;
        }

        if (index < 0)
            return false;

        name = line.Substring(index + keyLength).Trim();
        return name.Length > 0;
    }

    private void ScanAllMethods()
    {
        methods.Clear();
        unusedCount = 0;

        string[] excluded = ParseExcludedFolders();

        string[] files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories)
            .Where(f => !IsExcluded(ToAssetPath(f), excluded))
            .ToArray();

        var globalTokens = new Dictionary<string, int>();
        var fileTokens = new Dictionary<string, Dictionary<string, int>>();
        var globalDecls = new Dictionary<string, int>();
        var fileDecls = new Dictionary<string, Dictionary<string, int>>();
        var found = new List<MethodEntry>();

        try
        {
            for (int i = 0; i < files.Length; i++)
            {
                if (i % 25 == 0 &&
                    EditorUtility.DisplayCancelableProgressBar("Scanning scripts", ToAssetPath(files[i]), (float)i / files.Length))
                {
                    Debug.Log("Scan cancelled.");
                    return;
                }

                string path = ToAssetPath(files[i]);
                string code;

                try
                {
                    code = StripComments(File.ReadAllText(files[i]));
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not read {files[i]}: {e.Message}");
                    continue;
                }

                var tokens = new Dictionary<string, int>();
                fileTokens[path] = tokens;

                foreach (Match token in TokenRegex.Matches(code))
                {
                    Increment(tokens, token.Value);
                    Increment(globalTokens, token.Value);
                }

                var decls = new Dictionary<string, int>();
                fileDecls[path] = decls;

                int lastIndex = 0;
                int line = 1;

                foreach (Match match in DeclarationRegex.Matches(code))
                {
                    string name = match.Groups["name"].Value.TrimStart('@');

                    if (NotMethodNames.Contains(name))
                        continue;

                    string[] typeTokens = SplitTokens(match.Groups["type"].Value);

                    if (typeTokens.Any(t => RejectedTypeTokens.Contains(t)))
                        continue;

                    if (typeTokens.All(t => Modifiers.Contains(t)))
                        continue;

                    for (int k = lastIndex; k < match.Index; k++)
                    {
                        if (code[k] == '\n')
                            line++;
                    }

                    lastIndex = match.Index;

                    Increment(decls, name);
                    Increment(globalDecls, name);

                    string[] mods = SplitTokens(match.Groups["mods"].Value);

                    if (mods.Contains("override") ||
                        UnityMessages.Contains(name) ||
                        HasEntryPointAttribute(code, match.Index))
                        continue;

                    found.Add(new MethodEntry
                    {
                        name = name,
                        path = path,
                        line = line,
                        isPrivate = !mods.Any(m => m == "public" || m == "protected" || m == "internal")
                    });
                }
            }

            var inspectorCounts = new Dictionary<string, int>();

            bool completed = ForEachInspectorMethod(
                (file, method) => Increment(inspectorCounts, method),
                "Scanning inspector references");

            if (!completed)
            {
                Debug.Log("Scan cancelled.");
                return;
            }

            foreach (MethodEntry entry in found)
            {
                int total;
                int declared;

                if (entry.isPrivate)
                {
                    total = Get(fileTokens[entry.path], entry.name);
                    declared = Get(fileDecls[entry.path], entry.name);
                }
                else
                {
                    total = Get(globalTokens, entry.name);
                    declared = Get(globalDecls, entry.name);
                }

                entry.codeReferences = Math.Max(0, total - declared);
                entry.inspectorReferences = Get(inspectorCounts, entry.name);
            }

            methods.AddRange(found
                .OrderBy(m => m.codeReferences + m.inspectorReferences)
                .ThenBy(m => m.name));

            unusedCount = methods.Count(m => m.Unused);

            Debug.Log($"Scan complete. Found {methods.Count} method declarations ({unusedCount} without references).");
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }

    private static void Increment(Dictionary<string, int> dictionary, string key)
    {
        dictionary.TryGetValue(key, out int value);
        dictionary[key] = value + 1;
    }

    private static int Get(Dictionary<string, int> dictionary, string key)
    {
        return dictionary.TryGetValue(key, out int value) ? value : 0;
    }

    private static string[] SplitTokens(string text)
    {
        return text.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
    }

    private string[] ParseExcludedFolders()
    {
        if (string.IsNullOrWhiteSpace(excludedFolders))
            return new string[0];

        return excludedFolders
            .Split(new[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(f => f.Trim().Replace('\\', '/').Trim('/'))
            .Where(f => f.Length > 0)
            .ToArray();
    }

    private static bool IsExcluded(string assetPath, string[] excluded)
    {
        foreach (string folder in excluded)
        {
            if (assetPath.Contains("/" + folder + "/"))
                return true;
        }

        return false;
    }
    private static bool HasEntryPointAttribute(string code, int lineStart)
    {
        int pos = lineStart;
        int guard = 0;

        while (pos > 0 && guard++ < 6)
        {
            int end = pos - 1;
            int start = end > 0 ? code.LastIndexOf('\n', end - 1) + 1 : 0;
            string line = code.Substring(start, end - start).Trim();

            if (line.Length == 0)
            {
                pos = start;
                continue;
            }

            if (!line.StartsWith("[", StringComparison.Ordinal))
                return false;

            foreach (string attribute in EntryPointAttributes)
            {
                if (line.Contains(attribute))
                    return true;
            }

            pos = start;
        }

        return false;
    }

    private static string StripComments(string source)
    {
        var sb = new StringBuilder(source.Length);
        int i = 0;
        int n = source.Length;

        while (i < n)
        {
            char c = source[i];
            char next = i + 1 < n ? source[i + 1] : '\0';

            if (c == '/' && next == '/')
            {
                while (i < n && source[i] != '\n' && source[i] != '\r')
                {
                    sb.Append(' ');
                    i++;
                }
            }
            else if (c == '/' && next == '*')
            {
                sb.Append("  ");
                i += 2;

                while (i < n && !(source[i] == '*' && i + 1 < n && source[i + 1] == '/'))
                {
                    sb.Append(source[i] == '\n' || source[i] == '\r' ? source[i] : ' ');
                    i++;
                }

                if (i < n)
                {
                    sb.Append("  ");
                    i += 2;
                }
            }
            else if (c == '"')
            {
                bool verbatim = (i > 0 && source[i - 1] == '@') ||
                                (i > 1 && source[i - 1] == '$' && source[i - 2] == '@');

                sb.Append(c);
                i++;

                while (i < n)
                {
                    char s = source[i];
                    sb.Append(s);
                    i++;

                    if (verbatim)
                    {
                        if (s == '"')
                        {
                            if (i < n && source[i] == '"')
                            {
                                sb.Append('"');
                                i++;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (s == '\\' && i < n)
                        {
                            sb.Append(source[i]);
                            i++;
                        }
                        else if (s == '"' || s == '\n')
                        {
                            break;
                        }
                    }
                }
            }
            else if (c == '\'')
            {
                sb.Append(c);
                i++;

                while (i < n)
                {
                    char s = source[i];
                    sb.Append(s);
                    i++;

                    if (s == '\\' && i < n)
                    {
                        sb.Append(source[i]);
                        i++;
                    }
                    else if (s == '\'' || s == '\n')
                    {
                        break;
                    }
                }
            }
            else
            {
                sb.Append(c);
                i++;
            }
        }

        return sb.ToString();
    }

    private static string ToAssetPath(string fullPath)
    {
        return "Assets" + fullPath.Substring(Application.dataPath.Length).Replace('\\', '/');
    }
}
#endif