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

        if (roomPoints != null)
            roomPoints.OpenTheDoor();

        if (restoreState)
        {
            if (roomCleared)
            {
                enemiesAlive = 0;

                if (exit != null)
                    exit.SetActive(false);

                if (roomPoints != null)
                    roomPoints.OpenTheDoor();
            }
            else
            {
                if (exit != null)
                    exit.SetActive(true);

                if (roomPoints != null)
                    roomPoints.OpenTheDoor();

                if (enemySpawner != null)
                    enemiesAlive = enemySpawner.RestoreEnemies(savedEnemies);

                if (enemiesAlive == 0)
                {
                    roomCleared = false;

                    if (exit != null)
                        exit.SetActive(true);

                    if (roomPoints != null)
                        roomPoints.OpenTheDoor();
                }
            }

            return;
        }

        minPhases = Mathf.Max(1, minPhases);
        maxPhases = Mathf.Max(minPhases, maxPhases);

        totalPhases = Random.Range(minPhases, maxPhases + 1);
        currentPhase = 1;

        if (exit != null)
            exit.SetActive(true);

        if (enemySpawner != null)
            enemiesAlive = enemySpawner.SpawnPhase();

        if (enemiesAlive == 0)
            RoomCleared();
    }

    private void Update()
    {
        if (roomCleared || entryClosed || mapBounds == null || roomPoints == null)
            return;

        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        foreach (Player player in players)
        {
            if (player == null || !player.gameObject.activeInHierarchy)
                continue;

            if (mapBounds.bounds.Contains(player.transform.position))
            {
                roomPoints.CloseTheDoor();
                entryClosed = true;
                return;
            }
        }
    }

    public void EnemyKilled()
    {
        if (roomCleared || phaseChanging)
            return;

        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

        if (enemiesAlive > 0)
            return;

        if (currentPhase >= totalPhases)
        {
            RoomCleared();
            return;
        }

        phaseChanging = true;
        currentPhase++;

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

        if (exit != null)
            exit.SetActive(false);

        if (roomPoints != null)
            roomPoints.OpenTheDoor();

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