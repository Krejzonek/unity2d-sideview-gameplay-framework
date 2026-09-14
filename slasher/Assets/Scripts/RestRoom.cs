using UnityEngine;

public class RestRoom : MonoBehaviour
{
    [SerializeField] private float healPerSecond = 5f;

    private Player player;
    private bool playerInside;

    private void Update()
    {
        if (!playerInside || player == null)
            return;

        player.SetCanAttack(false);
        player.Heal(healPerSecond * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player playerComponent = collision.GetComponentInParent<Player>();

        if (playerComponent == null)
            return;

        player = playerComponent;
        playerInside = true;
        player.SetCanAttack(false);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player playerComponent = collision.GetComponentInParent<Player>();

        if (playerComponent == null)
            return;

        if (playerComponent == player)
        {
            player.SetCanAttack(true);
            player = null;
            playerInside = false;
        }
    }
}