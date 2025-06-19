using UnityEngine;
using UnityEngine.UI; // Для роботи з Slider та Image

public class PlayerController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TouchSlider _touchSlider; // Посилання на наш TouchSlider
    [SerializeField] private Image _nextObjectUIImageView; // Нове: UI Image для відображення наступного об'єкта
    
    // Посилання на менеджер спавну
    [SerializeField] private SpawnManager _spawnManager;

    private MergeableObject _currentActiveObject; // Об'єкт, яким керує гравець

    private void Awake()
    {
        if (_touchSlider == null) Debug.LogError("TouchSlider не призначений в PlayerController!");
        if (_spawnManager == null) Debug.LogError("SpawnManager не призначений в PlayerController!");
        // Нове: Перевірка посилання на Image
        if (_nextObjectUIImageView == null) Debug.LogWarning("Next Object UI Image View не призначений в PlayerController! Наступний об'єкт не буде відображатися.");


        // Підписуємося на події з TouchSlider
        _touchSlider.OnPointerDragEvent += OnSliderDrag;
        _touchSlider.OnPointerUpEvent += OnSliderRelease;
        _touchSlider.OnPointerDownEvent += OnSliderPress; // Опціонально, якщо потрібно щось робити при натисканні
    }

    private void Start()
    {
        // Запускаємо спавн першого об'єкта на початку гри
        _spawnManager.SpawnNextPlayerObject(this);
        // Оновлюємо UI наступного об'єкта одразу після запуску гри
        UpdateNextObjectUI();
    }

    private void OnDisable()
    {
        // Важливо відписатися від подій, щоб уникнути витоків пам'яті
        _touchSlider.OnPointerDragEvent -= OnSliderDrag;
        _touchSlider.OnPointerUpEvent -= OnSliderRelease;
        _touchSlider.OnPointerDownEvent -= OnSliderPress;
    }

    /// <summary>
    /// Встановлює поточний активний об'єкт, яким керує гравець.
    /// Викликається SpawnManager'ом.
    /// </summary>
    public void SetCurrentActiveObject(MergeableObject obj)
    {
        _currentActiveObject = obj;
        if (_currentActiveObject != null)
        {
            // Встановлюємо початкову позицію об'єкта відповідно до поточної позиції слайдера (0 - центр)
            float initialX = _touchSlider.GetComponent<Slider>().value; // Отримуємо поточне значення слайдера (яке має бути 0)
            _currentActiveObject.transform.position = new Vector3(
                initialX,
                _currentActiveObject.transform.position.y,
                _currentActiveObject.transform.position.z
            );
        }
        // Нове: Оновлюємо UI наступного об'єкта щоразу, коли встановлюється новий поточний об'єкт
        UpdateNextObjectUI();
    }

    /// <summary>
    /// Обробляє подію натискання на слайдер.
    /// </summary>
    private void OnSliderPress()
    {
        // Можна додати логіку, коли гравець тільки натиснув на слайдер
        // Наприклад, візуальне підсвічування або звук
        // Debug.Log("Слайдер натиснуто!");
    }

    /// <summary>
    /// Обробляє подію перетягування слайдера.
    /// Об'єкт точно слідує за X-позицією слайдера, використовуючи його значення як координату.
    /// </summary>
    /// <param name="sliderValue">Значення слайдера (від -1 до 1).</param>
    private void OnSliderDrag(float sliderValue)
    {
        if (_currentActiveObject != null && _currentActiveObject.IsPlayerControlled)
        {
            // Встановлюємо позицію об'єкта безпосередньо, без затримки
            // Використовуємо значення слайдера як X-координату в ігровому світі
            _currentActiveObject.transform.position = new Vector3(
                sliderValue, // sliderValue вже в діапазоні -1 до 1
                _currentActiveObject.transform.position.y,
                _currentActiveObject.transform.position.z
            );
        }
    }

    /// <summary>
    /// Обробляє подію відпускання слайдера.
    /// Відпускає поточний об'єкт і запитує новий.
    /// </summary>
    private void OnSliderRelease()
    {
        if (_currentActiveObject == null) return;

        // Перевіряємо, чи об'єкт дійсно контролювався гравцем до цього
        if (_currentActiveObject.IsPlayerControlled)
        {
            ReleaseCurrentObject();
        }
    }

    /// <summary>
    /// Відпускає поточний об'єкт, активує його фізику і запитує новий об'єкт.
    /// </summary>
    private void ReleaseCurrentObject()
    {
        if (_currentActiveObject == null) return;

        _currentActiveObject.ActivatePhysics(); // Активуємо фізику об'єкта
        _currentActiveObject = null; // Обнуляємо посилання на поточний об'єкт

        // Запитуємо у SpawnManager новий об'єкт для гравця
        _spawnManager.SpawnNextPlayerObject(this);
    }

    /// <summary>
    /// Оновлює UI Image, відображаючи спрайт наступного об'єкта, який буде спавнений.
    /// </summary>
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
                _nextObjectUIImageView.gameObject.SetActive(false); // Ховаємо, якщо немає наступного спрайту
            }
        }
    }
}
