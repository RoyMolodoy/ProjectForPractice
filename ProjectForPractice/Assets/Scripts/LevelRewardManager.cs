using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System; // Обов'язково додаємо для Action

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

    public TextMeshProUGUI Defence;
    public TextMeshProUGUI Damage;
    public TextMeshProUGUI Health;

    private List<SkillData> currentChoices;
    private CanvasGroup canvasGroup; // Компонент для керування прозорістю

    // Словник, який зберігає зв'язок між типом скіла і відповідною функцією
    private Dictionary<SkillType, Action<GameObject, SkillData>> skillFunctions;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // Реєструємо наші функції у словнику при старті
        skillFunctions = new Dictionary<SkillType, Action<GameObject, SkillData>>
        {
            { SkillType.HealFull, ApplyHealFull },
            { SkillType.MaxHPUp, ApplyMaxHPUp },
            { SkillType.DefenseUp, ApplyDefenseUp },
            { SkillType.DamageUp, ApplyDamageUp },
            { SkillType.DashUnlock, ApplyDashUnlock },
            { SkillType.DoubleJumpUnlock, ApplyDoubleJumpUnlock }
        };
    }

    private void Start()
    {
        if (rewardPanel != null)
        {
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

        currentChoices = new List<SkillData>();
        List<SkillData> tempCommon = new List<SkillData>(commonSkills);
        List<SkillData> tempLegendary = new List<SkillData>(legendarySkills);

        for (int i = 0; i < 3; i++)
        {
            SkillData pickedSkill = null;

            int roll = UnityEngine.Random.Range(0, 100);
            if (roll < legendaryChance && tempLegendary.Count > 0)
            {
                int r = UnityEngine.Random.Range(0, tempLegendary.Count);
                pickedSkill = tempLegendary[r];
                tempLegendary.RemoveAt(r);
            }
            else if (tempCommon.Count > 0)
            {
                int r = UnityEngine.Random.Range(0, tempCommon.Count);
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

        rewardPanel.SetActive(true);
        Time.timeScale = 0f;
        StartCoroutine(FadeInPanel());
    }

    public void ChooseSkill(int index)
    {
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        SkillData chosenSkill = currentChoices[index];
        ApplySkillEffect(chosenSkill);

        if (chosenSkill.rarity == SkillRarity.Legendary)
        {
            legendarySkills.Remove(chosenSkill);
        }

        StartCoroutine(FadeOutPanel());
    }

    private IEnumerator FadeInPanel()
    {
        canvasGroup.alpha = 0f;
        rewardPanel.transform.localScale = new Vector3(0.8f, 0.8f, 1f);

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;

            canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            float scale = Mathf.Lerp(0.8f, 1f, t);
            rewardPanel.transform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

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

            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            float scale = Mathf.Lerp(1f, 0.8f, t);
            rewardPanel.transform.localScale = new Vector3(scale, scale, 1f);

            yield return null;
        }

        rewardPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    // --- ОНОВЛЕНИЙ МЕТОД ЗАСТОСУВАННЯ ЕФЕКТУ ---
    private void ApplySkillEffect(SkillData skill)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Гравець не знайдений! Перевір тег 'Player'.");
            return;
        }

        // Шукаємо функцію у словнику і відразу виконуємо її
        if (skillFunctions.ContainsKey(skill.type))
        {
            skillFunctions[skill.type].Invoke(player, skill);
        }
        else
        {
            Debug.LogWarning($"Для скіла {skill.type} ще не створено функції!");
        }

        Debug.Log($"<color=green>Вибрано скіл:</color> {skill.skillName}");
    }

    // =========================================================
    // --- ОКРЕМІ ФУНКЦІЇ ДЛЯ КОЖНОГО СКІЛА ---
    // =========================================================

    private void ApplyHealFull(GameObject player, SkillData skill)
    {
        var hpSystemFull = player.GetComponent<HPSystem>();
        if (hpSystemFull != null) hpSystemFull.PlusHP(999);
    }

    private void ApplyMaxHPUp(GameObject player, SkillData skill)
    {
        var hpSystemMax = player.GetComponent<HPSystem>();
        if (hpSystemMax != null)
        {
            hpSystemMax.MaxHP += skill.value;
            hpSystemMax.HP += skill.value;
            Health.text = $"{hpSystemMax.MaxHP}";

            if (hpSystemMax.HPBar != null)
                hpSystemMax.HPBar.fillAmount = (float)hpSystemMax.HP / hpSystemMax.MaxHP;
        }
    }

    private void ApplyDefenseUp(GameObject player, SkillData skill)
    {
        var hpSysDef = player.GetComponent<HPSystem>();
        if (hpSysDef != null)
        {
            hpSysDef.defense += skill.value;
            Defence.text = $"{hpSysDef.defense}";
        }
    }

    private void ApplyDamageUp(GameObject player, SkillData skill)
    {
        var attackScript = player.GetComponent<PlayerAttack>();
        if (attackScript != null)
        {
            attackScript.attackDamage += (int)skill.value;
            Damage.text = $"{attackScript.attackDamage}";
        }
    }

    private void ApplyDashUnlock(GameObject player, SkillData skill)
    {
        var moveScriptDash = player.GetComponent<PlayerMovement>();
        if (moveScriptDash != null) moveScriptDash.canDash = true;
    }

    private void ApplyDoubleJumpUnlock(GameObject player, SkillData skill)
    {
        var moveScriptJump = player.GetComponent<PlayerMovement>();
        if (moveScriptJump != null) moveScriptJump.canDoubleJump = true;
    }
}