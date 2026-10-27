using UnityEngine;

[CreateAssetMenu(fileName = "ClassTier", menuName = "Game/Class Tier")]
public class ClassTier : ScriptableObject
{
    public Class classData;
    [Min(0)] public int tier;
    public Class[] nextClasses;
}