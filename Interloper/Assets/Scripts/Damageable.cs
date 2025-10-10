using UnityEngine;
using UnityEngine.Events;

public class Damageable : MonoBehaviour
{
    [Header("Hits to kill")]
    public int maxHits = 1;

    [Header("On death")]
    public UnityEvent onDeath;
    public bool destroyOnDeath = true;
    public GameObject destroyTarget;

    int hits;

public void TakeHit(int amount = 1)
{
    hits += amount;
    int remaining = Mathf.Max(0, maxHits - hits);
    Debug.Log($"{gameObject.name} was hit! Total hits: {hits}/{maxHits}. Remaining: {remaining}");

    if (hits >= maxHits)
    {
        Debug.Log($"{gameObject.name} destroyed!");
        Die();
    }
}

    void Die()
    {
        onDeath?.Invoke();

        if (destroyOnDeath)
        {
            var target = destroyTarget ? destroyTarget : gameObject;
            Destroy(target);
        }
    }
}
