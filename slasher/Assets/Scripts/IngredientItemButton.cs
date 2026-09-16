using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngredientItemButton : MonoBehaviour
{
    [SerializeField] private TMP_Text itemNameText;

    private IngredientData ingredient;

    public void Setup(IngredientData newIngredient, int amount)
    {
        ingredient = newIngredient;

        if (itemNameText != null)
            itemNameText.text = ingredient.itemName;

        Button button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(Select);
        }
    }

    public void Select()
    {
        if (ingredient == null)
            return;

        if (ItemDescriptionPanel.Instance == null)
            return;

        ItemDescriptionPanel.Instance.ShowIngredient(ingredient);
    }
}