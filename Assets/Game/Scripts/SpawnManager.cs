using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpawnManager : MonoBehaviour
{
    [System.Serializable]
    public struct SpawnableObject
    {
        public MergeableObject prefab;
        [Range(0, 100)]
        public int spawnWeight;
        [Range(0, 1000)]
        public int pointsValue;
    }

    [Header("Spawn Settings")]
    [Tooltip("Spawn point for ALL new objects (player's & merged)")]
    public Transform _spawnPoint; // <-- ЗМІНА: Зроблено public
    [SerializeField] private List<SpawnableObject> _spawnableObjects;
    [SerializeField] private float _spawnDelay = 0.5f;
    public bool _test = true;

    private PlayerController _playerController;

    private Coroutine _spawnRoutine;

    private MergeableObject _nextPrefabForUI; 

    private void Awake()
    {
        if (_spawnPoint == null) Debug.LogError("Spawn Point is not assigned in SpawnManager!");
        if (_spawnableObjects == null || _spawnableObjects.Count == 0) Debug.LogError("Spawnable Objects list is empty or not assigned in SpawnManager!");
    }

    private void Start()
    {
        SelectRandomSpawnablePrefab();
    }

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
        
        _nextPrefabForUI = _spawnableObjects[0].prefab; 
    }

    public Sprite GetNextPlayerObjectSprite()
    {
        if (_nextPrefabForUI != null)
        {
            return _nextPrefabForUI.GetComponent<SpriteRenderer>().sprite;
        }
        return null;
    }

    public void SpawnNextPlayerObject(PlayerController playerController)
    {
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine);
        _playerController = playerController;
        if(_test == true)
        _spawnRoutine = StartCoroutine(SpawnPlayerObjectRoutine());
    }

    private System.Collections.IEnumerator SpawnPlayerObjectRoutine()
    {
        yield return new WaitForSeconds(_spawnDelay);

        MergeableObject prefabToSpawnThisTurn = _nextPrefabForUI; 

        if (prefabToSpawnThisTurn == null)
        {
            Debug.LogError("No prefab selected to spawn this turn!");
            yield break;
        }

        SelectRandomSpawnablePrefab(); 

        MergeableObject newObject = Instantiate(prefabToSpawnThisTurn, _spawnPoint.position, Quaternion.identity);
        newObject.Initialize(prefabToSpawnThisTurn.MergeLevel, prefabToSpawnThisTurn.GetComponent<SpriteRenderer>().sprite, prefabToSpawnThisTurn.PointsOnMerge, true);
        
        _playerController.SetCurrentActiveObject(newObject);
    }

    public void SpawnMergedObject(Vector3 position, int newLevel, int pointsValue)
    {
        MergeableObject prefab = GetPrefabByLevel(newLevel);
        if (prefab != null)
        {
            Vector3 spawnPosition = new Vector3(position.x, position.y, position.z);
            MergeableObject newObject = Instantiate(prefab, spawnPosition, Quaternion.identity);
            
            newObject.Initialize(newLevel, prefab.GetComponent<SpriteRenderer>().sprite, pointsValue, false); 
        }
        else
        {
            Debug.LogWarning($"Prefab for level {newLevel} not found. Add it to the MergeableObject prefabs list!");
        }
    }

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
}