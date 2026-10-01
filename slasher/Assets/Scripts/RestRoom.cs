using System.Collections.Generic;
using UnityEngine;

public class RestRoom : MonoBehaviour
{
    [SerializeField] private float healPerSecond = 5f;

    private List<Player> playersInside = new List<Player>();

    private void Update()
    {
        foreach (Player player in playersInside)
        {
            if (player == null || player.IsDead())
                continue;

            player.SetCanAttack(false);
            player.Heal(healPerSecond * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponentInParent<Player>();

        if (player == null || playersInside.Contains(player))
            return;

        playersInside.Add(player);
        player.SetCanAttack(false);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponentInParent<Player>();

        if (player == null || !playersInside.Contains(player))
            return;

        playersInside.Remove(player);
        player.SetCanAttack(true);
    }
}