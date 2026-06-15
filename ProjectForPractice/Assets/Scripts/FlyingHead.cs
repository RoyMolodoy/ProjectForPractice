using UnityEngine;

public class FlyingHead : MonoBehaviour
{
    [SerializeField] private float speed = 4.5f;
    [SerializeField] private float lifetime = 3f; // „ас житт€ голови в секундах
    [SerializeField] public float damage = 1;

    private Transform _target;
    private float _destroyTime;
    private bool _facingRight = false;

    public void SetTarget(Transform target)
    {
        _target = target;
    }

    private void Start()
    {
        // ¬ираховуЇмо точний час, коли голова маЇ зникнути
        _destroyTime = Time.time + lifetime;
    }

    private void Update()
    {
        // якщо 3 секунди минуло Ч голова вибухаЇ або просто зникаЇ
        if (Time.time >= _destroyTime)
        {
            Explode();
            return;
        }

        if (_target == null) return;

        // –ух у б≥к гравц€ (плавне пересл≥дуванн€)
        Vector2 direction = ((Vector2)_target.position - (Vector2)transform.position).normalized;
        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        // ѕоворот самоњ голови в б≥к польоту (вл≥во/вправо)
        if (direction.x > 0 && !_facingRight)
        {
            _facingRight = true;
            transform.localEulerAngles = new Vector3(0, 180, 0);
        }
        else if (direction.x < 0 && _facingRight)
        {
            _facingRight = false;
            transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // якщо влучили в гравц€
        if (collision.CompareTag("Player"))
        {
            collision.gameObject.SendMessage("MinusHP", damage, SendMessageOptions.DontRequireReceiver);
            Explode();
        }
    }

    private void Explode()
    {
        // “ут можна заспавнити красив≥ партикли або ефект вибуху
        Destroy(gameObject);
    }
}