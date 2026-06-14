using System.Collections;
using UnityEngine;

public class SettingsButton : MonoBehaviour
{
    [Header("UI Налаштування")]
    public GameObject menuPanel;
    public bool needToPause = true;
    public float fadeDuration = 0.2f; // Швидкість плавного з'явлення

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    private bool isMenuOpen = false;

    private void Start()
    {
        if (menuPanel != null)
        {
            // Автоматично додаємо CanvasGroup на панель меню, якщо його немає
            canvasGroup = menuPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = menuPanel.AddComponent<CanvasGroup>();
            }

            // Переконуємось, що меню вимкнене і прозоре при старті гри
            canvasGroup.alpha = 0f;
            menuPanel.SetActive(false);
            isMenuOpen = false;
        }
    }

    public void ToggleMenu()
    {
        if (menuPanel == null) return;

        // Перемикаємо логічний стан меню
        isMenuOpen = !isMenuOpen;

        // Якщо зараз вже йде якась анімація (наприклад, гравець дуже швидко клацає кнопку) - зупиняємо її
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

    // --- КОРУТИНИ ДЛЯ ПЛАВНОСТІ ---

    private IEnumerator FadeIn()
    {
        // 1. Одразу вмикаємо об'єкт і зупиняємо гру
        menuPanel.SetActive(true);
        if (needToPause) Time.timeScale = 0f;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        // 2. Плавно збільшуємо прозорість
        while (elapsed < fadeDuration)
        {
            // Обов'язково використовуємо unscaledDeltaTime, бо ігровий час (можливо) зупинено!
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, elapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOut()
    {
        // 1. Одразу блокуємо кліки по меню, щоб гравець не натиснув нічого під час зникнення
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        // 2. Плавно зменшуємо прозорість
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }

        // 3. Коли меню повністю зникло - відновлюємо час і ховаємо об'єкт
        canvasGroup.alpha = 0f;
        menuPanel.SetActive(false);

        if (needToPause) Time.timeScale = 1f;
    }
}