using System.Collections.Generic;
using UnityEngine;

public class RestRoom : MonoBehaviour
{
    [Header("Healing")]
    [SerializeField] private float healPercentPerSecond = 5f;

    [Header("Resurrection")]
    [SerializeField] private GameObject resurrectionStatuePrefab;
    [SerializeField] private Transform statueSpawnPoint;

    private readonly List<Player> playersInside = new List<Player>();
    private bool mapSaved;

    private void Start()
    {
        if (resurrectionStatuePrefab == null)
            return;

        Vector3 position = statueSpawnPoint != null ? statueSpawnPoint.position : transform.position;
        Instantiate(resurrectionStatuePrefab, position, Quaternion.identity, transform);
    }

    private void Update()
    {
        foreach (Player player in playersInside)
        {
            if (player == null || player.IsDead())
                continue;

            player.Heal(player.GetMaxHealth() * (healPercentPerSecond / 100f) * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Player player = collision.GetComponentInParent<Player>();

        if (player == null || playersInside.Contains(player))
            return;

        playersInside.Add(player);

        if (!mapSaved && MapSaveSystem.Instance != null)
        {
            RoomGenerator generator = FindFirstObjectByType<RoomGenerator>();

            if (generator != null)
            {
                MapSaveSystem.Instance.SaveMap(generator);
                mapSaved = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Player player = collision.GetComponentInParent<Player>();

        if (player == null || !playersInside.Contains(player))
            return;

        playersInside.Remove(player);
    }
}