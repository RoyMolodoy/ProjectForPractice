using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    public int LevelNumber = 1;
    void Start()
    {

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
