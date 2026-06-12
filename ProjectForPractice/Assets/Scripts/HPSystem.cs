using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPSystem : MonoBehaviour
{
    public int HP = 3;
    public int MaxHP = 3;
    private AnimsController animsController;

    private void Start()
    {
        animsController = GetComponent<AnimsController>();
    }
    void MinusHP(int minusHP)
    {
        HP -= minusHP;
        if (animsController != null)
        {
            if(HP <= 0)
            {
                if(HP + minusHP > 0)
                    animsController.DeathAnim();
            }
            else
            {
                //animsController.HurtAnim();
            }
        }
        else
        {
            if (HP <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
    void PlusHP(int plusHP)
    {
        if (HP < MaxHP)
        {
            HP += plusHP;
        }
    }
}
