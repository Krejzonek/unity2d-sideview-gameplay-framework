using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [SerializeField] private List<ItemData> inventory = new();

    private Player currentPlayer;

    private void Start()
    {
        UpdateCurrentPlayer();
        RefreshAllWeaponVisuals();
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

    private void SaveEquipment()
    {
        if (MapSaveSystem.Instance != null)
            MapSaveSystem.Instance.SaveCharactersNow();
    }

    public List<ItemData> GetInventory()
    {
        return inventory;
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

    public bool IsItemEquippedByOtherCharacter(ItemData item, CharacterData ignoredCharacter = null)
    {
        if (item == null || CharacterManager.Instance == null)
            return false;

        foreach (CharacterData character in CharacterManager.Instance.Characters)
        {
            if (character == null || character == ignoredCharacter || character.equippedItems == null)
                continue;

            if (character.equippedItems.Contains(item))
                return true;
        }

        return false;
    }

    public void EquipItem(EquipmentSlot slot, ItemData item)
    {
        if (slot == null || item == null || slot.Character == null)
            return;

        CharacterData character = slot.Character;

        if (!slot.CanEquip(item) || IsItemEquippedByOtherCharacter(item, character))
            return;

        ItemData oldItem = GetEquippedItemForCharacter(character, slot.Type);

        if (oldItem != null)
            inventory.Add(oldItem);

        if (character.equippedItems == null)
            character.equippedItems = new List<ItemData>();

        character.equippedItems.RemoveAll(x => x != null && x.equipmentType == slot.Type);
        character.equippedItems.Add(item);

        inventory.Remove(item);

        Player player = FindPlayerForCharacter(character);

        if (player != null)
            player.RefreshStats();

        RefreshWeaponVisual(player, character);
        RefreshEquipmentUI();
        SaveEquipment();
    }

    public void EquipItemToCharacter(CharacterData character, ItemData item)
    {
        if (character == null || item == null)
            return;

        if (IsItemEquippedByOtherCharacter(item, character))
            return;

        if (item.equipmentType == EquipmentType.Weapon)
        {
            if (character.characterClass == null)
                return;

            switch (item.weaponType)
            {
                case WeaponType.Sword:
                    if (character.characterClass.className != "Warrior") return;
                    break;
                case WeaponType.Bow:
                    if (character.characterClass.className != "Archer") return;
                    break;
                case WeaponType.Staff:
                    if (character.characterClass.className != "Mage") return;
                    break;
                default:
                    return;
            }
        }

        ItemData oldItem = GetEquippedItemForCharacter(character, item.equipmentType);

        if (oldItem != null)
            inventory.Add(oldItem);

        if (character.equippedItems == null)
            character.equippedItems = new List<ItemData>();

        character.equippedItems.RemoveAll(x => x != null && x.equipmentType == item.equipmentType);
        character.equippedItems.Add(item);

        inventory.Remove(item);

        Player player = FindPlayerForCharacter(character);

        if (player != null)
            player.RefreshStats();

        RefreshWeaponVisual(player, character);
        RefreshEquipmentUI();
        SaveEquipment();
    }

    public void RefreshWeaponVisual(Player player, CharacterData character)
    {
        if (player == null || character == null)
            return;

        WeaponVisualEquipment visual = player.GetComponent<WeaponVisualEquipment>();

        if (visual != null)
            visual.RefreshFromCharacter(character);
    }

    public void RefreshAllWeaponVisuals()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (Player player in players)
        {
            if (player == null)
                continue;

            CharacterData character = player.GetCharacterData();

            if (character != null)
                RefreshWeaponVisual(player, character);
        }
    }

    public void RefreshEquipmentUI()
    {
        EquipmentSlot[] slots = FindObjectsByType<EquipmentSlot>(FindObjectsSortMode.None);

        foreach (EquipmentSlot slot in slots)
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

    private Player FindPlayerForCharacter(CharacterData character)
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (Player player in players)
        {
            if (player != null && player.GetCharacterData() == character)
                return player;
        }

        return null;
    }

    public void SetInventory(List<ItemData> items)
    {
        inventory.Clear();

        if (items != null)
            inventory.AddRange(items);

        RefreshEquipmentUI();
    }

    public void SaveAllEquipment()
    {
        SaveEquipment();
    }
}