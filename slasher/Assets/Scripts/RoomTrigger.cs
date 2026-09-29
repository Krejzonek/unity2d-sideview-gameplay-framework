using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [SerializeField] private Room room;
    [SerializeField] private RoomPoints roomPoints;

    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        Player player = other.GetComponent<Player>();

        if (player == null)
            return;

        CameraFollow cameraFollow = FindFirstObjectByType<CameraFollow>();

        if (cameraFollow != null)
            cameraFollow.SetBounds(room.GetMapBounds());

        if (activated)
            return;

        activated = true;

        if (roomPoints == null)
            roomPoints = GetComponentInParent<RoomPoints>();

        if (roomPoints != null)
            roomPoints.CloseTheDoor();

        RoomGenerator generator = FindFirstObjectByType<RoomGenerator>();

        if (generator != null)
            generator.GenerateNextRoom();
    }
}