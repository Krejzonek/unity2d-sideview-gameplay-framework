
using UnityEngine;

public class LootItem : MonoBehaviour
{
    private LootDrop lootDrop;
    private int amount;
    private float luck;
    private Rigidbody2D rb;

    public LootDrop GetLootDrop() => lootDrop;
    public int GetAmount() => amount;
    public float GetLuck() => luck;

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
        luck = killer != null && killer.GetStats() != null ? killer.GetStats().luck : 1f;
    }

    public void SetupSaved(LootDrop drop, int itemAmount, float savedLuck)
    {
        lootDrop = drop;
        amount = itemAmount;
        luck = savedLuck;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || lootDrop == null)
            return;

        if (TryGiveLoot(out string itemName, out int receivedAmount))
        {
            PickupNotificationManager notificationManager = FindFirstObjectByType<PickupNotificationManager>();

            if (notificationManager != null)
                notificationManager.ShowPickup($"{lootDrop.lootType}:{itemName}", itemName, receivedAmount);

            Destroy(gameObject);
        }
    }

    private bool TryGiveLoot(out string itemName, out int receivedAmount)
    {
        itemName = "";
        receivedAmount = 0;

        switch (lootDrop.lootType)
        {
            case LootType.Ingredient:
                {
                    ConsumableManager manager = FindFirstObjectByType<ConsumableManager>();

                    if (manager == null || lootDrop.ingredient == null)
                        return false;

                    IngredientInventory inventory = manager.GetIngredientInventory();

                    if (inventory == null)
                        return false;

                    itemName = lootDrop.ingredient.itemName;
                    receivedAmount = LuckSystem.GetAmount(amount, luck);
                    inventory.AddIngredient(lootDrop.ingredient, receivedAmount);
                    return true;
                }
            case LootType.Consumable:
                {
                    ConsumableManager manager = FindFirstObjectByType<ConsumableManager>();

                    if (manager == null || lootDrop.consumable == null)
                        return false;

                    ConsumableInventory inventory = manager.GetConsumableInventory();

                    if (inventory == null)
                        return false;

                    ConsumableData consumable = Instantiate(lootDrop.consumable);
                    consumable.rarity = LuckSystem.GetConsumableRarity(luck);
                    itemName = consumable.itemName;
                    receivedAmount = LuckSystem.GetAmount(amount, luck);
                    inventory.AddConsumable(consumable, receivedAmount);
                    return true;
                }
            case LootType.Equipment:
                {
                    if (lootDrop.equipment == null)
                        return false;

                    EquipmentManager manager = FindFirstObjectByType<EquipmentManager>();

                    if (manager == null)
                        return false;

                    EquipmentData equipment = Instantiate(lootDrop.equipment);
                    equipment.SetRarity(LuckSystem.GetItemRarity(luck));

                    if (RarityManager.RollMisc())
                        equipment.SetMiscType(RarityManager.GetRandomMisc());

                    itemName = equipment.itemName;
                    receivedAmount = 1;
                    manager.AddToInventory(equipment);
                    return true;
                }
        }

        return false;
    }
}
