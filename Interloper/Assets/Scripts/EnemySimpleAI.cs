using UnityEngine;

[RequireComponent(typeof(Animator))]
public class EnemySimpleAI : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Ranges")]
    public float detectRange = 12f;
    public float attackRange = 1.8f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float rotationSpeed = 8f;

    [Header("Attack")]
    public float attackCooldown = 1.0f;
    public float hitDelay = 0.4f;

    static readonly int SpeedID  = Animator.StringToHash("Speed");
    static readonly int AttackID = Animator.StringToHash("Attack");

    Animator anim;
    float lastAttackTime = -999f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }
        if (anim) anim.applyRootMotion = false;
    }

    void Update()
    {
        if (!target) { anim.SetFloat(SpeedID, 0f); return; }

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > detectRange)
        {
            anim.SetFloat(SpeedID, 0f);
            return;
        }

        Vector3 to = target.position - transform.position;
        to.y = 0f;
        if (to.sqrMagnitude > 0.0001f)
        {
            Quaternion look = Quaternion.LookRotation(to);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
        }

        if (dist > attackRange)
        {
            anim.SetFloat(SpeedID, 1f);
            transform.position += transform.forward * (moveSpeed * Time.deltaTime);
        }
        else
        {
            anim.SetFloat(SpeedID, 0f);
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;
        lastAttackTime = Time.time;

        anim.ResetTrigger(AttackID);
        anim.SetTrigger(AttackID);

        Invoke(nameof(DealDamageIfStillInRange), hitDelay);
    }

    void DealDamageIfStillInRange()
    {
        if (!target) return;
        if (Vector3.Distance(transform.position, target.position) <= attackRange + 0.3f)
        {
            var life = target.GetComponentInParent<PlayerLife>();
            if (life) life.Die();
        }
    }

    public void AnimEvent_DealDamage() => DealDamageIfStillInRange();

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0,0,1,0.25f);
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = new Color(1,0,0,0.25f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
