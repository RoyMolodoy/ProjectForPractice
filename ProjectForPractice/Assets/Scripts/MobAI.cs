using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MobAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stopDistance = 0.8f;

    [Header("Attack")]
    public float attackRange = 1f;
    public float attackCooldown = 1.2f;
    public int damage = 10;

    Transform player;
    Rigidbody2D rb;
    float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null)
            player = go.transform;
        else
        {
            var pm = FindObjectOfType<PlayerMovement>();
            if (pm != null)
                player = pm.transform;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector2 pos = rb.position;
        Vector2 target = player.position;
        Vector2 dir = target - pos;
        float dist = dir.magnitude;

        if (dist > stopDistance)
        {
            Vector2 move = dir.normalized * moveSpeed * Time.fixedDeltaTime;
            rb.MovePosition(pos + move);
        }

        // ѕоворот спрайта вправо/вл≥во по позиц≥њ гравц€
        if (player.position.x - transform.position.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void Update()
    {
        if (player == null) return;

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            float dist = Vector2.Distance(transform.position, player.position);
            /*if (dist <= attackRange)
                Attack();*/
        }
    }

    /*void Attack()
    {
        lastAttackTime = Time.time;

        // якщо у гравц€ Ї PlayerHealth Ч викликати TakeDamage, ≥накше використовуЇмо SendMessage (необов'€зково)
        var ph = player.GetComponent<PlayerHealth>();
        if (ph != null)
            ph.TakeDamage(damage);
        else
            player.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        // “ут можна додати ан≥мац≥ю атаки, звук або в≥дштовхуванн€
    }*/

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);
    }
}
