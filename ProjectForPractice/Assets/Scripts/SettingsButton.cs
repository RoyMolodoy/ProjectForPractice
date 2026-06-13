using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    public GameObject menuPanel;

    public void ToggleMenu()
    {
        if (menuPanel != null)
        {
            bool currentState = menuPanel.activeSelf;

            if (!currentState)
            {
                Time.timeScale = 0f;
                menuPanel.SetActive(true);
            }
            else
            {
                Time.timeScale = 1f;
                menuPanel.SetActive(false);
            }
        }
    }
}