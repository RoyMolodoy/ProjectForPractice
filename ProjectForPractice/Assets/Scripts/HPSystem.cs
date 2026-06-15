using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HPSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float HP = 3;
    public float MaxHP = 3;
    public float defense = 1;
    public bool isBoss = false;
    public Image HPBar;
    public GameObject BossHPBar;

    [Header("Invulnerability (Тільки для Player)")]
    [SerializeField] private float invulnerabilityDuration = 1.5f;
    [SerializeField] private int numberOfFlashes = 6;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Death Screen (Тільки для Player)")]
    public GameObject deathScreenPanel;
    public float deathFadeDuration = 1.5f; // За скільки секунд екран плавно з'явиться
    [Range(0.1f, 1f)]
    public float maxDeathAlpha = 0.8f; // Максимальна непрозорість (не на 100%)

    private AnimsController animsController;
    private bool _isInvulnerable = false;
    public AudioSource aSourse;
    public AudioClip damageSound;
    public AudioClip deathSound;
    [Range(0, 1)]
    public float damageVolume = 0.1f;

    private void Start()
    {
        if (isBoss)
        {
            GameObject barObj = GameObject.FindGameObjectWithTag("BossHPBar");
            if (barObj != null)
                HPBar = barObj.GetComponent<Image>();

            BossHPBar = GameObject.FindGameObjectWithTag("BossHPObject");
            if (BossHPBar != null)
                BossHPBar.SetActive(false);
        }

        animsController = GetComponent<AnimsController>();

        if (gameObject.CompareTag("Player") && spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        // Ховаємо екран смерті на старті і налаштовуємо його прозорість на 0
        if (deathScreenPanel != null)
        {
            CanvasGroup cg = deathScreenPanel.GetComponent<CanvasGroup>();
            if (cg == null) cg = deathScreenPanel.AddComponent<CanvasGroup>();

            cg.alpha = 0f;
            deathScreenPanel.SetActive(false);
        }
    }

    public void MinusHP(float minusHP)
    {
        if (_isInvulnerable) return;
        if (defense > 0)
            minusHP = minusHP / defense;
        HP -= minusHP;

        // Оновлюємо UI
        if (HPBar != null)
            HPBar.fillAmount = (float)HP / MaxHP;

        if (animsController != null)
        {
            if (HP <= 0)
            {
                if (HP + minusHP > 0)
                {
                    gameObject.GetComponent<Rigidbody2D>().isKinematic = true;
                    gameObject.GetComponent<Collider2D>().enabled = false;
                    PlayerMovement playerMovement = GetComponent<PlayerMovement>();
                    PlayerAttack playerAttack = GetComponent<PlayerAttack>();

                    if (playerMovement != null)
                        playerMovement.enabled = false;
                    if (playerAttack != null)
                        playerAttack.enabled = false;

                    animsController.DeathAnim();

                    if (aSourse != null && deathSound != null)
                        aSourse.PlayOneShot(deathSound);

                    // ВИКЛИКАЄМО ПЛАВНИЙ ЕКРАН СМЕРТІ ОДРАЗУ
                    if (gameObject.CompareTag("Player"))
                    {
                        StartCoroutine(ShowDeathScreenRoutine());
                    }
                }
            }
            else
            {
                if (gameObject.CompareTag("Player"))
                {
                    if (aSourse != null && damageSound != null)
                    {
                        aSourse.volume = damageVolume;
                        aSourse.PlayOneShot(damageSound);
                    }
                    StartCoroutine(InvulnerabilityRoutine());
                }
            }
        }
        else
        {
            if (HP <= 0)
            {
                if (gameObject.CompareTag("Player"))
                {
                    StartCoroutine(ShowDeathScreenRoutine());
                }
                else
                {
                    Destroy(gameObject);
                }
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

    // 🔥 ОНОВЛЕНА КОРУТИНА (Плавна поява)
    private IEnumerator ShowDeathScreenRoutine()
    {
        if (deathScreenPanel != null)
        {
            deathScreenPanel.SetActive(true);

            CanvasGroup canvasGroup = deathScreenPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = deathScreenPanel.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f; // Починаємо з абсолютної прозорості
            float elapsed = 0f;

            // Використовуємо unscaledDeltaTime на випадок, якщо ти потім додаш паузу гри (Time.timeScale = 0)
            while (elapsed < deathFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(0f, maxDeathAlpha, elapsed / deathFadeDuration);
                yield return null;
            }

            // Гарантуємо, що фінальна прозорість точно відповідає налаштуванню
            canvasGroup.alpha = maxDeathAlpha;
            Time.timeScale = 0f; // Зупиняємо гру після появи екрану смерті
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