using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerShooter : MonoBehaviour
{
    public Camera playerCamera;
    public float range = 25f;
    public float shotsPerSecond = 5f;
    public int damagePerShot = 1;
    public LayerMask shootMask = ~0;
    public float hitScanRadius = 0f;

    InputAction fireAction;
    float nextFireTime;

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
        if (DialogueController.Instance && DialogueController.Instance.IsActive)
            return;

        if (!fireAction.IsPressed()) return;
        if (Time.time < nextFireTime) return;

        nextFireTime = Time.time + 1f / Mathf.Max(0.01f, shotsPerSecond);
        FireOnce();
    }

void FireOnce()
{
    Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
    bool hitSomething;

    RaycastHit hit;

    if (hitScanRadius > 0f)
        hitSomething = Physics.SphereCast(ray, hitScanRadius, out hit, range, shootMask, QueryTriggerInteraction.Ignore);
    else
        hitSomething = Physics.Raycast(ray, out hit, range, shootMask, QueryTriggerInteraction.Ignore);

    if (!hitSomething) return;

    var dmg = hit.collider.GetComponentInParent<Damageable>();
    if (dmg != null)
    {
        dmg.TakeHit(damagePerShot);
    }
}
}
