using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    [Header("яке меню в≥дкривати/закривати?")]
    public GameObject menuPanel; // ѕерет€гни сюди свою панель меню

    // ÷ей метод в≥шаЇмо на кнопку
    public void ToggleMenu()
    {
        if (menuPanel != null)
        {
            // Ѕеремо поточний стан меню (true або false) ≥ ставимо протилежний (!)
            bool currentState = menuPanel.activeSelf;
            menuPanel.SetActive(!currentState);
        }
        else
        {
            Debug.LogWarning("“и забув перет€гнути панель меню в ≥нспектор!");
        }
    }
}