using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private Collider2D mapBounds;

    private Camera cam;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        if (TeamManager.Instance != null && TeamManager.Instance.CurrentPlayer != null)
            player = TeamManager.Instance.CurrentPlayer.transform;
    }

    private void LateUpdate()
    {
        if (TeamManager.Instance != null && TeamManager.Instance.CurrentPlayer != null)
            player = TeamManager.Instance.CurrentPlayer.transform;

        if (player == null || mapBounds == null)
            return;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        Bounds bounds = mapBounds.bounds;

        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;

        float minY = bounds.min.y + halfHeight;
        float maxY = bounds.max.y - halfHeight;

        float x = player.position.x;
        float y = player.position.y;

        if (minX < maxX) x = Mathf.Clamp(x, minX, maxX);
        else x = bounds.center.x;

        if (minY < maxY) y = Mathf.Clamp(y, minY, maxY);
        else y = bounds.center.y;

        transform.position = new Vector3(x, y, transform.position.z);
    }

    public void SetBounds(Collider2D newBounds)
    {
        mapBounds = newBounds;
    }

    public void SetPlayer(Transform newPlayer)
    {
        player = newPlayer;
    }
}