using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    public int playerHP;
    public int MaxHP;

    void MinusHP(int minusHP)
    {
        playerHP -= minusHP;
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
