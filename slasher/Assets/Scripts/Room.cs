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
    private bool phaseChanging;
    private bool roomCleared;

    private void Start()
    {
        roomPoints = GetComponentInChildren<RoomPoints>();
        enemySpawner = GetComponentInChildren<RoomEnemySpawner>();

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

    public void EnemyKilled()
    {
        if (roomCleared || phaseChanging)
            return;

        enemiesAlive--;

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
        if (roomCleared)
            return;

        roomCleared = true;
        enemiesAlive = 0;
        phaseChanging = false;

        if (exit != null)
            exit.SetActive(false);

        if (roomPoints != null)
            roomPoints.OpenTheDoor();

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
}