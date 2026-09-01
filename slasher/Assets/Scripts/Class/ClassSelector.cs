using UnityEngine;

public class ClassSelector : MonoBehaviour
{
    [SerializeField] private GameObject uiClass;
    [SerializeField] private LayerMask classStationLayer;

    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        if (uiClass != null)
            uiClass.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

        Collider2D hit = Physics2D.OverlapPoint(
            mousePosition,
            classStationLayer
        );

        if (hit != null && hit.transform == transform)
        {
            uiClass.SetActive(true);
        }
    }
}