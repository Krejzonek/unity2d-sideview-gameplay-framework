using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogueManager : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text greetingText;
    [SerializeField] private Transform activityContainer;
    [SerializeField] private Button activityButtonPrefab;
    [SerializeField] private Button closeButton;

    private NPC currentNPC;

    private void Awake()
    {
        dialoguePanel.SetActive(false);
        closeButton.onClick.AddListener(Close);
    }

    public void Open(NPC npc)
    {
        if (npc == null || npc.Data == null)
            return;

        currentNPC = npc;
        dialoguePanel.SetActive(true);
        npcNameText.text = npc.Data.npcName;
        greetingText.text = npc.Data.greeting;

        ClearActivities();

        foreach (NPCActivityData activity in npc.Data.activities)
        {
            if (activity == null)
                continue;

            Button button = Instantiate(activityButtonPrefab, activityContainer);
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();

            if (text != null)
                text.text = activity.activityName;

            button.onClick.AddListener(() => SelectActivity(activity));
        }
    }

    private void SelectActivity(NPCActivityData activity)
    {
        if (activity == null || currentNPC == null)
            return;

        switch (activity.activityType)
        {
            case NPCActivityType.Dialogue:
                greetingText.text = activity.description;
                break;

            case NPCActivityType.Blacksmith:
                greetingText.text = "Blacksmith activity will be implemented next.";
                break;

            case NPCActivityType.Shop:
                greetingText.text = "Shop activity will be implemented next.";
                break;

            case NPCActivityType.Quest:
                greetingText.text = "Quest activity will be implemented next.";
                break;
        }
    }

    private void ClearActivities()
    {
        foreach (Transform child in activityContainer)
            Destroy(child.gameObject);
    }

    public void Close()
    {
        dialoguePanel.SetActive(false);
        currentNPC = null;
        ClearActivities();
    }

    public NPC GetCurrentNPC()
    {
        return currentNPC;
    }
}