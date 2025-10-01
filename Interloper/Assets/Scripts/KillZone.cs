using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillZone : MonoBehaviour
{
    void Reset()
    {
        // Make sure it's a trigger volume
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // Works even if the player's collider is on a child
        var life = other.GetComponentInParent<PlayerLife>();
        if (life != null)
        {
            life.Die();
        }
    }
}
