
using UnityEngine;

[System.Serializable]
public class WeaponPartSettings
{
    public bool enabled = true;
    public Sprite texture;
    [Range(0.1f, 3f)] public float length = 1f;
    [Range(0.1f, 3f)] public float width = 1f;
    public Vector2 localPosition;
    public float localRotation;
}

[CreateAssetMenu(fileName = "New Weapon Visual Profile", menuName = "Weapons/Visual Profile")]
public class WeaponVisualProfile : ScriptableObject
{
    public GameObject weaponPrefab;

    [Header("Handle")]
    public WeaponPartSettings handle = new WeaponPartSettings();

    [Header("Guard")]
    public WeaponPartSettings guard = new WeaponPartSettings();

    [Header("Blade")]
    public WeaponPartSettings blade = new WeaponPartSettings();

    [Header("Grip")]
    public Vector2 gripPosition;
    public float gripRotation;
    public Vector2 gripScale = Vector2.one;
}
