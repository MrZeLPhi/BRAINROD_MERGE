using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.Audio; 

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    [Header("Audio Mixer Configuration")]
    [Tooltip("Головний Audio Mixer. Призначте його в Inspector.")]
    public AudioMixer masterMixer; 

    [Tooltip("Назви параметрів гучності в Audio Mixer (повинні бути Exposed та перейменовані).")]
    public string masterVolumeParam = "MasterVolume";
    public string sfxVolumeParam = "SFXVolume";
    public string menuVolumeParam = "MenuVolume"; 
    public string musicVolumeParam = "MusicVolume";

    // Constants for PlayerPrefs keys
    // ЗМІНА ТУТ: Зробимо MASTER_SOUND_TOGGLE_KEY публічним для доступу з SoundManager
    public const string MASTER_SOUND_TOGGLE_KEY = "MasterSoundToggle"; 
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MENU_VOLUME_KEY = "MenuVolume"; 
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string VIBRATION_TOGGLE_KEY = "VibrationToggle";
    private const string FPS_KEY = "TargetFPS";

    private int[] fpsOptions = { 30, 60, 120 }; 

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this; 
        DontDestroyOnLoad(gameObject); 

        ApplyGameSettingsFromPlayerPrefs();
        Debug.Log("SettingsManager: Initial game settings applied from PlayerPrefs.");
    }

    // --- Methods for Audio Control ---
    public void SetMasterSound(bool isEnabled)
    {
        float volume = isEnabled ? 0f : -80f; 
        if (masterMixer != null)
        {
            bool success = masterMixer.SetFloat(masterVolumeParam, volume);
            if (!success) Debug.LogWarning($"Audio Mixer parameter '{masterVolumeParam}' not found. Make sure it's exposed and named correctly.");
            PlayerPrefs.SetInt(MASTER_SOUND_TOGGLE_KEY, isEnabled ? 1 : 0);
            PlayerPrefs.Save(); 
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dbVolume = volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20; 
            bool success = masterMixer.SetFloat(sfxVolumeParam, dbVolume);
            if (!success) Debug.LogWarning($"Audio Mixer parameter '{sfxVolumeParam}' not found. Make sure it's exposed and named correctly.");
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, volume);
            PlayerPrefs.Save(); 
        }
    }

    public void SetMenuSoundVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dbVolume = volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20;
            bool success = masterMixer.SetFloat(menuVolumeParam, dbVolume);
            if (!success) Debug.LogWarning($"Audio Mixer parameter '{menuVolumeParam}' not found. Make sure it's exposed and named correctly.");
            PlayerPrefs.SetFloat(MENU_VOLUME_KEY, volume);
            PlayerPrefs.Save(); 
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (masterMixer != null)
        {
            float dbVolume = volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20;
            bool success = masterMixer.SetFloat(musicVolumeParam, dbVolume);
            if (!success) Debug.LogWarning($"Audio Mixer parameter '{musicVolumeParam}' not found. Make sure it's exposed and named correctly.");
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, volume);
            PlayerPrefs.Save(); 
        }
    }

    // --- Methods for Other Settings ---
    public void SetVibration(bool isEnabled)
    {
        PlayerPrefs.SetInt(VIBRATION_TOGGLE_KEY, isEnabled ? 1 : 0);
        PlayerPrefs.Save(); 
    }

    public bool IsVibrationEnabled() 
    {
        return PlayerPrefs.GetInt(VIBRATION_TOGGLE_KEY, 1) == 1; // Default to enabled
    }

    // --- Methods for FPS Settings ---
    public void SetTargetFPSFromDropdown(int dropdownIndex)
    {
        if (dropdownIndex >= 0 && dropdownIndex < fpsOptions.Length)
        {
            int selectedFPS = fpsOptions[dropdownIndex];
            Application.targetFrameRate = selectedFPS;
            PlayerPrefs.SetInt(FPS_KEY, selectedFPS);
            PlayerPrefs.Save(); 
            Debug.Log($"Target FPS set to: {selectedFPS} (from Dropdown index {dropdownIndex})");
        }
        else
        {
            Debug.LogWarning($"Invalid FPS Dropdown index: {dropdownIndex}. No FPS change.");
        }
    }

    public void SetTargetFPS(int fps)
    {
        Application.targetFrameRate = fps;
        PlayerPrefs.SetInt(FPS_KEY, fps);
        PlayerPrefs.Save(); 
        Debug.Log($"Target FPS set to: {fps}");
    }

    // --- New: Method to apply settings to game systems (called once on Awake) ---
    private void ApplyGameSettingsFromPlayerPrefs()
    {
        // Apply audio settings
        SetMasterSound(PlayerPrefs.GetInt(MASTER_SOUND_TOGGLE_KEY, 1) == 1);
        SetSFXVolume(PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f));
        SetMenuSoundVolume(PlayerPrefs.GetFloat(MENU_VOLUME_KEY, 1f));
        SetMusicVolume(PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f));

        // Apply FPS setting
        SetTargetFPS(PlayerPrefs.GetInt(FPS_KEY, 60)); // Default to 60 FPS
    }


    // --- Modified: Method to load settings AND update UI elements ---
    public void LoadSettingsToUI(
        Toggle masterSoundToggleUI, Slider sfxSliderUI, Slider menuSoundSliderUI, Slider musicSliderUI,
        Toggle vibrationToggleUI, TMP_Dropdown fpsDropdownUI
    )
    {
        Debug.Log("SettingsManager: Loading settings to UI elements.");

        // Clear existing listeners to prevent multiple subscriptions
        if (masterSoundToggleUI != null) masterSoundToggleUI.onValueChanged.RemoveAllListeners();
        if (sfxSliderUI != null) sfxSliderUI.onValueChanged.RemoveAllListeners();
        if (menuSoundSliderUI != null) menuSoundSliderUI.onValueChanged.RemoveAllListeners();
        if (musicSliderUI != null) musicSliderUI.onValueChanged.RemoveAllListeners();
        if (vibrationToggleUI != null) vibrationToggleUI.onValueChanged.RemoveAllListeners();
        if (fpsDropdownUI != null) fpsDropdownUI.onValueChanged.RemoveAllListeners();


        // Load and apply to UI (and re-apply to game systems via Set methods)
        
        // Master Sound Toggle
        bool masterSoundEnabled = PlayerPrefs.GetInt(MASTER_SOUND_TOGGLE_KEY, 1) == 1; 
        if (masterSoundToggleUI != null) masterSoundToggleUI.isOn = masterSoundEnabled;
        // Attach listener AFTER setting value to avoid immediate callback
        if (masterSoundToggleUI != null) masterSoundToggleUI.onValueChanged.AddListener(SetMasterSound);
        SetMasterSound(masterSoundEnabled); // Re-apply to mixer/game (already in PlayerPrefs)

        // SFX Volume
        float sfxVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, 1f); 
        if (sfxSliderUI != null) sfxSliderUI.value = sfxVolume;
        if (sfxSliderUI != null) sfxSliderUI.onValueChanged.AddListener(SetSFXVolume);
        SetSFXVolume(sfxVolume); 

        // Menu Sound Volume
        float menuVolume = PlayerPrefs.GetFloat(MENU_VOLUME_KEY, 1f);
        if (menuSoundSliderUI != null) menuSoundSliderUI.value = menuVolume;
        if (menuSoundSliderUI != null) menuSoundSliderUI.onValueChanged.AddListener(SetMenuSoundVolume);
        SetMenuSoundVolume(menuVolume);

        // Music Volume
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, 1f);
        if (musicSliderUI != null) musicSliderUI.value = musicVolume;
        if (musicSliderUI != null) musicSliderUI.onValueChanged.AddListener(SetMusicVolume);
        SetMusicVolume(musicVolume);

        // Vibration Toggle
        bool vibrationEnabled = PlayerPrefs.GetInt(VIBRATION_TOGGLE_KEY, 1) == 1;
        if (vibrationToggleUI != null) vibrationToggleUI.isOn = vibrationEnabled;
        if (vibrationToggleUI != null) vibrationToggleUI.onValueChanged.AddListener(SetVibration);
        SetVibration(vibrationEnabled);

        // FPS Dropdown
        int loadedFPS = PlayerPrefs.GetInt(FPS_KEY, 60); 
        int dropdownIndexToSet = -1;
        for (int i = 0; i < fpsOptions.Length; i++)
        {
            if (fpsOptions[i] == loadedFPS)
            {
                dropdownIndexToSet = i;
                break;
            }
        }

        if (fpsDropdownUI != null)
        {
            if (dropdownIndexToSet != -1)
            {
                fpsDropdownUI.value = dropdownIndexToSet; 
            }
            else
            {
                fpsDropdownUI.value = 1; 
                loadedFPS = 60; 
            }
            if (fpsDropdownUI != null) fpsDropdownUI.onValueChanged.AddListener(SetTargetFPSFromDropdown);
            SetTargetFPS(loadedFPS); // This also saves to PlayerPrefs
        }
        
        Debug.Log("SettingsManager: UI elements updated and listeners re-attached.");
    }
}