using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.EventSystems; // Для EventTrigger

public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button settingsButton;
    public Button shopButton;
    public Button levelPlayButton; 

    [Header("Panels")]
    public GameObject settingsPanel;
    public Button settingsCloseButton; 

    public GameObject shopPanel;
    public Button shopCloseButton; 

    [Header("Campaign Panel - UI Elements")]
    public GameObject campaignPanel; 
    
    // ЗМІНА: Посилання на окремий менеджер для свайпу рівнів
    public LevelSwipeManager levelSwipeManager; 

    // Private змінна для зберігання посилання на екземпляр SettingsManager
    private SettingsManager _settingsManagerInstance; 

    [Header("Settings Panel UI References")]
    public Toggle masterSoundToggleRef;
    public Slider sfxSliderRef;
    public Slider menuSoundSliderRef;
    public Slider musicSliderRef;
    public Toggle vibrationToggleRef;
    public TMP_Dropdown fpsDropdownRef;


    void Awake()
    {
        // ЗМІНА ТУТ: Знаходимо екземпляр SettingsManager через його Singleton Instance
        _settingsManagerInstance = SettingsManager.Instance;
        if (_settingsManagerInstance == null)
        {
            Debug.LogError("MainMenuManager: SettingsManager.Instance не знайдено! Переконайтесь, що SettingsManager знаходиться на сцені 'Bootstrap' та має скрипт Singleton.");
        }

        Time.timeScale = 1.0f;

        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (campaignPanel != null) campaignPanel.SetActive(true); 

        if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettingsPanel);
        if (shopButton != null) shopButton.onClick.AddListener(ShowShopPanel);

        if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(HideSettingsPanel);
        if (shopCloseButton != null) shopCloseButton.onClick.AddListener(HideShopPanel);

        // ЗМІНА: Кнопка Play тепер викликає метод на LevelSwipeManager
        if (levelPlayButton != null && levelSwipeManager != null) 
        {
            levelPlayButton.onClick.AddListener(levelSwipeManager.PlaySelectedLevel);
        }
        else
        {
            if (levelPlayButton != null) Debug.LogError("MainMenuManager: levelSwipeManager не призначено. Кнопка 'Play' не буде працювати.");
        }
    }

    // --- Методи для UI панелей ---
    public void ShowSettingsPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(false); 
        if (settingsPanel != null) settingsPanel.SetActive(true);

        if (_settingsManagerInstance != null)
        {
            _settingsManagerInstance.LoadSettingsToUI(
                masterSoundToggleRef, sfxSliderRef, menuSoundSliderRef, musicSliderRef,
                vibrationToggleRef, fpsDropdownRef
            );
            Debug.Log("MainMenuManager: Settings UI updated with latest values."); 
        }
        else
        {
            Debug.LogError("MainMenuManager: _settingsManagerInstance не знайдено. Неможливо завантажити налаштування UI.");
        }
        Debug.Log("MainMenuManager: Показано панель налаштувань.");
    }

    public void HideSettingsPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        Debug.Log("MainMenuManager: Приховано панель налаштувань.");
    }

    public void ShowShopPanel()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false); 
        if (shopPanel != null) shopPanel.SetActive(true);
        Debug.Log("MainMenuManager: Показано панель магазину.");
    }

    public void HideShopPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(false);
        Debug.Log("MainMenuManager: Приховано панель магазину.");
    }

    public void HideAllPanels()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        Debug.Log("MainMenuManager: Приховано всі додаткові панелі.");
    }
}