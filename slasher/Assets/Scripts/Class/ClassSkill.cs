using UnityEngine;

public abstract class ClassSkill : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
}