using UnityEngine;

public class GamepadInteraction : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private float interactionRange = 1.5f;
    [SerializeField] private LayerMask interactableLayers = -1;

    private GameInputManager inputManager;
    private Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        ConnectInputManager();
    }

    private void OnDisable()
    {
        DisconnectInputManager();
    }

    private void Update()
    {
        if (inputManager == null)
            ConnectInputManager();
    }

    private void ConnectInputManager()
    {
        if (inputManager != null || GameInputManager.Instance == null)
            return;

        inputManager = GameInputManager.Instance;
        inputManager.InteractStarted += Interact;
    }

    private void DisconnectInputManager()
    {
        if (inputManager == null)
            return;

        inputManager.InteractStarted -= Interact;
        inputManager = null;
    }

    private void Interact()
    {
        if (player == null || player.IsDead() || TeamManager.Instance == null)
            return;

        if (TeamManager.Instance.CurrentPlayer != player)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactionRange, interactableLayers);

        IInteractable closestInteractable = null;
        float closestDistance = float.MaxValue;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            IInteractable interactable = hit.GetComponentInParent<IInteractable>();

            if (interactable == null)
                continue;

            float distance = Vector2.Distance(transform.position, hit.ClosestPoint(transform.position));

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestInteractable = interactable;
            }
        }

        closestInteractable?.Interact();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}