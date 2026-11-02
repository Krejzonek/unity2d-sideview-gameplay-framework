using UnityEngine;

[CreateAssetMenu(fileName = "ClassTier", menuName = "Class/Class Tier")]
public class ClassTier : ScriptableObject
{
    public Class classData;
    [Min(0)] public int tier;
    public Class[] nextClasses;
}