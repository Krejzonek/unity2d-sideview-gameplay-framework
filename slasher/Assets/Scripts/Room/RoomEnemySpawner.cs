using System.Collections;
using System.Collections.Generic;
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
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || spawnPoint == null || enemyParent == null)
        {
            Debug.LogWarning($"[EnemySpawner] SpawnPhase aborted | prefabs={enemyPrefabs?.Length ?? 0} | spawnPoint={spawnPoint != null} | enemyParent={enemyParent != null}", this);
            return 0;
        }

        int amount = Random.Range(minEnemies, maxEnemies + 1);
        float startX = -((amount - 1) * spacing) / 2f;

        GameObject supportPrefab = null;
        GameObject tankPrefab = null;
        List<GameObject> normalPrefabs = new();

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

            normalPrefabs.Add(prefab);
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
                enemyPrefab = tankPrefab;
            else if (i == amount - 1 && spawnSupport)
                enemyPrefab = supportPrefab;
            else
            {
                if (normalPrefabs.Count == 0)
                    continue;

                enemyPrefab = normalPrefabs[Random.Range(0, normalPrefabs.Count)];
            }

            if (enemyPrefab == null)
                continue;

            Vector3 position = spawnPoint.position;
            position.x += startX + i * spacing;

            GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity, enemyParent);
            EnemyAI enemy = enemyObject.GetComponent<EnemyAI>();

            if (enemy != null)
            {
                StartCoroutine(SpawnEnemy(enemy));
                spawnedEnemies++;
            }
            else
            {
                Destroy(enemyObject);
            }
        }
        return spawnedEnemies;
    }

    public List<MapSaveSystem.EnemySaveData> GetEnemySaveData()
    {
        List<MapSaveSystem.EnemySaveData> savedEnemies = new();

        if (enemyParent == null)
        {
            Debug.LogWarning("[EnemySpawner] GetEnemySaveData: enemyParent is NULL", this);
            return savedEnemies;
        }

        EnemyAI[] enemies = enemyParent.GetComponentsInChildren<EnemyAI>(true);

        foreach (EnemyAI enemy in enemies)
        {
            if (enemy == null || enemy.IsDead())
                continue;

            string prefabName = enemy.gameObject.name.Replace("(Clone)", "").Trim();
            savedEnemies.Add(enemy.GetSaveData(prefabName));
        }
        return savedEnemies;
    }

    public int RestoreEnemies(List<MapSaveSystem.EnemySaveData> savedEnemies)
    {
        if (savedEnemies == null || savedEnemies.Count == 0 || enemyParent == null)
        {
            Debug.LogWarning($"[EnemySpawner] RestoreEnemies returned 0 | savedListNull={savedEnemies == null} | savedCount={savedEnemies?.Count ?? -1} | enemyParent={enemyParent != null}", this);
            return 0;
        }

        int restoredCount = 0;

        foreach (MapSaveSystem.EnemySaveData saved in savedEnemies)
        {
            if (saved == null)
            {
                Debug.LogWarning("[EnemySpawner] Skipped NULL EnemySaveData", this);
                continue;
            }

            GameObject prefab = FindEnemyPrefab(saved.prefabName);

            if (prefab == null)
            {
                Debug.LogError($"[EnemySpawner] Missing enemy prefab='{saved.prefabName}' | available={(enemyPrefabs == null ? 0 : enemyPrefabs.Length)}", this);
                continue;
            }

            GameObject enemyObject = Instantiate(prefab, saved.position, Quaternion.identity, enemyParent);
            enemyObject.transform.localScale = saved.localScale;

            EnemyAI enemy = enemyObject.GetComponent<EnemyAI>();

            if (enemy == null)
            {
                Debug.LogError($"[EnemySpawner] Prefab '{saved.prefabName}' has no EnemyAI component", enemyObject);
                Destroy(enemyObject);
                continue;
            }

            enemy.RestoreFromSave(saved);
            restoredCount++;
        }
        return restoredCount;
    }

    private GameObject FindEnemyPrefab(string prefabName)
    {
        if (enemyPrefabs == null)
            return null;

        foreach (GameObject prefab in enemyPrefabs)
        {
            if (prefab != null && prefab.name == prefabName)
                return prefab;
        }

        return null;
    }

    private IEnumerator SpawnEnemy(EnemyAI enemy)
    {
        if (enemy == null)
            yield break;

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
        float duration = enemy.GetSpawnTime();
        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.velocity = Vector2.zero;

        while (timer < duration)
        {
            if (enemy == null)
                yield break;

            timer += Time.deltaTime;

            float progress = duration > 0f ? Mathf.Clamp01(timer / duration) : 1f;
            progress = progress * progress * (3f - 2f * progress);

            enemyTransform.position = Vector3.Lerp(startPosition, targetPosition, progress);
            yield return null;
        }

        if (enemy == null)
            yield break;

        enemyTransform.position = targetPosition;

        if (rb != null)
            rb.velocity = Vector2.zero;

        enemy.SetSpawning(false);
    }
}