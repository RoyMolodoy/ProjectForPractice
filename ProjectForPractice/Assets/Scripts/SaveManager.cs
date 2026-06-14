using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;
    private string savePath;

    public TextMeshProUGUI Health;
    public TextMeshProUGUI Defence;
    public TextMeshProUGUI Damage;

    private void Awake()
    {
        // Робимо так, щоб SaveManager НІКОЛИ не знищувався при перезавантаженні сцени
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        savePath = Application.persistentDataPath + "/savegame.json";
    }

    // --- ПІДПИСКА НА ПОДІЮ ЗАВАНТАЖЕННЯ СЦЕНИ ---
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Цей метод автоматично спрацює ЩОРАЗУ, коли сцена завантажується чи перезапускається
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        LoadGame();
    }

    public void SaveGame()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return;

        PlayerMovement movement = playerObj.GetComponent<PlayerMovement>();
        HPSystem hpSystem = playerObj.GetComponent<HPSystem>();
        PlayerAttack attack = playerObj.GetComponent<PlayerAttack>();

        if (movement == null || hpSystem == null || attack == null) return;

        PlayerSaveData data = new PlayerSaveData();
        data.savedSceneName = SceneManager.GetActiveScene().name;
        data.playerPosX = playerObj.transform.position.x;
        data.playerPosY = playerObj.transform.position.y;
        data.currentHP = hpSystem.HP;
        data.maxHP = hpSystem.MaxHP;
        data.defense = hpSystem.defense;
        data.damage = attack.attackDamage;
        data.canDash = movement.canDash;
        data.canDoubleJump = movement.canDoubleJump;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"<color=green>Чекпоінт збережено!</color>");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath)) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null) return; // Якщо гравця ще немає, перериваємось

        PlayerMovement movement = playerObj.GetComponent<PlayerMovement>();
        HPSystem hpSystem = playerObj.GetComponent<HPSystem>();
        PlayerAttack attack = playerObj.GetComponent<PlayerAttack>();

        if (movement == null || hpSystem == null || attack == null) return;

        string json = File.ReadAllText(savePath);
        PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

        playerObj.transform.position = new Vector2(data.playerPosX, data.playerPosY);

        hpSystem.MaxHP = data.maxHP;
        hpSystem.HP = data.currentHP;
        hpSystem.defense = data.defense;
        attack.attackDamage = data.damage;
        movement.canDash = data.canDash;
        movement.canDoubleJump = data.canDoubleJump;

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

    // --- ФІШКА ДЛЯ ТЕСТУВАННЯ В UNITY ---
    // Додає кнопку прямо в інспектор скрипта, щоб швидко видалити збереження!
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
    // --- МЕТОД ДЛЯ КНОПКИ В ГОЛОВНОМУ МЕНЮ ---
    public void ContinueGame()
    {
        if (File.Exists(savePath))
        {
            // Читаємо файл, щоб дізнатися, на якому рівні ми були
            string json = File.ReadAllText(savePath);
            PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(json);

            // Якщо назва сцени не порожня, завантажуємо її
            if (!string.IsNullOrEmpty(data.savedSceneName))
            {
                Debug.Log("Завантажуємо збережену сцену: " + data.savedSceneName);
                SceneManager.LoadScene(data.savedSceneName);
            }
            else
            {
                Debug.LogWarning("У збереженні немає назви сцени! Запускаємо перший рівень.");
                SceneManager.LoadScene("Pavlo'sScene");
            }
        }
        else
        {
            Debug.Log("Файлу збереження немає. Починаємо нову гру.");
            SceneManager.LoadScene("Pavlo'sScene");
        }
    }
    // --- МЕТОД ДЛЯ КНОПКИ "ОЧИСТИТИ ЗБЕРЕЖЕННЯ" (В НАЛАШТУВАННЯХ) ---
    public void ResetProgress()
    {
        // 1. Видаляємо файл прогресу (наш JSON)
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