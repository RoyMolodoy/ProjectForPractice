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
        if (rewardPanel != null)
        {
            rewardCanvasGroup = rewardPanel.GetComponent<CanvasGroup>();
            if (rewardCanvasGroup == null)
            {
                rewardCanvasGroup = rewardPanel.AddComponent<CanvasGroup>();
            }
            rewardPanel.SetActive(false);
        }

        if (endPanel != null)
        {
            endCanvasGroup = endPanel.GetComponent<CanvasGroup>();
            if (endCanvasGroup == null)
            {
                endCanvasGroup = endPanel.AddComponent<CanvasGroup>();
            }
            endCanvasGroup.alpha = 0f;
            endPanel.SetActive(false);
        }
    }

    public void ShowRewards()
    {
        if (rewardPanel == null) return;

        TogglePlayerScripts(false);
        isPanelActive = true;

        // 1. Знаходимо гравця, щоб перевірити його поточні навички
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        PlayerMovement playerMovement = playerObj != null ? playerObj.GetComponent<PlayerMovement>() : null;

        currentChoices = new List<SkillData>();

        // 2. Фільтруємо списки: додаємо тільки ті скіли, яких у гравця ЩЕ НЕМАЄ
        List<SkillData> tempCommon = new List<SkillData>();
        foreach (var skill in commonSkills)
        {
            if (!IsSkillAlreadyUnlocked(skill, playerMovement))
            {
                tempCommon.Add(skill);
            }
        }

        List<SkillData> tempLegendary = new List<SkillData>();
        foreach (var skill in legendarySkills)
        {
            if (!IsSkillAlreadyUnlocked(skill, playerMovement))
            {
                tempLegendary.Add(skill);
            }
        }

        // 3. Генеруємо 3 картки з уже відфільтрованих списків
        for (int i = 0; i < 3; i++)
        {
            SkillData pickedSkill = null;

            int roll = UnityEngine.Random.Range(0, 100);

            if (roll < legendaryChance && tempLegendary.Count > 0)
            {
                int r = UnityEngine.Random.Range(0, tempLegendary.Count);
                pickedSkill = tempLegendary[r];
                tempLegendary.RemoveAt(r);

                // Ми залишаємо це тут для інших легендарок (наприклад, +50 ХП),
                // щоб вони не випадали двічі за один забіг.
                legendarySkills.Remove(pickedSkill);
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

    // 🔥 НОВИЙ МЕТОД-ФІЛЬТР
    private bool IsSkillAlreadyUnlocked(SkillData skill, PlayerMovement pm)
    {
        if (pm == null) return false;

        // Перевіряємо, чи цей скіл уже відкритий у гравця
        if (skill.type == SkillType.DashUnlock && pm.canDash)
            return true;

        if (skill.type == SkillType.DoubleJumpUnlock && pm.canDoubleJump)
            return true;

        // Якщо в майбутньому додаш інші унікальні навички (наприклад, canBlock),
        // просто додай перевірку сюди!

        return false;
    }

    public void ChooseSkill(int index)
    {
        rewardCanvasGroup.interactable = false;
        rewardCanvasGroup.blocksRaycasts = false;

        SkillData chosenSkill = currentChoices[index];
        ApplySkillEffect(chosenSkill);

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
            rewardCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        rewardCanvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutPanel()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            rewardCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            yield return null;
        }

        rewardPanel.SetActive(false);

        if (endPanel != null)
        {
            endPanel.SetActive(true);
            endCanvasGroup.interactable = true;
            endCanvasGroup.blocksRaycasts = true;

            endCanvasGroup.alpha = 0f;

            elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                endCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }

            endCanvasGroup.alpha = 1f;
        }

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
        if (player == null) return;

        if (skillFunctions.ContainsKey(skill.type))
        {
            skillFunctions[skill.type].Invoke(player, skill);
        }
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
            if (Health != null) Health.text = $"{hpSystemMax.MaxHP}";

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
            if (Defence != null) Defence.text = $"{hpSysDef.defense}";
        }
    }

    private void ApplyDamageUp(GameObject player, SkillData skill)
    {
        var attackScript = player.GetComponent<PlayerAttack>();
        if (attackScript != null)
        {
            attackScript.attackDamage += (int)skill.value;
            if (Damage != null) Damage.text = $"{attackScript.attackDamage}";
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