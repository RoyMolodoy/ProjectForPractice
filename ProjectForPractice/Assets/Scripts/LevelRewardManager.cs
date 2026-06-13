using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelRewardManager : MonoBehaviour
{
    public static LevelRewardManager Instance;

    [Header("UI Налаштування")]
    public GameObject rewardPanel;
    public Button[] skillButtons;
    public TextMeshProUGUI[] skillNamesTexts;
    public TextMeshProUGUI[] skillDescTexts;

    [Header("Списки скілів")]
    public List<SkillData> commonSkills;     // Звичайні картки
    public List<SkillData> legendarySkills;  // Легендарні картки

    [Header("Налаштування")]
    [Range(0, 100)]
    public int legendaryChance = 10; // Шанс легендарки (10%)

    [Header("Анімація UI")]
    public float fadeDuration = 0.3f; // Швидкість плавного з'явлення

    private List<SkillData> currentChoices;
    private CanvasGroup canvasGroup; // Компонент для керування прозорістю

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (rewardPanel != null)
        {
            // Автоматично шукаємо або додаємо CanvasGroup на панель
            canvasGroup = rewardPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = rewardPanel.AddComponent<CanvasGroup>();
            }

            rewardPanel.SetActive(false);
        }
    }

    public void ShowRewards()
    {
        if (rewardPanel == null) return;

        // Генеруємо списки...
        currentChoices = new List<SkillData>();
        List<SkillData> tempCommon = new List<SkillData>(commonSkills);
        List<SkillData> tempLegendary = new List<SkillData>(legendarySkills);

        for (int i = 0; i < 3; i++)
        {
            SkillData pickedSkill = null;

            int roll = Random.Range(0, 100);
            if (roll < legendaryChance && tempLegendary.Count > 0)
            {
                int r = Random.Range(0, tempLegendary.Count);
                pickedSkill = tempLegendary[r];
                tempLegendary.RemoveAt(r);
            }
            else if (tempCommon.Count > 0)
            {
                int r = Random.Range(0, tempCommon.Count);
                pickedSkill = tempCommon[r];
                tempCommon.RemoveAt(r);
            }

            if (pickedSkill != null)
            {
                currentChoices.Add(pickedSkill);

                if (skillNamesTexts[i] != null) skillNamesTexts[i].text = pickedSkill.skillName;
                if (skillDescTexts[i] != null) skillDescTexts[i].text = pickedSkill.description;

                int buttonIndex = currentChoices.Count - 1;
                skillButtons[i].onClick.RemoveAllListeners();
                skillButtons[i].onClick.AddListener(() => ChooseSkill(buttonIndex));
                skillButtons[i].gameObject.SetActive(true);
            }
            else
            {
                skillButtons[i].gameObject.SetActive(false);
            }
        }

        // Вмикаємо панель, зупиняємо час і запускаємо анімацію
        rewardPanel.SetActive(true);
        Time.timeScale = 0f;
        StartCoroutine(FadeInPanel());
    }

    public void ChooseSkill(int index)
    {
        // Блокуємо кнопки, щоб гравець не міг клікнути двічі під час зникнення
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        SkillData chosenSkill = currentChoices[index];
        ApplySkillEffect(chosenSkill);

        if (chosenSkill.rarity == SkillRarity.Legendary)
        {
            legendarySkills.Remove(chosenSkill);
        }

        // Запускаємо плавне зникнення
        StartCoroutine(FadeOutPanel());
    }

    // --- КОРУТИНИ ДЛЯ ПЛАВНОСТІ ---

    private IEnumerator FadeInPanel()
    {
        // Робимо панель повністю прозорою і трохи меншою
        canvasGroup.alpha = 0f;
        rewardPanel.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            // Використовуємо unscaledDeltaTime, бо звичайний час зупинено!
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            // Плавно збільшуємо прозорість та масштаб
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            float scale = Mathf.Lerp(0.8f, 1f, t);
            rewardPanel.transform.localScale = new Vector3(scale, scale, 1f);

            yield return null; // Чекаємо наступного кадру
        }

        // Гарантуємо, що в кінці значення ідеальні
        canvasGroup.alpha = 1f;
        rewardPanel.transform.localScale = Vector3.one;
    }

    private IEnumerator FadeOutPanel()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            // Плавно зменшуємо прозорість та масштаб назад
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            float scale = Mathf.Lerp(1f, 0.8f, t);
            rewardPanel.transform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        // Коли анімація закінчилась — ховаємо панель і ВІДНОВЛЮЄМО ЧАС
        rewardPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // --- ТУТ ЗАСТОСОВУЮТЬСЯ ЕФЕКТИ ---
    private void ApplySkillEffect(SkillData skill)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Гравець не знайдений! Перевір тег 'Player'.");
            return;
        }

        switch (skill.type)
        {
            case SkillType.HealFull:
                var hpSystemFull = player.GetComponent<HPSystem>();
                if (hpSystemFull != null) hpSystemFull.PlusHP(999);
                break;

            case SkillType.MaxHPUp:
                var hpSystemMax = player.GetComponent<HPSystem>();
                if (hpSystemMax != null)
                {
                    hpSystemMax.MaxHP += skill.value;
                    hpSystemMax.HP += skill.value;

                    if (hpSystemMax.HPBar != null)
                        hpSystemMax.HPBar.fillAmount = (float)hpSystemMax.HP / hpSystemMax.MaxHP;
                }
                break;

            case SkillType.DefenseUp:
                var hpSysDef = player.GetComponent<HPSystem>();
                if (hpSysDef != null) hpSysDef.defense += skill.value;
                break;

            case SkillType.DamageUp:
                var attackScript = player.GetComponent<PlayerAttack>();
                if (attackScript != null) attackScript.attackDamage += (int)skill.value;
                break;

            case SkillType.DashUnlock:
                var moveScriptDash = player.GetComponent<PlayerMovement>();
                if (moveScriptDash != null) moveScriptDash.canDash = true;
                break;

            case SkillType.DoubleJumpUnlock:
                var moveScriptJump = player.GetComponent<PlayerMovement>();
                if (moveScriptJump != null) moveScriptJump.canDoubleJump = true;
                break;
        }

        Debug.Log($"<color=green>Вибрано скіл:</color> {skill.skillName}");
    }
}