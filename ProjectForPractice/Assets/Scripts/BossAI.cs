using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Ціль (Гравець)")]
    [SerializeField] private string playerTag = "Player";
    private Transform _player;

    [Header("Загальні налаштування")]
    [SerializeField] private float attackCooldown = 3.5f; // Пауза між атаками
    [SerializeField] private Transform staffTip; // Точка на кінці посоху (для спавну голови)

    [Header("Атака 1: Промінь зверху")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private float beamSpawnHeight = 7f; // На якій висоті над гравцем спавнити промінь

    [Header("Атака 2: Летюча Голова")]
    [SerializeField] private GameObject flyingHeadPrefab;

    [Header("Компоненти")]
    [SerializeField] public HPSystem HP;
    // [SerializeField] private AnimsController animsController; // Розкоментуй, якщо додаси анімації босу

    private float _lastAttackTime;
    private bool _isAttacking = false;
    private bool _facingRight = true;

    private void Awake()
    {
        if (HP == null) HP = GetComponent<HPSystem>();
    }

    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
        {
            _player = playerObj.transform;
        }

        // Робимо невеличку затримку перед першою атакою на початку бою
        _lastAttackTime = Time.time - 1f;
    }

    private void Update()
    {
        if (_player == null) return;

        // Повертатися до гравця босс повинен завжди, крім моменту, коли він уже щось чаклує
        if (!_isAttacking)
        {
            HandleFlip();
        }

        // Автоматичний вибір атаки за таймером
        if (!_isAttacking && Time.time - _lastAttackTime >= attackCooldown)
        {
            ChooseRandomAttack();
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

        // Випадково обираємо 0 або 1
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

    // --- ЛОГІКА АТАК И 1: ПРОМІНЬ ЗВЕРХУ ---
    private IEnumerator SkyBeamAttackCoroutine()
    {
        _isAttacking = true;

        // Тут можна запустити тригер анімації підняття посоху:
        // animsController?.SetTrigger("LiftStaff");

        yield return new WaitForSeconds(0.6f); // Час на замах/анімацію

        if (_player != null)
        {
            // Визначаємо точку прямо НАД гравцем на заданій висоті
            Vector2 spawnPos = new Vector2(_player.position.x, _player.position.y + beamSpawnHeight);
            Instantiate(beamPrefab, spawnPos, Quaternion.identity);
        }

        yield return new WaitForSeconds(1f); // Пауза після касту, щоб бос не одразу повернувся до звичайного стану
        _isAttacking = false;
    }

    // --- ЛОГІКА АТАК И 2: ЛЕТЮЧА ГОЛОВА ---
    private IEnumerator FlyingHeadAttackCoroutine()
    {
        _isAttacking = true;

        // Тут можна запустити тригер анімації виклику голови:
        // animsController?.SetTrigger("CastHead");

        yield return new WaitForSeconds(0.5f);

        // Визначаємо точку спавну (якщо немає staffTip, спавнимо в центрі боса)
        Vector3 spawnPos = staffTip != null ? staffTip.position : transform.position;

        GameObject headObj = Instantiate(flyingHeadPrefab, spawnPos, Quaternion.identity);

        // Передаємо створеній голові посилання на гравця, щоб вона знала за чим летіти
        FlyingHead headScript = headObj.GetComponent<FlyingHead>();
        if (headScript != null)
        {
            headScript.SetTarget(_player);
        }

        yield return new WaitForSeconds(0.8f);
        _isAttacking = false;
    }
}