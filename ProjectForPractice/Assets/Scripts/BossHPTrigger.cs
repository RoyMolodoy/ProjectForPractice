using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHPTrigger : MonoBehaviour
{
    public HPSystem hpSystem;
    public bool needToActiveHPBar = true;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (needToActiveHPBar) 
        {
            // ќбов'€зкова перев≥рка, що в зону зайшов саме √–ј¬≈÷№
            if (collision.CompareTag("Player"))
            {
                if (hpSystem.BossHPBar != null)
                {
                    hpSystem.BossHPBar.SetActive(true);
                }
            }
        }
    }
}