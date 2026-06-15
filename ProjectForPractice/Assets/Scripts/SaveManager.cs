using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    private string savePath;

    [Header("UI (Краще оновлювати з інших скриптів)")]
    public TextMeshProUGUI Health;
    public TextMeshProUGUI Defence;
    public TextMeshProUGUI Damage;

    // levelSystem більше не робимо public для Інспектора, ми будемо шукати його самі
    private LevelSystem levelSystem;

    private void Awake()
    {
        // 🔥 ВИПРАВЛЕНО: Тепер об'єкт ДІЙСНО не знищується
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Цього рядка не вистачало!
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Application.persistentDataPath + "/savegame.json";
        Debug.Log("Шлях збереження: " + savePath);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        LoadGame();
    }

    public void SaveGame()
    {
        PlayerMovement movement = FindObjectOfType<PlayerMovement>();

        if (movement == null)
        {
            Debug.LogError("ЗБЕРЕЖЕННЯ ПРОВАЛЕНО: На сцені взагалі немає об'єкта зі скриптом PlayerMovement!");
            return;
        }

        // Беремо об'єкт, на якому висить цей скрипт
        GameObject playerObj = movement.gameObject;
        HPSystem hpSystem = playerObj.GetComponent<HPSystem>();
        PlayerAttack attack = playerObj.GetComponent<PlayerAttack>();

        if (hpSystem == null) Debug.LogError("ЗБЕРЕЖЕННЯ ПРОВАЛЕНО: Не знайдено HPSystem на гравці!");
        if (attack == null) Debug.LogError("ЗБЕРЕЖЕННЯ ПРОВАЛЕНО: Не знайдено PlayerAttack на гравці!");

        if (hpSystem == null || attack == null) return;

        levelSystem = FindObjectOfType<LevelSystem>();

        PlayerSaveData data = new PlayerSaveData();
        data.savedSceneName = SceneManager.GetActiveScene().name;
        data.maxHP = hpSystem.MaxHP;
        data.defense = hpSystem.defense;
        data.damage = attack.attackDamage;
        data.canDash = movement.canDash;
        data.canDoubleJump = movement.canDoubleJump;

        if (levelSystem != null)
            data.currentLevel = levelSystem.LevelNumber;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"<color=green>ЧЕКПОІНТ УСПІШНО ЗБЕРЕЖЕНО! Файл: {savePath}</color>");
        Debug.Log($"Збережено дані: ХП={data.maxHP}, Урон={data.damage}, Деш={data.canDash}");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;

        // 🔥 ШУКАЄМО ЗА СКРИПТОМ, А НЕ ЗА ТЕГОМ!
        PlayerMovement movement = FindObjectOfType<PlayerMovement>();
        if (movement == null) return;

        GameObject playerObj = movement.gameObject;
        HPSystem hpSystem = playerObj.GetComponent<HPSystem>();
        PlayerAttack attack = playerObj.GetComponent<PlayerAttack>();

        if (hpSystem == null || attack == null) return;

        string json = File.ReadAllText(savePath);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

        // 🔥 ВИПРАВЛЕНО: Шукаємо LevelSystem на НОВІЙ завантаженій сцені
        levelSystem = FindObjectOfType<LevelSystem>();
        if (levelSystem != null && data.currentLevel > 0)
        {
            levelSystem.LevelNumber = data.currentLevel;
        }

        // Якщо дані з файлу пусті (нулі), ставимо базові значення, щоб гравець не був безсмертним
        hpSystem.MaxHP = data.maxHP > 0 ? data.maxHP : 3f; // 3 - це базове ХП
        hpSystem.HP = hpSystem.MaxHP;

        hpSystem.defense = data.defense > 0 ? data.defense : 1f; // 1 - це базовий захист (щоб не було ділення на 0)
        attack.attackDamage = data.damage > 0 ? data.damage : 1; // 1 - базовий урон
        movement.canDash = data.canDash;
        movement.canDoubleJump = data.canDoubleJump;

        // Оновлення UI. (Якщо ці тексти знищуються при зміні сцени, вони тут не оновляться. 
        // Найкраще, щоб UI сам читав значення з гравця у своєму методі Start).
        if (Health != null && Defence != null && Damage != null)
        {
            Health.text = $"{hpSystem.MaxHP}";
            Defence.text = $"{hpSystem.defense}";
            Damage.text = $"{attack.attackDamage}";
        }

        if (hpSystem.HPBar != null)
        {
            hpSystem.HPBar.fillAmount = (float)hpSystem.HP / hpSystem.MaxHP;
        }

        Debug.Log("<color=cyan>Прогрес успішно завантажено на новій сцені!</color>");
    }

    [ContextMenu("Видалити збереження (Очистити прогрес)")]
    public void DeleteSaveFile()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("<color=red>Файл збереження видалено. Наступний запуск буде з самого початку!</color>");
        }
        else
        {
            Debug.Log("Файлу збереження і так немає.");
        }
    }

    public void ContinueGame()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

            if (!string.IsNullOrEmpty(data.savedSceneName))
            {
                Debug.Log("Завантажуємо збережену сцену: " + data.savedSceneName);
                SceneManager.LoadScene(data.savedSceneName);
            }
            else
            {
                Debug.LogWarning("У збереженні немає назви сцени! Запускаємо перший рівень.");
                SceneManager.LoadScene("Level Generator");
            }
        }
        else
        {
            Debug.Log("Файлу збереження немає. Починаємо нову гру.");
            SceneManager.LoadScene("Level Generator");
        }
    }

    public void ResetProgress()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("<color=red>Прогрес гри повністю очищено!</color>");
        }
        else
        {
            Debug.Log("Збережень і так немає.");
        }
    }
}