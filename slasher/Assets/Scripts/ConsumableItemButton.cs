using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConsumableItemButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text itemName;

    private ConsumableData item;
    private ConsumableHotbar hotbar;

    public void Setup(
        ConsumableData newItem,
        ConsumableHotbar newHotbar)
    {
        item = newItem;
        hotbar = newHotbar;

        itemName.text = item.itemName;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(Select);
    }

    private void Select()
    {
        hotbar.SelectItem(item);
    }
}