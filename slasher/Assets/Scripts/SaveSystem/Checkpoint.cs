using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color activeColor = Color.green;

    private static Checkpoint activeCheckpoint;

    private void Start()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (CharacterManager.Instance != null && CharacterManager.Instance.HasCheckpoint &&
            Vector2.Distance(CharacterManager.Instance.CheckpointPosition, transform.position) < 0.1f)
            activeCheckpoint = this;

        UpdateColor();
    }

    private void OnMouseDown()
    {
        if (CharacterManager.Instance == null || TeamManager.Instance == null)
        {
            Debug.LogError("Checkpoint: CharacterManager or TeamManager is missing!");
            return;
        }

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