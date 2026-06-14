using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MobAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.8f;

    [Header("Jump Over Obstacles")]
    [SerializeField] private float jumpForce = 6f; // Сила стрибка моба
    [SerializeField] private float obstacleCheckDistance = 0.6f; // На якій відстані моб помічає стіну
    [SerializeField] private float jumpCooldown = 1.0f; // Затримка між стрибками, щоб не стрибав без упину
    [SerializeField] private Vector2 obstacleRayOffset = new Vector2(0f, 0.2f); // Зміщення променя по висоті (щоб не чіпляв підлогу)

    [Header("Vision (Line of Sight)")]
    [SerializeField] private float visionRange = 10f;
    [SerializeField] private LayerMask obstacleLayer; // Цей же шар використовуємо для стін, через які треба стрибати
    [SerializeField] private Vector2 eyeOffset = new Vector2(0f, 0.5f);

    [Header("Ground & Animations")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.12f;
    [SerializeField] private AnimsController animsController;
    [SerializeField] public HPSystem HP;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private float verticalThreshold = 0.1f;
    [SerializeField] private float flipDeadzone = 0.05f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackDuration = 0.6f;
    [SerializeField] private float attackHitDelay = 0.2f;

    private bool _isDead = false;
    private Rigidbody2D _rb;
    private Transform _player;
    private bool facingRight = true;
    private bool isGrounded;
    private float _lastAttackTime = -999f;
    private float _lastJumpTime = -999f; // Таймер для стрибків
    private bool _isAttacking = false;
    private bool _isMoving = false;

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

        if (HP == null) HP = GetComponent<HPSystem>();
        if (animsController == null) animsController = GetComponent<AnimsController>();

        if (animsController != null)
        {
            animsController.OnAttackHit += HandleAttackHit;
            animsController.OnAttackEnd += HandleAttackEnd;
        }
    }

    private void OnDestroy()
    {
        if (animsController != null)
        {
            animsController.OnAttackHit -= HandleAttackHit;
            animsController.OnAttackEnd -= HandleAttackEnd;
        }
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;
            playerAttack = _player.gameObject.GetComponent<PlayerAttack>();
        }
    }

    private void FixedUpdate()
    {
        if (_player == null) return;
        if (_isAttacking)
        {
            _rb.velocity = new Vector2(0f, _rb.velocity.y);
            UpdateAnimations();
            return;
        }

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (CanSeePlayer())
        {
            ChasePlayer();
        }
        else
        {
            StopMoving();
        }

        HandleFlip();
        UpdateAnimations();
    }

    private bool CanSeePlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        if (distanceToPlayer > visionRange) return false;

        Vector2 startPos = (Vector2)transform.position + eyeOffset;
        Vector2 targetPos = (Vector2)_player.position + eyeOffset;

        RaycastHit2D hit = Physics2D.Linecast(startPos, targetPos, obstacleLayer);

        if (hit.collider != null)
        {
            Debug.DrawLine(startPos, hit.point, Color.red);
            return false;
        }
        else
        {
            Debug.DrawLine(startPos, targetPos, Color.green);
            return true;
        }
    }

    private void ChasePlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer <= stopDistance)
        {
            StopMoving();
            TryAttack();
            return;
        }

        float directionX = Mathf.Sign(_player.position.x - transform.position.x);
        _rb.velocity = new Vector2(directionX * moveSpeed, _rb.velocity.y);

        if (!_isMoving)
        {
            _isMoving = true;
            animsController?.SetRunning(true);
        }

        // ПЕРЕВІРКА ПЕРЕШКОД І СТРИБОК
        CheckObstacleAndJump(directionX);
    }

    // --- ЛОГІКА СТРИБКА ---
    // --- ЛОГІКА СТРИБКА ---
    private void CheckObstacleAndJump(float directionX)
    {
        // Стрибати можна тільки якщо моб на землі і пройшов кулдаун після минулого стрибка
        if (!isGrounded || Time.time - _lastJumpTime < jumpCooldown) return;

        // Позиція, з якої пускаємо промінь
        Vector2 startPos = (Vector2)transform.position + obstacleRayOffset;
        Vector2 endPos = startPos + new Vector2(directionX * obstacleCheckDistance, 0f);

        RaycastHit2D hit = Physics2D.Linecast(startPos, endPos, obstacleLayer);

        if (hit.collider != null)
        {
            // Перешкоду знайдено - стрибаємо!
            _lastJumpTime = Time.time;
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);

            // ПРИМУСОВО ВИКЛИКАЄМО АНІМАЦІЮ СТРИБКА (Безпечно для мобів без неї)
            animsController?.SetJumping(true);

            // Якщо в тебе є окремий тригер саме для початку стрибка (за бажанням):
            // animsController?.isJumpingTrigger(); // (якщо ти додаси такий метод в AnimsController)
        }
    }

    private void TryAttack()
    {
        if (_isAttacking) return;
        if (Time.time - _lastAttackTime < attackCooldown) return;
        if (_player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        if (distanceToPlayer > attackRange) return;

        FacePlayerInstant();

        _lastAttackTime = Time.time;
        StartCoroutine(DoAttackCoroutine());
    }

    private System.Collections.IEnumerator DoAttackCoroutine()
    {
        _isAttacking = true;
        animsController?.SetRunning(false);
        animsController?.SetAttacking(true);
        _rb.velocity = new Vector2(0f, _rb.velocity.y);

        if (attackHitDelay > 0f)
            yield return new WaitForSeconds(attackHitDelay);

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        if (distanceToPlayer <= attackRange)
            _player.gameObject.SendMessage("MinusHP", damage, SendMessageOptions.DontRequireReceiver);

        float remaining = Mathf.Max(0f, attackDuration - attackHitDelay);
        if (remaining > 0f)
            yield return new WaitForSeconds(remaining);

        animsController?.ResetAttackBool();
        _isAttacking = false;
    }

    private void StopMoving()
    {
        _rb.velocity = new Vector2(0f, _rb.velocity.y);

        if (_isMoving)
        {
            _isMoving = false;
            animsController?.SetRunning(false);
        }
    }

    private void HandleAttackHit()
    {
        if (!_isAttacking || _player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        if (distanceToPlayer <= attackRange)
        {
            _player.gameObject.SendMessage("MinusHP", damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void HandleAttackEnd()
    {
        _isAttacking = false;
    }

    private void FacePlayerInstant()
    {
        if (_player == null) return;
        bool playerIsRight = _player.position.x > transform.position.x;
        Vector3 e = transform.localEulerAngles;
        e.y = playerIsRight ? 180f : 0f;
        transform.localEulerAngles = e;
        facingRight = playerIsRight;
    }

    public void TakeDamage(int damageAmount)
    {
        if (_isDead) return;
        if (HP == null) return;

        HP.HP -= damageAmount;
    }

    private void HandleFlip()
    {
        if (_isAttacking) return;

        float horizontal = _rb.velocity.x;

        if (horizontal < -flipDeadzone && facingRight)
        {
            facingRight = false;
            Vector3 e = transform.localEulerAngles;
            e.y = 0f;
            transform.localEulerAngles = e;
        }
        else if (horizontal > flipDeadzone && !facingRight)
        {
            facingRight = true;
            Vector3 e = transform.localEulerAngles;
            e.y = 180f;
            transform.localEulerAngles = e;
        }
    }

    private void UpdateAnimations()
    {
        if (animsController == null) return;

        float horizontal = Mathf.Abs(_rb.velocity.x);
        float vertical = _rb.velocity.y;

        bool isRunning = _isMoving && !_isAttacking;
        animsController.SetRunning(isRunning);

        bool isJumping = vertical > verticalThreshold && !isGrounded;
        animsController.SetJumping(isJumping);

        bool isFalling = vertical < -verticalThreshold && !isGrounded;
        animsController.SetFalling(isFalling);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Малюємо лінію перевірки перешкоди у Scene View
        Gizmos.color = Color.yellow;
        Vector2 startPos = (Vector2)transform.position + obstacleRayOffset;
        Vector2 endPos = startPos + new Vector2((facingRight ? 1f : -1f) * obstacleCheckDistance, 0f);
        Gizmos.DrawLine(startPos, endPos);
    }
}