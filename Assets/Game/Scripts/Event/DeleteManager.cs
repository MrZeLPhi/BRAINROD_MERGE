using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Linq;

public class DeleteManager : MonoBehaviour
{
    public static DeleteManager Instance { get; private set; }

    [Header("Deletion Settings")]
    [Tooltip("Мінімальна кількість об'єктів для видалення за один раз.")]
    public int minObjectsToDelete = 1;
    [Tooltip("Максимальна кількість об'єктів для видалення за один раз.")]
    public int maxObjectsToDelete = 2;
    [Tooltip("Затримка (у секундах) між видаленням кожного об'єкта.")]
    public float deletionDelay = 0.5f;

    // Прапорець, щоб уникнути повторного запуску, поки міні-гра триває
    private bool _isWaitingForShake = false;

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

    // Підписка на подію, коли скрипт стає активним
    void OnEnable()
    {
        ShakeController.OnShakeSequenceEnded += OnShakeEnded;
    }

    // Відписка від події, коли скрипт стає неактивним або знищується
    void OnDisable()
    {
        ShakeController.OnShakeSequenceEnded -= OnShakeEnded;
    }
    
    // Метод-обробник події, який викликається, коли міні-гра завершилася
    private void OnShakeEnded()
    {
        // Якщо ми чекали закінчення міні-гри, запускаємо нашу послідовність
        if (_isWaitingForShake)
        {
            StartCoroutine(DeleteObjectsRoutine());
            _isWaitingForShake = false; // Скидаємо прапорець
        }
    }

    public void StartDeletionSequence()
    {
        // Перевіряємо, чи запущена міні-гра тряски
        if (ShakeController.Instance != null && ShakeController.Instance.IsSequenceRunning)
        {
            Debug.Log("DeleteManager: ShakeController працює. Чекаю на завершення...");
            _isWaitingForShake = true; // Встановлюємо прапорець очікування
            return;
        }

        // Якщо міні-гра не запущена, починаємо відразу
        StartCoroutine(DeleteObjectsRoutine());
    }

    private IEnumerator DeleteObjectsRoutine()
    {
        if (EventManager.Instance == null)
        {
            Debug.LogError("DeleteManager: EventManager не знайдено. Неможливо отримати список об'єктів.");
            yield break;
        }
        
        int objectsToDeleteCount = Random.Range(minObjectsToDelete, maxObjectsToDelete + 1);
        
        List<GameObject> allSpawnedObjects = EventManager.Instance.SpawnedEventObjects.ToList();
        
        List<GameObject> objectsToRemove = new List<GameObject>();
        for (int i = 0; i < objectsToDeleteCount && allSpawnedObjects.Count > 0; i++)
        {
            int randomIndex = Random.Range(0, allSpawnedObjects.Count);
            objectsToRemove.Add(allSpawnedObjects[randomIndex]);
            allSpawnedObjects.RemoveAt(randomIndex);
        }
        
        foreach (GameObject obj in objectsToRemove)
        {
            if (obj != null)
            {
                EventManager.Instance.RemoveSpawnedObject(obj);
                Destroy(obj);
                yield return new WaitForSeconds(deletionDelay);
            }
        }
        
        Debug.Log($"DeleteManager: Видалено {objectsToRemove.Count} об'єктів.");
        
        if (EventSequenceManager.Instance != null && EventManager.Instance.SpawnedEventObjects.Count < EventManager.Instance.maxSpawnedObjects)
        {
            EventSequenceManager.Instance.StartEventCycle();
        }
    }
}