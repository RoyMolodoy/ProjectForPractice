using UnityEngine;
using TMPro;
using System;

public class KeybindManager : MonoBehaviour
{
    [Header("Посилання на Гравця (необов'язково)")]
    public PlayerMovement player; // Якщо гравець на сцені, ми одразу оновимо йому кнопки

    [Header("Тексти на кнопках (TMP)")]
    public TextMeshProUGUI leftKeyText;
    public TextMeshProUGUI rightKeyText;
    public TextMeshProUGUI jumpKeyText;
    public TextMeshProUGUI dashKeyText;

    // Змінні для збереження поточних налаштувань
    private KeyCode leftKey;
    private KeyCode rightKey;
    private KeyCode jumpKey;
    private KeyCode dashKey;

    // Змінні для логіки "очікування натискання"
    private bool isWaitingForInput = false;
    private string actionToRebind = "";

    void Start()
    {
        // При старті завантажуємо збережені кнопки (або ставимо стандартні, якщо гра запущена вперше)
        LoadKeys();
        UpdateUI();
    }

    // Цей метод ловить БУДЬ-ЯКЕ натискання клавіатури
    void OnGUI()
    {
        if (isWaitingForInput && Event.current.isKey && Event.current.type == EventType.KeyDown)
        {
            KeyCode newKey = Event.current.keyCode;

            // Ігноруємо натискання Escape, щоб не зламати меню
            if (newKey != KeyCode.None && newKey != KeyCode.Escape)
            {
                AssignNewKey(newKey);
                isWaitingForInput = false; // Вимикаємо режим очікування
            }
        }
    }

    // Цей метод ми будемо вішати на UI-кнопки
    public void StartRebind(string actionName)
    {
        actionToRebind = actionName;
        isWaitingForInput = true;

        // Змінюємо текст на кнопці, щоб гравець зрозумів, що треба щось натиснути
        switch (actionName)
        {
            case "Left": leftKeyText.text = "..."; break;
            case "Right": rightKeyText.text = "..."; break;
            case "Jump": jumpKeyText.text = "..."; break;
            case "Dash": dashKeyText.text = "..."; break;
        }
    }

    private void AssignNewKey(KeyCode newKey)
    {
        // Призначаємо нову кнопку залежно від того, що ми зараз змінюємо
        switch (actionToRebind)
        {
            case "Left": leftKey = newKey; break;
            case "Right": rightKey = newKey; break;
            case "Jump": jumpKey = newKey; break;
            case "Dash": dashKey = newKey; break;
        }

        SaveKeys();
        UpdateUI();

        // Якщо гравець зараз на сцені - оновлюємо скрипт миттєво
        if (player != null)
        {
            player.leftKey = leftKey;
            player.rightKey = rightKey;
            player.jumpKey = jumpKey;
            player.dashKey = dashKey;
        }
    }

    private void SaveKeys()
    {
        // Зберігаємо клавіші в пам'ять комп'ютера як текст (string)
        PlayerPrefs.SetString("LeftKey", leftKey.ToString());
        PlayerPrefs.SetString("RightKey", rightKey.ToString());
        PlayerPrefs.SetString("JumpKey", jumpKey.ToString());
        PlayerPrefs.SetString("DashKey", dashKey.ToString());
        PlayerPrefs.Save();
    }

    private void LoadKeys()
    {
        // Читаємо з пам'яті. Якщо збережень ще немає, ставимо (A, D, Space, LeftShift)
        leftKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LeftKey", "A"));
        rightKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("RightKey", "D"));
        jumpKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("JumpKey", "Space"));
        dashKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("DashKey", "LeftShift"));
    }

    private void UpdateUI()
    {
        // Оновлюємо текст на кнопках, щоб він показував актуальні клавіші
        if (leftKeyText != null) leftKeyText.text = leftKey.ToString();
        if (rightKeyText != null) rightKeyText.text = rightKey.ToString();
        if (jumpKeyText != null) jumpKeyText.text = jumpKey.ToString();
        if (dashKeyText != null) dashKeyText.text = dashKey.ToString();
    }
}