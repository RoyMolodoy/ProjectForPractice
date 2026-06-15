using System.Collections.Generic;

[System.Serializable]
public class PlayerSaveData
{
    public string savedSceneName;
    public float playerPosX;
    public float playerPosY;
    //public float currentHP;
    public float maxHP;
    public float defense;
    public int damage;
    public bool canDash;
    public bool canDoubleJump;
    public int currentLevel;

    public List<string> activatedCheckpoints = new List<string>();
}