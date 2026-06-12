using System.Collections.Generic;
using UnityEngine;

using System;
using System.Collections;
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
    [SerializeField] public string isPlayerAttacking2 = "isPlayerAttacking2";
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

    public void DeathAnim()
    {
        if (animator == null) return;

        animator.SetTrigger("Death");
        // запускаем корутину, которая дождётся окончания состояния смерти и отключит Animator
        StartCoroutine(DisableAnimatorAfterDeath("Death", 0));
    }

    // Ожидает перехода в указанный стейт на слое layerIndex и затем полного окончания анимации (normalizedTime >= 1).
    // После этого выключает animator.enabled = false.
    private IEnumerator DisableAnimatorAfterDeath(string stateName, int layerIndex)
    {
        if (animator == null)
            yield break;

        int stateHash = Animator.StringToHash(stateName);

        // дождаться, пока Animator перейдёт в состояние "Death"
        var safetyTimer = 0f;
        const float safetyTimeout = 5f; // на случай, если переход не произойдёт
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

        // дождаться завершения проигрывания (normalizedTime >= 1)
        while (animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime < 1f)
        {
            yield return null;
        }

        // окончательно отключаем аниматор
        animator.enabled = false;
        if (debugLogs) Debug.Log($"{name}: animator отключен после завершения '{stateName}'", this);
    }
}