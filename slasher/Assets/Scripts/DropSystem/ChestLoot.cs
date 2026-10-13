using UnityEngine;

public class ChestLoot : MonoBehaviour
{
    [SerializeField] private LootTable lootTable;
    [SerializeField] private string chestId = "Chest1";

    private LootManager lootManager;
    private string saveId;
    private bool opened;

    private void Start()
    {
        lootManager = FindFirstObjectByType<LootManager>();

        Room room = GetComponentInParent<Room>();

        if (room == null)
        {
            Debug.LogError("ChestDrop: Room component not found in parents!", this);
            return;
        }

        saveId = room.GetRoomNumber() + "_" + chestId;

        if (MapSaveSystem.Instance != null)
            opened = MapSaveSystem.Instance.IsChestOpened(saveId);
    }

    private void OnMouseDown()
    {
        if (opened || string.IsNullOrEmpty(saveId))
            return;

        if (lootManager == null || lootTable == null || TeamManager.Instance == null)
            return;

        Player player = TeamManager.Instance.CurrentPlayer;

        if (player == null)
            return;

        opened = true;

        if (MapSaveSystem.Instance != null)
            MapSaveSystem.Instance.RegisterOpenedChest(saveId);

        lootManager.GenerateLoot(lootTable, transform.position, player);
    }
}