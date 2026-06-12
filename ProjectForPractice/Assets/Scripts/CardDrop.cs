using UnityEngine;

public class CardDrop : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Перевіряємо, чи підібрав гравець
        if (collision.CompareTag("Player"))
        {
            // Викликаємо панель з вибором карток
            if (LevelRewardManager.Instance != null)
            {
                LevelRewardManager.Instance.ShowRewards();
            }
            else
            {
                Debug.LogError("На сцені немає об'єкта з LevelRewardManager!");
            }

            // Видаляємо предмет з рівня
            Destroy(gameObject);
        }
    }
}