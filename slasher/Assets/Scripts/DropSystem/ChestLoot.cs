using UnityEngine;
using UnityEngine.EventSystems;

public class ChestLoot : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private LayerMask chestLayer;

    [SerializeField] private LootTable lootTable;
    [SerializeField] private string chestId = "Chest1";

    private Camera cam;
    private LootManager lootManager;
    private string saveId;
    private bool opened;

    private void Start()
    {
        cam = Camera.main;
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

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || cam == null)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePosition, chestLayer);

        if (hit != null && hit.transform == transform)
            OpenChest();
    }

    private void OpenChest()
    {
        if (opened || string.IsNullOrEmpty(saveId))
            return;

        if (lootManager == null || lootTable == null || TeamManager.Instance == null)
            return;

        Player player = TeamManager.Instance.CurrentPlayer;

        if (player == null)
            return;

        Player.BlockAttackUntilMouseRelease();

        opened = true;

        if (MapSaveSystem.Instance != null)
            MapSaveSystem.Instance.RegisterOpenedChest(saveId);

        lootManager.GenerateLoot(lootTable, transform.position, player);
    }
}