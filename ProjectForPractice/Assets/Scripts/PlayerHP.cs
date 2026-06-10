using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    public int playerHP = 3;
    public int MaxHP = 3;

    void Start()
    {
        
    }

    void MinusHP(int minusHP)
    {
        playerHP -= minusHP;
        //временно
        if (playerHP <= 0)
        {
            Destroy(gameObject);
        }
    }
    void PlusHP(int plusHP)
    {
       if (playerHP < MaxHP)
        {
            playerHP += plusHP;
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            MinusHP(1);
        }
        if (collision.gameObject.CompareTag("Heal"))
        {
            PlusHP(1);
        }
    }
}
