using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableItemButton : MonoBehaviour
{
    [SerializeField] private TMP_Text itemNameText;

    private ConsumableData item;
    private ConsumableHotbar hotbar;

    public void Setup(ConsumableData newItem, ConsumableHotbar newHotbar)
    {
        item = newItem;
        hotbar = newHotbar;

        if (itemNameText != null)
            itemNameText.text = item.itemName;

        Image buttonImage = GetComponent<Image>();

        if (buttonImage != null)
            buttonImage.color = RarityColor.GetColor((ItemRarity)item.rarity);

        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Select);
        }
    }

    public void Select()
    {
        if (item == null)
            return;

        if (hotbar == null)
            return;

        hotbar.ShowConsumableDescription(item);
    }
}