using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerShooter : MonoBehaviour
{
    [Header("Core")]
    public Camera playerCamera;
    public float range = 25f;
    public float shotsPerSecond = 5f;
    public int damagePerShot = 1;
    public LayerMask shootMask = ~0;
    public float hitScanRadius = 0f;

    [Header("Recoil")]
    public CameraRecoil recoil;           // assign CameraRecoil on your camera
    public float recoilUp = 1.2f;         // degrees up per shot
    public float recoilSide = 0.35f;      // max sideways (±) per shot

    [Header("Muzzle Flash")]
    public Light muzzleLight;             // child light on camera
    public float flashIntensity = 4f;     // Built-in RP: 2–6 ; URP/HDRP HDR may need 2000–6000
    public float flashTime = 0.06f;

    [Header("Audio")]
    public AudioSource shotAudio;         // on camera; Spatial Blend = 0
    public AudioClip shotClip;
    [Range(0f, 0.1f)] public float pitchJitter = 0.03f;

    [Header("Impact FX")]
    public GameObject impactFXPrefab;     // short particle puff

    private InputAction fireAction;
    private float nextFireTime;

    void Awake()
    {
        if (!playerCamera) playerCamera = Camera.main;

        fireAction = new InputAction("Fire");
        fireAction.AddBinding("<Mouse>/leftButton");
        fireAction.AddBinding("<Gamepad>/rightTrigger");
    }

    void OnEnable()  => fireAction.Enable();
    void OnDisable() => fireAction.Disable();

    void Update()
    {
        // Don't shoot during dialogue (if you use it)
        if (DialogueController.Instance && DialogueController.Instance.IsActive)
            return;

        if (!fireAction.IsPressed()) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + 1f / Mathf.Max(0.01f, shotsPerSecond);
        FireOnce();
    }

    void FireOnce()
    {
        // --- Always-do effects (even if we miss) ---
        DoRecoil();
        DoMuzzleFlash();
        PlayShotAudio();

        // --- Hitscan ---
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        bool hitSomething;
        RaycastHit hit;

        if (hitScanRadius > 0f)
            hitSomething = Physics.SphereCast(ray, hitScanRadius, out hit, range, shootMask, QueryTriggerInteraction.Ignore);
        else
            hitSomething = Physics.Raycast(ray, out hit, range, shootMask, QueryTriggerInteraction.Ignore);

        if (!hitSomething)
        {
            Debug.Log("Missed — no shootable object hit.");
            return;
        }

        // Impact FX
        if (impactFXPrefab)
        {
            Quaternion look = Quaternion.LookRotation(hit.normal);
            Instantiate(impactFXPrefab, hit.point, look);
        }

        // Damageable
        var dmg = hit.collider.GetComponentInParent<Damageable>();
        if (dmg != null)
        {
            dmg.TakeHit(damagePerShot);
            Debug.Log($"Hit {hit.collider.name} at distance {hit.distance:F2}m.");
        }
        else
        {
            Debug.Log($"Hit {hit.collider.name} but it is not shootable.");
        }
    }

    void DoRecoil()
    {
        if (!recoil) return;
        float side = Random.Range(-recoilSide, recoilSide);
        recoil.AddRecoil(recoilUp, side);
    }

    void DoMuzzleFlash()
    {
        if (!muzzleLight) return;
        StopCoroutine(nameof(MuzzleFlashRoutine)); // in case of spam
        StartCoroutine(MuzzleFlashRoutine());
    }

    IEnumerator MuzzleFlashRoutine()
    {
        // pop to full then fade out quickly
        float t = 0f;
        muzzleLight.intensity = flashIntensity;
        while (t < flashTime)
        {
            t += Time.deltaTime;
            // linear fade; swap to SmoothStep if you want a curve
            float k = 1f - Mathf.Clamp01(t / flashTime);
            muzzleLight.intensity = flashIntensity * k;
            yield return null;
        }
        muzzleLight.intensity = 0f;
    }

    void PlayShotAudio()
    {
        if (!shotAudio || !shotClip) return;
        float basePitch = 1f + Random.Range(-pitchJitter, pitchJitter);
        shotAudio.pitch = basePitch;
        shotAudio.PlayOneShot(shotClip, 1f);
    }
}
