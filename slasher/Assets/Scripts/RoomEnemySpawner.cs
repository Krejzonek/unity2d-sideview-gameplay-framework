using UnityEngine;

public class RoomEnemySpawner : MonoBehaviour
{
    [Header("Enemies")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Amount")]
    [SerializeField] private int minEnemies = 2;
    [SerializeField] private int maxEnemies = 5;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform enemyParent;
    [SerializeField] private float spacing = 2f;

    private void Awake()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return;
        if (spawnPoint == null) return;
        if (enemyParent == null) return;

        int amount = Random.Range(minEnemies,maxEnemies + 1);
        float startX = -((amount - 1) * spacing) / 2f;

        for (int i = 0; i < amount; i++)
        {
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            if (enemyPrefab == null) continue;

            Vector3 position = spawnPoint.position;
            position.x += startX + i * spacing;
            Instantiate(enemyPrefab,position,Quaternion.identity,enemyParent);
        }
    }
}