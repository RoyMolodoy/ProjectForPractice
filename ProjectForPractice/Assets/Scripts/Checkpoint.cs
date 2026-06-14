using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Checkpoint : MonoBehaviour
{
    [Header("ID")]
    public string objectID;

    [Header("Settings")]
    public bool isReusable = true;

    public bool hasBeenUsed = false;

    private void Awake()
    {
        GetComponent<BoxCollider2D>().isTrigger = true;
    }

    private void Start()
    {
        if (string.IsNullOrEmpty(objectID)) return;

        // ПЕРЕВІРКА ПРИ ЗАВАНТАЖЕННІ СЦЕНИ:
        // Якщо цей чекпоінт уже є в базі збережень як "активований"
        if (SaveManager.Instance != null && SaveManager.Instance.IsCheckpointActivated(objectID))
        {
            hasBeenUsed = true;

            // 💡 ТУТ ТИ МОЖЕШ ДОДАТИ СВОЇ ЕФЕКТИ:
            // Наприклад, змінити спрайт на запалене вогнище, увімкнути партикли світла тощо.
            // GetComponent<SpriteRenderer>().sprite = activeSprite;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // Якщо він одноразовий і вже використовувався — ігноруємо
            if (!isReusable && hasBeenUsed) return;

            if (SaveManager.Instance != null)
            {
                // 1. Спочатку реєструємо цей чекпоінт у списку активованих
                SaveManager.Instance.MarkCheckpointAsActivated(objectID);

                // 2. Тільки після цього викликаємо збереження файлу
                SaveManager.Instance.SaveGame();

                hasBeenUsed = true;
                Debug.Log("<color=yellow>Прогрес та стан чекпоінту збережено!</color>");
            }
        }
    }
}