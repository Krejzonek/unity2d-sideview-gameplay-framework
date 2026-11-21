using System.Collections.Generic;
using UnityEngine;

public enum DialogueSpeaker
{
    Player,
    Enemy
}

[System.Serializable]
public class DialogueEntry
{
    public DialogueSpeaker speaker;

    [TextArea(2, 5)]
    public string text;
}

[CreateAssetMenu(fileName = "NewDialogue", menuName = "Game/Dialogue")]
public class DialogueLine : ScriptableObject
{
    public List<DialogueEntry> entries = new();
}