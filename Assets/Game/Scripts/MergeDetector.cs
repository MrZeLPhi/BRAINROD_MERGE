using UnityEngine;
using System.Collections; // For Coroutine

[RequireComponent(typeof(Collider2D))] // Ensure Collider2D is present
public class MergeDetector : MonoBehaviour
{
    private MergeableObject _mergeableObject; // Reference to its own MergeableObject component
    private SpawnManager _spawnManager; // Reference to SpawnManager

    [Header("Explosion Settings on Merge")]
    [SerializeField] private float _explosionForce = 100f; // Сила "вибуху"
    [SerializeField] private float _explosionRadius = 1.5f; // Радіус дії "вибуху"
    [SerializeField] private LayerMask _explosionLayerMask; // Маска шарів для об'єктів, на які діє вибух

    [Header("Merge Effects")]
    [SerializeField] private ParticleSystem _mergeSmokeEffectPrefab; // NEW: ParticleSystem prefab for smoke effect on merge

    private void Awake()
    {
        _mergeableObject = GetComponent<MergeableObject>();
        if (_mergeableObject == null)
        {
            Debug.LogError("MergeDetector requires a MergeableObject component on the same GameObject!");
            enabled = false; // Disable script if MergeableObject is missing
        }
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

        // NEW: Play merge smoke effect
        if (_mergeSmokeEffectPrefab != null)
        {
            ParticleSystem smokeInstance = Instantiate(_mergeSmokeEffectPrefab, mergePoint, Quaternion.identity);
            smokeInstance.Play();
            Destroy(smokeInstance.gameObject, smokeInstance.main.duration); // Destroy effect after it finishes
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
}
