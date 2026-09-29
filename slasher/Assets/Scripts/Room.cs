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
        enemiesAlive = enemyParent.childCount;
        roomPoints = GetComponentInChildren<RoomPoints>();

        if (exit != null)
            exit.SetActive(true);

        if (enemiesAlive == 0)
            RoomCleared();
    }

    public void EnemyKilled()
    {
        enemiesAlive--;

        if (IsCleared())
            RoomCleared();
    }

    private void RoomCleared()
    {
        enemiesAlive = 0;

        if (exit != null)
            exit.SetActive(false);

        if (roomPoints != null)
            roomPoints.OpenTheDoor();
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