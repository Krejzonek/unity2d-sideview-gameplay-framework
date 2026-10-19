
using UnityEngine;

public class WeaponVisualController : MonoBehaviour
{
    [Header("Parts")]
    [SerializeField] private Transform handle;
    [SerializeField] private Transform guard;
    [SerializeField] private Transform blade;

    [Header("Colliders")]
    [SerializeField] private BoxCollider2D bladeCollider;

    private Vector3 handleBasePosition;
    private Vector3 handleBaseScale;
    private float handleBaseRotation;
    private Vector3 guardBasePosition;
    private Vector3 guardBaseScale;
    private float guardBaseRotation;
    private Vector3 bladeBasePosition;
    private Vector3 bladeBaseScale;
    private float bladeBaseRotation;
    private bool baseTransformsCached;

    private void Awake()
    {
        CacheBaseTransforms();
    }

    private void CacheBaseTransforms()
    {
        CachePart(handle, out handleBasePosition, out handleBaseScale, out handleBaseRotation);
        CachePart(guard, out guardBasePosition, out guardBaseScale, out guardBaseRotation);
        CachePart(blade, out bladeBasePosition, out bladeBaseScale, out bladeBaseRotation);
        baseTransformsCached = true;
    }

    private void CachePart(Transform part, out Vector3 position, out Vector3 scale, out float rotation)
    {
        position = part != null ? part.localPosition : Vector3.zero;
        scale = part != null ? part.localScale : Vector3.one;
        rotation = part != null ? part.localEulerAngles.z : 0f;
    }

    public void ApplyProfile(WeaponVisualProfile profile)
    {
        if (profile == null)
            return;

        if (!baseTransformsCached)
            CacheBaseTransforms();

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.Euler(0f, 0f, profile.gripRotation);
        transform.localScale = new Vector3(profile.gripScale.x, profile.gripScale.y, 1f);

        ApplyPart(handle, profile.handle, handleBasePosition, handleBaseScale, handleBaseRotation, -1f);
        ApplyPart(guard, profile.guard, guardBasePosition, guardBaseScale, guardBaseRotation, 0f);
        ApplyPart(blade, profile.blade, bladeBasePosition, bladeBaseScale, bladeBaseRotation, 1f);

        if (handle != null)
        {
            Vector3 handleOffset = Vector3.Scale(handle.localPosition, transform.localScale);
            handleOffset = transform.localRotation * handleOffset;
            transform.localPosition = (Vector3)profile.gripPosition - handleOffset;
        }
        else
        {
            transform.localPosition = profile.gripPosition;
        }

        if (bladeCollider != null)
        {
            bool isSword = profile.weaponType == WeaponType.Sword;
            bladeCollider.enabled = isSword && blade != null && profile.blade.enabled;

            if (bladeCollider.enabled)
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

    private void ApplyPart(Transform part, WeaponPartSettings settings, Vector3 basePosition, Vector3 baseScale, float baseRotation, float lengthDirection)
    {
        if (part == null || settings == null)
            return;

        part.gameObject.SetActive(settings.enabled);

        SpriteRenderer spriteRenderer = part.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null && settings.texture != null)
            spriteRenderer.sprite = settings.texture;

        float lengthDifference = baseScale.y * (settings.length - 1f);

        part.localPosition = basePosition + new Vector3(settings.localPosition.x, settings.localPosition.y + lengthDifference * 0.5f * lengthDirection, 0f);
        part.localRotation = Quaternion.Euler(0f, 0f, baseRotation + settings.localRotation);
        part.localScale = new Vector3(baseScale.x * settings.width, baseScale.y * settings.length, baseScale.z);
    }
}
