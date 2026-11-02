
using System.Collections.Generic;
using UnityEngine;

public class WeaponVisualController : MonoBehaviour
{
    [Header("Colliders")]
    [SerializeField] private BoxCollider2D bladeCollider;

    private readonly Dictionary<string, Transform> partTransforms = new Dictionary<string, Transform>();
    private readonly Dictionary<Transform, Vector3> basePositions = new Dictionary<Transform, Vector3>();
    private readonly Dictionary<Transform, Vector3> baseScales = new Dictionary<Transform, Vector3>();
    private readonly Dictionary<Transform, float> baseRotations = new Dictionary<Transform, float>();
    private readonly Dictionary<Transform, bool> baseActiveStates = new Dictionary<Transform, bool>();
    private readonly Dictionary<Transform, Sprite> baseSprites = new Dictionary<Transform, Sprite>();
    private bool baseTransformsCached;

    private void Awake()
    {
        CacheBaseTransforms();
    }

    private void CacheBaseTransforms()
    {
        partTransforms.Clear();
        basePositions.Clear();
        baseScales.Clear();
        baseRotations.Clear();
        baseActiveStates.Clear();
        baseSprites.Clear();

        foreach (Transform part in GetComponentsInChildren<Transform>(true))
        {
            if (part != transform)
                partTransforms[part.name] = part;

            basePositions[part] = part.localPosition;
            baseScales[part] = part.localScale;
            baseRotations[part] = part.localEulerAngles.z;
            baseActiveStates[part] = part.gameObject.activeSelf;

            SpriteRenderer renderer = part.GetComponent<SpriteRenderer>();

            if (renderer != null)
                baseSprites[part] = renderer.sprite;
        }

        baseTransformsCached = true;
    }

    public void ApplyProfile(WeaponVisualProfile profile)
    {
        if (profile == null)
            return;

        if (!baseTransformsCached)
            CacheBaseTransforms();

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0f, 0f, profile.gripRotation);
        transform.localScale = new Vector3(
            Mathf.Approximately(profile.gripScale.x, 0f) ? 1f : profile.gripScale.x,
            Mathf.Approximately(profile.gripScale.y, 0f) ? 1f : profile.gripScale.y,
            1f
        );

        foreach (KeyValuePair<Transform, Vector3> part in basePositions)
        {
            part.Key.localPosition = part.Value;
            part.Key.localScale = baseScales[part.Key];
            part.Key.localRotation = Quaternion.Euler(0f, 0f, baseRotations[part.Key]);
            part.Key.gameObject.SetActive(baseActiveStates[part.Key]);

            SpriteRenderer renderer = part.Key.GetComponent<SpriteRenderer>();

            if (renderer != null)
            {
                renderer.enabled = true;

                if (baseSprites.TryGetValue(part.Key, out Sprite sprite))
                    renderer.sprite = sprite;
            }
        }

        foreach (WeaponPartSettings settings in profile.parts)
        {
            if (settings == null || string.IsNullOrWhiteSpace(settings.partName))
                continue;

            if (!partTransforms.TryGetValue(settings.partName, out Transform part))
            {
                Debug.LogWarning($"[WeaponVisualController] Part '{settings.partName}' was not found on '{name}'.", this);
                continue;
            }

            ApplyPart(part, settings);
        }

        if (partTransforms.TryGetValue("Handle", out Transform handle))
        {
            Vector3 handleOffset = Vector3.Scale(handle.localPosition, transform.localScale);
            handleOffset = transform.localRotation * handleOffset;
            transform.localPosition = (Vector3)profile.gripPosition - handleOffset;
        }
        else
            transform.localPosition = profile.gripPosition;

        if (bladeCollider != null)
        {
            bool isSword = profile.weaponType == WeaponType.Sword;
            bladeCollider.enabled = isSword;

            if (isSword && partTransforms.TryGetValue("Blade", out Transform blade))
            {
                SpriteRenderer spriteRenderer = blade.GetComponent<SpriteRenderer>();

                if (spriteRenderer != null && spriteRenderer.sprite != null)
                {
                    Bounds bounds = spriteRenderer.sprite.bounds;
                    bladeCollider.size = bounds.size;
                    bladeCollider.offset = Vector2.zero;
                }
            }
        }
    }

    private void ApplyPart(Transform part, WeaponPartSettings settings)
    {
        if (part == null || !basePositions.ContainsKey(part))
            return;

        part.gameObject.SetActive(settings.enabled);

        SpriteRenderer spriteRenderer = part.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && settings.texture != null)
            spriteRenderer.sprite = settings.texture;

        Vector3 basePosition = basePositions[part];
        Vector3 baseScale = baseScales[part];

        float length = Mathf.Approximately(settings.length, 0f) ? 1f : Mathf.Abs(settings.length);
        float width = Mathf.Approximately(settings.width, 0f) ? 1f : Mathf.Abs(settings.width);
        float lengthDifference = baseScale.y * (length - 1f);
        float direction = settings.partName == "Handle" ? -1f : 1f;

        part.localPosition = basePosition + new Vector3(settings.localPosition.x, settings.localPosition.y + lengthDifference * 0.5f * direction, 0f);
        part.localRotation = Quaternion.Euler(0f, 0f, baseRotations[part] + settings.localRotation);
        part.localScale = new Vector3(baseScale.x * width, baseScale.y * length, baseScale.z);
    }
}
