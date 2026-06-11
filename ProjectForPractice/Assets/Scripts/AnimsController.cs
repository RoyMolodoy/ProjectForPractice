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

    [Header("Debug")]
    [SerializeField] bool debugLogs = false;

    // Кеш имен параметров Animator для быстрой проверки
    private HashSet<string> _animParams;

    // События для внешних подписчиков (например MobAI)
    public event Action OnAttackHit;
    public event Action OnAttackEnd;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning($"{name}: Animator не найден в {nameof(AnimsController)}. Привяжите Animator в инспекторе.", this);
            return;
        }

        // Собираем параметры в HashSet для быстрых проверок
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
            if (debugLogs) Debug.LogWarning($"{name}: Animator не содержит параметр '{runningParam}'", this);
            return;
        }

        animator.SetBool(runningParam, value);
        if (debugLogs) Debug.Log($"{name}: {runningParam} = {value}", this);
    }

    public void SetJumping(bool value)
    {
        if (!HasParam(jumpingParam))
        {
            if (debugLogs) Debug.LogWarning($"{name}: Animator не содержит параметр '{jumpingParam}'", this);
            return;
        }

        animator.SetBool(jumpingParam, value);
        if (debugLogs) Debug.Log($"{name}: {jumpingParam} = {value}", this);
    }

    public void SetFalling(bool value)
    {
        if (!HasParam(fallingParam))
        {
            if (debugLogs) Debug.LogWarning($"{name}: Animator не содержит параметр '{fallingParam}'", this);
            return;
        }

        animator.SetBool(fallingParam, value);
        if (debugLogs) Debug.Log($"{name}: {fallingParam} = {value}", this);
    }

    // Булевий параметр атаки (вмикається/вимикається)
    public void SetAttacking(bool value)
    {
        if (!HasParam(attackingBoolParam))
        {
            if (debugLogs) Debug.LogWarning($"{name}: Animator не содержит параметр '{attackingBoolParam}'", this);
            return;
        }

        animator.SetBool(attackingBoolParam, value);
        if (debugLogs) Debug.Log($"{name}: {attackingBoolParam} = {value}", this);
    }

    public void ResetAttackBool()
    {
        if (!HasParam(attackingBoolParam))
        {
            if (debugLogs) Debug.LogWarning($"{name}: Animator не содержит параметр '{attackingBoolParam}'", this);
            return;
        }

        animator.SetBool(attackingBoolParam, false);
        if (debugLogs) Debug.Log($"{name}: {attackingBoolParam} = false", this);
    }

    // Вызвать из Animation Event в момент попадания
    public void AttackHitEvent()
    {
        if (debugLogs) Debug.Log($"{name}: AttackHitEvent invoked", this);
        OnAttackHit?.Invoke();
    }

    // Вызвать из Animation Event в конце анимации атаки
    public void AttackEndEvent()
    {
        if (debugLogs) Debug.Log($"{name}: AttackEndEvent invoked", this);
        // Сбрасываем bool — безопасно
        ResetAttackBool();
        OnAttackEnd?.Invoke();
    }
}