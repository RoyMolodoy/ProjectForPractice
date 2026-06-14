using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class LevelRewardManager : MonoBehaviour
{
    public static LevelRewardManager Instance;

    [Header("UI Налаштування")]
    public GameObject rewardPanel;
    public GameObject endPanel;
    public Button[] skillButtons;
    public TextMeshProUGUI[] skillNamesTexts;
    public TextMeshProUGUI[] skillDescTexts;

    [Header("Списки скілів")]
    public List<SkillData> commonSkills;
    public List<SkillData> legendarySkills;

    [Header("Налаштування")]
    [Range(0, 100)]
    public int legendaryChance = 10;

    [Header("Анімація UI")]
    public float fadeDuration = 0.3f;

    public TextMeshProUGUI Defence;
    public TextMeshProUGUI Damage;
    public TextMeshProUGUI Health;

    public bool isPanelActive = false;

    private List<SkillData> currentChoices;

    // Два окремі CanvasGroup для кожної панелі
    private CanvasGroup rewardCanvasGroup;
    private CanvasGroup endCanvasGroup;

    private Dictionary<SkillType, Action<GameObject, SkillData>> skillFunctions;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

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
        // Налаштовуємо панель нагород
        if (rewardPanel != null)
        {
            rewardCanvasGroup = rewardPanel.GetComponent<CanvasGroup>();
            if (rewardCanvasGroup == null)
            {
                rewardCanvasGroup = rewardPanel.AddComponent<CanvasGroup>();
            }
            rewardPanel.SetActive(false);
        }

        // Налаштовуємо фінальну панель
        if (endPanel != null)
        {
            endCanvasGroup = endPanel.GetComponent<CanvasGroup>();
            if (endCanvasGroup == null)
            {
                endCanvasGroup = endPanel.AddComponent<CanvasGroup>();
            }
            endCanvasGroup.alpha = 0f; // Робимо прозорою на старті
            endPanel.SetActive(false);
        }
    }

    public void ShowRewards()
    {
        if (rewardPanel == null) return;

        TogglePlayerScripts(false);
        isPanelActive = true;

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
        rewardCanvasGroup.interactable = false;
        rewardCanvasGroup.blocksRaycasts = false;

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
        rewardCanvasGroup.alpha = 0f;

        rewardCanvasGroup.interactable = true;
        rewardCanvasGroup.blocksRaycasts = true;

        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            // Змінюємо ТІЛЬКИ прозорість, масштаб не чіпаємо
            rewardCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        rewardCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutPanel()
    {
        float elapsed = 0f;

        // 1. Плавно ховаємо панель нагород (тільки прозорість)
        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            rewardCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        rewardPanel.SetActive(false);

        // 2. Плавно проявляємо фінальну панель (endPanel)
        if (endPanel != null)
        {
            endPanel.SetActive(true);
            endCanvasGroup.interactable = true;
            endCanvasGroup.blocksRaycasts = true;

            endCanvasGroup.alpha = 0f;

            elapsed = 0f; // Скидаємо таймер для нової анімації

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                // Змінюємо ТІЛЬКИ прозорість
                endCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }

            endCanvasGroup.alpha = 1f;
        }

        isPanelActive = false;

        yield return null;

        TogglePlayerScripts(true);
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

    private void ApplySkillEffect(SkillData skill)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Гравець не знайдений! Перевір тег 'Player'.");
            return;
        }

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