using UnityEngine;
using TMPro;
using System;

public class KeybindManager : MonoBehaviour
{
    public PlayerMovement player;

    [Header("“ексти на кнопках (TMP)")]
    public TextMeshProUGUI leftKeyText;
    public TextMeshProUGUI rightKeyText;
    public TextMeshProUGUI jumpKeyText;
    public TextMeshProUGUI dashKeyText;

    // «м≥нн≥ дл€ збереженн€ поточних налаштувань
    private KeyCode leftKey;
    private KeyCode rightKey;
    private KeyCode jumpKey;
    private KeyCode dashKey;

    // «м≥нн≥ дл€ лог≥ки "оч≥куванн€ натисканн€"
    private bool isWaitingForInput = false;
    private string actionToRebind = "";

    void Start()
    {
        // ѕри старт≥ завантажуЇмо збережен≥ кнопки
        LoadKeys();
        UpdateUI();

        // ?? Ќќ¬≈: якщо гравець Ї на сцен≥ разом з меню паузи, одразу передаЇмо йому ц≥ кнопки
        if (player != null)
        {
            player.leftKey = leftKey;
            player.rightKey = rightKey;
            player.jumpKey = jumpKey;
            player.dashKey = dashKey;
        }
    }


    // ÷ей метод ловить Ѕ”ƒ№-я ≈ натисканн€ клав≥атури
    void OnGUI()
    {
        if (isWaitingForInput && Event.current.isKey && Event.current.type == EventType.KeyDown)
        {
            KeyCode newKey = Event.current.keyCode;

            // ≤гноруЇмо натисканн€ Escape, щоб не зламати меню
            if (newKey != KeyCode.None && newKey != KeyCode.Escape)
            {
                AssignNewKey(newKey);
                isWaitingForInput = false; // ¬имикаЇмо режим оч≥куванн€
            }
        }
    }

    // ÷ей метод ми будемо в≥шати на UI-кнопки
    public void StartRebind(string actionName)
    {
        actionToRebind = actionName;
        isWaitingForInput = true;

        // «м≥нюЇмо текст на кнопц≥, щоб гравець зрозум≥в, що треба щось натиснути
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
        // ѕризначаЇмо нову кнопку залежно в≥д того, що ми зараз зм≥нюЇмо
        switch (actionToRebind)
        {
            case "Left": leftKey = newKey; break;
            case "Right": rightKey = newKey; break;
            case "Jump": jumpKey = newKey; break;
            case "Dash": dashKey = newKey; break;
        }

        SaveKeys();
        UpdateUI();

        // якщо гравець зараз на сцен≥ - оновлюЇмо скрипт миттЇво
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
        // «бер≥гаЇмо клав≥ш≥ в пам'€ть комп'ютера €к текст (string)
        PlayerPrefs.SetString("LeftKey", leftKey.ToString());
        PlayerPrefs.SetString("RightKey", rightKey.ToString());
        PlayerPrefs.SetString("JumpKey", jumpKey.ToString());
        PlayerPrefs.SetString("DashKey", dashKey.ToString());
        PlayerPrefs.Save();
    }

    private void LoadKeys()
    {
        // „итаЇмо з пам'€т≥. якщо збережень ще немаЇ, ставимо (A, D, Space, LeftShift)
        leftKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("LeftKey", "A"));
        rightKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("RightKey", "D"));
        jumpKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("JumpKey", "Space"));
        dashKey = (KeyCode)Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("DashKey", "LeftShift"));
    }

    private void UpdateUI()
    {
        // ќновлюЇмо текст на кнопках, щоб в≥н показував актуальн≥ клав≥ш≥
        if (leftKeyText != null) leftKeyText.text = leftKey.ToString();
        if (rightKeyText != null) rightKeyText.text = rightKey.ToString();
        if (jumpKeyText != null) jumpKeyText.text = jumpKey.ToString();
        if (dashKeyText != null) dashKeyText.text = dashKey.ToString();
    }
}