using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HPSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float HP = 3;
    public float MaxHP = 3;
    public float defense = 1;
    public Image HPBar;

    [Header("Invulnerability (Тільки для Player)")]
    [SerializeField] private float invulnerabilityDuration = 1.5f;
    [SerializeField] private int numberOfFlashes = 6;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private AnimsController animsController;
    private bool _isInvulnerable = false;

    private void Start()
    {
        animsController = GetComponent<AnimsController>();

        if (gameObject.CompareTag("Player") && spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    void MinusHP(float minusHP)
    {
        if (_isInvulnerable) return;
        if (defense > 0)
            minusHP = minusHP/defense;
        HP -= minusHP;

        // Оновлюємо UI
        if (HPBar != null)
            HPBar.fillAmount = (float)HP / MaxHP;

        if (animsController != null)
        {
            if (HP <= 0)
            {
                if (HP + minusHP > 0)
                    animsController.DeathAnim();
            }
            else
            {
                // animsController.HurtAnim();

                if (gameObject.CompareTag("Player"))
                {
                    StartCoroutine(InvulnerabilityRoutine());
                }
            }
        }
        else
        {
            if (HP <= 0)
            {
                Destroy(gameObject);
            }
            else if (gameObject.CompareTag("Player"))
            {
                StartCoroutine(InvulnerabilityRoutine());
            }
        }
    }

    public void PlusHP(int plusHP)
    {
        if (HP < MaxHP)
        {
            HP += plusHP;

            if (HP > MaxHP) HP = MaxHP;

            if (HPBar != null)
                HPBar.fillAmount = (float)HP / MaxHP;
        }
    }

    private IEnumerator InvulnerabilityRoutine()
    {
        _isInvulnerable = true;

        if (spriteRenderer != null)
        {
            float flashDuration = invulnerabilityDuration / (numberOfFlashes * 2);

            for (int i = 0; i < numberOfFlashes; i++)
            {
                SetSpriteAlpha(0.5f);
                yield return new WaitForSeconds(flashDuration);

                SetSpriteAlpha(1f);
                yield return new WaitForSeconds(flashDuration);
            }

            SetSpriteAlpha(1f);
        }
        else
        {
            yield return new WaitForSeconds(invulnerabilityDuration);
        }

        _isInvulnerable = false;
    }

    private void SetSpriteAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }
    }
}