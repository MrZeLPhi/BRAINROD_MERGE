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
    [SerializeField] private int _pointsOnMerge = 0; // Points awarded when this object is CREATED by a merge
    [SerializeField] private bool _isPlayerControlled = true; // Whether it follows the slider
    [SerializeField] private bool _isBeingMerged = false; // Whether it's in the process of merging
    [SerializeField] private bool _hasPhysics = false; // Whether physics is active (after release)

    // NEW: Game Over Trigger Logic using boolean flags
    private bool _hasEnteredGameOverTriggerOnce = false; // True if this object has entered the specific game over trigger at least once before
    private bool _isInGameOverTriggerCurrently = false; // True if this object is currently inside the game over trigger

    // NEW: Fall Physics Settings
    [Header("Fall Physics Settings")]
    [SerializeField] private float _fallGravityScale = 1f; // Gravity scale when the object is falling (after release)
    [SerializeField] private float _fallLinearDrag = 0.05f; // Linear drag when the object is falling (adds "air resistance")

    // Private references to components obtained at start
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;
    private Collider2D _collider;

    // Public properties (readable, but not modifiable from outside without need)
    public int MergeLevel => _mergeLevel;
    public int PointsOnMerge => _pointsOnMerge; // Public accessor for points
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
        _rb.gravityScale = _isPlayerControlled ? 0f : _fallGravityScale; // Apply fallGravityScale if not player-controlled
        _rb.linearDamping = _fallLinearDrag; // Apply drag on Awake
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
    public void Initialize(int level, Sprite sprite, int points, bool isPlayerControlled)
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
        _rb.gravityScale = isPlayerControlled ? 0f : _fallGravityScale; // Apply fallGravityScale if not player-controlled
        _rb.linearDamping = _fallLinearDrag; // Apply drag on Initialize
        _collider.enabled = !isPlayerControlled; // Enable collider only when object is falling or on board

        // Reset game over flags for new objects
        _hasEnteredGameOverTriggerOnce = false;
        _isInGameOverTriggerCurrently = false;
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
        _rb.gravityScale = _fallGravityScale; // Apply fallGravityScale
        _rb.linearDamping = _fallLinearDrag; // Apply fallLinearDrag
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

    /// <summary>
    /// Handles trigger entry events.
    /// </summary>
    /// <param name="other">The other Collider2D involved in the collision.</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider has the "GameOverTrigger" tag
        if (other.CompareTag("GameOverTrigger"))
        {
            if (_isInGameOverTriggerCurrently) return; // Already inside the zone

            _isInGameOverTriggerCurrently = true; // Mark as currently inside the zone

            // If this is not the first time it has entered AND it's not being player controlled AND not merging, trigger game over.
            if (_hasEnteredGameOverTriggerOnce && !_isPlayerControlled && !_isBeingMerged)
            {
                Debug.Log($"Object {gameObject.name} entered GameOverTrigger for the SECOND time and is not player controlled. Triggering Game Over!");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
                else
                {
                    Debug.LogWarning("GameManager.Instance is not available. Cannot trigger Game Over.");
                }
            }
            else if (!_hasEnteredGameOverTriggerOnce)
            {
                // This is the first time it enters. Just mark it.
                _hasEnteredGameOverTriggerOnce = true;
                Debug.Log($"Object {gameObject.name} entered GameOverTrigger for the FIRST time.");
            }
        }
    }

    /// <summary>
    /// Handles trigger exit events.
    /// </summary>
    /// <param name="other">The other Collider2D involved in the collision.</param>
    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the collider has the "GameOverTrigger" tag
        if (other.CompareTag("GameOverTrigger"))
        {
            _isInGameOverTriggerCurrently = false; // Mark as no longer inside the zone
            Debug.Log($"Object {gameObject.name} exited GameOverTrigger.");
        }
    }
}
