using UnityEngine;

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

    private Rigidbody2D _rb;
    private Transform _player;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
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
        // Якщо лінія врізається в obstacleLayer (стіну), повертається true
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

        // Рухаємось
        _rb.velocity = new Vector2(directionX * moveSpeed, _rb.velocity.y);
    }

    private void StopMoving()
    {
        _rb.velocity = new Vector2(0f, _rb.velocity.y);
    }
}