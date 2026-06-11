using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

[RequireComponent(typeof(Rigidbody2D))]
public class MobAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.8f;

    [Header("Vision (Line of Sight)")]
    [SerializeField] private float visionRange = 10f; // Максимальна дальність зору
    [SerializeField] private LayerMask obstacleLayer; // Шар, який блокує зір (стіни, земля)
    [SerializeField] private Vector2 eyeOffset = new Vector2(0f, 0.5f); // Зміщення "очей", щоб промінь не чіпляв підлогу

    [Header("Ground & Animations")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private AnimsController animsController;
    [SerializeField] private float runThreshold = 0.1f; // мінімальна швидкість для режиму "біг"
    [SerializeField] private float verticalThreshold = 0.1f; // поріг по Y для стрибка/падіння анімацій
    [SerializeField] private float flipDeadzone = 0.05f;

    private Rigidbody2D _rb;
    private Transform _player;
    private bool facingRight = true;
    private bool isGrounded;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (groundCheck == null)
        {
            var go = new GameObject("GroundCheck");
            go.transform.parent = transform;
            go.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            groundCheck = go.transform;
        }

        if (animsController == null)
            animsController = GetComponent<AnimsController>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }
    }

    private void FixedUpdate()
    {
        if (_player == null) return;

        // Ground check
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Перевіряємо, чи бачимо гравця
        if (CanSeePlayer())
        {
            ChasePlayer();
        }
        else
        {
            // Якщо не бачимо - просто стоїмо (або тут можна додати логіку патрулювання)
            StopMoving();
        }

        HandleFlip();
        UpdateAnimations();
    }

    private bool CanSeePlayer()
    {
        // 1. Перевірка дистанції: якщо гравець занадто далеко, ми його не бачимо
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        if (distanceToPlayer > visionRange)
        {
            return false;
        }

        // 2. Визначаємо точки початку (очі моба) і кінця (центр гравця) променя зору
        Vector2 startPos = (Vector2)transform.position + eyeOffset;
        Vector2 targetPos = (Vector2)_player.position + eyeOffset; // Припускаємо, що у гравця центр теж трохи вище ніг

        // 3. Кидаємо лінію (Linecast) між мобом і гравцем
        RaycastHit2D hit = Physics2D.Linecast(startPos, targetPos, obstacleLayer);

        // Малюємо промінь для дебагу: Червоний - не бачить, Зелений - бачить
        if (hit.collider != null)
        {
            Debug.DrawLine(startPos, hit.point, Color.red);
            return false; // Зір заблоковано стіною
        }
        else
        {
            Debug.DrawLine(startPos, targetPos, Color.green);
            return true; // Шлях вільний, бачимо гравця
        }
    }

    private void ChasePlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        // Зупиняємось, якщо підійшли впритул
        if (distanceToPlayer <= stopDistance)
        {
            StopMoving();
            return;
        }

        // Визначаємо напрямок: 1 (вправо) або -1 (вліво)
        float directionX = Mathf.Sign(_player.position.x - transform.position.x);

        // Рухаємось (через velocity, щоб працювала фізика та анімації)
        _rb.velocity = new Vector2(directionX * moveSpeed, _rb.velocity.y);
    }

    private void StopMoving()
    {
        _rb.velocity = new Vector2(0f, _rb.velocity.y);
    }

    private void HandleFlip()
    {
        float horizontal = _rb.velocity.x;

        if (horizontal < -flipDeadzone && facingRight)
        {
            facingRight = false;
            Vector3 e = transform.localEulerAngles;
            e.y = 180f;
            transform.localEulerAngles = e;
        }
        else if (horizontal > flipDeadzone && !facingRight)
        {
            facingRight = true;
            Vector3 e = transform.localEulerAngles;
            e.y = 0f;
            transform.localEulerAngles = e;
        }
    }

    private void UpdateAnimations()
    {
        if (animsController == null) return;

        float horizontal = Mathf.Abs(_rb.velocity.x);
        float vertical = _rb.velocity.y;

        // running: коли є горизонтальний рух і ми на землі
        bool isRunning = horizontal > runThreshold && isGrounded;
        animsController.SetRunning(isRunning);

        // jumping: коли вертикальна швидкість позитивна і не на землі
        bool isJumping = vertical > verticalThreshold && !isGrounded;
        animsController.SetJumping(isJumping);

        // falling: коли вертикальна швидкість негативна і не на землі
        bool isFalling = vertical < -verticalThreshold && !isGrounded;
        animsController.SetFalling(isFalling);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f); // простий візуал для моба

        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // vision range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}