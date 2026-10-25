using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCTradeButton : MonoBehaviour
{
    [SerializeField] private Button requiredItemButton;
    [SerializeField] private Image requiredItemImage;
    [SerializeField] private TMP_Text requiredItemAmount;

    [SerializeField] private Button rewardItemButton;
    [SerializeField] private Image rewardItemImage;
    [SerializeField] private TMP_Text rewardItemAmount;

    [SerializeField] private Button tradeButton;
    [SerializeField] private TMP_Text tradeButtonText;

    public void Setup(NPCTradeData trade, bool canTrade, Action onTrade)
    {
        if (trade == null)
            return;

        SetItem(requiredItemImage, requiredItemAmount,
            trade.requiredItemType,
            trade.requiredIngredient,
            trade.requiredConsumable,
            trade.requiredEquipment,
            trade.requiredAmount);

        SetItem(rewardItemImage, rewardItemAmount,
            trade.rewardItemType,
            trade.rewardIngredient,
            trade.rewardConsumable,
            trade.rewardEquipment,
            trade.rewardAmount);

        if (tradeButton != null)
        {
            tradeButton.onClick.RemoveAllListeners();
            tradeButton.interactable = canTrade;

            if (onTrade != null)
                tradeButton.onClick.AddListener(() => onTrade());
        }

        if (tradeButtonText != null)
            tradeButtonText.text = trade.requiredAmount + " → " + trade.rewardAmount;
    }

    private void SetItem(Image image, TMP_Text text, NPCTradeItemType type,
        IngredientData ingredient, ConsumableData consumable, ItemData equipment, int amount)
    {
        Sprite icon = null;

        switch (type)
        {
            case NPCTradeItemType.Ingredient:
                if (ingredient != null)
                {
                    icon = ingredient.icon;
                }
                break;

            case NPCTradeItemType.Consumable:
                if (consumable != null)
                {
                    icon = consumable.icon;
                }
                break;

            case NPCTradeItemType.Equipment:
                if (equipment != null)
                {
                    icon = equipment.icon;
                }
                break;
        }

        if (image != null)
        {
            image.sprite = icon;
            image.enabled = icon != null;
            image.preserveAspect = true;
        }

        if (text != null)
            text.text = amount.ToString();
    }
}