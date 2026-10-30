using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDescriptionPanel : MonoBehaviour
{
    public static ItemDescriptionPanel Instance;

    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Image comparisonImage;
    [SerializeField] private Button actionButton;
    [SerializeField] private Button closeButton;

    [Header("Position")]
    [SerializeField] private Vector2 panelOffset = new Vector2(20f, 0f);

    private EquipmentData equipmentItem;
    private ConsumableData consumableItem;
    private ConsumableHotbar consumableHotbar;
    private UiEq uiEq;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        gameObject.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }
    }

    public void ShowEquipment(EquipmentData item)
    {
        ShowEquipment(item, null);
    }

    public void ShowEquipment(EquipmentData item, UiEq sourceUiEq)
    {
        if (item == null)
            return;

        equipmentItem = item;
        consumableItem = null;
        consumableHotbar = null;
        uiEq = sourceUiEq;

        itemNameText.text = item.itemName;
        descriptionText.text = "";

        ShowEquipmentStats(item);
        ShowEquipmentComparison(item);

        actionButton.gameObject.SetActive(true);

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(EquipItem);

        gameObject.SetActive(true);

        SetPosition(sourceUiEq);
    }

    private void SetPosition(UiEq sourceUiEq)
    {
        if (sourceUiEq == null)
            return;

        RectTransform sourceRect = sourceUiEq.GetComponent<RectTransform>();
        RectTransform panelRect = GetComponent<RectTransform>();

        if (sourceRect == null || panelRect == null)
            return;

        panelRect.position = sourceRect.position + (Vector3)panelOffset;
    }

    private void ShowEquipmentStats(EquipmentData item)
    {
        string stats = "";

        if (item.healthBonus != 0)
            stats += "Health: +" + item.healthBonus + "\n";

        if (item.defenceBonus != 0)
            stats += "Defence: +" + item.defenceBonus + "\n";

        if (item.staminaBonus != 0)
            stats += "Stamina: +" + item.staminaBonus + "\n";

        if (item.speedBonus != 0)
            stats += "Speed: +" + item.speedBonus + "\n";

        if (item.attackBonus != 0)
            stats += "Attack: +" + item.attackBonus + "\n";

        if (item.expBonus != 0)
            stats += "EXP Bonus: +" + (item.expBonus * 100f) + "%\n";

        float miscValue = item.GetMiscValue();

        if (miscValue != 0)
        {
            switch (item.miscType)
            {
                case MiscType.CritRate:
                    stats += "Crit Rate: +" + miscValue + "%\n";
                    break;

                case MiscType.Knockback:
                    stats += "Knockback: +" + miscValue + "\n";
                    break;

                case MiscType.Stun:
                    stats += "Stun Chance: +" + miscValue + "%\n";
                    break;
            }
        }

        if (stats == "")
            stats = "No stat bonuses";

        statsText.text = stats;
    }

    private void ShowEquipmentComparison(EquipmentData item)
    {
        if (comparisonImage == null)
            return;

        EquipmentManager equipmentManager = FindFirstObjectByType<EquipmentManager>();

        if (equipmentManager == null)
            return;

        CharacterData targetCharacter = uiEq != null ? uiEq.Character : TeamManager.Instance != null && TeamManager.Instance.CurrentPlayer != null ? TeamManager.Instance.CurrentPlayer.GetCharacterData() : null;

        if (targetCharacter == null)
            return;

        EquipmentData equippedItem = equipmentManager.GetEquippedItemForCharacter(targetCharacter, item.equipmentType);

        if (equippedItem == null)
        {
            comparisonImage.color = Color.green;
            comparisonImage.gameObject.SetActive(true);
            return;
        }

        float newValue = equipmentManager.GetEquipmentPowerForCharacter(item, targetCharacter);
        float oldValue = equipmentManager.GetEquipmentPowerForCharacter(equippedItem, targetCharacter);

        comparisonImage.color = newValue > oldValue ? Color.green : newValue < oldValue ? Color.red : Color.gray;
        comparisonImage.gameObject.SetActive(true);
    }

    public void ShowConsumable(ConsumableData item, ConsumableHotbar hotbar)
    {
        if (item == null)
            return;

        consumableItem = item;
        equipmentItem = null;
        consumableHotbar = hotbar;
        uiEq = null;

        itemNameText.text = item.itemName;
        descriptionText.text = item.description;
        statsText.text = "";

        if (comparisonImage != null)
            comparisonImage.gameObject.SetActive(false);

        actionButton.gameObject.SetActive(true);

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(EquipConsumable);

        gameObject.SetActive(true);
    }

    public void ShowIngredient(IngredientData item)
    {
        if (item == null)
            return;

        equipmentItem = null;
        consumableItem = null;
        consumableHotbar = null;
        uiEq = null;

        itemNameText.text = item.itemName;
        descriptionText.text = item.description;
        statsText.text = "";

        if (comparisonImage != null)
            comparisonImage.gameObject.SetActive(false);

        actionButton.gameObject.SetActive(false);

        gameObject.SetActive(true);
    }

    private void EquipItem()
    {
        if (equipmentItem == null)
            return;

        if (uiEq != null)
        {
            uiEq.SelectItem(equipmentItem);
            Close();
            return;
        }

        EquipmentManager equipmentManager = FindFirstObjectByType<EquipmentManager>();

        if (equipmentManager == null)
            return;

        EquipmentSlot[] slots = FindObjectsByType<EquipmentSlot>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (EquipmentSlot slot in slots)
        {
            if (slot == null || slot.Type != equipmentItem.equipmentType)
                continue;

            equipmentManager.EquipItem(slot, equipmentItem);
            Close();
            return;
        }
    }

    private void EquipConsumable()
    {
        if (consumableItem == null)
            return;

        if (consumableHotbar == null)
            return;

        consumableHotbar.SelectItemFromDescription(consumableItem);

        Close();
    }

    public void Close()
    {
        gameObject.SetActive(false);

        equipmentItem = null;
        consumableItem = null;
        consumableHotbar = null;
        uiEq = null;
    }
}