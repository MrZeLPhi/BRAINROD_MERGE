using UnityEngine;
using UnityEngine.Rendering; // Для SortingGroup, якщо використовуєте

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))] // Додамо Collider2D
[RequireComponent(typeof(Rigidbody2D))] // Додамо Rigidbody2D
public class MergeableObject : MonoBehaviour
{
    // [SerializeField] робить приватну змінну видимою в Inspector
    [SerializeField] private int _mergeLevel; // Значення 2, 4, 8, ...
    [SerializeField] private Sprite _objectSprite; // Спрайт для цього рівня
    [SerializeField] private bool _isPlayerControlled = true; // Чи слідує за слайдером
    [SerializeField] private bool _isBeingMerged = false; // Чи знаходиться в процесі злиття
    [SerializeField] private bool _hasPhysics = false; // Чи активована фізика (після відпускання)

    // Приватні посилання на компоненти, які отримуємо на старті
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;
    private Collider2D _collider;

    // Публічні властивості (читабельні, але не модифіковані ззовні без потреби)
    public int MergeLevel => _mergeLevel;
    public bool IsPlayerControlled => _isPlayerControlled;
    public bool IsBeingMerged => _isBeingMerged;
    public bool HasPhysics => _hasPhysics;


    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        // Переконаємось, що початковий стан Rigidbody2D коректний
        _rb.isKinematic = _isPlayerControlled; // Якщо контролюється гравцем, вимкніть фізику
        _rb.gravityScale = _isPlayerControlled ? 0f : 1f; // Вимкніть гравітацію, якщо контролюється
        //_rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Заборонити обертання
        _collider.enabled = !_isPlayerControlled; // Колайдер вимкнений, поки керується гравцем

        UpdateSprite();
    }

    /// <summary>
    /// Ініціалізує об'єкт після спавну.
    /// </summary>
    /// <param name="level">Рівень об'єднання (2, 4, 8, ...)</param>
    /// <param name="sprite">Спрайт для цього рівня</param>
    /// <param name="isPlayerControlled">Чи буде керуватися гравцем (true для нового об'єкта зверху)</param>
    public void Initialize(int level, Sprite sprite, bool isPlayerControlled)
    {
        _mergeLevel = level;
        _objectSprite = sprite;
        _isPlayerControlled = isPlayerControlled;
        _isBeingMerged = false;
        _hasPhysics = !isPlayerControlled; // Якщо не контролюється, значить, фізика вже активна

        UpdateSprite();

        // Налаштування Rigidbody2D та Collider2D залежно від того, чи керується гравцем
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_collider == null) _collider = GetComponent<Collider2D>();

        _rb.isKinematic = isPlayerControlled;
        _rb.gravityScale = isPlayerControlled ? 0f : 1f;
        _collider.enabled = !isPlayerControlled; // Колайдер вмикаємо лише коли об'єкт падає або лежить на дошці
    }

    /// <summary>
    /// Оновлює спрайт об'єкта відповідно до _objectSprite.
    /// </summary>
    private void UpdateSprite()
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null && _objectSprite != null)
        {
            _spriteRenderer.sprite = _objectSprite;
        }
    }

    /// <summary>
    /// Активує фізику об'єкта, коли гравець його "відпускає".
    /// </summary>
    public void ActivatePhysics()
    {
        _isPlayerControlled = false;
        _rb.isKinematic = false; // Увімкнути фізику
        _rb.gravityScale = 1f; // Увімкнути гравітацію
        _collider.enabled = true; // Увімкнути колайдер
        _hasPhysics = true;
    }

    /// <summary>
    /// Позначає об'єкт як такий, що зливається, щоб уникнути подвійних злиттів.
    /// </summary>
    public void SetBeingMerged(bool status)
    {
        _isBeingMerged = status;
        // Можливо, тимчасово вимкнути колайдер, щоб уникнути подальших зіткнень під час анімації злиття
        // _collider.enabled = !status; 
    }
}
