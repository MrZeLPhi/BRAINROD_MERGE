using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.EventSystems;

// Клас, який керує логікою свайпу карт рівнів
// Він буде отримувати події через Event Trigger
public class LevelSwipeManager : MonoBehaviour
{
    [Header("Campaign Panel - UI Elements")]
    public RectTransform levelsContainer; 
    public TextMeshProUGUI levelNameText; 
    
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
    private float levelCardBaseWidth = 700f; 
    private float effectiveCardWidth; 


    void Awake()
    {
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

    // --- ОБГОРТКОВІ МЕТОДИ ДЛЯ EventTrigger ---
    // Ці методи мають сигнатуру, яку розпізнає EventTrigger (BaseEventData)
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

    // --- Методи для завантаження сцен ---
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (sceneIndex < 0 || sceneIndex >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogError($"LevelSwipeManager: Індекс сцени '{sceneIndex}' недійсний або не доданий до Build Settings!");
            return;
        }
        SceneManager.LoadScene(sceneIndex);
        Debug.Log($"LevelSwipeManager: Завантажую сцену за індексом: {sceneIndex}");
    }

    // --- Методи для логіки кампанії ---
    private void UpdateLevelDisplay()
    {
        if (levelNameText == null || levelSceneBuildIndices == null || levelSceneBuildIndices.Count == 0)
        {
            if (levelNameText != null) levelNameText.text = "Немає карт";
            return;
        }

        currentLevelIndex = Mathf.Clamp(currentLevelIndex, 0, levelSceneBuildIndices.Count - 1);

        string scenePath = SceneUtility.GetScenePathByBuildIndex(levelSceneBuildIndices[currentLevelIndex]);
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath); 

        levelNameText.text = $"Рівень {currentLevelIndex + 1}: {sceneName}"; 
    }

    public void PlaySelectedLevel()
    {
        if (levelSceneBuildIndices != null && currentLevelIndex >= 0 && currentLevelIndex < levelSceneBuildIndices.Count)
        {
            LoadSceneByIndex(levelSceneBuildIndices[currentLevelIndex]);
        }
        else
        {
            Debug.LogWarning("LevelSwipeManager: Немає обраного рівня для запуску.");
        }
    }

    // --- Реалізація інтерфейсів перетягування (тепер приватні, оскільки викликаються з HandleDrag) ---
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