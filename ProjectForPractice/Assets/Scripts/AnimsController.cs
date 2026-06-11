using System;
using System.Collections.Generic;
using UnityEngine;

public class AnimsController : MonoBehaviour
{
    [SerializeField] Animator animator;

    [SerializeField] string runningParam = "isRunning";
    [SerializeField] string jumpingParam = "isJumping";
    [SerializeField] string fallingParam = "isFalling";

    [Header("Attack")]
    [SerializeField] string attackingBoolParam = "isAttacking";
    [SerializeField] public string isPlayerAttacking1 = "isPlayerAttacking1";
    [SerializeField] public  string isPlayerAttacking2 = "isPlayerAttacking2";
    [Header("Debug")]
    [SerializeField] bool debugLogs = false;
    private HashSet<string> _animParams;
    public event Action OnAttackHit;
    public event Action OnAttackEnd;

    void Awake()
    {
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
        if (!HasParam(runningParam))
        {
            return;
        }

        animator.SetBool(runningParam, value);
        if (debugLogs) Debug.Log($"{name}: {runningParam} = {value}", this);
    }

    public void SetJumping(bool value)
    {
        if (!HasParam(jumpingParam))
        {
            return;
        }

        animator.SetBool(jumpingParam, value);
        if (debugLogs) Debug.Log($"{name}: {jumpingParam} = {value}", this);
    }

    public void SetFalling(bool value)
    {
        if (!HasParam(fallingParam))
        {
            return;
        }

        animator.SetBool(fallingParam, value);
    }
    public void SetAttacking(bool value)
    {
        if (!HasParam(attackingBoolParam))
        {
            return;
        }

        animator.SetBool(attackingBoolParam, value);
    }

    public void ResetAttackBool()
    {
        if (!HasParam(attackingBoolParam))
        {
            return;
        }

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
        if (!HasParam(isPlayerAttacking1))
        {
            return;
        }
        animator.SetTrigger(isPlayerAttacking1);
    }
    public void isAttacking2()
    {
        if (!HasParam(isPlayerAttacking1))
        {
            return;
        }
        animator.SetTrigger(isPlayerAttacking2);
    }
}