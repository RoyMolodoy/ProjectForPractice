using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class SkyBeam : MonoBehaviour
{
    [Header("Таймінги атаки")]
    [SerializeField] private float warningDuration = 0.6f; // Скільки часу є у гравця, щоб втекти
    [SerializeField] private float strikeDuration = 0.2f;  // Скільки часу промінь залишається небезпечним
    [SerializeField] private int damage = 2;

    [Header("Візуал (Кольори)")]
    [SerializeField] private SpriteRenderer beamSprite;
    [SerializeField] private Color warningColor = new Color(1f, 0f, 0f, 0.4f); // Напівпрозорий червоний (попередження)
    [SerializeField] private Color strikeColor = new Color(1f, 1f, 1f, 1f);    // Яскраво-білий або жовтий (сам удар)

    private BoxCollider2D _collider;
    private bool _hasDamaged = false;

    private void Awake()
    {
        _collider = GetComponent<BoxCollider2D>();

        // Якщо ти забув перетягнути спрайт в інспекторі, скрипт знайде його сам
        if (beamSprite == null) beamSprite = GetComponentInChildren<SpriteRenderer>();

        // ОДРАЗУ вимикаємо колайдер, щоб він не наносив шкоду під час попередження
        _collider.enabled = false;
    }

    private void Start()
    {
        // Запускаємо послідовність атаки
        StartCoroutine(BeamSequence());
    }

    private IEnumerator BeamSequence()
    {
        // --- ФАЗА 1: ПОПЕРЕДЖЕННЯ ---
        // Промінь з'являється, але він напівпрозорий і не наносить шкоди
        if (beamSprite != null) beamSprite.color = warningColor;

        // Чекаємо, поки гравець відстрибне
        yield return new WaitForSeconds(warningDuration);


        // --- ФАЗА 2: УДАР ---
        // Промінь стає яскравим, колайдер вмикається і починає бити!
        if (beamSprite != null) beamSprite.color = strikeColor;
        _collider.enabled = true;

        // (Тут можна додати звук удару блискавки або лазера)

        // Чекаємо мить, поки промінь "горить"
        yield return new WaitForSeconds(strikeDuration);


        // --- ФАЗА 3: ЗНИКНЕННЯ ---
        // Вимикаємо колайдер, щоб гравець міг безпечно пройти через місце, де щойно був промінь
        _collider.enabled = false;

        // Видаляємо промінь зі сцени
        Destroy(gameObject);
    }

    // Тепер ми використовуємо OnTriggerEnter замість OnTriggerStay, 
    // бо колайдер вмикається рівно в момент удару
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !_hasDamaged)
        {
            collision.gameObject.SendMessage("MinusHP", damage, SendMessageOptions.DontRequireReceiver);
            _hasDamaged = true; // Захист від подвійного урону
        }
    }
}