using UnityEngine;
using UnityEngine.SceneManagement; // Обов'язково для роботи зі сценами

public class NextScene : MonoBehaviour
{
    [Header("Налаштування завантаження")]
    [Tooltip("Точна назва сцени, яку треба завантажити (як у вікні Build Settings)")]
    public string sceneToLoad;

    [Header("Збереження")]
    [Tooltip("Чи зберігати стами та позицію перед переходом на новий рівень?")]
    public bool saveBeforeTransition = true;

    // --- ВАРІАНТ 1: ДЛЯ UI КНОПКИ ---
    // Цей метод можна викликати при натисканні на кнопку в меню
    public void LoadScene()
    {
        ExecuteTransition();
    }

    // --- ВАРІАНТ 2: ДЛЯ ТРИГЕРА НА РІВНІ ---
    // Спрацьовує, коли гравець заходить у зону порталу/дверей
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ExecuteTransition();
        }
    }

    private void ExecuteTransition()
    {
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError($"{name}: Назву сцени не вказано в Інспекторі!");
            return;
        }

        // Перед тим як знищити поточну сцену, фіксуємо прогрес у файл,
        // щоб SaveManager автоматично підтягнув його на новому рівні.
        if (saveBeforeTransition && SaveManager.Instance != null)
        {
            SaveManager.Instance.SaveGame();
        }

        // Переходимо на іншу сцену
        SceneManager.LoadScene(sceneToLoad);
    }
}