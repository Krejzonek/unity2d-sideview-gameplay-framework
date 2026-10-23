public class NPCActivityContext
{
    public NPC NPC { get; }
    public NPCDialogueManager UI { get; }

    public NPCActivityContext(NPC npc, NPCDialogueManager ui)
    {
        NPC = npc;
        UI = ui;
    }

    public void ShowText(string text)
    {
        if (UI != null)
            UI.ShowActivityText(text);
    }
}