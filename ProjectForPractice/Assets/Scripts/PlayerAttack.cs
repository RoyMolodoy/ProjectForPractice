using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnimsController))]
public class PlayerAttack : MonoBehaviour
{
    [Header("Attack settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 10;
    public float attackCooldown = 0.5f;
    public string enemyTag = "Enemy"; // Тег врагов
    public bool AlternativeAtack = true; // Включить альтернативную атаку
    [SerializeField] int AttackAnimationChanse = 40;

    [Header("Delay")]
    public float damageDelay1 = 0.7f;
    public float damageDelay2 = 0.4f;// Задержка перед нанесением урона

    private float lastAttackTime = -999f;
    private AnimsController animsController;

    void Awake()
    {
        animsController = GetComponent<AnimsController>();
        if (animsController == null)
            Debug.LogWarning("AnimsController not found on the GameObject.");
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= lastAttackTime + attackCooldown)
        {
            Attack();
        }
    }

    void Attack()
    {
        lastAttackTime = Time.time;
        if (attackPoint == null)
        {
            Debug.LogWarning("attackPoint не задан в инспекторе.");
            return;
        }
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        List<Collider2D> targets = new List<Collider2D>();
        foreach (Collider2D col in hitColliders)
        {
            if (!col.CompareTag(enemyTag)) continue;
            targets.Add(col);
        }
        if (animsController != null)
        {
            int rand = UnityEngine.Random.Range(0, 101);
            if (AlternativeAtack)
            {
                if (rand < AttackAnimationChanse)
                {
                    animsController.isAttacking1();
                    if (targets.Count > 0)
                    {
                        StartCoroutine(ApplyDamageAfterDelay(targets.ToArray(), attackDamage, damageDelay1));
                    }
                }
                else
                {
                    animsController.isAttacking2();
                    if (targets.Count > 0)
                    {
                        StartCoroutine(ApplyDamageAfterDelay(targets.ToArray(), attackDamage, damageDelay2));
                    }
                }
            }
            else
            {
                animsController.isAttacking1();
            }
        }

    }

    private IEnumerator ApplyDamageAfterDelay(Collider2D[] targets, int damage, float delay)
    {
        yield return new WaitForSeconds(delay);

        foreach (Collider2D col in targets)
        {
            if (col == null) continue; // Объект мог быть уничтожен
            if (!col.CompareTag(enemyTag)) continue;

            // Попытка вызвать MinusHP на компоненте PlayerHP (без явной зависимости)
            Component hpComp = col.GetComponent("PlayerHP");
            if (hpComp != null)
            {
                hpComp.SendMessage("MinusHP", damage, SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                col.gameObject.SendMessage("MinusHP", damage, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}