using System.Collections;
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

    public int SpawnPhase()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0) return 0;
        if (spawnPoint == null) return 0;
        if (enemyParent == null) return 0;

        int amount = Random.Range(minEnemies, maxEnemies + 1);
        float startX = -((amount - 1) * spacing) / 2f;

        GameObject supportPrefab = null;
        GameObject tankPrefab = null;

        GameObject[] normalPrefabs = new GameObject[enemyPrefabs.Length];
        int normalCount = 0;

        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab == null)
                continue;

            EnemyAI enemyAI = prefab.GetComponent<EnemyAI>();

            if (enemyAI == null)
                continue;

            if (enemyAI.GetEnemyClass() == EnemyClass.Support)
            {
                if (supportPrefab == null)
                    supportPrefab = prefab;

                continue;
            }

            if (enemyAI.GetEnemyClass() == EnemyClass.Tank)
            {
                if (tankPrefab == null)
                    tankPrefab = prefab;

                continue;
            }

            normalPrefabs[normalCount] = prefab;
            normalCount++;
        }

        bool spawnSupport = supportPrefab != null && Random.Range(0, 2) == 1;
        bool spawnTank = tankPrefab != null && Random.Range(0, 2) == 1;

        if (amount == 1)
        {
            spawnSupport = false;
            spawnTank = tankPrefab != null && Random.Range(0, 2) == 1;
        }

        if (amount == 2 && spawnSupport && spawnTank)
        {
            if (Random.Range(0, 2) == 0)
                spawnSupport = false;
            else
                spawnTank = false;
        }

        int spawnedEnemies = 0;

        for (int i = 0; i < amount; i++)
        {
            GameObject enemyPrefab = null;

            if (i == 0 && spawnTank)
            {
                enemyPrefab = tankPrefab;
            }
            else if (i == amount - 1 && spawnSupport)
            {
                enemyPrefab = supportPrefab;
            }
            else
            {
                if (normalCount == 0)
                    continue;

                enemyPrefab = normalPrefabs[Random.Range(0, normalCount)];
            }

            Vector3 position = spawnPoint.position;
            position.x += startX + i * spacing;

            GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity, enemyParent);
            EnemyAI enemy = enemyObject.GetComponent<EnemyAI>();

            if (enemy != null)
            {
                StartCoroutine(SpawnEnemy(enemy));
                spawnedEnemies++;
            }
        }

        return spawnedEnemies;
    }

    private IEnumerator SpawnEnemy(EnemyAI enemy)
    {
        EnemySpawnType spawnType = enemy.GetSpawnType();

        if (spawnType == EnemySpawnType.None)
            yield break;

        Transform enemyTransform = enemy.transform;

        Vector3 targetPosition = enemyTransform.position;
        Vector3 startPosition = targetPosition;

        if (spawnType == EnemySpawnType.FromGround)
            startPosition += Vector3.down * enemy.GetSpawnDistance();

        if (spawnType == EnemySpawnType.FromAir)
            startPosition += Vector3.up * enemy.GetSpawnDistance();

        enemyTransform.position = startPosition;
        enemy.SetSpawning(true);

        float timer = 0f;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.velocity = Vector2.zero;

        while (timer < enemy.GetSpawnTime())
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / enemy.GetSpawnTime());
            progress = progress * progress * (3f - 2f * progress);

            enemyTransform.position = Vector3.Lerp(startPosition, targetPosition, progress);

            yield return null;
        }

        enemyTransform.position = targetPosition;

        if (rb != null)
            rb.velocity = Vector2.zero;

        enemy.SetSpawning(false);
    }
}