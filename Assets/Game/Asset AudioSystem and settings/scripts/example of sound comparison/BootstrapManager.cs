using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{
    [Tooltip("Назва сцени, яка завантажиться після ініціалізації (наприклад, 'GameScene').")]
    public string firstSceneToLoad = "GameScene"; 

    [Header("Main Game Music")]
    [Tooltip("Основна музика для гри. Призначте MusicData асет сюди.")]
    public MusicData mainGameplayMusic; // <-- НОВЕ ПОЛЕ

    public bool loadScene = true;

    void Start()
    {
        // Перевіряємо, чи SoundManager вже ініціалізований.
        // Це важливо, бо SoundManager може ініціалізуватися трохи пізніше в тому ж Awake/Start циклі.
        // Дамо йому трохи часу або перевіримо Instance.
        if (SoundManager.Instance == null)
        {
            Debug.LogError("BootstrapManager: SoundManager.Instance не знайдено при старті. Музика не буде відтворена.");
            // Можна додати yield return new WaitForSeconds(0.1f); і спробувати ще раз,
            // але в більшості випадків, якщо SoundManager теж в Bootstrap сцені, він буде ініціалізований.
        }
        else if (mainGameplayMusic != null)
        {
            // Перевіряємо, чи музика вже не грає або чи не грає вже цей конкретний трек.
            // Це для уникнення перезапуску, якщо гра повертається до Bootstrap з якихось причин.
            if (!SoundManager.Instance.musicAudioSource.isPlaying || SoundManager.Instance.musicAudioSource.clip != mainGameplayMusic.clip)
            {
                SoundManager.Instance.PlayMusic(mainGameplayMusic);
                Debug.Log($"BootstrapManager: Початок відтворення основної музики: {mainGameplayMusic.name}");
            }
            else
            {
                Debug.Log($"BootstrapManager: Основна музика {mainGameplayMusic.name} вже грає.");
            }
        }
        else
        {
            Debug.LogWarning("BootstrapManager: Основна музика для гри не призначена.");
        }

        // Завантажуємо першу сцену гри.
        // Це має відбутися після спроби запустити музику.
        // if (loadScene != true)
        // {
        //     SceneManager.LoadScene(firstSceneToLoad);
        // }
    }
}