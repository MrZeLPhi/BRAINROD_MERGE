using UnityEngine;

public class GameActions : MonoBehaviour
{
    [Header("Sound References")]
    [Tooltip("Вибух SFX Data (перетягніть SFX_Explosion ассет сюди).")]
    public SFXData explosionSFX; 
    [Tooltip("Звук кліку меню (перетягніть SFX_Click ассет сюди).")]
    public SFXData clickSFX;
    [Tooltip("Музика для гри (перетягніть Music_Gameplay ассет сюди).")]
    public MusicData gameplayMusic;

    void Start()
    {
        // Перевіряємо, чи SoundManager готовий
        if (SoundManager.Instance == null)
        {
            Debug.LogError("SoundManager is not found! Make sure it's set up in the Bootstrap scene.");
            return;
        }

        // Відтворюємо музику при старті сцени
        if (gameplayMusic != null)
        {
            SoundManager.Instance.PlayMusic(gameplayMusic);
        }
    }

    public void OnPlayerExplodes()
    {
        // Відтворюємо SFX вибуху в позиції об'єкта
        if (explosionSFX != null)
        {
            SoundManager.Instance.PlaySFX(explosionSFX, transform.position);
        }
    }

    public void OnButtonClick()
    {
        // Відтворюємо звук кліку меню
        if (clickSFX != null)
        {
            SoundManager.Instance.PlayMenuSound(clickSFX);
        }
    }

    // Приклад виклику корутини з цього класу (для затримки або складних послідовностей)
    public void PlaySoundSequenceWithDelay()
    {
        if (SoundManager.Instance != null && clickSFX != null)
        {
            StartCoroutine(DelayedSoundSequence());
        }
    }

    private System.Collections.IEnumerator DelayedSoundSequence()
    {
        SoundManager.Instance.PlaySFX(clickSFX); // Перший клік
        yield return new WaitForSeconds(0.5f); // Затримка 0.5 секунди
        SoundManager.Instance.PlaySFX(clickSFX); // Другий клік
        yield return new WaitForSeconds(0.5f);
        SoundManager.Instance.PlaySFX(explosionSFX); // Вибух
    }
}