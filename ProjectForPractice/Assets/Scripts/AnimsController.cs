using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

public class AnimsController : MonoBehaviour
{
    [SerializeField] bool isPlayer = false;
    [SerializeField] private float timeToDestroy = 5.0f;
    [SerializeField] Animator animator;
    private Collider2D col;
    private Rigidbody2D rb;

    [Header("Movement")]
    [SerializeField] string runningParam = "isRunning";
    [SerializeField] string jumpingParam = "isJumping";
    [SerializeField] string fallingParam = "isFalling";
    [SerializeField] string dashingParam = "isDashing"; // <--- ДОДАНО ПАРАМЕТР ДЕШУ

    [Header("Attack")]
    [SerializeField] string attackingBoolParam = "isAttacking";
    [SerializeField] public string isPlayerAttacking1 = "isPlayerAttacking1";
    [SerializeField] public string isPlayerAttacking2 = "isPlayerAttacking2";

    [Header("Debug")]
    [SerializeField] bool debugLogs = false;

    private MobAI mob;
    private HashSet<string> _animParams;

    public event Action OnAttackHit;
    public event Action OnAttackEnd;

    void Awake()
    {
        if (isPlayer == false)
        {
            mob = GetComponent<MobAI>();
            col = GetComponent<Collider2D>();
            rb = GetComponent<Rigidbody2D>();
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
        {
            return;
        }

        _animParams = new HashSet<string>();
        foreach (var p in animator.parameters)
            _animParams.Add(p.name);
    }

    bool HasParam(string paramName)
    {
        return animator != null && _animParams != null && _animParams.Contains(paramName);
    }

    public void SetRunning(bool value)
    {
        if (!HasParam(runningParam)) return;
        animator.SetBool(runningParam, value);
        if (debugLogs) Debug.Log($"{name}: {runningParam} = {value}", this);
    }

    public void SetJumping(bool value)
    {
        if (!HasParam(jumpingParam)) return;
        animator.SetBool(jumpingParam, value);
        if (debugLogs) Debug.Log($"{name}: {jumpingParam} = {value}", this);
    }

    public void SetFalling(bool value)
    {
        if (!HasParam(fallingParam)) return;
        animator.SetBool(fallingParam, value);
    }

    // --- ДОДАНИЙ МЕТОД ДЛЯ ДЕШУ ---
    public void SetDashing(bool value)
    {
        if (!HasParam(dashingParam)) return;
        animator.SetBool(dashingParam, value);
        if (debugLogs) Debug.Log($"{name}: {dashingParam} = {value}", this);
    }

    public void SetAttacking(bool value)
    {
        if (!HasParam(attackingBoolParam)) return;
        animator.SetBool(attackingBoolParam, value);
    }

    public void ResetAttackBool()
    {
        if (!HasParam(attackingBoolParam)) return;
        animator.SetBool(attackingBoolParam, false);
    }

    public void AttackHitEvent()
    {
        OnAttackHit?.Invoke();
    }

    public void AttackEndEvent()
    {
        ResetAttackBool();
        OnAttackEnd?.Invoke();
    }

    public void isAttacking1()
    {
        if (!HasParam(isPlayerAttacking1)) return;
        animator.SetTrigger(isPlayerAttacking1);
    }

    public void isAttacking2()
    {
        if (!HasParam(isPlayerAttacking2)) return; // Виправлено баг з isPlayerAttacking1
        animator.SetTrigger(isPlayerAttacking2);
    }

    public void DeathAnim()
    {
        if (animator == null) return;

        if (isPlayer == false && mob != null)
        {
            mob.enabled = false;
            rb.isKinematic = true;
            Destroy(col);
            Destroy(gameObject, timeToDestroy);
        }

        animator.SetTrigger("Death");
        StartCoroutine(DisableAnimatorAfterDeath("Death", 0));
    }

    private IEnumerator DisableAnimatorAfterDeath(string stateName, int layerIndex)
    {
        if (animator == null)
            yield break;

        int stateHash = Animator.StringToHash(stateName);

        var safetyTimer = 0f;
        const float safetyTimeout = 5f;

        while (animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash != stateHash)
        {
            yield return null;
            safetyTimer += Time.deltaTime;
            if (safetyTimer > safetyTimeout)
            {
                if (debugLogs) Debug.LogWarning($"{name}: ожидание состояния '{stateName}' тайм-аут.", this);
                yield break;
            }
        }

        while (animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime < 1f)
        {
            yield return null;
        }

        animator.enabled = false;
        if (debugLogs) Debug.Log($"{name}: animator отключен после завершения '{stateName}'", this);
    }
}