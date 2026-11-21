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
    private bool encounterStarted;
    private bool combatStarted;
    private bool enemiesRestored;
    private List<MapSaveSystem.EnemySaveData> savedEnemies = new();

    private void Start()
    {
        roomPoints = GetComponentInChildren<RoomPoints>();
        enemySpawner = GetComponentInChildren<RoomEnemySpawner>();

        if (enemySpawner == null)
            enemySpawner = GetComponent<RoomEnemySpawner>();

        roomGenerator = FindFirstObjectByType<RoomGenerator>();

        if (restoreState)
        {
            if (roomCleared)
            {
                enemiesAlive = 0;

                if (exit != null)
                    exit.SetActive(false);

                if (roomPoints != null)
                    roomPoints.OpenTheDoor();

                if (roomGenerator != null)
                    roomGenerator.OpenNextRoomEntry(roomNumber);
            }
            else
            {
                if (exit != null)
                    exit.SetActive(true);

                if (enemySpawner != null)
                    enemiesAlive = enemySpawner.RestoreEnemies(savedEnemies);

                enemiesRestored = true;

                if (enemySpawner != null && enemySpawner.HasEliteSpawned())
                    totalPhases = 1;

                if (enemiesAlive == 0)
                {
                    roomCleared = false;
                    Debug.LogWarning("[Room] Room " + roomNumber + " restored with 0 enemies but remains uncleared.");
                }
            }

            return;
        }

        if (roomNumber == 0)
        {
            roomCleared = true;
            encounterStarted = true;
            combatStarted = false;
            enemiesAlive = 0;

            if (exit != null)
                exit.SetActive(false);

            if (roomPoints != null)
                roomPoints.OpenTheDoor();

            if (roomGenerator != null)
                roomGenerator.OpenNextRoomEntry(roomNumber);

            return;
        }

        if (roomPoints != null)
            roomPoints.OpenTheDoor();

        minPhases = Mathf.Max(1, minPhases);
        maxPhases = Mathf.Max(minPhases, maxPhases);

        totalPhases = isBossRoom ? 1 : Random.Range(minPhases, maxPhases + 1);
        currentPhase = 1;

        if (exit != null)
            exit.SetActive(true);
    }

    public void BeginEncounter()
    {
        if (roomCleared || encounterStarted || roomNumber == 0)
            return;

        encounterStarted = true;

        if (!enemiesRestored)
        {
            if (enemySpawner != null)
                enemiesAlive = enemySpawner.SpawnPhase(false);
        }

        if (enemySpawner != null && enemySpawner.HasEliteSpawned())
            totalPhases = 1;

        if (enemiesAlive == 0)
            RoomCleared();
    }

    public void StartCombat()
    {
        if (roomCleared || roomNumber == 0)
            return;

        combatStarted = true;
    }

    public bool IsCombatStarted()
    {
        return combatStarted;
    }

    public bool IsStartingRoom()
    {
        return roomNumber == 0;
    }

    public bool AreEnemiesSpawning()
    {
        return enemySpawner != null && enemySpawner.AreEnemiesSpawning();
    }

    public DialogueLine GetDialogueLine()
    {
        if (enemyParent == null)
            return null;

        EnemyAI[] enemies = enemyParent.GetComponentsInChildren<EnemyAI>(true);

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null || enemy.IsDead() || !enemy.IsElite())
                continue;

            DialogueLine dialogue = enemy.GetDialogueLine();

            if (dialogue != null)
                return dialogue;
        }

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null || enemy.IsDead())
                continue;

            DialogueLine dialogue = enemy.GetDialogueLine();

            if (dialogue != null)
                return dialogue;
        }

        return null;
    }

    public void RestoreEntryDoorState(bool close)
    {
        if (roomPoints == null)
            roomPoints = GetComponentInChildren<RoomPoints>();

        if (roomPoints == null)
            return;

        bool shouldClose = close && !roomCleared && roomNumber != 0;

        if (shouldClose)
        {
            roomPoints.CloseTheDoor();
            entryClosed = true;
        }
        else
        {
            roomPoints.OpenTheDoor();
            entryClosed = false;
        }
    }

    public void EnemyKilled()
    {
        if (roomCleared || phaseChanging || !combatStarted)
            return;

        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);

        if (enemiesAlive > 0)
            return;

        if (enemySpawner != null && enemySpawner.HasEliteSpawned())
        {
            RoomCleared();
            return;
        }

        if (currentPhase >= totalPhases)
        {
            RoomCleared();
            return;
        }

        phaseChanging = true;
        currentPhase++;

        if (enemySpawner != null)
            enemiesAlive = enemySpawner.SpawnPhase(true);

        phaseChanging = false;

        if (enemiesAlive == 0)
            RoomCleared();
    }

    private void RoomCleared()
    {
        roomCleared = true;
        enemiesAlive = 0;
        phaseChanging = false;
        combatStarted = false;

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

    public int GetRoomNumber()
    {
        return roomNumber;
    }

    public void SetRoomNumber(int number)
    {
        roomNumber = number;
        isBossRoom = number > 0 && number % 10 == 0;
    }

    public Collider2D GetMapBounds()
    {
        return mapBounds;
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