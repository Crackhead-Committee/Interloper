using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class EnemySimpleAI : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Ranges")]
    public float detectRange = 12f;
    public float attackRange = 1.8f;

    [Header("Attack")]
    public float attackCooldown = 1.0f;
    public float hitDelay = 0.4f;

    static readonly int SpeedID  = Animator.StringToHash("Speed");
    static readonly int AttackID = Animator.StringToHash("Attack");

    Animator anim;
    NavMeshAgent agent;
    float lastAttackTime = -999f;

    void Awake()
    {
        anim = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.stoppingDistance = Mathf.Max(0.1f, attackRange * 0.95f);

        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }
    }

    void Update()
    {
        if (!target)
        {
            agent.isStopped = true;
            anim.SetFloat(SpeedID, 0f);
            return;
        }

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > detectRange)
        {
            agent.isStopped = true;
            anim.SetFloat(SpeedID, 0f);
            return;
        }

        bool inAttack = dist <= attackRange;
        agent.isStopped = inAttack == true;

        if (!agent.isStopped)
        {
            agent.SetDestination(target.position);
        }

        Vector3 to = target.position - transform.position; to.y = 0f;
        if (to.sqrMagnitude > 0.0001f)
        {
            float turn = agent.angularSpeed * Mathf.Deg2Rad * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(to), turn);
        }

        float move01 = Mathf.InverseLerp(0f, agent.speed, agent.velocity.magnitude);
        anim.SetFloat(SpeedID, move01, 0.1f, Time.deltaTime);

        if (inAttack)
            TryAttack();
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
        Gizmos.color = new Color(0,0,1,0.25f); Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = new Color(1,0,0,0.25f); Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
