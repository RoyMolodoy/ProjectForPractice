using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Ціль (Гравець)")]
    [SerializeField] private string playerTag = "Player";
    private Transform _player;

    [Header("Зір Боса")]
    [SerializeField] private float visionRange = 15f;

    [Header("Загальні налаштування")]
    [SerializeField] private float attackCooldown = 3.5f;
    [SerializeField] private Transform staffTip;

    [Header("Атака 1: Промінь зверху")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private float beamSpawnHeight = 7f;

    [Header("Атака 2: Летюча Голова")]
    [SerializeField] private GameObject flyingHeadPrefab;

    [Header("Події після смерті")]
    [SerializeField] private GameObject spawnOnDeathPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, -1f, 0f); // Налаштування позиції (нижче)
    [SerializeField] private float spawnSmoothDuration = 0.5f; // За скільки секунд об'єкт виросте

    [Header("Компоненти")]
    [SerializeField] public HPSystem HP;
    private Animator _anim;

    private float _lastAttackTime;
    private bool _isAttacking = false;
    private bool _facingRight = true;

    private bool _isDead = false;

    private void Awake()
    {
        if (HP == null) HP = GetComponent<HPSystem>();
        _anim = GetComponent<Animator>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }

        _lastAttackTime = Time.time - 1f;
    }

    private void Update()
    {
        if (_isDead) return;

        // ПЕРЕВІРКА НА СМЕРТЬ
        if (HP != null && HP.HP <= 0)
        {
            Die();
            return;
        }

        if (_player == null || _isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (distanceToPlayer <= visionRange)
        {
            HandleFlip();

            if (Time.time - _lastAttackTime >= attackCooldown)
            {
                ChooseRandomAttack();
            }
        }
    }

    private void HandleFlip()
    {
        bool playerIsRight = _player.position.x > transform.position.x;

        if (playerIsRight && !_facingRight)
        {
            _facingRight = true;
            transform.localEulerAngles = new Vector3(0f, 180f, 0f);
        }
        else if (!playerIsRight && _facingRight)
        {
            _facingRight = false;
            transform.localEulerAngles = new Vector3(0f, 0f, 0f);
        }
    }

    private void ChooseRandomAttack()
    {
        _lastAttackTime = Time.time;
        int attackIndex = Random.Range(0, 2);

        if (attackIndex == 0)
        {
            StartCoroutine(SkyBeamAttackCoroutine());
        }
        else
        {
            StartCoroutine(FlyingHeadAttackCoroutine());
        }
    }

    private IEnumerator SkyBeamAttackCoroutine()
    {
        _isAttacking = true;
        if (_anim != null) _anim.SetBool("BeamAttack", true);

        yield return new WaitForSeconds(0.6f);

        if (_player != null)
        {
            Vector2 spawnPos = new Vector2(_player.position.x, _player.position.y + beamSpawnHeight);
            Instantiate(beamPrefab, spawnPos, Quaternion.identity);
        }

        if (_anim != null) _anim.SetBool("BeamAttack", false);
        _isAttacking = false;
    }

    private IEnumerator FlyingHeadAttackCoroutine()
    {
        _isAttacking = true;
        if (_anim != null) _anim.SetBool("HeadAttack", true);

        yield return new WaitForSeconds(1f);

        Vector3 spawnPos = staffTip != null ? staffTip.position : transform.position;
        GameObject headObj = Instantiate(flyingHeadPrefab, spawnPos, Quaternion.identity);

        FlyingHead headScript = headObj.GetComponent<FlyingHead>();
        if (headScript != null)
        {
            headScript.SetTarget(_player);
        }

        if (_anim != null) _anim.SetBool("HeadAttack", false);
        _isAttacking = false;
    }

    // --- ЛОГІКА СМЕРТІ ---
    private void Die()
    {
        _isDead = true;

        StopAllCoroutines();

        if (_anim != null)
        {
            _anim.SetBool("BeamAttack", false);
            _anim.SetBool("HeadAttack", false);
        }

        StartCoroutine(DeathTimerRoutine());
    }

    private IEnumerator DeathTimerRoutine()
    {
        yield return new WaitForSeconds(3f);

        if (spawnOnDeathPrefab != null)
        {
            // 1. Зміщуємо позицію спавну (за замовчуванням Y = -1, тобто під ноги босу)
            Vector3 spawnPos = transform.position + spawnOffset;

            // 2. Створюємо об'єкт
            GameObject spawnedObj = Instantiate(spawnOnDeathPrefab, spawnPos, Quaternion.identity);

            // 3. Автоматично вішаємо на нього наш скрипт плавної появи
            if (spawnSmoothDuration > 0f)
            {
                SmoothScaler scaler = spawnedObj.AddComponent<SmoothScaler>();
                scaler.duration = spawnSmoothDuration;
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Малюємо крапочку, де саме з'явиться лут після смерті (щоб тобі було легше налаштовувати)
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + spawnOffset, 0.2f);
    }
}

// ====================================================================
// ДОПОМІЖНИЙ СКРИПТ (Автоматично збільшує об'єкт і самознищується)
// ====================================================================
public class SmoothScaler : MonoBehaviour
{
    public float duration = 0.5f;
    private Vector3 originalScale;

    private void Start()
    {
        // Запам'ятовуємо, якого розміру мав бути об'єкт
        originalScale = transform.localScale;

        // Зменшуємо його до нуля (щоб він був невидимим)
        transform.localScale = Vector3.zero;

        // Запускаємо анімацію зростання
        StartCoroutine(ScaleRoutine());
    }

    private IEnumerator ScaleRoutine()
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale, elapsed / duration);
            yield return null;
        }

        // Гарантуємо, що розмір ідеальний
        transform.localScale = originalScale;

        // Видаляємо ЦЕЙ СКРИПТ (не сам об'єкт!), бо він більше не потрібен
        Destroy(this);
    }
}