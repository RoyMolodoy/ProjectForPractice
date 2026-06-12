using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Skills (Unlockable)")]
    public bool canDoubleJump = false; // Вмикається через Skill System
    public bool canDash = false;       // Вмикається через Skill System

    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jump")]
    public bool useVelocityJump = true;
    public float jumpForce = 14f;
    public float doubleJumpForce = 12f; // Сила второго стрибка
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.12f;

    [Header("Dash")]
    public float dashSpeed = 20f;       // Швидкість ривка
    public float dashDuration = 0.2f;   // Скільки часу триває ривок
    public float dashCooldown = 1f;     // Перезарядка ривка
    public KeyCode dashKey = KeyCode.F; // Кнопка для дешу

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

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravity = rb.gravityScale; // Зберігаємо стандартну гравітацію

        if (rb.bodyType != RigidbodyType2D.Dynamic)
            Debug.LogWarning($"{name}: Rigidbody2D должен быть Dynamic для прыжка (текущий тип: {rb.bodyType}).", this);

        if (groundLayer.value == 0)
            Debug.LogWarning($"{name}: LayerMask groundLayer не задан. Установите слой(и) земли или задайте маску.", this);

        if (animsController == null)
            animsController = GetComponentInChildren<AnimsController>();

        if (animsController == null && debugDraw)
            Debug.LogWarning($"{name}: AnimsController не найден. Привяжите в инспекторе.", this);

        facingRight = Mathf.Approximately(transform.localEulerAngles.y, 0f);
    }

    void Update()
    {
        // Якщо персонаж робить деш, блокуємо інше управління
        if (isDashing) return;

        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
            jumpRequest = true;

        // Логіка запиту на деш
        if (canDash && Input.GetKeyDown(dashKey) && dashCooldownTimer <= 0f && !isDashing)
        {
            dashRequest = true;
        }

        // Таймер перезарядки дешу
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        HandleFlip();
    }

    void FixedUpdate()
    {
        // --- ЛОГІКА ДЕШУ ---
        if (dashRequest)
        {
            StartCoroutine(PerformDash());
            dashRequest = false;
            return; // Пропускаємо звичайний рух у цьому кадрі
        }

        if (isDashing)
        {
            return; // Якщо ми в процесі дешу, фізика керується корутиною
        }

        // --- ЗВИЧАЙНИЙ РУХ ---
        int mask = (groundLayer.value == 0) ? ~0 : groundLayer.value;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, mask);

        // Відновлюємо можливість подвійного стрибка, коли торкаємося землі
        if (isGrounded)
        {
            doubleJumpAvailable = true;
        }

        rb.velocity = new Vector2(horizontal * moveSpeed, rb.velocity.y);

        // --- ЛОГІКА СТРИБКА ---
        if (jumpRequest)
        {
            if (isGrounded)
            {
                ExecuteJump(jumpForce);
            }
            else if (canDoubleJump && doubleJumpAvailable)
            {
                ExecuteJump(doubleJumpForce);
                doubleJumpAvailable = false; // Використали другий стрибок
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
            rb.velocity = new Vector2(rb.velocity.x, 0f); // Скидаємо швидкість падіння перед стрибком
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

        if (animsController != null) animsController.SetDashing(false);
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
        if (animsController == null)
            return;

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