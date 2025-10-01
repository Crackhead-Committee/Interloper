using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerLife : MonoBehaviour
{
    [Header("Respawn")]
    public Transform respawnPoint;
    public float respawnDelay = 0.05f;

    Rigidbody rb;
    MonoBehaviour movement;
    bool respawning;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<MonoBehaviour>();
        movement = GetComponent<PlayerController>() ?? movement;
    }

    public void Die()
    {
        if (respawning) return;
        StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        respawning = true;

        if (movement) movement.enabled = false;

        // stop motion
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(respawnDelay);

        if (respawnPoint != null)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning("PlayerLife: No respawnPoint assigned.");
        }

        yield return new WaitForFixedUpdate();

        if (movement) movement.enabled = true;
        respawning = false;
    }

    void OnDrawGizmosSelected()
    {
        if (respawnPoint == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(respawnPoint.position, 0.25f);
        Gizmos.DrawLine(transform.position, respawnPoint.position);
    }
}
