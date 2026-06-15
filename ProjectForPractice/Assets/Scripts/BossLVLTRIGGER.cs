using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossLVLTRIGGER : MonoBehaviour
{
    void Start()
    {
        
    }
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerAttack pa = collision.GetComponent<PlayerAttack>();
            pa.attackAngle = 360f;
        }
    }
}
