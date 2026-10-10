using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Rooms")]
    [SerializeField] private GameObject[] roomPrefabs;
    [SerializeField] private GameObject bossRoomPrefab;
    [SerializeField] private GameObject restRoomPrefab;

    [Header("Generation")]
    [SerializeField] private int maxRooms = 100;

    [Header("Start")]
    [SerializeField] private Room startRoom;

    private Room currentRoom;
    private int nextRoomNumber = 1;
    private Transform previousExit;
    private int lastRoomIndex = -1;
    private readonly List<Room> generatedRooms = new List<Room>();
    private readonly List<string> generatedPrefabNames = new List<string>();

    private void Start()
    {
        if (startRoom == null)
            return;

        currentRoom = startRoom;

        RoomPoints points = startRoom.GetComponentInChildren<RoomPoints>();

        if (points == null)
            return;

        if (MapSaveSystem.Instance != null && MapSaveSystem.Instance.TryLoadMap(out MapSaveSystem.MapData data))
        {
            LoadMap(data);
            return;
        }

        previousExit = points.GetExit();
        generatedRooms.Add(startRoom);
        generatedPrefabNames.Add(startRoom.gameObject.name);

        GenerateNextRoom();
    }

    public void GenerateNextRoom()
    {
        if (nextRoomNumber > maxRooms)
            return;

        GameObject prefab;
        int roomPosition = nextRoomNumber % 10;
        int selectedIndex = -1;

        if (roomPosition == 0 && bossRoomPrefab != null)
        {
            prefab = bossRoomPrefab;
        }
        else if ((roomPosition == 5 || roomPosition == 9) && restRoomPrefab != null)
        {
            prefab = restRoomPrefab;
        }
        else
        {
            if (roomPrefabs == null || roomPrefabs.Length == 0)
                return;

            int randomIndex;

            do
            {
                randomIndex = Random.Range(0, roomPrefabs.Length);
            }
            while (roomPrefabs.Length > 1 && randomIndex == lastRoomIndex);

            lastRoomIndex = randomIndex;
            selectedIndex = randomIndex;
            prefab = roomPrefabs[randomIndex];
        }

        if (!CreateRoom(prefab, nextRoomNumber, null))
            return;

        nextRoomNumber++;
    }

    private bool CreateRoom(GameObject prefab, int number, MapSaveSystem.RoomData savedData)
    {
        if (prefab == null || previousExit == null)
            return false;

        GameObject roomObject = Instantiate(prefab, Vector3.zero, Quaternion.identity, transform);

        Room room = roomObject.GetComponent<Room>();
        RoomPoints points = roomObject.GetComponentInChildren<RoomPoints>();

        if (room == null || points == null)
        {
            Destroy(roomObject);
            return false;
        }

        room.SetRoomNumber(number);

        if (savedData != null)
        {
            roomObject.transform.position = savedData.position;
            room.RestoreState(savedData.isCleared, savedData.currentPhase, savedData.totalPhases);
        }
        else
        {
            Transform entry = points.GetEntry();
            roomObject.transform.position += previousExit.position - entry.position;
        }

        previousExit = points.GetExit();
        currentRoom = room;
        generatedRooms.Add(room);
        generatedPrefabNames.Add(prefab.name);

        return true;
    }

    public MapSaveSystem.MapData GetSaveData()
    {
        MapSaveSystem.MapData data = new MapSaveSystem.MapData
        {
            nextRoomNumber = nextRoomNumber,
            lastRoomIndex = lastRoomIndex,
            currentRoomNumber = currentRoom != null ? currentRoom.GetRoomNumber() : 0
        };

        for (int i = 0; i < generatedRooms.Count; i++)
        {
            if (generatedRooms[i] == null)
                continue;

            data.rooms.Add(generatedRooms[i].GetSaveData(generatedPrefabNames[i]));
        }

        return data;
    }

    private void LoadMap(MapSaveSystem.MapData data)
    {
        if (startRoom == null || data == null || data.rooms.Count == 0)
            return;

        RoomPoints startPoints = startRoom.GetComponentInChildren<RoomPoints>();

        if (startPoints == null)
            return;

        currentRoom = startRoom;
        previousExit = startPoints.GetExit();
        nextRoomNumber = data.nextRoomNumber;
        lastRoomIndex = data.lastRoomIndex;

        generatedRooms.Clear();
        generatedPrefabNames.Clear();
        generatedRooms.Add(startRoom);
        generatedPrefabNames.Add(startRoom.gameObject.name);

        for (int i = 1; i < data.rooms.Count; i++)
        {
            MapSaveSystem.RoomData roomData = data.rooms[i];
            GameObject prefab = FindPrefab(roomData.prefabName);

            if (prefab == null)
            {
                Debug.LogError("Missing room prefab: " + roomData.prefabName);
                continue;
            }

            if (!CreateRoom(prefab, roomData.roomNumber, roomData))
                continue;

            Room loadedRoom = generatedRooms[generatedRooms.Count - 1];

            if (roomData.roomNumber == data.currentRoomNumber)
                currentRoom = loadedRoom;
        }

        if (currentRoom == null)
            currentRoom = startRoom;
    }

    private GameObject FindPrefab(string prefabName)
    {
        if (roomPrefabs != null)
        {
            foreach (GameObject prefab in roomPrefabs)
                if (prefab != null && prefab.name == prefabName)
                    return prefab;
        }

        if (bossRoomPrefab != null && bossRoomPrefab.name == prefabName)
            return bossRoomPrefab;

        if (restRoomPrefab != null && restRoomPrefab.name == prefabName)
            return restRoomPrefab;

        return null;
    }
}