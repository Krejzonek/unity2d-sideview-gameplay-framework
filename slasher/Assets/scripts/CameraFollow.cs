using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float groundY = -5f;

    private UnityEngine.Camera cam;

    private void Start()
    {
        cam = GetComponent<UnityEngine.Camera>();
    }

    private void LateUpdate()
    {
        if (player == null)
            return;

        float targetX = player.position.x;
        float targetY = player.position.y;

        float cameraBottom = targetY - cam.orthographicSize;

        if (cameraBottom < groundY)
        {
            targetY = groundY + cam.orthographicSize;
        }

        Vector3 targetPosition = new Vector3(
            targetX,
            targetY,
            transform.position.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );
    }
}