using UnityEngine;
using UnityEngine.UI;

public class NPCBlacksmithItemButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Image selectionOutline;
    [SerializeField] private Color compatibleColor = Color.white;
    [SerializeField] private Color incompatibleColor = Color.gray;

    public Button Button => button;

    private void Awake()
    {
        if (button == null)
            button = GetComponentInChildren<Button>(true);

        if (itemIcon == null && button != null)
        {
            Transform iconTransform = button.transform.Find("ItemIcon");

            if (iconTransform != null)
                itemIcon = iconTransform.GetComponent<Image>();
        }

        if (selectionOutline == null)
        {
            Transform outlineTransform = transform.Find("Outline");

            if (outlineTransform != null)
                selectionOutline = outlineTransform.GetComponent<Image>();
        }

        if (selectionOutline != null)
            selectionOutline.raycastTarget = false;

        if (itemIcon != null)
            itemIcon.raycastTarget = false;
    }

    public void SetItem(ItemData item, bool selected, bool compatible)
    {
        if (button == null || item == null)
            return;

        gameObject.SetActive(true);
        button.gameObject.SetActive(true);

        if (itemIcon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = item.icon != null;
            itemIcon.preserveAspect = true;
            itemIcon.color = compatible || selected ? compatibleColor : incompatibleColor;
        }

        if (button.image != null)
            button.image.color = compatible || selected ? Color.white : incompatibleColor;

        if (selectionOutline != null)
        {
            selectionOutline.color = Color.yellow;
            selectionOutline.gameObject.SetActive(selected);
        }

        button.interactable = true;
    }
}