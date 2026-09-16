using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ItemDescriptionPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Button actionButton;
    [SerializeField] private Button closeButton;

    private ItemData equipmentItem;
    private ConsumableData consumableItem;

    private ConsumableHotbar consumableHotbar;

    public static ItemDescriptionPanel Instance;
    private void Start()
    {
        gameObject.SetActive(false);
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }
    private void Awake()
    {
        Instance = this;
    }
    public void ShowEquipment(ItemData item)
    {
        if (item == null)
            return;

        equipmentItem = item;
        consumableItem = null;

        itemNameText.text = item.itemName;
        descriptionText.text = item.description;

        ShowEquipmentStats(item);

        actionButton.gameObject.SetActive(true);
        

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(EquipItem);

        gameObject.SetActive(true);
    }

    private void ShowEquipmentStats(ItemData item)
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

    public void ShowConsumable(ConsumableData item, ConsumableHotbar hotbar)
    {
        if (item == null)
            return;

        consumableItem = item;
        equipmentItem = null;
        consumableHotbar = hotbar;

        itemNameText.text = item.itemName;
        descriptionText.text = item.description;
        statsText.text = "";

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

        itemNameText.text = item.itemName;
        descriptionText.text = item.description;
        statsText.text = "";

        actionButton.gameObject.SetActive(false);

        gameObject.SetActive(true);
    }

    private void EquipItem()
    {
        if (equipmentItem == null)
            return;

        EquipmentManager equipmentManager = EquipmentManager.Instance;

        if (equipmentManager == null)
            return;

        equipmentManager.SelectItem(equipmentItem);

        Close();
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
    }
}