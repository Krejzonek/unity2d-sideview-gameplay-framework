using UnityEngine;

[CreateAssetMenu(fileName = "SpecialRoom", menuName = "Special Room")]
public class SpecialRoomData : ScriptableObject
{
    [Min(1)] public int roomNumber;
    public GameObject roomPrefab;
}