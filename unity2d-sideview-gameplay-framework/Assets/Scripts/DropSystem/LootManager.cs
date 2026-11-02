using UnityEngine;

public class LootManager : MonoBehaviour
{
    [SerializeField] private GameObject lootPrefab;

    public void GenerateLoot(LootTable lootTable, Vector3 position, Player killer)
    {
        if (lootTable == null)
            return;

        foreach (LootDrop drop in lootTable.drops)
        {
            if (drop == null)
                continue;

            if (Random.Range(0f, 100f) > drop.dropChance)
                continue;

            int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);

            if (amount <= 0)
                continue;

            SpawnLoot(drop, amount, position, killer);
        }
    }

    private void SpawnLoot(LootDrop drop, int amount, Vector3 position, Player killer)
    {
        if (lootPrefab == null)
            return;

        GameObject lootObject = Instantiate(lootPrefab, position, Quaternion.identity);

        LootItem lootItem = lootObject.GetComponent<LootItem>();

        if (lootItem == null)
        {
            Destroy(lootObject);
            return;
        }

        lootItem.Setup(drop, amount, killer);
    }
}