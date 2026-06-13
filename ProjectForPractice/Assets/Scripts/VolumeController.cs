using UnityEngine;
using UnityEngine.UI; // Обов'язково для роботи з UI

public class VolumeController : MonoBehaviour
{
    [Header("Посилання на твій Slider")]
    public Slider volumeSlider;

    void Start()
    {
        // 1. Завантажуємо збережену гучність. 
        // Якщо гра запущена вперше, ставимо максимум (1.0f)
        float savedVolume = PlayerPrefs.GetFloat("GameVolume", 1f);

        // 2. Встановлюємо цю гучність для гри
        AudioListener.volume = savedVolume;

        // 3. Встановлюємо повзунок у правильне положення
        if (volumeSlider != null)
        {
            volumeSlider.value = savedVolume;

            // 4. Підписуємо скрипт на подію: коли гравець рухає повзунок, викликаємо ChangeVolume
            volumeSlider.onValueChanged.AddListener(ChangeVolume);
        }
    }

    // Цей метод автоматично отримує нове значення від Slider (від 0 до 1)
    public void ChangeVolume(float newValue)
    {
        // Міняємо гучність у грі
        AudioListener.volume = newValue;

        // Зберігаємо нове значення в пам'ять
        PlayerPrefs.SetFloat("GameVolume", newValue);
        PlayerPrefs.Save();
    }
}