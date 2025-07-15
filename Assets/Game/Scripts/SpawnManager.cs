using UnityEngine;
using System.Collections.Generic; // For List
using System.Linq; // For OrderBy

public class SpawnManager : MonoBehaviour
{
    [System.Serializable] // Makes this struct visible in the Inspector
    public struct SpawnableObject
    {
        public MergeableObject prefab;
        [Range(0, 100)] // Rarity as a percentage or weight
        public int spawnWeight; // Higher weight means higher chance to spawn
        [Range(0, 1000)] // Points awarded when this object is created (for merged objects)
        public int pointsValue; // Points value for this specific prefab
    }

    [Header("Spawn Settings")]
    [SerializeField] private Transform _spawnPoint; // Spawn point for ALL new objects (player's & merged)
    [SerializeField] private List<SpawnableObject> _spawnableObjects; // List of objects that can be spawned for the player, with their weights
    [SerializeField] private float _spawnDelay = 0.5f; // Delay before spawning a new player object
    [SerializeField] private bool _test = true;
    
    // Removed: Play Area Bounds (for finding free spawn spot) - no longer needed

    // Reference to PlayerController to pass the new object
    private PlayerController _playerController;

    private Coroutine _spawnRoutine; // For controlling the spawn coroutine

    // The prefab of the next object to be spawned FOR THE PLAYER (this is what the UI shows)
    private MergeableObject _nextPrefabForUI; 

    private void Awake()
    {
        if (_spawnPoint == null) Debug.LogError("Spawn Point is not assigned in SpawnManager!");
        if (_spawnableObjects == null || _spawnableObjects.Count == 0) Debug.LogError("Spawnable Objects list is empty or not assigned in SpawnManager!");
        
        // Ensure that spawn weights are set up correctly, e.g., sum up to 100 or adjust internally.
    }

    private void Start()
    {
        // On game start, pre-select the first object that will appear
        // This is the object that will be displayed in the "next object" UI initially
        SelectRandomSpawnablePrefab();
    }

    /// <summary>
    /// Selects a random prefab from _spawnableObjects based on their weights.
    /// This selected prefab becomes the _nextPrefabForUI.
    /// </summary>
    private void SelectRandomSpawnablePrefab()
    {
        if (_spawnableObjects.Count == 0)
        {
            _nextPrefabForUI = null;
            Debug.LogWarning("No spawnable objects available in the list to select from!");
            return;
        }

        int totalWeight = 0;
        foreach (var obj in _spawnableObjects)
        {
            totalWeight += obj.spawnWeight;
        }

        if (totalWeight == 0)
        {
            Debug.LogWarning("Total spawn weight is zero. Cannot select a random object based on weight. Defaulting to the first object.");
            _nextPrefabForUI = _spawnableObjects[0].prefab;
            return;
        }

        int randomWeight = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var obj in _spawnableObjects)
        {
            currentWeight += obj.spawnWeight;
            if (randomWeight < currentWeight)
            {
                _nextPrefabForUI = obj.prefab;
                return;
            }
        }
        
        // Fallback in case something goes wrong (shouldn't happen with correct weights)
        _nextPrefabForUI = _spawnableObjects[0].prefab; 
    }

    /// <summary>
    /// Returns the sprite of the next object that will be spawned for the player.
    /// Used for UI display.
    /// </summary>
    public Sprite GetNextPlayerObjectSprite()
    {
        if (_nextPrefabForUI != null)
        {
            return _nextPrefabForUI.GetComponent<SpriteRenderer>().sprite;
        }
        return null;
    }

    /// <summary>
    /// Starts the coroutine to spawn the next player object.
    /// </summary>
    /// <param name="playerController">Reference to PlayerController to pass the object.</param>
    public void SpawnNextPlayerObject(PlayerController playerController)
    {
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine); // Stop previous spawn if it's still running
        _playerController = playerController;
        if(_test == true)
        _spawnRoutine = StartCoroutine(SpawnPlayerObjectRoutine());
    }

    /// <summary>
    /// Coroutine for spawning a new player object with a delay.
    /// </summary>
    private System.Collections.IEnumerator SpawnPlayerObjectRoutine()
    {
        yield return new WaitForSeconds(_spawnDelay);

        // This is the prefab we are spawning NOW (which was the "next" one until now)
        MergeableObject prefabToSpawnThisTurn = _nextPrefabForUI; 

        if (prefabToSpawnThisTurn == null)
        {
            Debug.LogError("No prefab selected to spawn this turn!");
            yield break;
        }

        // BEFORE spawning the current object, select the *actual next* object for the UI
        SelectRandomSpawnablePrefab(); // Updates _nextPrefabForUI to the *next* object

        // Spawn the object that was designated for *this* turn
        // For player-controlled objects, we don't need to find a free spot, as PlayerController moves it.
        // It will start at _spawnPoint.position.
        MergeableObject newObject = Instantiate(prefabToSpawnThisTurn, _spawnPoint.position, Quaternion.identity);
        newObject.Initialize(prefabToSpawnThisTurn.MergeLevel, prefabToSpawnThisTurn.GetComponent<SpriteRenderer>().sprite, prefabToSpawnThisTurn.PointsOnMerge, true);
        
        // Pass the new object to the player controller.
        // PlayerController.SetCurrentActiveObject will then call UpdateNextObjectUI,
        // which will now correctly get the *newly selected* _nextPrefabForUI.
        _playerController.SetCurrentActiveObject(newObject);
    }

    /// <summary>
    /// Spawns an object after two others have merged.
    /// </summary>
    /// <param name="position">Position (X-coordinate from the merge) where the new object will spawn.</param>
    /// <param name="newLevel">Level of the new object (e.g., 8 if 4+4 merged).</param>
    /// <param name="pointsValue">The points value for this newly created merged object.</param>
    public void SpawnMergedObject(Vector3 position, int newLevel, int pointsValue)
    {
        MergeableObject prefab = GetPrefabByLevel(newLevel);
        if (prefab != null)
        {
            // Spawn the merged object directly at the merge point's position.
            // This is the desired behavior for spawning at the collision point.
            Vector3 spawnPosition = new Vector3(position.x, position.y, position.z); // Use position.y directly
            MergeableObject newObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
            
            // Initialize the object as not player controlled, with physics activated
            newObject.Initialize(newLevel, prefab.GetComponent<SpriteRenderer>().sprite, pointsValue, false); 
        }
        else
        {
            Debug.LogWarning($"Prefab for level {newLevel} not found. Add it to the MergeableObject prefabs list!");
        }
    }

    /// <summary>
    /// Finds a prefab by the specified merge level.
    /// Used for spawning merged objects.
    /// </summary>
    /// <param name="level">Object level (2, 4, 8, ...)</param>
    /// <returns>MergeableObject prefab or null if not found.</returns>
    private MergeableObject GetPrefabByLevel(int level)
    {
        foreach (var spawnable in _spawnableObjects)
        {
            if (spawnable.prefab != null && spawnable.prefab.MergeLevel == level)
            {
                return spawnable.prefab;
            }
        }
        return null;
    }

    // Removed: FindFreeSpawnPosition method and related fields - no longer needed
}
