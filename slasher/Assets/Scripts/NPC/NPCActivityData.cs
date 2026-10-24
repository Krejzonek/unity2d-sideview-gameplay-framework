using UnityEngine;

public abstract class NPCActivityData : ScriptableObject
{
    public string activityName;

    public abstract void Execute(NPCActivityContext context);
}