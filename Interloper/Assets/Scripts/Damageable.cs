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
        if (hits >= maxHits) Die();
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
