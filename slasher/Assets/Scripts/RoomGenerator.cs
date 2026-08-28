using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Rooms")]
    [SerializeField] private GameObject[] roomPrefabs;
    [SerializeField] private GameObject bossRoomPrefab;

    [Header("Generation")]
    [SerializeField] private int maxRooms = 30;

    [Header("Start")]
    [SerializeField] private Room startRoom;

    private Room currentRoom;
    private int nextRoomNumber = 1;
    private Transform previousExit;
    private int lastRoomIndex = -1;

    private void Start()
    {
        if (startRoom == null) return;

        currentRoom = startRoom;

        RoomPoints points = startRoom.GetComponent<RoomPoints>();

        if (points == null) return;

        previousExit = points.GetExit();

        GenerateNextRoom();
    }

    public void GenerateNextRoom()
    {
        if (nextRoomNumber > maxRooms) return;

        GameObject prefab;

        if (nextRoomNumber % 10 == 0 && bossRoomPrefab != null)
        {
            prefab = bossRoomPrefab;
        }
        else
        {
            if (roomPrefabs.Length == 0) return;

            int randomIndex;

            do
            {
                randomIndex = Random.Range(0, roomPrefabs.Length);
            }
            while (roomPrefabs.Length > 1 && randomIndex == lastRoomIndex);
            lastRoomIndex = randomIndex;
            prefab = roomPrefabs[randomIndex];
        }

        GameObject roomObject = Instantiate(prefab,Vector3.zero,Quaternion.identity,transform);

        Room room = roomObject.GetComponent<Room>();
        RoomPoints points = roomObject.GetComponent<RoomPoints>();

        if (room == null || points == null)
        {
            Destroy(roomObject);
            return;
        }

        room.SetRoomNumber(nextRoomNumber);

        Transform entry = points.GetEntry();

        roomObject.transform.position += previousExit.position - entry.position;

        currentRoom = room;
        previousExit = points.GetExit();

        nextRoomNumber++;
    }
}