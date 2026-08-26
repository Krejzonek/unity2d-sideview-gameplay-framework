using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private LayerMask mapBoundsLayer;

    private Camera cam;
    private Collider2D mapBounds;

    private void Start()
    {
        cam = GetComponent<Camera>();

        Collider2D[] colliders = FindObjectsByType<Collider2D>(
            FindObjectsSortMode.None
        );

        foreach (Collider2D collider in colliders)
        {
            if ((mapBoundsLayer.value & (1 << collider.gameObject.layer)) != 0)
            {
                mapBounds = collider;
                break;
            }
        }
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 targetPosition = new Vector3(
            player.position.x,
            player.position.y,
            transform.position.z
        );

        if (mapBounds != null)
        {
            Bounds bounds = mapBounds.bounds;

            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            float minX = bounds.min.x + halfWidth;
            float maxX = bounds.max.x - halfWidth;

            float minY = bounds.min.y + halfHeight;
            float maxY = bounds.max.y - halfHeight;

            targetPosition.x = Mathf.Clamp(
                targetPosition.x,
                minX,
                maxX
            );

            targetPosition.y = Mathf.Clamp(
                targetPosition.y,
                minY,
                maxY
            );
        }

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}