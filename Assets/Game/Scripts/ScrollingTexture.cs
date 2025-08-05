using UnityEngine;
using UnityEngine.UI;

// Цей скрипт керує як анімацією прокручування текстури, так і порядком відображення Canvas.
// Прикріпіть його до вашого RawImage.
[RequireComponent(typeof(RawImage))]
public class ScrollingTexture : MonoBehaviour
{
    [Header("Texture Scrolling Settings")]
    [Tooltip("Швидкість прокручування текстури.")]
    public float scrollSpeed = 0.5f;

    [Header("Fading Settings")]
    [Tooltip("Швидкість появи/зникнення (у секундах).")]
    public float fadeSpeed = 2.0f;
    [Tooltip("Цільова прозорість при появі (0-255).")]
    [Range(0, 255)]
    public int targetAlpha = 220;

    [Header("Pulsing Settings")]
    [Tooltip("Швидкість пульсації прозорості.")]
    public float pulseSpeed = 2f;
    [Tooltip("Мінімальна прозорість для пульсації (0-255).")]
    [Range(0, 255)]
    public int pulseMinAlpha = 1;
    [Tooltip("Максимальна прозорість для пульсації (0-255).")]
    [Range(0, 255)]
    public int pulseMaxAlpha = 230;

    [Header("Canvas Sorting Settings")]
    [Tooltip("Назва Sorting Layer, який ви хочете встановити для Canvas цього об'єкта.")]
    public string sortingLayerName = "UI_Overlay";
    [Tooltip("Порядок у шарі (Order in Layer) для Canvas цього об'єкта.")]
    public int sortingOrder = 1;

    private RawImage _rawImage;
    private Canvas _parentCanvas;
    private float _fadeAlpha; // Змінна для керування появою/зникненням
    
    void Awake()
    {
        _rawImage = GetComponent<RawImage>();
        if (_rawImage == null)
        {
            Debug.LogError("RawImage компонент не знайдено! Переконайтесь, що скрипт прикріплений до RawImage.");
            enabled = false;
            return;
        }

        _parentCanvas = GetComponentInParent<Canvas>();
        if (_parentCanvas == null)
        {
            Debug.LogError("Canvas компонент не знайдено на батьківському елементі! Потрібен для налаштування Sorting Layer.");
            enabled = false;
            return;
        }
        
        _parentCanvas.sortingLayerName = sortingLayerName;
        _parentCanvas.sortingOrder = sortingOrder;

        // Встановлюємо початкову прозорість на 0, щоб вона була невидимою на старті
        Color initialColor = _rawImage.color;
        initialColor.a = 0f;
        _rawImage.color = initialColor;
        
        _fadeAlpha = 0f;
    }
    
    void Update()
    {
        // Логіка прокручування текстури
        float offset = Time.unscaledTime * scrollSpeed;
        Rect uvRect = _rawImage.uvRect;
        uvRect.y = -offset;
        _rawImage.uvRect = uvRect;
        
        // Логіка плавного з'явлення/зникнення
        float targetFadeAlpha = gameObject.activeInHierarchy ? 1f : 0f;
        _fadeAlpha = Mathf.MoveTowards(_fadeAlpha, targetFadeAlpha, Time.unscaledDeltaTime * fadeSpeed);
        
        // Логіка пульсації
        float pulseAlpha = (Mathf.Sin(Time.unscaledTime * pulseSpeed) * 0.5f + 0.5f); // Значення від 0 до 1
        float finalPulseAlpha = Mathf.Lerp(pulseMinAlpha / 255f, pulseMaxAlpha / 255f, pulseAlpha);

        // Комбінуємо плавне з'явлення та пульсацію для фінальної прозорості
        Color color = _rawImage.color;
        color.a = Mathf.Clamp01(_fadeAlpha * finalPulseAlpha);
        _rawImage.color = color;

        // Вимикаємо об'єкт, коли він повністю зник
        if (!gameObject.activeInHierarchy && _rawImage.color.a == 0)
        {
            gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// Вмикає об'єкт для плавного з'явлення.
    /// </summary>
    public void FadeIn()
    {
        gameObject.SetActive(true);
    }

    /// <summary>
    /// Запускає плавне зникнення.
    /// </summary>
    public void FadeOut()
    {
        // Тут ми просто вимикаємо сам GameObject. 
        // Логіка в Update() побачить, що об'єкт неактивний, і почне анімувати прозорість до 0.
        // Це гарантує, що анімація завершиться плавно.
        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        // Забезпечимо, що прозорість буде 0, коли об'єкт вимикається.
        Color c = _rawImage.color;
        c.a = 0;
        _rawImage.color = c;
    }
}