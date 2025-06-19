using UnityEngine;
using UnityEngine.Rendering; // For SortingGroup, if used

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))] // Add Collider2D
[RequireComponent(typeof(Rigidbody2D))] // Add Rigidbody2D
public class MergeableObject : MonoBehaviour
{
    // [SerializeField] makes a private variable visible in the Inspector
    [SerializeField] private int _mergeLevel; // Value 2, 4, 8, ...
    [SerializeField] private Sprite _objectSprite; // Sprite for this level
    [SerializeField] private int _pointsOnMerge = 0; // NEW: Points awarded when this object is CREATED by a merge
    [SerializeField] private bool _isPlayerControlled = true; // Whether it follows the slider
    [SerializeField] private bool _isBeingMerged = false; // Whether it's in the process of merging
    [SerializeField] private bool _hasPhysics = false; // Whether physics is active (after release)

    // Private references to components obtained at start
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;
    private Collider2D _collider;

    // Public properties (readable, but not modifiable from outside without need)
    public int MergeLevel => _mergeLevel;
    public int PointsOnMerge => _pointsOnMerge; // NEW: Public accessor for points
    public bool IsPlayerControlled => _isPlayerControlled;
    public bool IsBeingMerged => _isBeingMerged;
    public bool HasPhysics => _hasPhysics;


    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        // Ensure initial Rigidbody2D state is correct
        _rb.isKinematic = _isPlayerControlled; // If player-controlled, disable physics
        _rb.gravityScale = _isPlayerControlled ? 0f : 1f; // Disable gravity if controlled
        //_rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Freeze rotation
        _collider.enabled = !_isPlayerControlled; // Collider disabled while player-controlled

        UpdateSprite();
    }

    /// <summary>
    /// Initializes the object after spawning.
    /// </summary>
    /// <param name="level">Merge level (2, 4, 8, ...)</param>
    /// <param name="sprite">Sprite for this level</param>
    /// <param name="points">Points value for this object (awarded when it's created by merge)</param>
    /// <param name="isPlayerControlled">Whether it will be player-controlled (true for new object at top)</param>
    public void Initialize(int level, Sprite sprite, int points, bool isPlayerControlled) // Updated Initialize method
    {
        _mergeLevel = level;
        _objectSprite = sprite;
        _pointsOnMerge = points; // Set the points
        _isPlayerControlled = isPlayerControlled;
        _isBeingMerged = false;
        _hasPhysics = !isPlayerControlled; // If not player-controlled, physics is active

        UpdateSprite();

        // Rigidbody2D and Collider2D settings depending on whether it's player-controlled
        if (_rb == null) _rb = GetComponent<Rigidbody2D>();
        if (_collider == null) _collider = GetComponent<Collider2D>();

        _rb.isKinematic = isPlayerControlled;
        _rb.gravityScale = isPlayerControlled ? 0f : 1f;
        _collider.enabled = !isPlayerControlled; // Enable collider only when object is falling or on board
    }

    /// <summary>
    /// Updates the object's sprite according to _objectSprite.
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
    /// Activates the object's physics when the player "releases" it.
    /// </summary>
    public void ActivatePhysics()
    {
        _isPlayerControlled = false;
        _rb.isKinematic = false; // Enable physics
        _rb.gravityScale = 1f; // Enable gravity
        _collider.enabled = true; // Enable collider
        _hasPhysics = true;
    }

    /// <summary>
    /// Marks the object as being merged to prevent double merges.
    /// </summary>
    public void SetBeingMerged(bool status)
    {
        _isBeingMerged = status;
        // Optionally disable collider temporarily to avoid further collisions during merge animation
        // _collider.enabled = !status; 
    }
}
