
using UnityEngine;

[CreateAssetMenu(fileName = "New NPC Dialogue", menuName = "NPC/Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string dialogueId;
    [TextArea(2, 5)] public string text;
}
