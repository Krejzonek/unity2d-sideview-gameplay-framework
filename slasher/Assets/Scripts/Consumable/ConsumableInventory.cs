using System.Collections.Generic;
using UnityEngine;

public class ConsumableInventory : MonoBehaviour
{
    [System.Serializable]
    public class ConsumableStack
    {
        public ConsumableData item;
        public int amount;
    }

    [SerializeField] private List<ConsumableStack> consumables = new();

    public void AddConsumable(ConsumableData item, int amount)
    {
        if (item == null || amount <= 0)
            return;

        ConsumableStack stack = consumables.Find(x => x.item == item);

        if (stack != null)
        {
            stack.amount += amount;
        }
        else
        {
            consumables.Add(new ConsumableStack
            {
                item = item,
                amount = amount
            });
        }
    }

    public bool HasConsumable(ConsumableData item, int amount)
    {
        ConsumableStack stack = consumables.Find(x => x.item == item);

        if (stack == null)
            return false;

        return stack.amount >= amount;
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

        if (stack == null)
            return 0;

        return stack.amount;
    }

    public List<ConsumableStack> GetConsumables()
    {
        return consumables;
    }
}