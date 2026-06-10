using System.Collections.Generic;
using UnityEngine;

public class AnimsController : MonoBehaviour
{
    [SerializeField] Animator animator;

    [SerializeField] string runningParam = "isRunning";
    [SerializeField] string jumpingParam = "isJumping";
    [SerializeField] string fallingParam = "isFalling";

    [Header("Debug")]
    [SerializeField] bool debugLogs = false;

    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            Debug.LogWarning($"{name}: Animator не найден в {nameof(AnimsController)}. Привяжите Animator в инспекторе.", this);
    }

    public void SetRunning(bool value)
    {
        if (animator == null) return;
        animator.SetBool(runningParam, value);
        if (debugLogs) Debug.Log($"{name}: {runningParam} = {value}", this);
    }

    public void SetJumping(bool value)
    {
        if (animator == null) return;
        animator.SetBool(jumpingParam, value);
        if (debugLogs) Debug.Log($"{name}: {jumpingParam} = {value}", this);
    }

    public void SetFalling(bool value)
    {
        if (animator == null) return;
        animator.SetBool(fallingParam, value);
        if (debugLogs) Debug.Log($"{name}: {fallingParam} = {value}", this);
    }
}