using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening; // Додаємо DOTween

public class PlayerController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TouchSlider _touchSlider;
    [SerializeField] private Image _nextObjectUIImageView;
    [SerializeField] private ScrollingTexture _scrollingTexture; 
    [Tooltip("Посилання на HandleRect (візуальний повзунок) слайдера.")]
    public RectTransform _sliderHandleRect; // <-- НОВЕ ПОЛЕ: Посилання на HandleRect
    
    // Reference to the spawn manager
    [SerializeField] private SpawnManager _spawnManager;

    private MergeableObject _currentActiveObject;
    private Tween _fadeTween; // Зберігаємо посилання на анімацію, щоб її можна було зупинити

    private void Awake()
    {
        if (_touchSlider == null) Debug.LogError("TouchSlider is not assigned in PlayerController!");
        if (_spawnManager == null) Debug.LogError("SpawnManager is not assigned in PlayerController!");
        if (_nextObjectUIImageView == null) Debug.LogWarning("Next Object UI Image View is not assigned in PlayerController! The next object won't be displayed.");
        if (_scrollingTexture == null) Debug.LogWarning("ScrollingTexture is not assigned in PlayerController! The visual indicator won't be displayed.");
        if (_sliderHandleRect == null) Debug.LogError("Slider Handle Rect is not assigned in PlayerController!");


        _touchSlider.OnPointerDragEvent += OnSliderDrag;
        _touchSlider.OnPointerUpEvent += OnSliderRelease;
        _touchSlider.OnPointerDownEvent += OnSliderPress; 
    }

    private void Start()
    {
        _spawnManager.SpawnNextPlayerObject(this);
        UpdateNextObjectUI();
        
        if (_scrollingTexture != null) _scrollingTexture.FadeOut();
    }

    private void OnDisable()
    {
        _touchSlider.OnPointerDragEvent -= OnSliderDrag;
        _touchSlider.OnPointerUpEvent -= OnSliderRelease;
        _touchSlider.OnPointerDownEvent -= OnSliderPress;
        if (_fadeTween != null) _fadeTween.Kill();
    }
    
    // --- ЗМІНЕНО: Update() тепер синхронізує позицію об'єкта та SpawnPoint ---
    void Update()
    {
        // Ця логіка виконуватиметься постійно, якщо є об'єкт для керування.
        // Вона синхронізує світовий об'єкт з позицією handle UI.
        if (_currentActiveObject != null && _sliderHandleRect != null && _spawnManager != null)
        {
            // Конвертуємо позицію HandleRect з UI-простору в світовий простір
            Vector3 handleWorldPosition = Camera.main.ScreenToWorldPoint(_sliderHandleRect.position);
            
            // Застосовуємо X-позицію до об'єкта, зберігаючи його Y та Z
            _currentActiveObject.transform.position = new Vector3(
                handleWorldPosition.x,
                _currentActiveObject.transform.position.y,
                _currentActiveObject.transform.position.z
            );

            // Синхронізуємо _spawnPoint з позицією об'єкта
            if (_spawnManager._spawnPoint != null)
            {
                Vector3 newSpawnPointPosition = _spawnManager._spawnPoint.position;
                newSpawnPointPosition.x = handleWorldPosition.x;
                _spawnManager._spawnPoint.position = newSpawnPointPosition;
            }
        }
    }

    public void SetCurrentActiveObject(MergeableObject obj)
    {
        _currentActiveObject = obj;
        if (_currentActiveObject != null)
        {
            // При спавні об'єкт з'явиться в позиції _spawnPoint,
            // який вже буде синхронізований з handle.
            // Тому тут ми просто вмикаємо візуальний ефект.
            if (_scrollingTexture != null) _scrollingTexture.FadeIn();
        }
        UpdateNextObjectUI();
    }

    private void OnSliderPress()
    {
        // Add logic here for when the player first presses the slider
    }

    private void OnSliderDrag(float sliderValue)
    {
        // У цьому методі ми більше НЕ змінюємо позицію об'єкта.
        // Замість цього, ми дозволимо Slider'у автоматично переміщувати Handle,
        // а наш Update() буде слідувати за handle.
        //
        // Цей метод можна використовувати для іншої логіки, якщо потрібно.
    }

    private void OnSliderRelease()
    {
        if (_currentActiveObject == null) return;

        if (_currentActiveObject.IsPlayerControlled)
            ReleaseCurrentObject();
    }

    private void ReleaseCurrentObject()
    {
        if (_currentActiveObject == null) return;

        _currentActiveObject.ActivatePhysics(); 
        _currentActiveObject = null; 
        
        if (_scrollingTexture != null) _scrollingTexture.FadeOut();

        _spawnManager.SpawnNextPlayerObject(this);
    }

    private void UpdateNextObjectUI()
    {
        if (_nextObjectUIImageView != null && _spawnManager != null)
        {
            Sprite nextSprite = _spawnManager.GetNextPlayerObjectSprite();
            if (nextSprite != null)
            {
                _nextObjectUIImageView.sprite = nextSprite;
                _nextObjectUIImageView.gameObject.SetActive(true);
            }
            else
            {
                _nextObjectUIImageView.gameObject.SetActive(false); 
            }
        }
    }
}