using System.Collections;
using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    [Header("UI Налаштування")]
    public GameObject menuPanel;
    public bool needToPause = true;
    public float fadeDuration = 0.2f;

    [Header("Зв'язок з іншим UI")]
    public LevelRewardManager RM;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    private bool isMenuOpen = false;

    private void Start()
    {
        if (menuPanel != null)
        {
            canvasGroup = menuPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = menuPanel.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = 0f;
            menuPanel.SetActive(false);
            isMenuOpen = false;
        }
    }

    public void ToggleMenu()
    {
        if (menuPanel == null) return;

        isMenuOpen = !isMenuOpen;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);

        if (isMenuOpen)
        {
            fadeCoroutine = StartCoroutine(FadeIn());
        }
        else
        {
            fadeCoroutine = StartCoroutine(FadeOut());
        }
    }

    private IEnumerator FadeIn()
    {
        TogglePlayerScripts(false);

        menuPanel.SetActive(true);
        if (needToPause) Time.timeScale = 0f;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        menuPanel.SetActive(false);

        bool isRewardPanelOpen = RM != null && RM.isPanelActive;

        if (!isRewardPanelOpen)
        {
            if (needToPause) Time.timeScale = 1f;

            yield return null;

            TogglePlayerScripts(true);
        }
    }

    private void TogglePlayerScripts(bool isActive)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            PlayerAttack attack = player.GetComponent<PlayerAttack>();

            if (movement != null) movement.enabled = isActive;
            if (attack != null) attack.enabled = isActive;
        }
    }
}