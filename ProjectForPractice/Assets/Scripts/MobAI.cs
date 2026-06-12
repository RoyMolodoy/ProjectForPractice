using System.Collections;
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
    [SerializeField] public PlayerHP HP;
    [SerializeField] private PlayerAttack playerAttack;
    [SerializeField] private float verticalThreshold = 0.1f; // поріг по Y для стрибка/падіння анімацій
    [SerializeField] private float flipDeadzone = 0.05f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackDuration = 0.6f; // тривалість анімації атаки
    [SerializeField] private float attackHitDelay = 0.2f; // використовуется як запасний таймер, но основной — Animation Event

    private bool _isDead = false;
    private Rigidbody2D _rb;
    private Transform _player;
    private bool facingRight = true;
    private bool isGrounded;
    private float _lastAttackTime = -999f;
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

        if (HP == null)
            HP = GetComponent<PlayerHP>();
        if (animsController == null)
            animsController = GetComponent<AnimsController>();

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
        if (_isAttacking) // під час атаки рух дозволяти не будемо
        {
            _rb.velocity = new Vector2(0f, _rb.velocity.y);
            UpdateAnimations();
            return;
        }

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

        // Устанавливаем флаг движения и запускаем анимацию бега через контроллер
        if (!_isMoving)
        {
            _isMoving = true;
            animsController?.SetRunning(true);
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

    // Обработчик, вызываемый из AnimsController при событии попадания в анимации
    private void HandleAttackHit()
    {
        if (!_isAttacking || _player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);
        if (distanceToPlayer <= attackRange)
        {
            _player.gameObject.SendMessage("MinusHP", damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    // Обработчик конца анимации (если нужно)
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
        // 1. Захист від подвійної смерті
        if (_isDead) return;

        // 2. Перевірка: чи є взагалі здоров'я у моба?
        if (HP == null)
        {
            Debug.LogError($"<color=red>ПОМИЛКА:</color> На мобі {name} немає скрипта PlayerHP! Додай його в Інспекторі.", this);
            return;
        }

        // Віднімаємо здоров'я
        HP.playerHP -= damageAmount;
        Debug.Log($"Моб {name} отримав {damageAmount} шкоди. Залишилось ХП: {HP.playerHP}");

        // Перевіряємо смерть
        if (HP.playerHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"<color=orange>Моб {name} помирає!</color>");
        _isDead = true;

        // Повністю зупиняємо моба
        _rb.velocity = Vector2.zero;
        _isMoving = false;
        _isAttacking = false;

        // Вимикаємо всі анімації руху та атаки
        animsController?.SetRunning(false);
        animsController?.SetAttacking(false);

        // Вмикаємо анімацію смерті
        animsController?.DeathAnim();

        // Вимикаємо колайдер, щоб труп не заважав гравцю ходити (за бажанням)
        Collider2D coll = GetComponent<Collider2D>();
        if (coll != null) coll.enabled = false;

        // Вимикаємо MobAI, щоб FixedUpdate більше не працював
        this.enabled = false;
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

        // running: теперь учитываем флаг _isMoving, а не только скорость и isGrounded
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
    }
}