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

    private int enemiesAlive;
    private RoomPoints roomPoints;

    private void Start()
    {
        roomPoints = GetComponentInChildren<RoomPoints>();
        enemiesAlive = CountEnemies();

        if (exit != null)
            exit.SetActive(enemiesAlive > 0);

        if (enemiesAlive == 0)
            RoomCleared();
    }

    private int CountEnemies()
    {
        if (enemyParent == null)
            return 0;

        int count = 0;

        for (int i = 0; i < enemyParent.childCount; i++)
        {
            Transform child = enemyParent.GetChild(i);

            if (!child.gameObject.activeInHierarchy)
                continue;

            EnemyAI enemy = child.GetComponent<EnemyAI>();

            if (enemy != null)
                count++;
        }

        return count;
    }

    public void EnemyKilled()
    {
        enemiesAlive--;

        if (enemiesAlive <= 0)
            RoomCleared();
    }

    private void RoomCleared()
    {
        enemiesAlive = 0;

        if (exit != null)
            exit.SetActive(false);

        if (roomPoints != null)
            roomPoints.OpenTheDoor();

        if (TeamManager.Instance != null)
            TeamManager.Instance.SetFormationMode(BotFormationMode.Idle);
    }

    public bool IsCleared()
    {
        return enemiesAlive <= 0;
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
}