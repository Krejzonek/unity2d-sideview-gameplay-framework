using UnityEngine;

public class LootItem : MonoBehaviour
{
    private LootDrop lootDrop;
    private int amount;
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

    public void Setup(LootDrop drop, int itemAmount)
    {
        lootDrop = drop;
        amount = itemAmount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (lootDrop == null)
            return;

        if (!TryGiveLoot(other))
            return;

        Destroy(gameObject);
    }

    private bool TryGiveLoot(Collider2D player)
    {
        switch (lootDrop.lootType)
        {
            case LootType.Ingredient:

                IngredientInventory ingredientInventory = player.GetComponent<IngredientInventory>();

                if (ingredientInventory == null)
                    ingredientInventory = player.GetComponentInParent<IngredientInventory>();

                if (ingredientInventory == null)
                    return false;

                ingredientInventory.AddIngredient(lootDrop.ingredient, amount);

                return true;

            case LootType.Consumable:

                ConsumableInventory consumableInventory = player.GetComponent<ConsumableInventory>();

                if (consumableInventory == null)
                    consumableInventory = player.GetComponentInParent<ConsumableInventory>();

                if (consumableInventory == null)
                    return false;

                consumableInventory.AddConsumable(lootDrop.consumable, amount);

                return true;

            case LootType.Equipment:

                if (lootDrop.equipment == null)
                    return false;

                EquipmentManager equipmentManager = FindFirstObjectByType<EquipmentManager>();

                if (equipmentManager == null)
                    return false;

                ItemData equipment = Instantiate(lootDrop.equipment);
                equipment.SetRarity(RarityManager.GetRandomRarity());

                if (RarityManager.RollMisc())
                    equipment.SetMiscType(RarityManager.GetRandomMisc());

                equipmentManager.AddToInventory(equipment);

                return true;
        }

        return false;
    }
}