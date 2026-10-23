using UnityEngine;

public abstract class NPCActivityData : ScriptableObject
{
    public string activityName;
    [TextArea(2, 5)] public string description;

    public abstract void Execute(NPCActivityContext context);
}