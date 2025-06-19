using UnityEngine;
using System.Collections; // For Coroutine

[RequireComponent(typeof(Collider2D))] // Ensure Collider2D is present
public class MergeDetector : MonoBehaviour
{
    private MergeableObject _mergeableObject; // Reference to its own MergeableObject component
    private SpawnManager _spawnManager; // Reference to SpawnManager

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

        yield return new WaitForSeconds(0.2f); // Small delay for visual effect

        // Calculate the new level
        int newLevel = _mergeableObject.MergeLevel * 2;
        
        // NEW: Calculate points for the new merged object
        // For simplicity, let's say the new object's points are simply its merge level.
        // You can make this more complex (e.g., sum of merged objects' points, or a lookup table).
        int pointsForNewObject = newLevel; 

        // Inform SpawnManager to create a new object
        // Pass the points to SpawnManager so it can initialize the new object with them
        _spawnManager.SpawnMergedObject(mergePoint, newLevel, pointsForNewObject); // Updated call

        // NEW: Add points to the global score. We add the points of the *newly created* object.
        // It's crucial that ScoreManager.Instance is available here.
        if (ScoreManager.Instance != null)
        {
            // Add points based on the *newly formed* object's value
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
}
