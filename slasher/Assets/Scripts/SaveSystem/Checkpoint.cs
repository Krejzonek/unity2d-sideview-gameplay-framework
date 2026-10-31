using UnityEngine;
using UnityEngine.EventSystems;

public class Checkpoint : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private LayerMask checkpointLayer;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color activeColor = Color.green;

    private static Checkpoint activeCheckpoint;
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (CharacterManager.Instance != null && CharacterManager.Instance.HasCheckpoint &&
            Vector2.Distance(CharacterManager.Instance.CheckpointPosition, transform.position) < 0.1f)
            activeCheckpoint = this;

        UpdateColor();
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0) || cam == null)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Vector2 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePosition, checkpointLayer);

        if (hit != null && hit.transform == transform)
            ActivateCheckpoint();
    }

    private void ActivateCheckpoint()
    {
        if (CharacterManager.Instance == null || TeamManager.Instance == null)
        {
            Debug.LogError("Checkpoint: CharacterManager or TeamManager is missing!");
            return;
        }

        Player.BlockAttackUntilMouseRelease();

        activeCheckpoint = this;
        CharacterManager.Instance.SetCheckpoint(transform.position);
        TeamManager.Instance.SetCheckpoint(transform);

        Debug.Log("Checkpoint saved at: " + transform.position);
        Debug.Log("Saved position: " + CharacterManager.Instance.CheckpointPosition);

        UpdateAllCheckpoints();
    }

    private void UpdateAllCheckpoints()
    {
        Checkpoint[] checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);

        foreach (Checkpoint checkpoint in checkpoints)
        {
            if (checkpoint != null)
                checkpoint.UpdateColor();
        }
    }

    private void UpdateColor()
    {
        if (spriteRenderer != null)
            spriteRenderer.color = activeCheckpoint == this ? activeColor : inactiveColor;
    }
}