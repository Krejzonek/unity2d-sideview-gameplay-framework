using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;

    private Camera cam;
    private Collider2D mapBounds;
    private Room currentRoom;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        UpdatePlayer();

        if (player != null)
            UpdateRoom();
    }

    private void LateUpdate()
    {
        UpdatePlayer();

        if (player == null)
            return;

        UpdateRoom();

        float x = player.position.x;
        float y = player.position.y;

        if (mapBounds != null)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            Bounds bounds = mapBounds.bounds;

            float minX = bounds.min.x + halfWidth;
            float maxX = bounds.max.x - halfWidth;

            float minY = bounds.min.y + halfHeight;
            float maxY = bounds.max.y - halfHeight;

            if (minX < maxX)
                x = Mathf.Clamp(x, minX, maxX);
            else
                x = bounds.center.x;

            if (minY < maxY)
                y = Mathf.Clamp(y, minY, maxY);
            else
                y = bounds.center.y;
        }

        transform.position = new Vector3(x, y, transform.position.z);
    }

    private void UpdatePlayer()
    {
        if (TeamManager.Instance == null)
            return;

        Player currentPlayer = TeamManager.Instance.CurrentPlayer;

        if (currentPlayer != null)
            player = currentPlayer.transform;
    }

    private void UpdateRoom()
    {
        Room[] rooms = FindObjectsByType<Room>(FindObjectsSortMode.None);

        Room closestRoom = null;
        float closestDistance = Mathf.Infinity;

        foreach (Room room in rooms)
        {
            if (room == null)
                continue;

            Collider2D bounds = room.GetMapBounds();

            if (bounds == null)
                continue;

            if (bounds.OverlapPoint(player.position))
            {
                currentRoom = room;
                mapBounds = bounds;
                return;
            }

            float distance = Vector2.Distance(player.position, bounds.ClosestPoint(player.position));

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestRoom = room;
            }
        }

        if (closestRoom != null)
        {
            currentRoom = closestRoom;
            mapBounds = closestRoom.GetMapBounds();
        }
    }

    public void SetBounds(Collider2D newBounds)
    {
        if (newBounds == null)
            return;

        mapBounds = newBounds;
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }
}