using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    //[SerializeField] private Image icon;

    private ItemData item;

    public void Setup(ItemData newItem)
    {
        item = newItem;
        //icon.sprite = item.icon;
    }
    public void Select()
    {
        EquipmentManager.Instance.SelectItem(item);

        EquipmentManager.Instance.CloseItemList();
    }
}