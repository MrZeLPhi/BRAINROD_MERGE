using UnityEngine;
using System.Collections.Generic;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    [Header("Spawn Limit")]
    [Tooltip("Максимальна кількість об'єктів подій, які можуть бути на сцені одночасно.")]
    public int maxSpawnedObjects = 5;

    [Header("Collision Settings")]
    [Tooltip("Тег, з яким об'єкт буде заморожуватися при зіткненні.")]
    public string freezeOnCollisionTag = "Labubu";
    [Tooltip("Колайдери, з якими об'єкт буде заморожуватися при зіткненні.")]
    public List<Collider2D> freezeOnCollisionColliders;

    private List<GameObject> _spawnedEventObjects = new List<GameObject>();

    public List<GameObject> SpawnedEventObjects => _spawnedEventObjects;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void AddSpawnedObject(GameObject obj)
    {
        if (!_spawnedEventObjects.Contains(obj))
        {
            _spawnedEventObjects.Add(obj);
            Debug.Log($"EventManager: Об'єкт {obj.name} додано до списку. Поточна кількість: {_spawnedEventObjects.Count}");
        }
    }

    public void RemoveSpawnedObject(GameObject obj)
    {
        if (_spawnedEventObjects.Contains(obj))
        {
            _spawnedEventObjects.Remove(obj);
            Debug.Log($"EventManager: Об'єкт {obj.name} видалено зі списку. Поточна кількість: {_spawnedEventObjects.Count}");
        }
    }

    public bool HasReachedSpawnLimit()
    {
        return _spawnedEventObjects.Count >= maxSpawnedObjects;
    }
}