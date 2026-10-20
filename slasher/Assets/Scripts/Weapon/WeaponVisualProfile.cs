using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponPartSettings
{
    public string partName;
    public bool enabled = true;
    public Sprite texture;
    [Range(0.1f, 3f)] public float length = 1f;
    [Range(0.1f, 3f)] public float width = 1f;
    public Vector2 localPosition;
    public float localRotation;
}

public enum WeaponHand
{
    Left,
    Right
}

[CreateAssetMenu(fileName = "New Weapon Visual Profile", menuName = "Weapons/Visual Profile")]
public class WeaponVisualProfile : ScriptableObject
{
    public GameObject weaponPrefab;

    [Header("Weapon Type")]
    public WeaponType weaponType = WeaponType.Sword;

    [Header("Hand")]
    public WeaponHand hand = WeaponHand.Right;

    [Header("Parts")]
    public List<WeaponPartSettings> parts = new List<WeaponPartSettings>();

    [Header("Grip")]
    public Vector2 gripPosition;
    public float gripRotation;
    public Vector2 gripScale = Vector2.one;
}