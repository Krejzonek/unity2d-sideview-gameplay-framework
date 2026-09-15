using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private ItemData item;

    public void Setup(ItemData newItem)
    {
        item = newItem;

        if (button != null)
            button.image.color = RarityColor.GetColor(item.GetRarity());
    }

    public void Select()
    {
        EquipmentManager.Instance.SelectItem(item);
        EquipmentManager.Instance.CloseItemList();
    }
}