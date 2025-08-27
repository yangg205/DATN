using System.Collections.Generic;
using UnityEngine;

public class EnemyTriggerSpawner : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public List<GameObject> enemyPrefabs;

    [Header("Spawn Settings")]
    public Transform[] spawnPoints;
    public float lifeTime = 3;
    public int enemyCount = 5;
    public bool spawnOnlyOnce = true;

    private bool hasSpawned = false;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (spawnOnlyOnce && hasSpawned) return;

        SpawnEnemies();
        hasSpawned = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                // đánh dấu là despawn
                var des = enemy.GetComponent<EnemyDesQuest>();
                if (des != null) des.isDespawned = true;

                Destroy(enemy, lifeTime);
            }
        }

        spawnedEnemies.Clear();
    }


    void SpawnEnemies()
    {
        if (enemyPrefabs.Count == 0 || spawnPoints.Length == 0)
        {
            Debug.LogWarning("EnemyTriggerSpawner: Không có prefab hoặc spawn point!");
            return;
        }

        for (int i = 0; i < enemyCount; i++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Count)];
            Transform point = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject enemy = Instantiate(prefab, point.position, Quaternion.identity);
            spawnedEnemies.Add(enemy);
        }
    }
}
