using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCDialogueManager : MonoBehaviour
{
    [Header("Main UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text greetingText;
    [SerializeField] private Transform activityContainer;
    [SerializeField] private Button activityButtonPrefab;
    [SerializeField] private Button closeButton;

    [Header("Blacksmith UI")]
    [SerializeField] private GameObject blacksmithView;
    [SerializeField] private Transform blacksmithItemsContainer;
    [SerializeField] private NPCBlacksmithItemButton blacksmithItemButtonPrefab;
    [SerializeField] private Image selectedItem1Image;
    [SerializeField] private Image selectedItem2Image;
    [SerializeField] private TMP_Text blacksmithStatusText;
    [SerializeField] private Button upgradeButton;
    [SerializeField] private Image upgradeButtonImage;
    [SerializeField] private Button backButton;
    [SerializeField] private Color upgradeDisabledColor = Color.gray;
    [SerializeField] private Color upgradeReadyColor = Color.green;

    [Header("Trader UI")]
    [SerializeField] private GameObject traderView;
    [SerializeField] private Transform traderItemsContainer;
    [SerializeField] private NPCTradeButton traderItemButtonPrefab;
    [SerializeField] private Button traderBackButton;

    private NPC currentNPC;
    private NPCBlacksmithActivity currentBlacksmith;
    private NPCTraderActivity currentTrader;
    private readonly List<int> selectedInventoryIndices = new();
    private readonly Dictionary<NPCData, NPCDialogue> lastDialogues = new();

    private EquipmentManager Equipment => FindFirstObjectByType<EquipmentManager>();

    private void Awake()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        if (blacksmithView != null)
            blacksmithView.SetActive(false);

        if (traderView != null)
            traderView.SetActive(false);

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);

        if (backButton != null)
            backButton.onClick.AddListener(BackToActivities);

        if (traderBackButton != null)
            traderBackButton.onClick.AddListener(BackToActivities);

        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(UpgradeBlacksmithItems);

        if (selectedItem1Image != null)
            selectedItem1Image.enabled = true;

        if (selectedItem2Image != null)
            selectedItem2Image.enabled = true;
    }

    public void Open(NPC npc)
    {
        if (npc == null || npc.Data == null)
            return;

        currentNPC = npc;
        currentBlacksmith = null;
        currentTrader = null;
        selectedInventoryIndices.Clear();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        if (npcNameText != null)
            npcNameText.text = npc.Data.npcName;

        if (greetingText != null)
            greetingText.text = npc.Data.greeting;

        ShowActivityView();
        BuildActivities();
    }

    private void BuildActivities()
    {
        ClearContainer(activityContainer);

        if (currentNPC == null || currentNPC.Data == null ||
            activityButtonPrefab == null || activityContainer == null)
            return;

        Button talkButton = Instantiate(activityButtonPrefab, activityContainer);
        TMP_Text talkText = talkButton.GetComponentInChildren<TMP_Text>();

        if (talkText != null)
            talkText.text = "Talk";

        talkButton.onClick.AddListener(ShowRandomDialogue);

        if (currentNPC.Data.activities == null)
            return;

        foreach (NPCActivityData activity in currentNPC.Data.activities)
        {
            if (activity == null)
                continue;

            if (string.Equals(activity.activityName, "Dialogs", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(activity.activityName, "Talk", StringComparison.OrdinalIgnoreCase))
                continue;

            Button button = Instantiate(activityButtonPrefab, activityContainer);
            TMP_Text text = button.GetComponentInChildren<TMP_Text>();

            if (text != null)
                text.text = activity.activityName;

            NPCActivityData selectedActivity = activity;
            button.onClick.AddListener(() => SelectActivity(selectedActivity));
        }
    }

    private void ShowRandomDialogue()
    {
        if (currentNPC == null || currentNPC.Data == null ||
            currentNPC.Data.dialogues == null || currentNPC.Data.dialogues.Length == 0)
        {
            if (greetingText != null)
                greetingText.text = "I have nothing to tell you right now.";

            return;
        }

        NPCData data = currentNPC.Data;
        List<NPCDialogue> available = new();

        foreach (NPCDialogue dialogue in data.dialogues)
        {
            if (dialogue != null)
                available.Add(dialogue);
        }

        if (available.Count == 0)
        {
            if (greetingText != null)
                greetingText.text = "I have nothing to tell you right now.";

            return;
        }

        if (available.Count > 1 &&
            lastDialogues.TryGetValue(data, out NPCDialogue last))
            available.Remove(last);

        NPCDialogue selected = available[UnityEngine.Random.Range(0, available.Count)];
        lastDialogues[data] = selected;

        if (greetingText != null)
            greetingText.text = selected.text;
    }

    private void SelectActivity(NPCActivityData activity)
    {
        if (activity == null || currentNPC == null)
            return;

        selectedInventoryIndices.Clear();
        currentBlacksmith = null;
        currentTrader = null;

        activity.Execute(new NPCActivityContext(currentNPC, this));
    }

    public void OpenBlacksmith(NPCBlacksmithActivity activity)
    {
        if (activity == null)
            return;

        if (Equipment == null)
        {
            ShowActivityText("Equipment system not found.");
            return;
        }

        currentBlacksmith = activity;
        currentTrader = null;
        selectedInventoryIndices.Clear();

        if (npcNameText != null)
            npcNameText.gameObject.SetActive(false);

        if (greetingText != null)
            greetingText.gameObject.SetActive(false);

        if (activityContainer != null)
            activityContainer.gameObject.SetActive(false);

        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        if (blacksmithView != null)
            blacksmithView.SetActive(true);

        if (traderView != null)
            traderView.SetActive(false);

        if (blacksmithStatusText != null)
            blacksmithStatusText.text = activity.openingText;

        RefreshBlacksmithUI();
    }

    private void RefreshBlacksmithUI()
    {
        EquipmentManager equipment = Equipment;

        if (equipment == null || currentBlacksmith == null ||
            blacksmithItemsContainer == null || blacksmithItemButtonPrefab == null)
            return;

        List<ItemData> inventory = equipment.GetInventory();

        if (inventory == null)
            return;

        for (int i = selectedInventoryIndices.Count - 1; i >= 0; i--)
        {
            int index = selectedInventoryIndices[i];

            if (index < 0 || index >= inventory.Count || inventory[index] == null)
                selectedInventoryIndices.RemoveAt(i);
        }

        if (selectedInventoryIndices.Count == 2)
        {
            ItemData first = inventory[selectedInventoryIndices[0]];
            ItemData second = inventory[selectedInventoryIndices[1]];

            if (first == null || second == null ||
                first.GetRarity() != second.GetRarity() ||
                first.equipmentType != second.equipmentType ||
                first.GetRarity() == ItemRarity.Godest)
                selectedInventoryIndices.RemoveAt(1);
        }

        SetSelectedSlot(selectedItem1Image,
            selectedInventoryIndices.Count > 0 ? inventory[selectedInventoryIndices[0]] : null);

        SetSelectedSlot(selectedItem2Image,
            selectedInventoryIndices.Count > 1 ? inventory[selectedInventoryIndices[1]] : null);

        ClearContainer(blacksmithItemsContainer);

        ItemData firstSelected = selectedInventoryIndices.Count > 0
            ? inventory[selectedInventoryIndices[0]]
            : null;

        for (int i = 0; i < inventory.Count; i++)
        {
            ItemData item = inventory[i];

            if (item == null)
                continue;

            int index = i;
            bool selected = selectedInventoryIndices.Contains(index);
            bool compatible = item.GetRarity() != ItemRarity.Godest;

            if (firstSelected != null)
            {
                compatible &= item.GetRarity() == firstSelected.GetRarity() &&
                    item.equipmentType == firstSelected.equipmentType;
            }

            if (selectedInventoryIndices.Count >= 2 && !selected)
                compatible = false;

            NPCBlacksmithItemButton itemButton =
                Instantiate(blacksmithItemButtonPrefab, blacksmithItemsContainer);

            if (itemButton == null)
                continue;

            itemButton.SetItem(item, selected, compatible);

            if (itemButton.Button != null)
            {
                itemButton.Button.interactable = true;
                itemButton.Button.onClick.AddListener(() => SelectBlacksmithItem(index));
            }
        }

        bool ready = selectedInventoryIndices.Count == 2;

        if (ready)
        {
            ItemData first = inventory[selectedInventoryIndices[0]];
            ItemData second = inventory[selectedInventoryIndices[1]];

            ready = first != null && second != null &&
                first.GetRarity() == second.GetRarity() &&
                first.equipmentType == second.equipmentType &&
                first.GetRarity() != ItemRarity.Godest;
        }

        if (upgradeButton != null)
            upgradeButton.interactable = ready;

        if (upgradeButtonImage != null)
            upgradeButtonImage.color = ready ? upgradeReadyColor : upgradeDisabledColor;
    }

    private void SetSelectedSlot(Image slot, ItemData item)
    {
        if (slot == null)
            return;

        slot.enabled = true;
        slot.sprite = item != null ? item.icon : null;
        slot.preserveAspect = true;
        slot.color = Color.white;
    }

    private void SelectBlacksmithItem(int index)
    {
        EquipmentManager equipment = Equipment;

        if (equipment == null || currentBlacksmith == null)
            return;

        List<ItemData> inventory = equipment.GetInventory();

        if (inventory == null || index < 0 || index >= inventory.Count || inventory[index] == null)
            return;

        if (selectedInventoryIndices.Contains(index))
        {
            selectedInventoryIndices.Remove(index);
            UpdateBlacksmithStatus();
            RefreshBlacksmithUI();
            return;
        }

        if (selectedInventoryIndices.Count >= 2)
            return;

        ItemData item = inventory[index];

        if (item.GetRarity() == ItemRarity.Godest)
            return;

        if (selectedInventoryIndices.Count > 0)
        {
            ItemData first = inventory[selectedInventoryIndices[0]];

            if (first == null ||
                first.GetRarity() != item.GetRarity() ||
                first.equipmentType != item.equipmentType)
                return;
        }

        selectedInventoryIndices.Add(index);
        UpdateBlacksmithStatus();
        RefreshBlacksmithUI();
    }

    private void UpdateBlacksmithStatus()
    {
        if (blacksmithStatusText == null)
            return;

        if (selectedInventoryIndices.Count == 0)
            blacksmithStatusText.text = currentBlacksmith != null
                ? currentBlacksmith.openingText
                : "";
        else if (selectedInventoryIndices.Count == 1)
            blacksmithStatusText.text = "Selected 1 of 2 items. Choose one more.";
        else
            blacksmithStatusText.text = "Ready to upgrade.";
    }

    private void UpgradeBlacksmithItems()
    {
        if (currentBlacksmith == null || selectedInventoryIndices.Count != 2)
            return;

        EquipmentManager equipment = Equipment;

        if (equipment == null)
            return;

        List<ItemData> inventory = new List<ItemData>(equipment.GetInventory());
        int firstIndex = selectedInventoryIndices[0];
        int secondIndex = selectedInventoryIndices[1];

        if (firstIndex < 0 || secondIndex < 0 ||
            firstIndex >= inventory.Count || secondIndex >= inventory.Count)
        {
            selectedInventoryIndices.Clear();
            RefreshBlacksmithUI();
            return;
        }

        ItemData first = inventory[firstIndex];
        ItemData second = inventory[secondIndex];

        if (first == null || second == null ||
            first.GetRarity() != second.GetRarity() ||
            first.equipmentType != second.equipmentType ||
            first.GetRarity() == ItemRarity.Godest)
        {
            selectedInventoryIndices.Clear();
            RefreshBlacksmithUI();
            return;
        }

        ItemRarity nextRarity = (ItemRarity)((int)first.GetRarity() + 1);
        ItemData result = Instantiate(first);

        result.name = first.name + "_Upgraded";
        result.SetRarity(nextRarity);

        int highIndex = Mathf.Max(firstIndex, secondIndex);
        int lowIndex = Mathf.Min(firstIndex, secondIndex);

        inventory.RemoveAt(highIndex);
        inventory.RemoveAt(lowIndex);
        inventory.Insert(lowIndex, result);

        selectedInventoryIndices.Clear();

        equipment.SetInventory(inventory);
        equipment.SaveAllEquipment();

        RefreshBlacksmithUI();

        if (blacksmithStatusText != null)
            blacksmithStatusText.text = "Upgrade successful! You received " + result.itemName + ".";
    }

    public void OpenTrader(NPCTraderActivity activity)
    {
        if (activity == null)
            return;

        currentTrader = activity;
        currentBlacksmith = null;
        selectedInventoryIndices.Clear();

        if (dialoguePanel != null)
            dialoguePanel.SetActive(true);

        ShowTraderView();

        RefreshTraderUI();
    }

    private void ShowTraderView()
    {
        if (npcNameText != null)
            npcNameText.gameObject.SetActive(false);

        if (greetingText != null)
            greetingText.gameObject.SetActive(false);

        if (activityContainer != null)
            activityContainer.gameObject.SetActive(false);

        if (closeButton != null)
            closeButton.gameObject.SetActive(false);

        if (blacksmithView != null)
            blacksmithView.SetActive(false);

        if (traderView != null)
            traderView.SetActive(true);
    }

    private void RefreshTraderUI()
    {
        foreach (Transform child in traderItemsContainer)
            Destroy(child.gameObject);

        if (currentTrader == null || currentTrader.trades == null)
            return;

        foreach (NPCTradeData trade in currentTrader.trades)
        {
            if (trade == null || !currentTrader.CanTrade(trade))
                continue;

            NPCTradeButton tradeButton = Instantiate(traderItemButtonPrefab, traderItemsContainer);
            tradeButton.Setup(trade, true, () => ExecuteTraderTrade(trade));
        }
    }

    private void ExecuteTraderTrade(NPCTradeData trade)
    {
        if (currentTrader == null || trade == null)
            return;

        if (!currentTrader.CanTrade(trade))
        {
            RefreshTraderUI();
            return;
        }

        bool success = currentTrader.ExecuteTrade(trade);

        RefreshTraderUI();
    }

    private void ShowActivityView()
    {
        if (npcNameText != null)
            npcNameText.gameObject.SetActive(true);

        if (greetingText != null)
            greetingText.gameObject.SetActive(true);

        if (activityContainer != null)
            activityContainer.gameObject.SetActive(true);

        if (closeButton != null)
            closeButton.gameObject.SetActive(true);

        if (blacksmithView != null)
            blacksmithView.SetActive(false);

        if (traderView != null)
            traderView.SetActive(false);
    }

    private void BackToActivities()
    {
        if (currentNPC == null)
        {
            Close();
            return;
        }

        currentBlacksmith = null;
        currentTrader = null;
        selectedInventoryIndices.Clear();

        ClearContainer(blacksmithItemsContainer);
        ClearContainer(traderItemsContainer);

        if (npcNameText != null)
            npcNameText.text = currentNPC.Data.npcName;

        if (greetingText != null)
            greetingText.text = currentNPC.Data.greeting;

        ShowActivityView();
        BuildActivities();
    }

    public void ShowActivityText(string text)
    {
        if (blacksmithView != null && blacksmithView.activeSelf && blacksmithStatusText != null)
            blacksmithStatusText.text = text;
        else if (greetingText != null)
            greetingText.text = text;
    }

    private void ClearContainer(Transform container)
    {
        if (container == null)
            return;

        foreach (Transform child in container)
            Destroy(child.gameObject);
    }

    public void Close()
    {
        if (dialoguePanel != null)
            dialoguePanel.SetActive(false);

        currentNPC = null;
        currentBlacksmith = null;
        currentTrader = null;
        selectedInventoryIndices.Clear();

        ClearContainer(activityContainer);
        ClearContainer(blacksmithItemsContainer);
        ClearContainer(traderItemsContainer);

        if (blacksmithView != null)
            blacksmithView.SetActive(false);

        if (traderView != null)
            traderView.SetActive(false);
    }

    public NPC GetCurrentNPC()
    {
        return currentNPC;
    }
}