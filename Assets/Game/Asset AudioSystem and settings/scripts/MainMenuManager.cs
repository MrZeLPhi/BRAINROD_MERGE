using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.EventSystems; // Для EventTrigger

// MainMenuManager відповідає за навігацію між основними UI панелями та логіку свайпу карт кампанії.
// Цей скрипт має бути прикріплений до окремого GameObject на сцені (наприклад, "MainMenuManager").
public class MainMenuManager : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    public Button settingsButton;
    public Button shopButton;

    [Header("Panels")]
    public GameObject settingsPanel;
    public Button settingsCloseButton; 

    public GameObject shopPanel;
    public Button shopCloseButton; 

    [Header("Campaign Panel - UI Elements")]
    public GameObject campaignPanel; 
    public TextMeshProUGUI levelNameText; 
    public Button levelPlayButton; 
    
    [Tooltip("RectTransform, який є батьківським для всіх карток рівнів.")]
    public RectTransform levelsContainer; // Контейнер, який буде переміщатися
    
    [Tooltip("Horizontal Layout Group на LevelsContainer для отримання Spacing.")]
    public HorizontalLayoutGroup levelsLayoutGroup; 

    public float snapSpeed = 5f;
    public float swipeThreshold = 50f; 

    [Tooltip("Список ІНДЕКСІВ сцен (карток) у Build Settings в порядку проходження кампанії.")]
    public List<int> levelSceneBuildIndices; 
    private int currentLevelIndex = 0; 

    // Змінні для логіки перетягування
    private Vector2 dragStartMousePosition;
    private Vector2 dragCurrentContainerPosition;
    private Vector2 targetContainerPosition;
    private bool isDragging = false;

    // Змінні для розрахунку меж скролу
    private float levelCardBaseWidth = 700f; // Дефолтне значення, буде оновлено в Awake
    private float effectiveCardWidth; // Ширина картки + spacing

    // ЗМІНА ТУТ: Видаляємо публічне посилання на SettingsManager.
    // public SettingsManager settingsManager; // <-- ВИДАЛЕНО!

    // Private змінна для зберігання посилання на екземпляр SettingsManager
    private SettingsManager _settingsManagerInstance; 

    // НОВІ ПОСИЛАННЯ: UI-елементи з SettingsPanel, які MainMenuManager передасть SettingsManager
    [Header("Settings Panel UI References")]
    public Toggle masterSoundToggleRef;
    public Slider sfxSliderRef;
    public Slider menuSoundSliderRef;
    public Slider musicSliderRef;
    public Toggle vibrationToggleRef;
    public TMP_Dropdown fpsDropdownRef;


    void Awake()
    {
        Time.timeScale = 1.0f;

        // ЗМІНА ТУТ: Знаходимо екземпляр SettingsManager через його Singleton Instance
        _settingsManagerInstance = SettingsManager.Instance;
        if (_settingsManagerInstance == null)
        {
            Debug.LogError("MainMenuManager: SettingsManager.Instance не знайдено! Переконайтесь, що SettingsManager знаходиться на сцені 'Bootstrap' та має скрипт Singleton.");
        }


        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (campaignPanel != null) campaignPanel.SetActive(true); 

        if (settingsButton != null) settingsButton.onClick.AddListener(ShowSettingsPanel);
        if (shopButton != null) shopButton.onClick.AddListener(ShowShopPanel);

        if (settingsCloseButton != null) settingsCloseButton.onClick.AddListener(HideSettingsPanel);
        if (shopCloseButton != null) shopCloseButton.onClick.AddListener(HideShopPanel);

        if (levelPlayButton != null) levelPlayButton.onClick.AddListener(PlaySelectedLevel);

        if (levelsContainer != null && levelsContainer.childCount > 0)
        {
            levelCardBaseWidth = levelsContainer.GetChild(0).GetComponent<RectTransform>().rect.width;
        }

        if (levelsLayoutGroup != null)
        {
            effectiveCardWidth = levelCardBaseWidth + levelsLayoutGroup.spacing;
        }
        else
        {
            effectiveCardWidth = levelCardBaseWidth; 
            Debug.LogWarning("LevelsLayoutGroup не призначено в Inspector! Розрахунки скролу можуть бути неточними.");
        }

        if (levelsContainer != null)
        {
            targetContainerPosition = levelsContainer.anchoredPosition;
        }
    }

    void Start()
    {
        // Додаткова перевірка в Start, якщо Awake спрацював раніше
        if (_settingsManagerInstance == null)
        {
             _settingsManagerInstance = SettingsManager.Instance;
             if (_settingsManagerInstance == null)
             {
                 Debug.LogError("MainMenuManager: SettingsManager.Instance все ще не знайдено в Start. Це серйозна проблема.");
             }
        }

        UpdateLevelDisplay(); 
        if (levelsContainer != null && levelSceneBuildIndices != null && levelSceneBuildIndices.Count > 0)
        {
            targetContainerPosition = new Vector2(-currentLevelIndex * effectiveCardWidth, levelsContainer.anchoredPosition.y);
            levelsContainer.anchoredPosition = targetContainerPosition; 
        }
    }

    void Update()
    {
        if (!isDragging && levelsContainer != null)
        {
            levelsContainer.anchoredPosition = Vector2.Lerp(levelsContainer.anchoredPosition, targetContainerPosition, Time.deltaTime * snapSpeed);
        }
    }

    // --- Методи для UI панелей ---
    public void ShowSettingsPanel()
    {
        if (shopPanel != null) shopPanel.SetActive(false); 
        
        if (settingsPanel != null) settingsPanel.SetActive(true);

        // Тепер використовуємо _settingsManagerInstance, який знаходимо програмно
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

    // --- Методи для завантаження сцен ---
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"MainMenuManager: Індекс сцени '{sceneIndex}' недійсний або не доданий до Build Settings!");
            return;
        }
        SceneManager.LoadScene(sceneIndex);
        Debug.Log($"MainMenuManager: Завантажую сцену за індексом: {sceneIndex}");
    }

    // --- Методи для логіки кампанії ---
    private void UpdateLevelDisplay()
    {
        if (levelNameText == null || levelSceneBuildIndices == null || levelSceneBuildIndices.Count == 0)
        {
            if (levelNameText != null) levelNameText.text = "Немає карт";
            if (levelPlayButton != null) levelPlayButton.interactable = false; 
            return;
        }

        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levelSceneBuildIndices.Count - 1);

        string scenePath = SceneUtility.GetScenePathByBuildIndex(levelSceneBuildIndices[currentLevelIndex]);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath); 

        levelNameText.text = $"Рівень {currentLevelIndex + 1}: {sceneName}"; 
        
        bool isLevelPlayable = true; 
        
        if (levelPlayButton != null) levelPlayButton.interactable = isLevelPlayable;
    }

    public void PlaySelectedLevel()
    {
        if (levelSceneBuildIndices != null && currentLevelIndex >= 0 && currentLevelIndex < levelSceneBuildIndices.Count)
        {
            LoadSceneByIndex(levelSceneBuildIndices[currentLevelIndex]);
        }
        else
        {
            Debug.LogWarning("MainMenuManager: Немає обраного рівня для запуску.");
        }
    }

    // --- ОБГОРТКОВІ МЕТОДИ ДЛЯ EventTrigger ---
    public void HandleBeginDrag(BaseEventData eventData)
    {
        OnBeginDrag((PointerEventData)eventData);
    }

    public void HandleDrag(BaseEventData eventData)
    {
        OnDrag((PointerEventData)eventData);
    }

    public void HandleEndDrag(BaseEventData eventData)
    {
        OnEndDrag((PointerEventData)eventData);
    }

    private void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        dragStartMousePosition = eventData.position;
        dragCurrentContainerPosition = levelsContainer.anchoredPosition; 
    }

    private void OnDrag(PointerEventData eventData)
    {
        if (levelsContainer == null) return;

        float deltaX = eventData.position.x - dragStartMousePosition.x;
        Vector2 newPos = dragCurrentContainerPosition + new Vector2(deltaX, 0);

        float currentMaxX = 0f; 
        float currentMinX = -(levelSceneBuildIndices.Count - 1) * effectiveCardWidth;
        
        newPos.x = Mathf.Clamp(newPos.x, currentMinX, currentMaxX);

        levelsContainer.anchoredPosition = newPos;
    }

    private void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        float dragDistance = eventData.position.x - dragStartMousePosition.x;

        if (Mathf.Abs(dragDistance) > swipeThreshold)
        {
            if (dragDistance < 0) 
            {
                currentLevelIndex++;
            }
            else 
            {
                currentLevelIndex--;
            }
        }

        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levelSceneBuildIndices.Count - 1);

        targetContainerPosition = new Vector2(-currentLevelIndex * effectiveCardWidth, levelsContainer.anchoredPosition.y);
        
        UpdateLevelDisplay(); 
    }
}