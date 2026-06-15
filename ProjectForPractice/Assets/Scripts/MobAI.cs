using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MobAI : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float stopDistance = 0.8f;

    [Header("Jump Over Obstacles")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private float obstacleCheckDistance = 0.6f;
    [SerializeField] private float jumpCooldown = 1.0f;
    [SerializeField] private Vector2 obstacleRayOffset = new Vector2(0f, 0.2f);

    [Header("Vision")]
    [SerializeField] private float visionRange = 10f;
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private Vector2 eyeOffset = new Vector2(0f, 0.5f);

    [Header("Anti jitter (only directly above)")]
    [SerializeField] private float ignoreAboveHeight = 1.2f;
    [SerializeField] private float ignoreAboveHorizontalRange = 1.0f;

    [Header("Ground")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.12f;

    [Header("Animations")]
    [SerializeField] private AnimsController animsController;
    [SerializeField] public HPSystem HP;

    [Header("Attack")]
    [SerializeField] private float attackRange = 0.8f;
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackDuration = 0.6f;
    [SerializeField] private float attackHitDelay = 0.2f;

    private Rigidbody2D _rb;
    private Transform _player;

    private bool facingRight = true;
    private bool isGrounded;
    private bool _isAttacking = false;
    private bool _isMoving = false;

    private float _lastAttackTime = -999f;
    private float _lastJumpTime = -999f;

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
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            _player = playerObj.transform;

            // 🔥 FIX: одразу повертаємось до гравця при спавні
            FacePlayerInstant();
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

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );

        // 🔥 ANTI JITTER ONLY IF DIRECTLY ABOVE
        Vector2 toPlayer = _player.position - transform.position;

        bool playerDirectlyAbove =
            toPlayer.y > ignoreAboveHeight &&
            Mathf.Abs(toPlayer.x) < ignoreAboveHorizontalRange;

        if (playerDirectlyAbove)
        {
            StopMoving();
            _rb.velocity = new Vector2(0f, _rb.velocity.y);
            UpdateAnimations();
            return;
        }

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
        float dist = Vector2.Distance(transform.position, _player.position);
        if (dist > visionRange) return false;

        Vector2 start = (Vector2)transform.position + eyeOffset;
        Vector2 end = (Vector2)_player.position + eyeOffset;

        return Physics2D.Linecast(start, end, obstacleLayer).collider == null;
    }

    private void ChasePlayer()
    {
        float dist = Vector2.Distance(transform.position, _player.position);

        if (dist <= stopDistance)
        {
            StopMoving();
            TryAttack();
            return;
        }

        float dir = Mathf.Sign(_player.position.x - transform.position.x);

        _rb.velocity = new Vector2(dir * moveSpeed, _rb.velocity.y);

        if (!_isMoving)
        {
            _isMoving = true;
            animsController?.SetRunning(true);
        }

        CheckObstacleAndJump(dir);
    }

    private void CheckObstacleAndJump(float dir)
    {
        if (!isGrounded || Time.time - _lastJumpTime < jumpCooldown) return;

        Vector2 start = (Vector2)transform.position + obstacleRayOffset;
        Vector2 end = start + new Vector2(dir * obstacleCheckDistance, 0f);

        if (Physics2D.Linecast(start, end, obstacleLayer))
        {
            _lastJumpTime = Time.time;
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            animsController?.SetJumping(true);
        }
    }

    private void TryAttack()
    {
        if (_isAttacking) return;
        if (Time.time - _lastAttackTime < attackCooldown) return;

        if (Vector2.Distance(transform.position, _player.position) > attackRange)
            return;

        FacePlayerInstant();

        _lastAttackTime = Time.time;
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        animsController?.SetRunning(false);
        animsController?.SetAttacking(true);
        _rb.velocity = new Vector2(0f, _rb.velocity.y);

        yield return new WaitForSeconds(attackHitDelay);

        if (Vector2.Distance(transform.position, _player.position) <= attackRange)
        {
            _player.gameObject.SendMessage("MinusHP", damage,
                SendMessageOptions.DontRequireReceiver);
        }

        yield return new WaitForSeconds(Mathf.Max(0f, attackDuration - attackHitDelay));

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

    // 🔥 FIX: завжди дивимось на гравця правильно
    private void FacePlayerInstant()
    {
        if (_player == null) return;

        bool right = _player.position.x > transform.position.x;

        facingRight = right;

        Vector3 e = transform.localEulerAngles;
        e.y = right ? 180f : 0f;
        transform.localEulerAngles = e;
    }

    private void HandleFlip()
    {
        if (_isAttacking) return;

        float vx = _rb.velocity.x;

        if (vx < -0.05f && facingRight)
        {
            facingRight = false;
            transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        }
        else if (vx > 0.05f && !facingRight)
        {
            facingRight = true;
            transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        }
    }

    private void UpdateAnimations()
    {
        if (animsController == null) return;

        animsController.SetRunning(_isMoving && !_isAttacking);

        animsController.SetJumping(
            _rb.velocity.y > 0.1f && !isGrounded
        );

        animsController.SetFalling(
            _rb.velocity.y < -0.1f && !isGrounded
        );
    }
}