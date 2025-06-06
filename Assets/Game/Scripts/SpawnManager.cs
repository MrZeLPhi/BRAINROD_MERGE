using UnityEngine;
using System.Collections.Generic; // Для List

public class SpawnManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform _spawnPoint; // Точка, де спавнитиметься об'єкт гравця
    [SerializeField] private List<MergeableObject> _mergeableObjectPrefabs; // Список префабів MergeableObject, відсортований за рівнем (0: рівень 2, 1: рівень 4, ...)
    [SerializeField] private float _spawnDelay = 0.5f; // Затримка перед спавном нового об'єкта гравця
    
    // Посилання на PlayerController, щоб передати йому новий об'єкт
    private PlayerController _playerController;

    private Coroutine _spawnRoutine; // Для контролю корутини спавну

    private void Awake()
    {
        if (_spawnPoint == null) Debug.LogError("Spawn Point не призначений в SpawnManager!");
        if (_mergeableObjectPrefabs == null || _mergeableObjectPrefabs.Count == 0) Debug.LogError("Префаби MergeableObject не призначені в SpawnManager!");
        
        // Сортуємо префаби за рівнем, якщо ви хочете, щоб вони були гарантовано відсортовані
        _mergeableObjectPrefabs.Sort((a, b) => a.MergeLevel.CompareTo(b.MergeLevel));
    }

    /// <summary>
    /// Спавнить наступний об'єкт для гравця.
    /// </summary>
    /// <param name="playerController">Посилання на PlayerController для передачі об'єкта.</param>
    public void SpawnNextPlayerObject(PlayerController playerController)
    {
        if (_spawnRoutine != null) StopCoroutine(_spawnRoutine); // Зупинити попередній спавн, якщо він ще триває
        _playerController = playerController;
        _spawnRoutine = StartCoroutine(SpawnPlayerObjectRoutine());
    }

    /// <summary>
    /// Спавнить об'єкт після об'єднання двох інших.
    /// </summary>
    /// <param name="position">Позиція, де спавнити новий об'єкт.</param>
    /// <param name="newLevel">Рівень нового об'єкта (наприклад, 8, якщо злилося 4+4).</param>
    public void SpawnMergedObject(Vector3 position, int newLevel)
    {
        MergeableObject prefab = GetPrefabByLevel(newLevel);
        if (prefab != null)
        {
            MergeableObject newObject = Instantiate(prefab, position, Quaternion.identity);
            // Ініціалізуємо об'єкт як такий, що не контролюється гравцем, з активованою фізикою
            newObject.Initialize(newLevel, prefab.GetComponent<SpriteRenderer>().sprite, false); 
            
            // Якщо для з'єднаних об'єктів потрібна 2D фізика (а вона вже є), то вона вже активна через Initialize(..., false)
        }
        else
        {
            Debug.LogWarning($"Не знайдено префаб для рівня {newLevel}. Додайте його в список префабів!");
        }
    }

    /// <summary>
    /// Корутина для спавну нового об'єкта гравця з затримкою.
    /// </summary>
    private System.Collections.IEnumerator SpawnPlayerObjectRoutine()
    {
        // Затримка перед спавном
        yield return new WaitForSeconds(_spawnDelay);

        // Спавн об'єкта найменшого рівня (перший у відсортованому списку)
        MergeableObject prefabToSpawn = _mergeableObjectPrefabs[0]; 
        MergeableObject newObject = Instantiate(prefabToSpawn, _spawnPoint.position, Quaternion.identity);
        
        // Ініціалізуємо об'єкт як такий, що керується гравцем
        newObject.Initialize(prefabToSpawn.MergeLevel, prefabToSpawn.GetComponent<SpriteRenderer>().sprite, true);

        // Передаємо новий об'єкт контролеру гравця
        _playerController.SetCurrentActiveObject(newObject);
    }

    /// <summary>
    /// Знаходить префаб за вказаним рівнем об'єднання.
    /// </summary>
    /// <param name="level">Рівень об'єкта (2, 4, 8, ...)</param>
    /// <returns>Префаб MergeableObject або null, якщо не знайдено.</returns>
    private MergeableObject GetPrefabByLevel(int level)
    {
        foreach (var prefab in _mergeableObjectPrefabs)
        {
            if (prefab.MergeLevel == level)
            {
                return prefab;
            }
        }
        return null;
    }
}
