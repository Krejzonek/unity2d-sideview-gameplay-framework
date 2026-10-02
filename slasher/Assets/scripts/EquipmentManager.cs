using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private GameObject itemSelectionPanel;
    [SerializeField] private Transform itemListContent;
    [SerializeField] private GameObject itemButtonPrefab;
    [SerializeField] private EquipmentSlot[] equipmentSlots;
    [SerializeField] private Button closeButton;


    [SerializeField] private List<ItemData> inventory = new();

    private EquipmentSlot selectedSlot;
    private Player currentPlayer;

    private void Awake()
    {
        FindUI();
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseItemList);
    }

    private void Start()
    {
        UpdateCurrentPlayer();
        RefreshEquipmentUI();
    }

    private void Update()
    {
        Player player = TeamManager.Instance != null ? TeamManager.Instance.CurrentPlayer : null;

        if (player != currentPlayer)
        {
            UpdateCurrentPlayer();
            RefreshEquipmentUI();
        }
    }

    private void UpdateCurrentPlayer()
    {
        currentPlayer = TeamManager.Instance != null ? TeamManager.Instance.CurrentPlayer : null;
    }

    private void FindUI()
    {
        if (itemSelectionPanel == null)
        {
            GameObject panel = GameObject.Find("ItemSelectionPanel");

            if (panel != null)
                itemSelectionPanel = panel;
        }

        if (itemListContent == null && itemSelectionPanel != null)
        {
            Transform content = itemSelectionPanel.transform.Find("ItemListContent");

            if (content != null)
                itemListContent = content;
        }
    }

    public void OpenItemList(EquipmentSlot slot)
    {
        if (itemSelectionPanel == null)
            FindUI();

        if (itemSelectionPanel == null || itemListContent == null)
            return;

        UpdateCurrentPlayer();

        if (currentPlayer == null)
            return;

        selectedSlot = slot;
        currentPlayer.SetCanAttack(false);
        itemSelectionPanel.SetActive(true);

        RefreshItemList();
    }

    public void RefreshItemList()
    {
        if (itemListContent == null)
            return;

        foreach (Transform child in itemListContent)
            Destroy(child.gameObject);

        if (selectedSlot == null)
            return;

        ItemData equippedItem = GetEquippedItem(selectedSlot.Type);
        float equippedPower = equippedItem != null ? GetEquipmentPower(equippedItem) : 0f;

        List<ItemData> availableItems = new List<ItemData>();

        foreach (ItemData item in inventory)
        {
            if (item == null)
                continue;

            if (IsItemEquippedByOtherCharacter(item))
                continue;

            if (!selectedSlot.CanEquip(item))
                continue;

            availableItems.Add(item);
        }

        availableItems = availableItems.OrderByDescending(item => GetEquipmentPower(item) >= equippedPower).ThenByDescending(item => GetEquipmentPower(item)).ToList();

        foreach (ItemData item in availableItems)
        {
            GameObject button = Instantiate(itemButtonPrefab, itemListContent);

            ItemButton itemButton = button.GetComponent<ItemButton>();

            if (itemButton != null)
                itemButton.Setup(item);
        }
    }

    private bool IsItemEquippedByOtherCharacter(ItemData item)
    {
        if (item == null || CharacterManager.Instance == null || currentPlayer == null)
            return false;

        CharacterData currentCharacter = currentPlayer.GetCharacterData();

        foreach (CharacterData character in CharacterManager.Instance.Characters)
        {
            if (character == null || character == currentCharacter)
                continue;

            if (character.equippedItems != null && character.equippedItems.Contains(item))
                return true;
        }

        return false;
    }

    public void CloseItemList()
    {
        if (itemSelectionPanel == null)
            FindUI();

        if (itemSelectionPanel != null)
            itemSelectionPanel.SetActive(false);

        selectedSlot = null;

        if (currentPlayer != null)
            currentPlayer.SetCanAttack(true);
    }

    public void SelectItem(ItemData item)
    {
        if (selectedSlot == null)
            return;

        if (!selectedSlot.CanEquip(item))
            return;

        if (IsItemEquippedByOtherCharacter(item))
            return;

        selectedSlot.Equip(item);

        CloseItemList();
    }

    public ItemData GetEquippedItem(EquipmentType type)
    {
        UpdateCurrentPlayer();

        if (currentPlayer == null || currentPlayer.GetCharacterData() == null)
            return null;

        foreach (ItemData item in currentPlayer.GetCharacterData().equippedItems)
        {
            if (item != null && item.equipmentType == type)
                return item;
        }

        return null;
    }

    public void EquipItem(EquipmentSlot slot, ItemData item)
    {
        UpdateCurrentPlayer();

        if (currentPlayer == null || currentPlayer.GetCharacterData() == null)
            return;

        if (slot == null || item == null)
            return;

        if (IsItemEquippedByOtherCharacter(item))
            return;

        if (!slot.CanEquip(item))
            return;

        CharacterData character = currentPlayer.GetCharacterData();

        ItemData oldItem = GetEquippedItem(slot.Type);

        if (oldItem != null)
            inventory.Add(oldItem);

        character.equippedItems.RemoveAll(x => x != null && x.equipmentType == slot.Type);
        character.equippedItems.Add(item);

        inventory.Remove(item);

        currentPlayer.GetStats().UpdateStats();

        RefreshEquipmentUI();
        CloseItemList();
    }

    public void RefreshEquipmentUI()
    {
        UpdateCurrentPlayer();

        if (currentPlayer == null)
            return;

        foreach (EquipmentSlot slot in equipmentSlots)
        {
            if (slot != null)
                slot.Refresh();
        }
    }

    public void AddToInventory(ItemData item)
    {
        if (item == null)
            return;

        inventory.Add(item);

        BetterItemPopup popup = FindFirstObjectByType<BetterItemPopup>();

        if (popup != null)
            popup.Show(item);
    }

    public void RemoveFromInventory(ItemData item)
    {
        if (item == null)
            return;

        inventory.Remove(item);
    }

    public ItemData GetEquippedItemForCharacter(CharacterData character, EquipmentType type)
    {
        if (character == null || character.equippedItems == null)
            return null;

        foreach (ItemData item in character.equippedItems)
        {
            if (item != null && item.equipmentType == type)
                return item;
        }

        return null;
    }

    public float GetEquipmentPower(ItemData item)
    {
        if (TeamManager.Instance == null || TeamManager.Instance.CurrentPlayer == null)
            return 0f;

        CharacterData character = TeamManager.Instance.CurrentPlayer.GetCharacterData();

        if (character == null)
            return 0f;

        return GetEquipmentPowerForCharacter(item, character);
    }

    public float GetEquipmentPowerForCharacter(ItemData item, CharacterData character)
    {
        if (item == null || character == null)
            return 0f;

        float healthWeight = 1f;
        float defenceWeight = 1f;
        float staminaWeight = 1f;
        float speedWeight = 1f;
        float attackWeight = 1f;

        if (character.characterClass != null)
        {
            switch (character.characterClass.className)
            {
                case "Warrior":
                    healthWeight = 2f;
                    defenceWeight = 2f;
                    staminaWeight = 1.2f;
                    speedWeight = 0.8f;
                    attackWeight = 1.5f;
                    break;

                case "Archer":
                    healthWeight = 1f;
                    defenceWeight = 0.8f;
                    staminaWeight = 1.5f;
                    speedWeight = 2f;
                    attackWeight = 2f;
                    break;

                case "Mage":
                    healthWeight = 1f;
                    defenceWeight = 0.8f;
                    staminaWeight = 2f;
                    speedWeight = 1.2f;
                    attackWeight = 2f;
                    break;
            }
        }

        return item.healthBonus * healthWeight +
               item.defenceBonus * defenceWeight +
               item.staminaBonus * staminaWeight +
               item.speedBonus * speedWeight +
               item.attackBonus * attackWeight +
               item.expBonus +
               item.GetMiscValue();
    }

    public void EquipItemToCharacter(CharacterData character, ItemData item)
    {
        if (character == null || item == null)
            return;

        if (IsItemEquippedByOtherCharacter(item))
            return;

        if (item.equipmentType == EquipmentType.Weapon)
        {
            if (character.characterClass == null)
                return;

            switch (item.weaponType)
            {
                case WeaponType.Sword:
                    if (character.characterClass.className != "Warrior")
                        return;
                    break;

                case WeaponType.Bow:
                    if (character.characterClass.className != "Archer")
                        return;
                    break;

                case WeaponType.Staff:
                    if (character.characterClass.className != "Mage")
                        return;
                    break;

                default:
                    return;
            }
        }

        ItemData oldItem = GetEquippedItemForCharacter(character, item.equipmentType);

        if (oldItem != null)
            inventory.Add(oldItem);

        character.equippedItems.RemoveAll(x => x != null && x.equipmentType == item.equipmentType);
        character.equippedItems.Add(item);

        inventory.Remove(item);

        Player currentPlayer = TeamManager.Instance != null ? TeamManager.Instance.CurrentPlayer : null;

        if (currentPlayer != null && currentPlayer.GetCharacterData() == character)
            currentPlayer.RefreshStats();

        RefreshEquipmentUI();
        CloseItemList();
    }
}