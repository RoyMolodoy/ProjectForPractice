using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Skills (Unlockable)")]
    public bool canDoubleJump = false;
    public bool canDash = false;

    [Header("Controls")]
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode dashKey = KeyCode.LeftShift;

    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    public bool useVelocityJump = true;
    public float jumpForce = 14f;
    public float doubleJumpForce = 12f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.12f;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Dash UI")]
    public GameObject dashIconUI;
    public TextMeshProUGUI dashCooldownText;
    public float dashUIFadeDuration = 0.2f; // НОВЕ: Швидкість плавного з'явлення (в секундах)

    [Header("Debug")]
    public bool debugDraw = true;

    [Header("Animation")]
    [SerializeField] AnimsController animsController;
    [SerializeField] float animHorizontalDeadzone = 0.1f;

    [Header("Flip")]
    [SerializeField] float flipDeadzone = 0.1f;

    [Header("Fall settings")]
    [SerializeField] float fallThreshold = -0.1f;

    // Внутрішні змінні
    Rigidbody2D rb;
    float horizontal;
    bool isGrounded;
    bool facingRight = true;

    // Змінні станів
    bool jumpRequest;
    bool doubleJumpAvailable;
    bool dashRequest;
    bool isDashing;
    float dashCooldownTimer;
    float defaultGravity;

    // НОВЕ: Змінні для керування плавним UI
    private CanvasGroup dashCanvasGroup;
    private Coroutine fadeCoroutine;
    private bool isDashUIVisible = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale;

        if (PlayerPrefs.HasKey("LeftKey"))
        {
            leftKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LeftKey"));
            rightKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("RightKey"));
            jumpKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("JumpKey"));
            dashKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("DashKey"));
        }

        if (rb.bodyType != RigidbodyType2D.Dynamic)
            Debug.LogWarning($"{name}: Rigidbody2D должен быть Dynamic для прыжка.", this);

        if (groundLayer.value == 0)
            Debug.LogWarning($"{name}: LayerMask groundLayer не задан.", this);

        if (animsController == null)
            animsController = GetComponentInChildren<AnimsController>();

        facingRight = Mathf.Approximately(transform.localEulerAngles.y, 0f);
    }

    void Start()
    {
        // НОВЕ: Автоматично налаштовуємо CanvasGroup для плавності
        if (dashIconUI != null)
        {
            dashCanvasGroup = dashIconUI.GetComponent<CanvasGroup>();
            if (dashCanvasGroup == null)
            {
                dashCanvasGroup = dashIconUI.AddComponent<CanvasGroup>();
            }

            // Робимо іконку невидимою при старті
            dashCanvasGroup.alpha = 0f;
            dashIconUI.SetActive(false);
        }
    }

    void Update()
    {
        if (isDashing) return;

        horizontal = 0f;
        if (Input.GetKey(rightKey)) horizontal += 1f;
        if (Input.GetKey(leftKey)) horizontal -= 1f;

        if (Input.GetKeyDown(jumpKey))
            jumpRequest = true;

        if (canDash && Input.GetKeyDown(dashKey) && dashCooldownTimer <= 0f && !isDashing)
        {
            dashRequest = true;
        }

        // --- ЛОГІКА КУЛДАУНУ ТА ПЛАВНОГО UI ---
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;

            if (dashCooldownText != null)
            {
                dashCooldownText.text = $"{dashCooldownTimer:F1}s";
            }
        }
        else
        {
            // Якщо кулдаун закінчився, а іконка ще видима — плавно її ховаємо
            if (isDashUIVisible)
            {
                isDashUIVisible = false;
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeDashUI(0f));
            }
        }

        HandleFlip();
    }

    void FixedUpdate()
    {
        if (dashRequest)
        {
            StartCoroutine(PerformDash());
            dashRequest = false;
            return;
        }

        if (isDashing) return;

        int mask = (groundLayer.value == 0) ? ~0 : groundLayer.value;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, mask);

        if (isGrounded)
        {
            doubleJumpAvailable = true;
        }

        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        if (jumpRequest)
        {
            if (isGrounded)
            {
                ExecuteJump(jumpForce);
            }
            else if (canDoubleJump && doubleJumpAvailable)
            {
                ExecuteJump(doubleJumpForce);
                doubleJumpAvailable = false;
            }
            else
            {
                if (debugDraw) Debug.Log($"{name}: попытка прыжка, но игрок не на земле и нет двойного прыжка.", this);
            }

            jumpRequest = false;
        }

        UpdateAnimations();
    }

    private void ExecuteJump(float force)
    {
        if (useVelocityJump)
        {
            rb.velocity = new Vector2(rb.velocity.x, force);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;

        if (animsController != null) animsController.SetDashing(true);

        rb.gravityScale = 0f;
        float dashDirection = facingRight ? 1f : -1f;
        rb.velocity = new Vector2(dashDirection * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = defaultGravity;
        rb.velocity = new Vector2(0f, rb.velocity.y);

        isDashing = false;
        dashCooldownTimer = dashCooldown;

        // НОВЕ: Запускаємо плавне з'явлення UI іконки
        if (dashIconUI != null && !isDashUIVisible)
        {
            isDashUIVisible = true;
            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeDashUI(1f));
        }

        if (animsController != null) animsController.SetDashing(false);
    }

    // НОВЕ: Корутина для плавного з'явлення та зникнення
    private IEnumerator FadeDashUI(float targetAlpha)
    {
        // Перед початком проявлення переконуємося, що об'єкт увімкнений
        if (dashIconUI != null && !dashIconUI.activeSelf)
        {
            dashIconUI.SetActive(true);
        }

        float startAlpha = dashCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < dashUIFadeDuration)
        {
            elapsed += Time.deltaTime;
            dashCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / dashUIFadeDuration);
            yield return null;
        }

        // Гарантуємо ідеальне кінцеве значення
        dashCanvasGroup.alpha = targetAlpha;

        // Якщо ми повністю сховали іконку, вимикаємо об'єкт для економії ресурсів
        if (targetAlpha == 0f && dashIconUI != null)
        {
            dashIconUI.SetActive(false);
        }
    }

    void HandleFlip()
    {
        if (horizontal < -flipDeadzone && facingRight)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 180f, transform.localEulerAngles.z);
            facingRight = false;
        }
        else if (horizontal > flipDeadzone && !facingRight)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, 0f, transform.localEulerAngles.z);
            facingRight = true;
        }
    }

    void UpdateAnimations()
    {
        if (animsController == null) return;

        bool moving = Mathf.Abs(horizontal) > animHorizontalDeadzone;
        animsController.SetRunning(moving && isGrounded && !isDashing);

        if (isGrounded && !isDashing)
        {
            animsController.SetJumping(false);
            animsController.SetFalling(false);
            return;
        }

        float vy = rb.velocity.y;

        if (vy > 0.05f && !isDashing)
        {
            animsController.SetJumping(true);
            animsController.SetFalling(false);
        }
        else if (vy < fallThreshold && !isDashing)
        {
            animsController.SetJumping(false);
            animsController.SetFalling(true);
        }
        else
        {
            animsController.SetJumping(false);
            animsController.SetFalling(false);
        }
    }
}