using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using System.Linq;
using System;

public class EventSequenceManager : MonoBehaviour
{
    public static EventSequenceManager Instance { get; private set; }

    [Header("Event Settings")]
    [Tooltip("Мінімальний час (у секундах) між подіями.")]
    public float minEventInterval = 5f;
    [Tooltip("Максимальний час (у секундах) між подіями.")]
    public float maxEventInterval = 10f;
    [Tooltip("Колайдери сцени, які потрібно ігнорувати.")]
    public List<Collider2D> sceneBoundsColliders; 
    [Tooltip("Колайдери, які запускають імпульс (повинні бути тригерами).")]
    public List<Collider2D> impulseTriggerColliders; 

    [Header("Spawn Points")]
    [Tooltip("Список точок спавну для об'єктів.")]
    public List<Transform> spawnPoints; 

    [Header("Event Prefabs")]
    [Tooltip("Список асетів EventPrefabData, з яких буде вибиратися.")]
    public List<EventPrefabData> eventPrefabDataList; 
    
    [Header("Animation Settings")]
    [Tooltip("Animator для керування анімацією подій.")]
    public Animator eventAnimator;
    [Tooltip("Тривалість анімації появи (у секундах).")]
    public float eventAnimationDuration = 2f;
    [Tooltip("Тривалість анімації повернення (у секундах).")]
    public float eventReturnAnimationDuration = 2f;
    [Tooltip("Назва параметра Int в Animator для зміни стану.")]
    public string eventStateParameterName = "EventState";

    private bool _isEventRunning = false;
    public bool IsEventRunning => _isEventRunning;
    private int _initialEventState = 0; 


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

    void Start()
    {
        StartCoroutine(MainEventLoopCoroutine());
    }

    private IEnumerator MainEventLoopCoroutine()
    {
        while (true)
        {
            if (EventManager.Instance != null && EventManager.Instance.HasReachedSpawnLimit())
            {
                 Debug.Log($"EventSequenceManager: Досягнуто ліміту спавну ({EventManager.Instance.maxSpawnedObjects}). Очікую, поки об'єкти звільнять місце.");
                 yield return new WaitUntil(() => EventManager.Instance.SpawnedEventObjects.Count < EventManager.Instance.maxSpawnedObjects);
            }

            float delay = Random.Range(minEventInterval, maxEventInterval);
            yield return new WaitForSeconds(delay);

            StartCoroutine(EventSequenceCoroutine());
        }
    }
    
    private IEnumerator EventSequenceCoroutine()
    {
        if (_isEventRunning) yield break;

        _isEventRunning = true;
        
        _initialEventState = Random.Range(1, 3);
        
        if (eventAnimator != null)
        {
            eventAnimator.SetInteger(eventStateParameterName, _initialEventState);
            Debug.Log($"EventSequenceManager: Запущено анімацію стану {_initialEventState}.");
        }

        yield return new WaitForSeconds(eventAnimationDuration);
        
        // ==== ДОДАНО УМОВУ ОЧІКУВАННЯ ТУТ ====
        if (ShakeController.Instance != null && ShakeController.Instance.IsSequenceRunning)
        {
            Debug.Log("EventSequenceManager: ShakeController працює. Очікую завершення перед спавном.");
            yield return new WaitUntil(() => !ShakeController.Instance.IsSequenceRunning);
            Debug.Log("EventSequenceManager: ShakeController завершив роботу. Продовжую спавн.");
        }
        // ===================================

        Transform spawnPoint = null;
        if (spawnPoints.Count >= 2)
        {
            if (_initialEventState == 1)
            {
                spawnPoint = spawnPoints[1]; 
            }
            else
            {
                spawnPoint = spawnPoints[0]; 
            }
        }
        else
        {
            Debug.LogError("EventSequenceManager: Список точок спавну має містити щонайменше 2 елементи.");
            EndEventSequence();
            yield break;
        }

        EventPrefabData dataToSpawn = SelectRandomPrefabByWeight();
        if (dataToSpawn != null && spawnPoint != null)
        {
            Debug.Log($"EventSequenceManager: Анімація завершена, спавню об'єкт {dataToSpawn.prefab.name}.");
            SpawnEventPrefab(spawnPoint.position, dataToSpawn);
        }
        else
        {
            Debug.LogError("EventSequenceManager: Не вдалося спавнити префаб (dataToSpawn == null).");
            EndEventSequence();
            yield break;
        }
    }
    
    private EventPrefabData SelectRandomPrefabByWeight()
    {
        if (eventPrefabDataList.Count == 0) return null;
        
        int totalWeight = eventPrefabDataList.Sum(data => data.spawnWeight);
        int randomWeight = Random.Range(0, totalWeight);
        int currentWeight = 0;

        foreach (var data in eventPrefabDataList)
        {
            currentWeight += data.spawnWeight;
            if (randomWeight < currentWeight)
            {
                return data;
            }
        }
        
        return null;
    }

    public void OnPrefabActionComplete(int initialSpawnState)
    {
        Debug.Log("EventSequenceManager: Отримано зворотний виклик від префаба.");

        int nextAnimationState = (initialSpawnState == 1) ? 3 : 4;
        if (eventAnimator != null)
        {
            eventAnimator.SetInteger(eventStateParameterName, nextAnimationState);
            Debug.Log($"EventSequenceManager: Запущено анімацію повернення стану {nextAnimationState}.");
        }
        
        StartCoroutine(EndEventWithDelay());
    }

    private IEnumerator EndEventWithDelay()
    {
        yield return new WaitForSeconds(2f);
        EndEventSequence();
    }
    
    private void EndEventSequence()
    {
        _isEventRunning = false;
        
        if (eventAnimator != null)
        {
            eventAnimator.SetInteger(eventStateParameterName, 0); 
        }
    }
    
    private void SpawnEventPrefab(Vector3 position, EventPrefabData dataToSpawn)
    {
        if (dataToSpawn.prefab == null)
        {
            Debug.LogError("EventSequenceManager: Префаб в асеті даних порожній.");
            return;
        }
        
        GameObject newObject = Instantiate(dataToSpawn.prefab, position, Quaternion.identity);

        EventPrefabHandler handler = newObject.GetComponent<EventPrefabHandler>();
        if (handler != null)
        {
            handler.Setup(this, sceneBoundsColliders, impulseTriggerColliders, dataToSpawn.minImpulseForce, dataToSpawn.maxImpulseForce, _initialEventState);
        }
        else
        {
            Debug.LogError("EventSequenceManager: Скрипт EventPrefabHandler не знайдено на префабі! Об'єкт не буде налаштовано.");
        }

        if (EventManager.Instance != null)
        {
            EventManager.Instance.AddSpawnedObject(newObject);
        }
    }
    
    public void StartEventCycle()
    {
        StartCoroutine(EventSequenceCoroutine());
    }
}