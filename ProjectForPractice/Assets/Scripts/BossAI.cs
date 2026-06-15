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
    [SerializeField] public float beamDamage = 2;

    [Header("Атака 2: Летюча Голова")]
    [SerializeField] private GameObject flyingHeadPrefab;
    [SerializeField] public float headDamage = 1;

    [Header("Події після смерті")]
    [SerializeField] private GameObject spawnOnDeathPrefab;
    [SerializeField] private Vector3 spawnOffset = new Vector3(0f, -1f, 0f);
    [SerializeField] private float spawnSmoothDuration = 0.5f;

    [Header("Компоненти")]
    [SerializeField] public HPSystem HP;
    private Animator _anim;

    private float _lastAttackTime;
    private bool _isAttacking = false;
    private bool _facingRight = true;

    private bool _isDead = false;

    public AudioSource audioSource;
    public AudioClip beamAttack;
    public AudioClip headAttack;

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
        if(audioSource != null)
        {
            audioSource.PlayOneShot(beamAttack);
        }

        yield return new WaitForSeconds(0.6f);

        if (_player != null)
        {
            Vector2 spawnPos = new Vector2(_player.position.x, _player.position.y + beamSpawnHeight);
            GameObject spawnedBeam = Instantiate(beamPrefab, spawnPos, Quaternion.identity);

            var beamScript = spawnedBeam.GetComponent<SkyBeam>();
            if (beamScript != null)
            {
                beamScript.damage = beamDamage; 
            }
        }

        if (_anim != null) _anim.SetBool("BeamAttack", false);
        _isAttacking = false;
    }

    private IEnumerator FlyingHeadAttackCoroutine()
    {
        _isAttacking = true;
        if (_anim != null) _anim.SetBool("HeadAttack", true);
        if (audioSource != null)
        {
            audioSource.PlayOneShot(headAttack);
        }

        yield return new WaitForSeconds(1f);

        Vector3 spawnPos = staffTip != null ? staffTip.position : transform.position;
        GameObject headObj = Instantiate(flyingHeadPrefab, spawnPos, Quaternion.identity);

        FlyingHead headScript = headObj.GetComponent<FlyingHead>();
        if (headScript != null)
        {
            headScript.SetTarget(_player);

            headScript.damage = headDamage;
        }

        if (_anim != null) _anim.SetBool("HeadAttack", false);
        _isAttacking = false;
    }

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
            Vector3 spawnPos = transform.position + spawnOffset;
            GameObject spawnedObj = Instantiate(spawnOnDeathPrefab, spawnPos, Quaternion.identity);

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

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position + spawnOffset, 0.2f);
    }
}

public class SmoothScaler : MonoBehaviour
{
    public float duration = 0.5f;
    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
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

        transform.localScale = originalScale;
        Destroy(this);
    }
}