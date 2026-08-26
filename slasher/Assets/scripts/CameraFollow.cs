using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private BoxCollider2D mapBounds;

    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (player == null || mapBounds == null)
            return;

        Bounds bounds = mapBounds.bounds;

        float halfHeight = cam.orthographicSize;
        float halfWidth = halfHeight * cam.aspect;

        float minX = bounds.min.x + halfWidth;
        float maxX = bounds.max.x - halfWidth;

        float minY = bounds.min.y + halfHeight;
        float maxY = bounds.max.y - halfHeight;

        float x = Mathf.Clamp(player.position.x, minX, maxX);
        float y = Mathf.Clamp(player.position.y, minY, maxY);

        transform.position = new Vector3(
            x,
            y,
            transform.position.z
        );
    }
}