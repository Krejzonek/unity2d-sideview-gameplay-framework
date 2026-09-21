using System.Collections;
using UnityEngine;

public class ClassSelector : MonoBehaviour
{
    [SerializeField] private GameObject uiClass;
    [SerializeField] private LayerMask classStationLayer;

    private Camera cam;
    private Player player;

    private void Start()
    {
        cam = Camera.main;
        player = FindFirstObjectByType<Player>();

        if (uiClass != null)
            uiClass.SetActive(false);
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);

        Collider2D hit = Physics2D.OverlapPoint(mousePosition, classStationLayer);

        if (hit != null && hit.transform == transform)
        {
            bool isOpen = uiClass.activeSelf;
            uiClass.SetActive(!isOpen);

            if (player != null)
            {
                if (isOpen)
                    StartCoroutine(EnableAttack());
                else
                    player.SetCanAttack(false);
            }
        }
    }

    private IEnumerator EnableAttack()
    {
        yield return null;
        player.SetCanAttack(true);
    }
}