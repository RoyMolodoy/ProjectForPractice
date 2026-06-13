using UnityEngine;

public class GameExit : MonoBehaviour
{
    // Цей метод ми повісимо на кнопку
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}