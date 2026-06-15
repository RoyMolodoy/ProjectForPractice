using UnityEngine;

public class EnemyUpgrade : MonoBehaviour
{
    public static EnemyUpgrade Instance;

    public LevelSystem levelSystem;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void UpgradeAllEnemies()
    {
        HPSystem[] allEnemies = FindObjectsOfType<HPSystem>();

        foreach (HPSystem enemyHP in allEnemies)
        {
            if (enemyHP.gameObject.CompareTag("Enemy") && !enemyHP.isBoss)
            {
                enemyHP.MaxHP = enemyHP.MaxHP * levelSystem.LevelNumber;
                enemyHP.HP = enemyHP.MaxHP;

                MobAI mobAI = enemyHP.GetComponent<MobAI>();
                if (mobAI != null)
                {
                    mobAI.damage = mobAI.damage + levelSystem.LevelNumber;
                }
            }
            if (enemyHP.gameObject.CompareTag("Enemy") && enemyHP.isBoss)
            {
                enemyHP.MaxHP = enemyHP.MaxHP * levelSystem.LevelNumber;
                enemyHP.HP = enemyHP.MaxHP;

                BossAI bossAI = enemyHP.GetComponent<BossAI>();
                if (bossAI != null)
                {
                    bossAI.beamDamage = bossAI.beamDamage + levelSystem.LevelNumber/2;
                    bossAI.headDamage = bossAI.headDamage + levelSystem.LevelNumber/2;
                }
            }
        }
    }
}