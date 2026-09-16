using UnityEngine;
using UnityEngine.UI;

public class ItemButton : MonoBehaviour
{
    [SerializeField] private ItemDescriptionPanel descriptionPanel;

    private ItemData item;

    
    public void Setup(ItemData newItem)
    {
        item = newItem;

        Button button = GetComponent<Button>();

        if (button != null)
            button.image.color = RarityColor.GetColor(item.GetRarity());
    }

    public void Select()
    {
        if (item == null)
            return;

        if (ItemDescriptionPanel.Instance == null)
            return;

        ItemDescriptionPanel.Instance.ShowEquipment(item);
    }
}