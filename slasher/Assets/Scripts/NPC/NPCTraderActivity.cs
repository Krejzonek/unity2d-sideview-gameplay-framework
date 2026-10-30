using UnityEngine;

[CreateAssetMenu(fileName = "New Trader Activity", menuName = "NPC/Activities/Trader")]
public class NPCTraderActivity : NPCActivityData
{
    [Header("Trading Settings")]
    public bool allowBuying = true;
    public bool allowSelling = true;

    public NPCTradeData[] trades;

    private EquipmentManager Equipment => FindFirstObjectByType<EquipmentManager>();
    private ConsumableManager Consumables => FindFirstObjectByType<ConsumableManager>();

    public override void Execute(NPCActivityContext context)
    {
        if (context == null || context.UI == null)
            return;

        context.UI.OpenTrader(this);
    }

    public bool CanTrade(NPCTradeData trade)
    {
        if (trade == null || trade.requiredAmount <= 0 || trade.rewardAmount <= 0)
            return false;

        return HasRequiredItems(trade);
    }

    public bool ExecuteTrade(NPCTradeData trade)
    {
        if (!CanTrade(trade))
            return false;

        if (!RemoveRequiredItems(trade))
            return false;

        if (!AddRewardItems(trade))
        {
            AddRequiredItemsBack(trade);
            return false;
        }

        Equipment?.SaveAllEquipment();
        return true;
    }

    private bool HasRequiredItems(NPCTradeData trade)
    {
        switch (trade.requiredItemType)
        {
            case NPCTradeItemType.Ingredient:
                IngredientInventory ingredients = Consumables != null ? Consumables.GetIngredientInventory() : null;
                return ingredients != null && trade.requiredIngredient != null && ingredients.HasIngredient(trade.requiredIngredient, trade.requiredAmount);

            case NPCTradeItemType.Consumable:
                ConsumableInventory consumables = Consumables != null ? Consumables.GetConsumableInventory() : null;
                return consumables != null && trade.requiredConsumable != null && consumables.GetAmount(trade.requiredConsumable) >= trade.requiredAmount;

            case NPCTradeItemType.Equipment:
                EquipmentManager equipment = Equipment;
                if (equipment == null || trade.requiredEquipment == null)
                    return false;

                int count = 0;

                foreach (EquipmentData item in equipment.GetInventory())
                {
                    if (item != null && item.itemName == trade.requiredEquipment.itemName)
                        count++;
                }

                return count >= trade.requiredAmount;

            default:
                return false;
        }
    }

    private bool RemoveRequiredItems(NPCTradeData trade)
    {
        switch (trade.requiredItemType)
        {
            case NPCTradeItemType.Ingredient:
                return Consumables != null && Consumables.GetIngredientInventory() != null &&
                    Consumables.GetIngredientInventory().RemoveIngredient(trade.requiredIngredient, trade.requiredAmount);

            case NPCTradeItemType.Consumable:
                return Consumables != null && Consumables.GetConsumableInventory() != null &&
                    Consumables.GetConsumableInventory().RemoveConsumable(trade.requiredConsumable, trade.requiredAmount);

            case NPCTradeItemType.Equipment:
                EquipmentManager equipment = Equipment;

                if (equipment == null || trade.requiredEquipment == null)
                    return false;

                for (int i = 0; i < trade.requiredAmount; i++)
                {
                    EquipmentData item = equipment.GetInventory().Find(x => x != null && x.itemName == trade.requiredEquipment.itemName);

                    if (item == null)
                        return false;

                    equipment.RemoveFromInventory(item);
                }

                return true;

            default:
                return false;
        }
    }

    private bool AddRewardItems(NPCTradeData trade)
    {
        switch (trade.rewardItemType)
        {
            case NPCTradeItemType.Ingredient:
                if (Consumables == null || Consumables.GetIngredientInventory() == null || trade.rewardIngredient == null)
                    return false;

                Consumables.GetIngredientInventory().AddIngredient(trade.rewardIngredient, trade.rewardAmount);
                return true;

            case NPCTradeItemType.Consumable:
                if (Consumables == null || Consumables.GetConsumableInventory() == null || trade.rewardConsumable == null)
                    return false;

                Consumables.GetConsumableInventory().AddConsumable(Instantiate(trade.rewardConsumable), trade.rewardAmount);
                return true;

            case NPCTradeItemType.Equipment:
                EquipmentManager equipment = Equipment;

                if (equipment == null || trade.rewardEquipment == null)
                    return false;

                for (int i = 0; i < trade.rewardAmount; i++)
                    equipment.AddToInventory(Instantiate(trade.rewardEquipment));

                return true;

            default:
                return false;
        }
    }

    private void AddRequiredItemsBack(NPCTradeData trade)
    {
        switch (trade.requiredItemType)
        {
            case NPCTradeItemType.Ingredient:
                if (Consumables != null && Consumables.GetIngredientInventory() != null && trade.requiredIngredient != null)
                    Consumables.GetIngredientInventory().AddIngredient(trade.requiredIngredient, trade.requiredAmount);
                break;

            case NPCTradeItemType.Consumable:
                if (Consumables != null && Consumables.GetConsumableInventory() != null && trade.requiredConsumable != null)
                    Consumables.GetConsumableInventory().AddConsumable(Instantiate(trade.requiredConsumable), trade.requiredAmount);
                break;

            case NPCTradeItemType.Equipment:
                if (Equipment != null && trade.requiredEquipment != null)
                {
                    for (int i = 0; i < trade.requiredAmount; i++)
                        Equipment.AddToInventory(Instantiate(trade.requiredEquipment));
                }
                break;
        }
    }
}