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

        int amount = Random.Range(minEnemies, maxEnemies + 1);
        float startX = -((amount - 1) * spacing) / 2f;

        GameObject supportPrefab = null;
        GameObject[] normalPrefabs = new GameObject[enemyPrefabs.Length];
        int normalCount = 0;

        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab == null) continue;

            EnemyAI enemyAI = prefab.GetComponent<EnemyAI>();

            if (enemyAI != null && enemyAI.GetEnemyClass() == EnemyClass.Support)
            {
                supportPrefab = prefab;
            }
            else
            {
                normalPrefabs[normalCount] = prefab;
                normalCount++;
            }
        }

        bool spawnSupport = supportPrefab != null && Random.Range(0, 2) == 1;

        for (int i = 0; i < amount; i++)
        {
            GameObject enemyPrefab;

            if (spawnSupport && i == amount - 1)
            {
                enemyPrefab = supportPrefab;
            }
            else
            {
                if (normalCount == 0) continue;

                enemyPrefab = normalPrefabs[Random.Range(0, normalCount)];
            }

            Vector3 position = spawnPoint.position;
            position.x += startX + i * spacing;

            Instantiate(enemyPrefab, position, Quaternion.identity, enemyParent);
        }
    }
}