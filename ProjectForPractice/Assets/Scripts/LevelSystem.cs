using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    public GameObject BossHPBar;
    public int LevelNumber = 1;
    void Start()
    {
        BossHPBar = GameObject.FindGameObjectWithTag("BossHPObject");
        if (LevelNumber % 5 != 0)
        {
            if (BossHPBar != null)
                BossHPBar.SetActive(false);
        }
    }

    public void PlusLevel()
    {
        LevelNumber++;
    }

    public void ChangeLevel(int lvlnum)
    {
        LevelNumber = lvlnum;
    }
}
