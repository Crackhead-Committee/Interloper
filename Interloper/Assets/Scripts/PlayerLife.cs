using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class PlayerLife : MonoBehaviour
{
    [Header("Health")]
    public float MaxHealth = 100f;
    public float RegenDelay = 10f;
    public float RegenPerSecond = 5f;
    public float RespawnInvulnTime = 0.75f;
    public TMP_Text healthText;

    [Header("Game Over")]
    public GameOverUI gameOverUI;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float respawnDelay = 0.05f;

    public float CurrentHealth { get; private set; } = -1f;
    public float HealthPercent => Mathf.Clamp01(CurrentHealth / Mathf.Max(1f, MaxHealth));

    Rigidbody rb;
    MonoBehaviour movement;
    bool respawning;
    bool invulnerable;
    float lastDamageTime = -999f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        movement = GetComponent<PlayerController>() ?? GetComponent<MonoBehaviour>();
        if (CurrentHealth < 0f) CurrentHealth = MaxHealth;
    }

    void Update()
    {
        if (healthText) healthText.text = Mathf.RoundToInt(HealthPercent * 100f) + "%";

        if (!respawning && CurrentHealth > 0f && CurrentHealth < MaxHealth)
        {
            if (Time.time - lastDamageTime >= RegenDelay)
            {
                CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + RegenPerSecond * Time.deltaTime);
            }
        }
    }

    public void ApplyDamage(float amount)
    {
        if (respawning || invulnerable || amount <= 0f) return;
        CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
        lastDamageTime = Time.time;
        if (CurrentHealth <= 0f) Die();
    }

    public void Heal(float amount)
    {
        if (amount <= 0f) return;
        CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
    }

    public void Die()
    {
        if (respawning) return;
        respawning = true;

        if (movement) movement.enabled = false;
        if (rb)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (gameOverUI != null)
        {
            gameOverUI.ShowGameOver();
        }
        else
        {
            Debug.LogWarning("PlayerLife: No GameOverUI assigned.");
        }
    }

    IEnumerator RespawnRoutine()
    {
        respawning = true;
        invulnerable = true;

        if (movement) movement.enabled = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        yield return new WaitForSeconds(respawnDelay);

        if (respawnPoint)
        {
            transform.position = respawnPoint.position;
            transform.rotation = respawnPoint.rotation;
        }
        else
        {
            Debug.LogWarning("PlayerLife: No respawnPoint assigned.");
        }

        CurrentHealth = MaxHealth;
        lastDamageTime = -999f;

        yield return new WaitForFixedUpdate();

        if (movement) movement.enabled = true;

        yield return new WaitForSeconds(RespawnInvulnTime);
        invulnerable = false;
        respawning = false;
    }

    void OnDrawGizmosSelected()
    {
        if (!respawnPoint) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(respawnPoint.position, 0.25f);
        Gizmos.DrawLine(transform.position, respawnPoint.position);
    }
}
