using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The enemy prefab with SimpleEnemyAI.")]
    public GameObject enemyPrefab;

    [Tooltip("The location where the enemy will spawn.")]
    public Transform spawnPoint;

    [Tooltip("Tag used to identify the player.")]
    public string playerTag = "Player";

    [Header("Options")]
    [Tooltip("Should this trigger only work once?")]
    public bool oneTimeTrigger = true;

    [Tooltip("Optional: small delay before spawn.")]
    public float spawnDelay = 0f;

    private bool _triggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (_triggered && oneTimeTrigger) return;

        _triggered = true;
        if (spawnDelay > 0f)
            Invoke(nameof(SpawnEnemy), spawnDelay);
        else
            SpawnEnemy();
    }

    void SpawnEnemy()
    {
        if (!enemyPrefab || !spawnPoint)
        {
            Debug.LogWarning("[EnemySpawnTrigger] Missing prefab or spawn point.");
            return;
        }

        Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("[EnemySpawnTrigger] Enemy spawned!");
    }
}
