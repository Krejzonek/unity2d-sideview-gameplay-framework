using UnityEngine;

public class RoomPoints : MonoBehaviour
{
    [SerializeField] private Transform entry;
    [SerializeField] private Transform exit;

    public Transform GetEntry()
    {
        return entry;
    }

    public Transform GetExit()
    {
        return exit;
    }

    public void CloseTheDoor()
    {
        entry.gameObject.SetActive(true);
    }

    public void OpenTheDoor()
    {
        entry.gameObject.SetActive(false);
    }
}