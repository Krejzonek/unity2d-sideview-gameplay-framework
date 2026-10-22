using System;
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC", menuName = "NPC/NPC Data")]
public class NPCData : ScriptableObject
{
    public string npcName;
    public Sprite portrait;
    [TextArea(2, 5)] public string greeting;
    public NPCDialogue[] dialogues;
    public NPCActivityData[] activities;
}

[Serializable]
public class NPCDialogue
{
    public string dialogueId;
    [TextArea(2, 5)] public string text;
    public string[] responses;
}

[Serializable]
public class NPCActivityData
{
    public string activityName;
    public string description;
    public NPCActivityType activityType;
}

public enum NPCActivityType
{
    Dialogue,
    Blacksmith,
    Shop,
    Quest
}