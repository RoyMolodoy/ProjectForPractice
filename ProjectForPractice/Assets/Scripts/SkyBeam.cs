using UnityEngine;

public class SkyBeam : MonoBehaviour
{
    [SerializeField] private float duration = 1.2f; // Скільки часу промінь існує на сцені
    [SerializeField] private float damageDelay = 0.4f; // Затримка в секундах перед ударом (час, щоб гравець встиг відстрибнути)
    [SerializeField] private int damage = 2;

    private float _spawnTime;
    private bool _hasDamaged = false;

    private void Start()
    {
        _spawnTime = Time.time;
        // Кажемо променю автоматично знищитися після завершення тривалості
        Destroy(gameObject, duration);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Перевіряємо, чи вже пройшов час попередження і чи ми ще не наносили шкоду
        if (Time.time - _spawnTime >= damageDelay && !_hasDamaged)
        {
            if (collision.CompareTag("Player"))
            {
                collision.gameObject.SendMessage("MinusHP", damage, SendMessageOptions.DontRequireReceiver);
                _hasDamaged = true; // Захист від того, щоб урон не наносився кожен кадр
            }
        }
    }
}