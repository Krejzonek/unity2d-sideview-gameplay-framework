using UnityEngine;

public class LootItem : MonoBehaviour
{
    private LootDrop lootDrop;
    private int amount;
    private float luck;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 1f;
            rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
        }
    }

    public void Setup(LootDrop drop, int itemAmount, Player killer)
    {
        lootDrop = drop;
        amount = itemAmount;

        luck = 1f;

        if (killer != null && killer.GetStats() != null)
            luck = killer.GetStats().luck;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (lootDrop == null)
            return;

        if (!TryGiveLoot())
            return;

        Destroy(gameObject);
    }

    private bool TryGiveLoot()
    {
        switch (lootDrop.lootType)
        {
            case LootType.Ingredient:

                ConsumableManager consumableManager = FindFirstObjectByType<ConsumableManager>();

                if (consumableManager == null)
                    return false;

                IngredientInventory ingredientInventory = consumableManager.GetIngredientInventory();

                if (ingredientInventory == null || lootDrop.ingredient == null)
                    return false;

                int ingredientAmount = LuckSystem.GetAmount(amount, luck);

                ingredientInventory.AddIngredient(lootDrop.ingredient, ingredientAmount);

                return true;

            case LootType.Consumable:

                ConsumableManager manager = FindFirstObjectByType<ConsumableManager>();

                if (manager == null)
                    return false;

                ConsumableInventory consumableInventory = manager.GetConsumableInventory();

                if (consumableInventory == null || lootDrop.consumable == null)
                    return false;

                int consumableAmount = LuckSystem.GetAmount(amount, luck);

                ConsumableData consumable = Instantiate(lootDrop.consumable);
                consumable.rarity = LuckSystem.GetConsumableRarity(luck);

                consumableInventory.AddConsumable(consumable, consumableAmount);

                return true;

            case LootType.Equipment:

                if (lootDrop.equipment == null)
                    return false;

                EquipmentManager equipmentManager = FindFirstObjectByType<EquipmentManager>();

                if (equipmentManager == null)
                    return false;

                ItemData equipment = Instantiate(lootDrop.equipment);
                equipment.SetRarity(LuckSystem.GetItemRarity(luck));

                if (RarityManager.RollMisc())
                    equipment.SetMiscType(RarityManager.GetRandomMisc());

                equipmentManager.AddToInventory(equipment);

                return true;
        }

        return false;
    }
}