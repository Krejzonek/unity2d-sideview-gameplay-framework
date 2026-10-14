
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Header("Room")]
    [SerializeField] private int roomNumber;
    [SerializeField] private bool isBossRoom;

    [Header("Enemies")]
    [SerializeField] private Transform enemyParent;

    [Header("Exit")]
    [SerializeField] private GameObject exit;

    [Header("Camera")]
    [SerializeField] private Collider2D mapBounds;

    [Header("Phases")]
    [SerializeField] private int minPhases = 1;
    [SerializeField] private int maxPhases = 3;

    private int enemiesAlive;
    private int currentPhase;
    private int totalPhases;
    private RoomPoints roomPoints;
    private RoomEnemySpawner enemySpawner;
    private RoomGenerator roomGenerator;
    private bool phaseChanging;
    private bool roomCleared;
    private bool restoreState;
    private bool entryClosed;
    private List<MapSaveSystem.EnemySaveData> savedEnemies = new();

    private void Start()
    {
        roomPoints = GetComponentInChildren<RoomPoints>();
        enemySpawner = GetComponentInChildren<RoomEnemySpawner>();

        if (enemySpawner == null)
            enemySpawner = GetComponent<RoomEnemySpawner>();

        roomGenerator = FindFirstObjectByType<RoomGenerator>();

        Debug.Log("[Room] Start. Room: " + roomNumber + ", RestoreState: " + restoreState + ", Cleared: " + roomCleared + ", RoomPoints: " + (roomPoints != null));

        if (restoreState)
        {
            if (roomCleared)
            {
                enemiesAlive = 0;

                if (exit != null)
                    exit.SetActive(false);

                Debug.Log("[Room] Restored cleared room " + roomNumber + ". Opening entry door.");

                if (roomPoints != null)
                    roomPoints.OpenTheDoor();
            }
            else
            {
                if (exit != null)
                    exit.SetActive(true);

                Debug.Log("[Room] Restoring uncleared room " + roomNumber + ". Entry door will be set by RoomGenerator.");

                if (enemySpawner != null)
                    enemiesAlive = enemySpawner.RestoreEnemies(savedEnemies);

                if (enemiesAlive == 0)
                {
                    roomCleared = false;

                    if (exit != null)
                        exit.SetActive(true);

                    Debug.LogWarning("[Room] Room " + roomNumber + " restored with 0 enemies but remains uncleared.");
                }
            }

            return;
        }

        if (roomPoints != null)
        {
            Debug.Log("[Room] New room " + roomNumber + ". Opening entry door.");
            roomPoints.OpenTheDoor();
        }

        minPhases = Mathf.Max(1, minPhases);
        maxPhases = Mathf.Max(minPhases, maxPhases);

        totalPhases = Random.Range(minPhases, maxPhases + 1);
        currentPhase = 1;

        if (exit != null)
            exit.SetActive(true);

        if (enemySpawner != null)
            enemiesAlive = enemySpawner.SpawnPhase();

        Debug.Log("[Room] Room " + roomNumber + " spawned enemies: " + enemiesAlive);

        if (enemiesAlive == 0)
            RoomCleared();
    }

    public void RestoreEntryDoorState(bool close)
    {
        if (roomPoints == null)
            roomPoints = GetComponentInChildren<RoomPoints>();

        if (roomPoints == null)
        {
            Debug.LogError("[Room] Cannot restore entry door for room " + roomNumber + ": RoomPoints is NULL.");
            return;
        }

        bool shouldClose = close && !roomCleared;

        Debug.Log("[Room] RestoreEntryDoorState. Room: " + roomNumber + ", RequestedClose: " + close + ", Cleared: " + roomCleared + ", ShouldClose: " + shouldClose);

        if (shouldClose)
        {
            roomPoints.CloseTheDoor();
            entryClosed = true;
            Debug.Log("[Room] Entry door CLOSED for room " + roomNumber);
        }
        else
        {
            roomPoints.OpenTheDoor();
            entryClosed = false;
            Debug.Log("[Room] Entry door OPEN for room " + roomNumber);
        }
    }

    public void EnemyKilled()
    {
        if (roomCleared || phaseChanging)
            return;

        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

        Debug.Log("[Room] Enemy killed in room " + roomNumber + ". Remaining: " + enemiesAlive);

        if (enemiesAlive > 0)
            return;

        if (currentPhase >= totalPhases)
        {
            RoomCleared();
            return;
        }

        phaseChanging = true;
        currentPhase++;

        Debug.Log("[Room] Starting phase " + currentPhase + "/" + totalPhases + " in room " + roomNumber);

        if (enemySpawner != null)
            enemiesAlive = enemySpawner.SpawnPhase();

        phaseChanging = false;

        if (enemiesAlive == 0)
            RoomCleared();
    }

    private void RoomCleared()
    {
        roomCleared = true;
        enemiesAlive = 0;
        phaseChanging = false;

        Debug.Log("[Room] RoomCleared called for room " + roomNumber);

        if (exit != null)
            exit.SetActive(false);

        if (roomPoints != null)
        {
            roomPoints.OpenTheDoor();
            Debug.Log("[Room] Entry door opened because room " + roomNumber + " was cleared.");
        }

        entryClosed = false;

        if (roomGenerator != null)
            roomGenerator.OpenNextRoomEntry(roomNumber);

        if (TeamManager.Instance != null)
            TeamManager.Instance.SetFormationMode(BotFormationMode.Idle);
    }

    public bool IsCleared()
    {
        return roomCleared;
    }

    public bool IsBossRoom()
    {
        return isBossRoom;
    }

    public int GetRoomNumber()
    {
        return roomNumber;
    }

    public void SetRoomNumber(int number)
    {
        roomNumber = number;
        isBossRoom = number % 10 == 0;
    }

    public Collider2D GetMapBounds()
    {
        return mapBounds;
    }

    public int GetCurrentPhase()
    {
        return currentPhase;
    }

    public int GetTotalPhases()
    {
        return totalPhases;
    }

    public void RestoreState(bool cleared, int phase, int phases, List<MapSaveSystem.EnemySaveData> enemies)
    {
        restoreState = true;
        roomCleared = cleared;
        currentPhase = phase;
        totalPhases = phases;
        savedEnemies = enemies ?? new List<MapSaveSystem.EnemySaveData>();

        Debug.Log("[Room] RestoreState called. Room: " + roomNumber + ", Cleared: " + cleared + ", Phase: " + phase + "/" + phases + ", Saved enemies: " + savedEnemies.Count);
    }

    public MapSaveSystem.RoomData GetSaveData(string prefabName)
    {
        return new MapSaveSystem.RoomData
        {
            roomNumber = roomNumber,
            prefabName = prefabName,
            position = transform.position,
            isCleared = roomCleared,
            currentPhase = currentPhase,
            totalPhases = totalPhases,
            enemies = enemySpawner != null ? enemySpawner.GetEnemySaveData() : new List<MapSaveSystem.EnemySaveData>()
        };
    }
}