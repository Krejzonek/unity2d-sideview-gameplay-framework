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