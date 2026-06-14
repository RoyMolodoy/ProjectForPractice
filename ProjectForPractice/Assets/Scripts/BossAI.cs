using System.Collections;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    [Header("Ціль (Гравець)")]
    [SerializeField] private string playerTag = "Player";
    private Transform _player;

    [Header("Зір Боса")]
    [SerializeField] private float visionRange = 15f; // Радіус, в якому бос активується

    [Header("Загальні налаштування")]
    [SerializeField] private float attackCooldown = 3.5f;
    [SerializeField] private Transform staffTip;

    [Header("Атака 1: Промінь зверху")]
    [SerializeField] private GameObject beamPrefab;
    [SerializeField] private float beamSpawnHeight = 7f;

    [Header("Атака 2: Летюча Голова")]
    [SerializeField] private GameObject flyingHeadPrefab;

    [Header("Компоненти")]
    [SerializeField] public HPSystem HP;
    private Animator _anim;

    private float _lastAttackTime;
    private bool _isAttacking = false;
    private bool _facingRight = true;

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
        if (_player == null || _isAttacking) return;

        // Вираховуємо дистанцію від боса до гравця
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        // Якщо гравець зайшов у зону бачення боса
        if (distanceToPlayer <= visionRange)
        {
            // 1. Бос розвертається і постійно дивиться на гравця
            HandleFlip();

            // 2. Якщо таймер відкату пройшов - бос атакує
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

    // --- ЛОГІКА АТАКИ 1: ПРОМІНЬ ЗВЕРХУ ---
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

    // --- ЛОГІКА АТАКИ 2: ЛЕТЮЧА ГОЛОВА ---
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

    // --- ВІЗУАЛІЗАЦІЯ ЗОРУ В UNITY ---
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        // Малює блакитне коло навколо боса, щоб ти бачив, де починається його зона агро
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}