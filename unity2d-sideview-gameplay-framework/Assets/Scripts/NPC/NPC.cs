using UnityEngine;
using UnityEngine.EventSystems;

public class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] private NPCData npcData;

    public NPCData Data => npcData;

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Camera.main == null)
            return;

        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePosition, LayerMask.GetMask("NPC"));

        foreach (Collider2D hit in hits)
        {
            NPC npc = hit.GetComponentInParent<NPC>();

            if (npc != null)
            {
                npc.Interact();
                return;
            }
        }
    }

    public void Interact()
    {
        if (npcData == null)
        {
            Debug.LogError("NPC has no NPCData assigned!", this);
            return;
        }

        NPCDialogueManager manager = FindFirstObjectByType<NPCDialogueManager>();

        if (manager != null)
            manager.Open(this);
        else
            Debug.LogError("NPCDialogueManager not found in scene!");
    }
}