using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    private EquipmentData item;
    private UiEq uiEq;

    public void Setup(EquipmentData newItem, UiEq sourceUiEq)
    {
        item = newItem;
        uiEq = sourceUiEq;

        Button button = GetComponent<Button>();

        if (button != null)
            button.image.color = RarityColor.GetColor(item.GetRarity());
    }

    public void Select()
    {
        if (item == null || uiEq == null)
            return;

        uiEq.ShowItemDescription(item);
    }
}