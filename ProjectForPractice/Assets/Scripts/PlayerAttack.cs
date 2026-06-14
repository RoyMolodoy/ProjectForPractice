using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(AnimsController))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public float attackRadius = 1.2f;
    public float attackAngle = 90f;

    public int attackDamage = 1;
    public float attackCooldown = 0.5f;
    public string enemyTag = "Enemy";

    public bool AlternativeAtack = true;
    [SerializeField] int AttackAnimationChanse = 40;

    [Header("Delay")]
    public float damageDelay1 = 0.7f;
    public float damageDelay2 = 0.4f;

    [Header("Attack Lock")]
    public float attackLockDelay1 = 0.7f;
    public float attackLockDelay2 = 0.4f;

    private float lastAttackTime = -999f;

    private AnimsController animsController;
    private AudioManager audioManager;

    public static bool IsAttacking;

    void Awake()
    {
        animsController = GetComponent<AnimsController>();
        audioManager = GetComponent<AudioManager>();
    }

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetButtonDown("Fire1") &&
            Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;

        if (attackPoint == null) return;

        // get all nearby objects
        Collider2D[] allHits =
            Physics2D.OverlapCircleAll(attackPoint.position, attackRadius);

        List<Collider2D> targets = new List<Collider2D>();

        Vector2 forward = transform.localScale.x >= 0 ? Vector2.right : Vector2.left;

        foreach (Collider2D col in allHits)
        {
            if (!col.CompareTag(enemyTag)) continue;

            Vector2 dirToEnemy =
                ((Vector2)col.transform.position - (Vector2)transform.position).normalized;

            float angle = Vector2.Angle(forward, dirToEnemy);

            if (angle <= attackAngle * 0.5f)
            {
                targets.Add(col);
            }
        }

        if (animsController == null) return;

        int rand = Random.Range(0, 101);

        if (AlternativeAtack)
        {
            if (rand < AttackAnimationChanse)
            {
                StartAttack(true, damageDelay1, attackLockDelay1, targets);
            }
            else
            {
                StartAttack(false, damageDelay2, attackLockDelay2, targets);
            }
        }
        else
        {
            StartAttack(true, damageDelay1, attackLockDelay1, targets);
        }
    }

    void StartAttack(bool type1, float damageDelay, float lockDelay, List<Collider2D> targets)
    {
        IsAttacking = true;

        if (type1)
        {
            animsController.isAttacking1();
            StartCoroutine(PlayAttackSound(1, damageDelay * 0.5f));
        }
        else
        {
            animsController.isAttacking2();
            StartCoroutine(PlayAttackSound(2, damageDelay * 0.5f));
        }

        StartCoroutine(ApplyDamageAfterDelay(targets.ToArray(), attackDamage, damageDelay));
        StartCoroutine(ResetAttackState(lockDelay));
    }

    private IEnumerator PlayAttackSound(int type, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (audioManager == null) yield break;

        if (type == 1)
            audioManager.Atack1Sound();
        else
            audioManager.Atack2Sound();
    }

    private IEnumerator ApplyDamageAfterDelay(Collider2D[] targets, int damage, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (Collider2D col in targets)
        {
            if (col == null) continue;

            Component hpComp = col.GetComponent("HPSystem");

            if (hpComp != null)
                hpComp.SendMessage("MinusHP", damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    private IEnumerator ResetAttackState(float delay)
    {
        yield return new WaitForSeconds(delay);
        IsAttacking = false;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;

        // draw radius
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);

        // draw forward line (debug direction)
        Vector3 forward = transform.localScale.x >= 0 ? Vector3.right : Vector3.left;
        Gizmos.DrawLine(attackPoint.position, attackPoint.position + forward * attackRadius);
    }
}