using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider _moveSlider; // Слайдер для горизонтального руху
    [SerializeField] private Button _releaseButton; // Кнопка "відпустити" (опціонально, якщо не за відпусканням слайдера)

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 10f; // Швидкість руху об'єкта за слайдером
    [SerializeField] private float _minX = -4f; // Ліва межа руху
    [SerializeField] private float _maxX = 4f; // Права межа руху

    // Посилання на менеджер спавну
    [SerializeField] private SpawnManager _spawnManager;

    private MergeableObject _currentActiveObject; // Об'єкт, яким керує гравець
    private bool _isSliderPressed = false; // Чи натиснутий слайдер

    private void Awake()
    {
        if (_moveSlider == null) Debug.LogError("Слайдер не призначений в PlayerController!");
        if (_spawnManager == null) Debug.LogError("SpawnManager не призначений в PlayerController!");

        // Додаємо слухачі подій слайдера
        _moveSlider.onValueChanged.AddListener(OnSliderValueChanged);
        
        // Для відпускання слайдера, потрібно використовувати EventTrigger на UI Slider
        // Або більш простий варіант, якщо Slider - це лише візуалізація, а керування йде від тача
        // Для демонстрації, припустимо, що Input.GetMouseButtonUp(0) це відпускання
        // Якщо це тач, то потрібно OnPointerUp у EventTrigger на Slider
    }

    private void Start()
    {
        // Запускаємо спавн першого об'єкта на початку гри
        _spawnManager.SpawnNextPlayerObject(this);
    }

    private void OnDisable()
    {
        _moveSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
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
            _currentActiveObject.transform.position = new Vector3(
                Mathf.Lerp(_minX, _maxX, _moveSlider.value), // Початкова позиція за слайдером
                _currentActiveObject.transform.position.y,
                _currentActiveObject.transform.position.z
            );
        }
    }

    private void Update()
    {
        // Відстежуємо натискання/відпускання миші/дотику для мобільних
        if (Input.GetMouseButtonDown(0)) // Або Input.GetTouch(0).phase == TouchPhase.Began
        {
            _isSliderPressed = true;
        }
        else if (Input.GetMouseButtonUp(0)) // Або Input.GetTouch(0).phase == TouchPhase.Ended
        {
            // Перевіряємо, чи відпустили слайдер
            // Це дуже спрощено, в реальній грі потрібно перевіряти, чи палець був НАД слайдером
            if (_isSliderPressed && _currentActiveObject != null && _currentActiveObject.IsPlayerControlled)
            {
                ReleaseCurrentObject();
            }
            _isSliderPressed = false;
        }

        // Рух об'єкта за слайдером
        if (_currentActiveObject != null && _currentActiveObject.IsPlayerControlled)
        {
            MoveObjectWithSlider();
        }
    }

    /// <summary>
    /// Обробляє зміну значення слайдера.
    /// </summary>
    private void OnSliderValueChanged(float value)
    {
        // Якщо об'єкт під контролем гравця, то Update його перемістить.
        // Цей метод потрібен, щоб зареєструвати зміну значення.
    }

    /// <summary>
    /// Переміщує поточний активний об'єкт відповідно до значення слайдера.
    /// </summary>
    private void MoveObjectWithSlider()
    {
        if (_currentActiveObject == null) return;

        // Перетворюємо значення слайдера (0-1) на діапазон X-координат
        float targetX = Mathf.Lerp(_minX, _maxX, _moveSlider.value);
        
        // Плавно переміщуємо об'єкт до цільової позиції
        Vector3 targetPosition = new Vector3(targetX, _currentActiveObject.transform.position.y, _currentActiveObject.transform.position.z);
        _currentActiveObject.transform.position = Vector3.MoveTowards(_currentActiveObject.transform.position, targetPosition, _moveSpeed * Time.deltaTime);
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
}
