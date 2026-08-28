using UnityEngine;

public class RoomTrigger : MonoBehaviour
{
    [SerializeField] private Room room;
    [SerializeField] private RoomPoints roomPoints;

    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated) return;
        if (!other.CompareTag("Player")) return;
        activated = true;
        roomPoints = this.transform.GetComponentInParent<RoomPoints>();
        roomPoints.CloseTheDoor();
        CameraFollow cameraFollow = FindFirstObjectByType<CameraFollow>();
        if (cameraFollow != null) cameraFollow.SetBounds(room.GetMapBounds());
        RoomGenerator generator = FindFirstObjectByType<RoomGenerator>();
        if (generator != null) generator.GenerateNextRoom();
    }
}