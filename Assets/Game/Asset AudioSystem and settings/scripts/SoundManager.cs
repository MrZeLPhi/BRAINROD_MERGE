using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections; // Для корутин

public class SoundManager : MonoBehaviour
{
    // Singleton інстанс
    public static SoundManager Instance { get; private set; }

    [Header("Audio Mixer Groups")]
    [Tooltip("Призначте вашу SFX групу з Audio Mixer.")]
    public AudioMixerGroup sfxMixerGroup;
    [Tooltip("Призначте вашу Music групу з Audio Mixer.")]
    public AudioMixerGroup musicMixerGroup;
    [Tooltip("Призначте вашу Menu Sound групу з Audio Mixer.")]
    public AudioMixerGroup menuSoundMixerGroup; // Якщо у вас є окрема група для звуків меню

    [Header("SFX AudioSource Pool")]
    [Tooltip("Кількість AudioSource у пулі для SFX.")]
    [Range(5, 20)] // Обмеження для зручності в інспекторі
    public int sfxPoolSize = 10;
    private List<AudioSource> sfxAudioSources;
    private int sfxPoolIndex = 0;

    [Header("Music AudioSource")]
    [Tooltip("AudioSource для відтворення музики. Переконайтеся, що він не відтворює SFX!")]
    public AudioSource musicAudioSource;

    [Header("Menu Sound AudioSource (Optional)")]
    [Tooltip("AudioSource для відтворення звуків меню (якщо окремий).")]
    public AudioSource menuAudioSource;

    // Посилання на SettingsManager (знаходимо програмно)
    private SettingsManager _settingsManager;

    void Awake()
    {
        // Singleton логіка
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Ініціалізація пулу AudioSource для SFX
        sfxAudioSources = new List<AudioSource>();
        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject child = new GameObject($"SFX_AudioSource_{i}");
            child.transform.SetParent(this.transform);
            AudioSource audioSource = child.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxMixerGroup; // Призначаємо до SFX групи
            audioSource.playOnAwake = false;
            sfxAudioSources.Add(audioSource);
        }

        // Перевірка musicAudioSource та menuAudioSource
        if (musicAudioSource == null)
        {
            Debug.LogWarning("SoundManager: Music AudioSource not assigned. Creating one automatically.");
            GameObject musicGo = new GameObject("Music_AudioSource");
            musicGo.transform.SetParent(this.transform);
            musicAudioSource = musicGo.AddComponent<AudioSource>();
            musicAudioSource.outputAudioMixerGroup = musicMixerGroup;
            musicAudioSource.loop = true; // Музика зазвичай зациклена
            musicAudioSource.playOnAwake = false;
        }
        if (musicAudioSource.outputAudioMixerGroup == null && musicMixerGroup != null)
        {
             musicAudioSource.outputAudioMixerGroup = musicMixerGroup;
        }


        if (menuAudioSource == null && menuSoundMixerGroup != null)
        {
            Debug.LogWarning("SoundManager: Menu Sound AudioSource not assigned. Creating one automatically.");
            GameObject menuGo = new GameObject("Menu_AudioSource");
            menuGo.transform.SetParent(this.transform);
            menuAudioSource = menuGo.AddComponent<AudioSource>();
            menuAudioSource.outputAudioMixerGroup = menuSoundMixerGroup;
            menuAudioSource.playOnAwake = false;
        }
         if (menuAudioSource != null && menuAudioSource.outputAudioMixerGroup == null && menuSoundMixerGroup != null)
        {
             menuAudioSource.outputAudioMixerGroup = menuSoundMixerGroup;
        }
    }

    void Start()
    {
        // Знаходимо SettingsManager, оскільки він також DontDestroyOnLoad
        _settingsManager = SettingsManager.Instance;
        if (_settingsManager == null)
        {
            Debug.LogError("SoundManager: SettingsManager.Instance not found! Audio settings (master volume, individual volumes) will not be respected.");
        }
    }

    // --- Public API для відтворення звуків ---

    /// <summary>
    /// Відтворює звуковий ефект (SFX).
    /// </summary>
    /// <param name="sfxData">Дані SFX (SFXData ScriptableObject).</param>
    /// <param name="position">Позиція відтворення (для 3D звуків).</param>
    /// <returns>Відтворений AudioSource або null, якщо відтворити не вдалося.</returns>
    public AudioSource PlaySFX(SFXData sfxData, Vector3 position = default(Vector3))
    {
        if (sfxData == null || sfxData.clip == null)
        {
            Debug.LogWarning("SoundManager: SFXData or AudioClip is null. Cannot play SFX.");
            return null;
        }
        // ЗМІНА ТУТ: Перевірка майстер-звуку через PlayerPrefs та публічний ключ SettingsManager
        if (_settingsManager != null && PlayerPrefs.GetInt(SettingsManager.MASTER_SOUND_TOGGLE_KEY, 1) == 0) 
        {
            Debug.Log($"SoundManager: Master sound is off. Cannot play {sfxData.name}.");
            return null;
        }

        AudioSource audioSource = GetAvailableSFXAudioSource();
        if (audioSource == null)
        {
            Debug.LogWarning("SoundManager: SFX AudioSource pool exhausted. Consider increasing sfxPoolSize.");
            return null;
        }

        ConfigureAudioSource(audioSource, sfxData);
        audioSource.transform.position = position; // Встановлюємо позицію для 3D звуків

        if (sfxData.delay > 0)
        {
            StartCoroutine(PlaySFXWithDelay(audioSource, sfxData.delay));
        }
        else
        {
            audioSource.Play();
        }
        return audioSource;
    }

    /// <summary>
    /// Відтворює музичний трек.
    /// </summary>
    /// <param name="musicData">Дані музики (MusicData ScriptableObject).</param>
    /// <returns>Відтворений AudioSource (зазвичай musicAudioSource).</returns>
    public AudioSource PlayMusic(MusicData musicData)
    {
        if (musicData == null || musicData.clip == null)
        {
            Debug.LogWarning("SoundManager: MusicData or AudioClip is null. Cannot play music.");
            return null;
        }
        // ЗМІНА ТУТ: Перевірка майстер-звуку через PlayerPrefs та публічний ключ SettingsManager
        if (_settingsManager != null && PlayerPrefs.GetInt(SettingsManager.MASTER_SOUND_TOGGLE_KEY, 1) == 0) 
        {
            Debug.Log($"SoundManager: Master sound is off. Cannot play music {musicData.name}.");
            return null;
        }

        if (musicAudioSource == null)
        {
            Debug.LogError("SoundManager: Music AudioSource is null. Cannot play music.");
            return null;
        }

        // Зупиняємо попередній трек, якщо потрібно
        if (musicData.stopPreviousMusic && musicAudioSource.isPlaying)
        {
            StopMusic(musicData.fadeOutTime); // Плавно зупиняємо попередній
        }

        ConfigureAudioSource(musicAudioSource, musicData);
        musicAudioSource.loop = true; // Музика зазвичай зациклена

        if (musicData.fadeInTime > 0)
        {
            StartCoroutine(FadeInMusic(musicAudioSource, musicData.fadeInTime));
        }
        else
        {
            musicAudioSource.Play();
        }
        return musicAudioSource;
    }

    /// <summary>
    /// Зупиняє поточну музику з плавним затуханням.
    /// </summary>
    /// <param name="fadeOutTime">Час затухання.</param>
    public void StopMusic(float fadeOutTime = 0f)
    {
        if (musicAudioSource == null || !musicAudioSource.isPlaying) return;

        if (fadeOutTime > 0)
        {
            StartCoroutine(FadeOutMusic(musicAudioSource, fadeOutTime));
        }
        else
        {
            musicAudioSource.Stop();
        }
    }

    /// <summary>
    /// Відтворює звук для елементів меню/UI.
    /// </summary>
    /// <param name="sfxData">Дані звуку (SFXData ScriptableObject).</param>
    /// <returns>Відтворений AudioSource (зазвичай menuAudioSource).</returns>
    public AudioSource PlayMenuSound(SFXData sfxData)
    {
         if (sfxData == null || sfxData.clip == null)
        {
            Debug.LogWarning("SoundManager: Menu SFXData or AudioClip is null. Cannot play menu sound.");
            return null;
        }
        // ЗМІНА ТУТ: Перевірка майстер-звуку через PlayerPrefs та публічний ключ SettingsManager
        if (_settingsManager != null && PlayerPrefs.GetInt(SettingsManager.MASTER_SOUND_TOGGLE_KEY, 1) == 0) 
        {
            Debug.Log($"SoundManager: Master sound is off. Cannot play menu sound {sfxData.name}.");
            return null;
        }

        if (menuAudioSource == null)
        {
             Debug.LogError("SoundManager: Menu AudioSource is null. Cannot play menu sound.");
             return PlaySFX(sfxData); // Спроба відтворити як звичайний SFX, якщо Menu AudioSource не призначено
        }

        ConfigureAudioSource(menuAudioSource, sfxData);
        menuAudioSource.loop = false; // Меню-звуки зазвичай не зациклені

        if (sfxData.delay > 0)
        {
            StartCoroutine(PlaySFXWithDelay(menuAudioSource, sfxData.delay));
        }
        else
        {
            menuAudioSource.Play();
        }
        return menuAudioSource;
    }


    // --- Внутрішні допоміжні методи ---

    private AudioSource GetAvailableSFXAudioSource()
    {
        // Шукаємо вільний AudioSource в пулі
        for (int i = 0; i < sfxAudioSources.Count; i++)
        {
            if (!sfxAudioSources[i].isPlaying)
            {
                return sfxAudioSources[i];
            }
        }

        // Якщо всі зайняті, беремо наступний з пулу по колу
        sfxPoolIndex = (sfxPoolIndex + 1) % sfxAudioSources.Count;
        return sfxAudioSources[sfxPoolIndex];
    }

    private void ConfigureAudioSource(AudioSource source, SoundData data)
    {
        source.clip = data.clip;
        source.outputAudioMixerGroup = data.outputAudioMixerGroup; // Призначення групи мікшера
        source.volume = data.volume;
        source.pitch = Random.Range(data.minPitch, data.maxPitch); // Рандомна тональність
        source.loop = data.loop;
        source.spatialBlend = data.spatialBlend;
    }

    // --- Корутини для затухання та затримки ---

    private IEnumerator FadeInMusic(AudioSource source, float fadeTime)
    {
        float startVolume = 0f;
        float targetVolume = source.volume; 
        source.volume = startVolume;
        source.Play();

        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeTime);
            yield return null;
        }
        source.volume = targetVolume; 
    }

    private IEnumerator FadeOutMusic(AudioSource source, float fadeTime)
    {
        float startVolume = source.volume;
        float targetVolume = 0f;

        float timer = 0f;
        while (timer < fadeTime)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, targetVolume, timer / fadeTime);
            yield return null;
        }
        source.volume = targetVolume;
        source.Stop(); 
    }

    private IEnumerator PlaySFXWithDelay(AudioSource source, float delay)
    {
        yield return new WaitForSeconds(delay);
        source.Play();
    }

    // --- Додаткова функціональність: Перемикання Master Sound через SettingsManager ---
    // ЗМІНА ТУТ: Використовуємо _settingsManager.masterMixer та _settingsManager.masterVolumeParam
    public void UpdateMixerMasterVolume(bool isMasterSoundEnabled)
    {
        if (_settingsManager == null || _settingsManager.masterMixer == null) 
        {
            Debug.LogWarning("SoundManager: SettingsManager or its MasterMixer is null. Cannot update master volume directly.");
            return;
        }
        float volume = isMasterSoundEnabled ? 0f : -80f; 
        _settingsManager.masterMixer.SetFloat(_settingsManager.masterVolumeParam, volume);
    }
}