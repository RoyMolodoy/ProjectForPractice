using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "Game/Skill Data")]
public class SkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;
    public Sprite skillIcon;

    public SkillType type;
    public SkillRarity rarity;
    public int value;
}

public enum SkillRarity
{
    Common,    // Звичайні: +ХП, +Дамаг, +Захист
    Legendary  // Легендарні: Деш, Подвійний стрибок
}

public enum SkillType
{
    HealFull,
    MaxHPUp,
    DamageUp,
    DefenseUp,
    DashUnlock,
    DoubleJumpUnlock
}