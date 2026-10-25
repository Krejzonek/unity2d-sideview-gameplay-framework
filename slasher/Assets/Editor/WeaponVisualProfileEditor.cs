
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WeaponVisualProfile))]
public class WeaponVisualProfileEditor : Editor
{
    private PreviewRenderUtility preview;
    private GameObject previewObject;

    private void OnEnable()
    {
        preview = new PreviewRenderUtility();
        preview.cameraFieldOfView = 30f;
        RebuildPreview();
    }

    private void OnDisable()
    {
        if (previewObject != null)
            DestroyImmediate(previewObject);

        preview?.Cleanup();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();

        DrawDefaultInspector();

        bool changed = EditorGUI.EndChangeCheck();
        serializedObject.ApplyModifiedProperties();

        if (changed)
            RebuildPreview();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Weapon Preview", EditorStyles.boldLabel);

        Rect rect = GUILayoutUtility.GetAspectRect(1.5f);
        EditorGUI.DrawRect(rect, new Color(0.16f, 0.16f, 0.16f));

        if (previewObject == null)
        {
            EditorGUI.LabelField(rect, "Assign a Weapon Prefab to preview it.", EditorStyles.centeredGreyMiniLabel);
            return;
        }

        preview.BeginPreview(rect, GUIStyle.none);

        preview.camera.clearFlags = CameraClearFlags.SolidColor;
        preview.camera.backgroundColor = new Color(0.16f, 0.16f, 0.16f);
        preview.camera.orthographic = true;

        Bounds bounds = GetBounds();
        float size = Mathf.Max(bounds.size.x, bounds.size.y, 0.5f);
        preview.camera.orthographicSize = size * 0.75f;
        preview.camera.transform.position = new Vector3(bounds.center.x, bounds.center.y, -10f);
        preview.camera.transform.rotation = Quaternion.identity;

        preview.lights[0].intensity = 1.2f;
        preview.lights[0].transform.rotation = Quaternion.Euler(30f, 30f, 0f);
        preview.lights[1].intensity = 1f;

        preview.camera.Render();

        Texture result = preview.EndPreview();
        GUI.DrawTexture(rect, result, ScaleMode.StretchToFill, false);
    }

    private void RebuildPreview()
    {
        if (preview == null)
            return;

        if (previewObject != null)
        {
            DestroyImmediate(previewObject);
            previewObject = null;
        }

        WeaponVisualProfile profile = (WeaponVisualProfile)target;

        if (profile.weaponPrefab == null)
            return;

        previewObject = preview.InstantiatePrefabInScene(profile.weaponPrefab);
        previewObject.transform.position = Vector3.zero;
        previewObject.transform.rotation = Quaternion.identity;
        previewObject.transform.localScale = Vector3.one;

        previewObject.hideFlags = HideFlags.HideAndDontSave;

        foreach (Transform child in previewObject.GetComponentsInChildren<Transform>(true))
            child.gameObject.hideFlags = HideFlags.HideAndDontSave;

        WeaponVisualController controller = previewObject.GetComponent<WeaponVisualController>();

        if (controller != null)
            controller.ApplyProfile(profile);

        Repaint();
    }

    private Bounds GetBounds()
    {
        Renderer[] renderers = previewObject.GetComponentsInChildren<Renderer>(true);
        Bounds bounds = new Bounds(Vector3.zero, Vector3.one * 0.5f);
        bool found = false;

        foreach (Renderer renderer in renderers)
        {
            if (!renderer.enabled || !renderer.gameObject.activeInHierarchy)
                continue;

            if (!found)
            {
                bounds = renderer.bounds;
                found = true;
            }
            else
                bounds.Encapsulate(renderer.bounds);
        }

        return bounds;
    }
}
