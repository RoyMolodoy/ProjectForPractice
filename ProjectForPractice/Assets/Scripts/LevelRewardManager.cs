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

    private List<SkillData> currentChoices;

    private void Awake()
    {
        // Робимо скрипт доступним звідусіль (Singleton)
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }

    public void ShowRewards()
    {
        if (rewardPanel == null) return;

        rewardPanel.SetActive(true);
        Time.timeScale = 0f; // Зупиняємо гру

        currentChoices = new List<SkillData>();

        // Копіюємо списки
        List<SkillData> tempCommon = new List<SkillData>(commonSkills);
        List<SkillData> tempLegendary = new List<SkillData>(legendarySkills);

        for (int i = 0; i < 3; i++)
        {
            SkillData pickedSkill = null;

            // Логіка випадіння
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

            // Налаштування кнопок
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
    }

    public void ChooseSkill(int index)
    {
        SkillData chosenSkill = currentChoices[index];
        ApplySkillEffect(chosenSkill);

        // Якщо взяли легендарку - видаляємо її назавжди з гри, щоб не випала вдруге
        if (chosenSkill.rarity == SkillRarity.Legendary)
        {
            legendarySkills.Remove(chosenSkill);
        }

        // Ховаємо панель і відновлюємо час
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
            // ПОВНЕ ЛІКУВАННЯ
            case SkillType.HealFull:
                var hpSystemFull = player.GetComponent<HPSystem>();
                if (hpSystemFull != null)
                    hpSystemFull.PlusHP(999);
                break;

            // ЗБІЛЬШЕННЯ МАКСИМАЛЬНОГО ХП
            case SkillType.MaxHPUp:
                var hpSystemMax = player.GetComponent<HPSystem>();
                if (hpSystemMax != null)
                {
                    hpSystemMax.MaxHP += skill.value;
                    hpSystemMax.HP += skill.value; // Даємо це ХП одразу

                    // Оновлюємо UI смужку здоров'я
                    if (hpSystemMax.HPBar != null)
                        hpSystemMax.HPBar.fillAmount = (float)hpSystemMax.HP / hpSystemMax.MaxHP;
                }
                break;

            // ЗБІЛЬШЕННЯ ЗАХИСТУ
            case SkillType.DefenseUp:
                var hpSysDef = player.GetComponent<HPSystem>();
                if (hpSysDef != null) hpSysDef.defense += skill.value;
                break;

            // ЗБІЛЬШЕННЯ ШКОДИ
            case SkillType.DamageUp:
                var attackScript = player.GetComponent<PlayerAttack>();
                if (attackScript != null) attackScript.attackDamage += (int)skill.value;
                break;

            // ЛЕГЕНДАРКА: ДЕШ
            case SkillType.DashUnlock:
                var moveScriptDash = player.GetComponent<PlayerMovement>();
                if (moveScriptDash != null) moveScriptDash.canDash = true;
                break;

            // ЛЕГЕНДАРКА: ПОДВІЙНИЙ СТРИБОК
            case SkillType.DoubleJumpUnlock:
                var moveScriptJump = player.GetComponent<PlayerMovement>();
                if (moveScriptJump != null) moveScriptJump.canDoubleJump = true;
                break;
        }

        Debug.Log($"<color=green>Вибрано скіл:</color> {skill.skillName}");
    }
}