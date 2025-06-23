using UnityEngine;
using System.Collections; // For Coroutine

[RequireComponent(typeof(SpriteRenderer))] // Ensures SpriteRenderer is present
[RequireComponent(typeof(Collider2D))] // Ensures Collider2D is present
[RequireComponent(typeof(Rigidbody2D))] // Ensures Rigidbody2D is present
public class MergeDetector : MonoBehaviour
{
    private MergeableObject _mergeableObject; // Reference to its own MergeableObject component
    private SpawnManager _spawnManager; // Reference to SpawnManager

    [Header("Explosion Settings on Merge")]
    [SerializeField] private float _explosionForce = 100f; // Force of the "explosion"
    [SerializeField] private float _explosionRadius = 1.5f; // Radius of the "explosion"
    [SerializeField] private LayerMask _explosionLayerMask; // Layer mask for objects affected by the explosion

    [Header("Merge Effects")]
    [SerializeField] private ParticleSystem _mergeSmokeEffectPrefab; // ParticleSystem prefab for smoke effect on merge
    [SerializeField] private float _effectZOffset = -1f; // Z-offset for the effect to ensure it renders on top

    [Header("Game Over Condition: Time in Trigger")] // NEW HEADER
    [SerializeField] private float _gameOverTimeLimit = 2.0f; // Time in seconds object can stay in trigger before game over
    private float _timeInGameOverTrigger = 0f; // Tracks time spent inside the game over trigger

    // Game Over Trigger Logic using boolean flags (from MergeableObject.cs logic)
    private bool _hasEnteredGameOverTriggerOnce = false; // True if this object has entered the specific game over trigger at least once before
    private bool _isInGameOverTriggerCurrently = false; // True if this object is currently inside the game over trigger

    // Private references to components obtained at start
    private SpriteRenderer _spriteRenderer; // Assuming MergeableObject still has SpriteRenderer reference. This is for self.
    private Rigidbody2D _rb; // Assuming MergeableObject still has Rigidbody2D reference. This is for self.
    private Collider2D _collider; // Assuming MergeableObject still has Collider2D reference. This is for self.

    private void Awake()
    {
        _mergeableObject = GetComponent<MergeableObject>();
        if (_mergeableObject == null)
        {
            Debug.LogError("MergeDetector requires a MergeableObject component on the same GameObject!");
            enabled = false; // Disable script if MergeableObject is missing
        }
        
        // Ensure that _rb and _collider are also initialized here, as MergeableObject might not be the primary script.
        // For this architecture, MergeableObject.cs already has these, so we rely on its properties.
        // If this script were directly on the prefab, we'd add:
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        // Find SpawnManager in the scene. Recommended to be a Singleton or passed reference.
        _spawnManager = FindObjectOfType<SpawnManager>(); 
        if (_spawnManager == null)
        {
            Debug.LogError("SpawnManager not found in scene. MergeDetector will not work!");
            enabled = false;
        }

        // Initialize game over flags and timer
        _hasEnteredGameOverTriggerOnce = false;
        _isInGameOverTriggerCurrently = false;
        _timeInGameOverTrigger = 0f;
    }

    private void Update() // NEW: Added Update method for timer logic
    {
        // Check time spent in game over trigger
        if (_isInGameOverTriggerCurrently && !_mergeableObject.IsPlayerControlled && !_mergeableObject.IsBeingMerged)
        {
            _timeInGameOverTrigger += Time.deltaTime;
            if (_timeInGameOverTrigger >= _gameOverTimeLimit)
            {
                Debug.Log($"Object {_mergeableObject.gameObject.name} stayed in GameOverTrigger for too long ({_timeInGameOverTrigger}s). Triggering Game Over!");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
                else
                {
                    Debug.LogWarning("GameManager.Instance is not available. Cannot trigger Game Over from timer.");
                }
            }
        }
    }


    // This will trigger when two objects with Collider2D collide
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if we are already in the process of merging
        if (_mergeableObject.IsBeingMerged) return;

        // Get the MergeableObject component from the colliding object
        MergeableObject otherMergeableObject = collision.gameObject.GetComponent<MergeableObject>();

        // Check if it collided with another MergeableObject
        if (otherMergeableObject != null)
        {
            // Check if the other object is also not in the process of merging
            if (otherMergeableObject.IsBeingMerged) return;

            // Check if the merge levels are the same
            if (_mergeableObject.MergeLevel == otherMergeableObject.MergeLevel)
            {
                // Start the merge coroutine
                StartCoroutine(HandleMergeRoutine(otherMergeableObject, collision.contacts[0].point));
            }
        }
    }

    /// <summary>
    /// Coroutine to handle the merging process of two objects.
    /// </summary>
    /// <param name="otherObject">The other object being merged with.</param>
    /// <param name="mergePoint">The collision point for spawning the new object.</param>
    private IEnumerator HandleMergeRoutine(MergeableObject otherObject, Vector2 mergePoint)
    {
        // Mark both objects as being merged to prevent further collisions
        _mergeableObject.SetBeingMerged(true);
        otherObject.SetBeingMerged(true);

        // Temporarily disable colliders to avoid physics bugs during animation
        GetComponent<Collider2D>().enabled = false;
        otherObject.GetComponent<Collider2D>().enabled = false;

        // ==== Add visual and sound effects for merging here ====
        Debug.Log($"Objects of level {_mergeableObject.MergeLevel} are merging!");

        // Play merge smoke effect
        if (_mergeSmokeEffectPrefab != null)
        {
            // Ensure correct Z-position for rendering order in 2D
            Vector3 effectSpawnPosition = new Vector3(mergePoint.x, mergePoint.y, _effectZOffset); // Use Z-offset
            ParticleSystem smokeInstance = Instantiate(_mergeSmokeEffectPrefab, effectSpawnPosition, Quaternion.identity);
            
            // Ensure the GameObject and ParticleSystem component are active
            smokeInstance.gameObject.SetActive(true); 
            smokeInstance.Play();
            
            // Destroy effect after it finishes. Add a small buffer just in case.
            Destroy(smokeInstance.gameObject, smokeInstance.main.duration + 0.1f); 
        }

        // Apply explosion force to surrounding objects BEFORE spawning the new one
        ApplyExplosionForce(mergePoint);

        yield return new WaitForSeconds(0.2f); // Small delay for visual effect and explosion to take effect

        // Calculate the new level
        int newLevel = _mergeableObject.MergeLevel * 2;
        
        // Calculate points for the new merged object
        int pointsForNewObject = newLevel; 

        // Inform SpawnManager to create a new object
        _spawnManager.SpawnMergedObject(mergePoint, newLevel, pointsForNewObject); // Updated call

        // Add points to the global score.
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddPoints(pointsForNewObject); 
        }
        else
        {
            Debug.LogWarning("ScoreManager.Instance is not available. Cannot add points.");
        }

        // Destroy the original objects
        Destroy(otherObject.gameObject);
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Applies an explosion force to nearby Rigidbody2D objects.
    /// </summary>
    /// <param name="explosionOrigin">The center point of the explosion.</param>
    private void ApplyExplosionForce(Vector2 explosionOrigin)
    {
        // Find all colliders within the explosion radius on specified layers
        Collider2D[] colliders = Physics2D.OverlapCircleAll(explosionOrigin, _explosionRadius, _explosionLayerMask);

        foreach (Collider2D hitCollider in colliders)
        {
            Rigidbody2D rb = hitCollider.GetComponent<Rigidbody2D>();
            MergeableObject mergeObj = hitCollider.GetComponent<MergeableObject>();

            // Ensure it's a Rigidbody2D that is not kinematic (i.e., physics-controlled)
            // And ensure it's not one of the objects currently being merged
            if (rb != null && !rb.isKinematic && mergeObj != null && !mergeObj.IsBeingMerged)
            {
                Vector2 direction = hitCollider.transform.position - (Vector3)explosionOrigin;
                float distance = direction.magnitude;

                // Calculate force falloff based on distance
                float forceMultiplier = 1 - (distance / _explosionRadius);
                if (forceMultiplier < 0) forceMultiplier = 0; // Ensure positive force

                rb.AddForce(direction.normalized * _explosionForce * forceMultiplier, ForceMode2D.Impulse);
                Debug.Log($"Applied explosion force to {hitCollider.gameObject.name}");
            }
        }
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
            _timeInGameOverTrigger = 0f; // NEW: Reset timer on entry

            // If this is not the first time it has entered AND it's not being player controlled AND not merging, trigger game over.
            if (_hasEnteredGameOverTriggerOnce && !_mergeableObject.IsPlayerControlled && !_mergeableObject.IsBeingMerged)
            {
                Debug.Log($"Object {_mergeableObject.gameObject.name} entered GameOverTrigger for the SECOND time and is not player controlled. Triggering Game Over!");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.GameOver();
                }
                else
                {
                    Debug.LogWarning("GameManager.Instance is not available. Cannot trigger Game Over from second entry.");
                }
            }
            else if (!_hasEnteredGameOverTriggerOnce)
            {
                // This is the first time it enters. Just mark it.
                _hasEnteredGameOverTriggerOnce = true;
                Debug.Log($"Object {_mergeableObject.gameObject.name} entered GameOverTrigger for the FIRST time.");
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
            _timeInGameOverTrigger = 0f; // NEW: Reset timer on exit
            Debug.Log($"Object {_mergeableObject.gameObject.name} exited GameOverTrigger.");
        }
    }
}
