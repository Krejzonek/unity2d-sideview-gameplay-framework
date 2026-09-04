using UnityEngine;

public class ChestDrop : MonoBehaviour
{
    [SerializeField] private LootTable lootTable;

    private LootManager lootManager;
    private bool opened;

    private void Start()
    {
        lootManager = FindFirstObjectByType<LootManager>();
    }

    private void OnMouseDown()
    {
        if (opened)
            return;

        if (lootManager == null)
            return;

        if (lootTable == null)
            return;

        opened = true;

        lootManager.GenerateLoot(lootTable,transform.position);
    }
}