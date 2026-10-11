using System.Collections.Generic;
using UnityEngine;

public class ConsumableInventory : MonoBehaviour
{
    [System.Serializable]
    public class ConsumableStack
    {
        public ConsumableData item;
        public ConsumableRarity rarity;
        public int amount;
    }

    [SerializeField] private List<ConsumableStack> consumables = new();

    public void AddConsumable(ConsumableData item, int amount)
    {
        if (item == null || amount <= 0)
            return;

        ConsumableStack stack = consumables.Find(x => x.item.itemName == item.itemName && x.rarity == item.rarity);

        if (stack != null)
        {
            stack.amount += amount;
        }
        else
        {
            ConsumableStack newStack = new ConsumableStack
            {
                item = item,
                rarity = item.rarity,
                amount = amount
            };

            consumables.Add(newStack);
        }
    }
    public bool HasConsumable(ConsumableData item, int amount)
    {
        ConsumableStack stack = consumables.Find(x => x.item == item);

        return stack != null && stack.amount >= amount;
    }

    public bool RemoveConsumable(ConsumableData item, int amount)
    {
        if (!HasConsumable(item, amount))
            return false;

        ConsumableStack stack = consumables.Find(x => x.item == item);
        stack.amount -= amount;

        if (stack.amount <= 0)
            consumables.Remove(stack);

        return true;
    }

    public int GetAmount(ConsumableData item)
    {
        ConsumableStack stack = consumables.Find(x => x.item == item);

        return stack != null ? stack.amount : 0;
    }

    public List<ConsumableStack> GetConsumables()
    {
        return consumables;
    }

    public void ClearConsumables()
    {
        consumables.Clear();
    }
}