using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody2D))]
public class EventPrefabHandler : MonoBehaviour
{
    private Rigidbody2D _rb;
    private EventSequenceManager _eventManager;
    private List<Collider2D> _sceneColliders;
    private List<Collider2D> _impulseTriggerColliders;
    private bool _hasReceivedImpulse = false;

    private float _minImpulseForce;
    private float _maxImpulseForce;
    
    private int _initialSpawnState;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Setup(EventSequenceManager manager, List<Collider2D> sceneBoundsColliders, List<Collider2D> impulseTriggerColliders, float minForce, float maxForce, int initialSpawnState)
    {
        _eventManager = manager;
        _sceneColliders = sceneBoundsColliders;
        _impulseTriggerColliders = impulseTriggerColliders;
        _minImpulseForce = minForce;
        _maxImpulseForce = maxForce;
        _initialSpawnState = initialSpawnState;
        
        if (sceneBoundsColliders != null && sceneBoundsColliders.Count > 0)
        {
            foreach (Collider2D sceneCollider in _sceneColliders)
            {
                Physics2D.IgnoreCollision(GetComponent<Collider2D>(), sceneCollider, true);
            }
        }
    }

    // --- НОВИЙ МЕТОД: Обробка зіткнень ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (EventManager.Instance != null && _rb != null)
        {
            bool shouldFreeze = false;
            // Перевіряємо, чи зіткнулися з Labubu
            if (collision.gameObject.CompareTag(EventManager.Instance.freezeOnCollisionTag))
            {
                shouldFreeze = true;
            }
            // Перевіряємо, чи зіткнулися з одним з вибраних колайдерів
            if (EventManager.Instance.freezeOnCollisionColliders.Contains(collision.collider))
            {
                shouldFreeze = true;
            }
            // Перевіряємо, чи зіткнулися з самим собою (іншим EventPrefabHandler)
            if (collision.gameObject.GetComponent<EventPrefabHandler>() != null)
            {
                shouldFreeze = true;
            }

            if (shouldFreeze)
            {
                // Заморожуємо фізику
                _rb.linearVelocity = Vector2.zero;
                _rb.angularVelocity = 0;
                _rb.isKinematic = true; 
                
                Debug.Log($"EventPrefabHandler: Об'єкт {gameObject.name} заморожено після зіткнення з {collision.gameObject.name}.");
                enabled = false; 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_impulseTriggerColliders != null && _impulseTriggerColliders.Contains(other) && !_hasReceivedImpulse)
        {
            if (_rb != null)
            {
                float randomForce = Random.Range(_minImpulseForce, _maxImpulseForce);
                
                Vector2 direction = (Vector2)transform.position - Vector2.zero;
                _rb.AddForce(direction.normalized * randomForce * -1f, ForceMode2D.Impulse);

                foreach (Collider2D sceneCollider in _sceneColliders)
                {
                    Physics2D.IgnoreCollision(GetComponent<Collider2D>(), sceneCollider, false);
                }

                _hasReceivedImpulse = true;
            }
            
            if (_eventManager != null)
            {
                _eventManager.OnPrefabActionComplete(_initialSpawnState);
            }
        }
    }
    
    public void OnDestroy()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.RemoveSpawnedObject(gameObject);
        }
    }
}