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

        _touchSlider.OnPointerDragEvent += OnSliderDrag;
        _touchSlider.OnPointerUpEvent += OnSliderRelease;
        _touchSlider.OnPointerDownEvent += OnSliderPress; 
    }

    private void Start()
    {
        _spawnManager.SpawnNextPlayerObject(this);
        UpdateNextObjectUI();
        
        // Переконаємося, що індикатор прихований на старті
        if (_scrollingTexture != null) _scrollingTexture.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        _touchSlider.OnPointerDragEvent -= OnSliderDrag;
        _touchSlider.OnPointerUpEvent -= OnSliderRelease;
        _touchSlider.OnPointerDownEvent -= OnSliderPress;
        if (_fadeTween != null) _fadeTween.Kill();
    }
    
    void Update()
    {
        if (_currentActiveObject != null && _spawnManager != null && _spawnManager._spawnPoint != null)
        {
            Vector3 newSpawnPointPosition = _spawnManager._spawnPoint.position;
            newSpawnPointPosition.x = _currentActiveObject.transform.position.x;
            _spawnManager._spawnPoint.position = newSpawnPointPosition;
        }
    }

    public void SetCurrentActiveObject(MergeableObject obj)
    {
        _currentActiveObject = obj;
        if (_currentActiveObject != null)
        {
            float initialX = _touchSlider.GetComponent<Slider>().value; 
            _currentActiveObject.transform.position = new Vector3(
                initialX,
                _currentActiveObject.transform.position.y,
                _currentActiveObject.transform.position.z
            );
            
            // --- ВИПРАВЛЕНО: Викликаємо FadeIn() ---
            if (_scrollingTexture != null)
            {
                _scrollingTexture.FadeIn();
            }
        }
        UpdateNextObjectUI();
    }

    private void OnSliderPress()
    {
        // Add logic here for when the player first presses the slider
    }

    private void OnSliderDrag(float sliderValue)
    {
        if (_currentActiveObject != null && _currentActiveObject.IsPlayerControlled)
        {
            _currentActiveObject.transform.position = new Vector3(
                sliderValue, 
                _currentActiveObject.transform.position.y,
                _currentActiveObject.transform.position.z
            );
        }
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
        
        // --- ВИПРАВЛЕНО: Викликаємо FadeOut() ---
        if (_scrollingTexture != null)
        {
            _scrollingTexture.FadeOut();
        }

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