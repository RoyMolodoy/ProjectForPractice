using UnityEngine;

public class CardDrop : MonoBehaviour
{
    bool isUsed = false;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Перевіряємо, чи підібрав гравець
        if (collision.CompareTag("Player"))
        {
            // Викликаємо панель з вибором карток
            if (LevelRewardManager.Instance != null && !isUsed)
            {
                LevelRewardManager.Instance.ShowRewards();
                isUsed = true;
            }
        }
    }
}