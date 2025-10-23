using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;
using System.Collections.Generic;
using DG.Tweening; 
using System;
using Random = UnityEngine.Random;

public class ShakeController : MonoBehaviour
{
    public static ShakeController Instance { get; private set; }

    public static event Action OnShakeSequenceStarted;
    public static event Action OnShakeSequenceEnded;
    
    public bool IsSequenceRunning => _isSequenceRunning;

    [Header("Dependencies")]
    public SpawnManager spawnManagerRef; 
    public BoxCollider2D targetCollider;

    [Tooltip("Список MonoBehaviour скриптів, які потрібно вимкнути на час міні-гри (наприклад, PlayerController, EnemyAI).")]
    public List<MonoBehaviour> gameLogicScriptsToPause; 

    [Header("UI References")]
    public GameObject shakeMinigamePanel;
    public TextMeshProUGUI countdownText; 
    public Animator shakeAnimator; 
    public string startAnimationTrigger = "StartAnimation";
    public string endAnimationTrigger = "EndAnimation";

    [Header("Minigame Timings")]
    public float preShakeCountdownTime = 3f;
    public float activeShakeWindowDuration = 3f; 

    [Header("Shake Physics Settings")]
    public Camera mainCamera;
    public float shakeDampening = 5f; 
    private Vector3 _originalCameraLocalPos; 

    public float shakeSensitivity = 0.05f; 
    public float shakeThreshold = 0.5f; 
    public string shakeableObjectsTag = "Labubu"; 
    public float objectShakeImpulseForce = 0.5f;

    private Vector3 _lastAcceleration = Vector3.zero; 
    private Vector3 _currentCameraShakeOffset = Vector3.zero; 
    private bool _isShakeInputActive = false; 
    private bool _isSequenceRunning = false; 

    private bool _originalSpawnManagerTestValue;
    private bool _originalColliderIsTriggerValue; 
    private Dictionary<MonoBehaviour, bool> _originalScriptStates = new Dictionary<MonoBehaviour, bool>(); 


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        if (mainCamera != null)
        {
            _originalCameraLocalPos = mainCamera.transform.localPosition;
        }
        else
        {
            Debug.LogError("ShakeController: Головна камера не призначена або не знайдена в Awake.");
        }

        if (shakeMinigamePanel != null)
        {
            shakeMinigamePanel.SetActive(false);
        }
    }

    void Update()
    {
        if (!_isShakeInputActive && mainCamera != null && _currentCameraShakeOffset.magnitude > 0.001f)
        {
             // Ця частина також має працювати, навіть якщо час не заморожений
             _currentCameraShakeOffset = Vector3.Lerp(_currentCameraShakeOffset, Vector3.zero, Time.deltaTime * shakeDampening * 2); 
             mainCamera.transform.localPosition = _originalCameraLocalPos + _currentCameraShakeOffset;
        }
        else if (!_isShakeInputActive && mainCamera != null && _currentCameraShakeOffset.magnitude <= 0.001f && mainCamera.transform.localPosition != _originalCameraLocalPos)
        {
            mainCamera.transform.localPosition = _originalCameraLocalPos;
        }

        // Ця умова тепер перевіряє тільки _isShakeInputActive, щоб не залежати від Time.timeScale
        if (!_isShakeInputActive || mainCamera == null || !Application.isMobilePlatform)
        {
            return;
        }

        Vector3 currentAcceleration = Input.acceleration;
        Vector3 deltaAcceleration = currentAcceleration - _lastAcceleration;
        
        _lastAcceleration = currentAcceleration;

        if (deltaAcceleration.magnitude > shakeThreshold) 
        {
            ApplyShakeToLabubuObjects(deltaAcceleration);

            _currentCameraShakeOffset += (Vector3)Random.insideUnitCircle * (deltaAcceleration.magnitude * shakeSensitivity); 
        }
    }

    public void StartShakeSequence() 
    {
        if (_isSequenceRunning)
        {
            Debug.LogWarning("ShakeController: Shake sequence is already running. Ignoring new request.");
            return;
        }

        if (spawnManagerRef == null || targetCollider == null || shakeMinigamePanel == null || shakeAnimator == null || countdownText == null)
        {
            Debug.LogError("ShakeController: Missing required references. Please assign all fields in Inspector. Aborting.");
            return;
        }
        
        _isSequenceRunning = true;
        Debug.Log("ShakeController: Shake Sequence Started!");

        // Пауза більше не потрібна. Просто вимикаємо логіку
        Debug.Log("ShakeController: Game not paused, but game logic scripts are disabled.");
        OnShakeSequenceStarted?.Invoke();

        if (spawnManagerRef != null)
        {
            _originalSpawnManagerTestValue = spawnManagerRef._test; 
            spawnManagerRef._test = false; 
            Debug.Log($"ShakeController: Збережено SpawnManager._test: {_originalSpawnManagerTestValue}. Встановлено на false."); 
        }
        else
        {
            Debug.LogError("ShakeController: SpawnManagerRef не призначено."); 
        }

        if (targetCollider != null)
        {
            _originalColliderIsTriggerValue = targetCollider.isTrigger; 
            Debug.Log($"ShakeController: Збережено targetCollider.isTrigger: {_originalColliderIsTriggerValue}."); 
        }
        else
        {
            Debug.LogError("ShakeController: TargetCollider не призначено."); 
        }

        _originalScriptStates.Clear();
        foreach (MonoBehaviour script in gameLogicScriptsToPause)
        {
            if (script != null)
            {
                _originalScriptStates[script] = script.enabled; 
                script.enabled = false; // Вимикаємо скрипти логіки
                Debug.Log($"ShakeController: Скрипт {script.name} вимкнено. Оригінальний стан: {_originalScriptStates[script]}"); 
            }
            else
            {
                Debug.LogWarning("ShakeController: gameLogicScriptsToPause містить NULL скрипт. Будь ласка, перевірте Inspector."); 
            }
        }
        
        shakeMinigamePanel.SetActive(true);
        countdownText.gameObject.SetActive(true); 
        
        StartCoroutine(FullShakeSequenceCoroutine());
    }

    private IEnumerator FullShakeSequenceCoroutine()
    {
        yield return StartCoroutine(CountdownCoroutine(preShakeCountdownTime));

        if (targetCollider != null)
        {
            targetCollider.isTrigger = false; 
            Debug.Log($"ShakeController: Collider '{targetCollider.name}' isTrigger set to False for active shake."); 
        }

        _isShakeInputActive = true; 
        _lastAcceleration = Input.acceleration; 
        
        if (shakeAnimator != null)
        {
            shakeAnimator.SetTrigger(startAnimationTrigger); 
            Debug.Log($"ShakeController: Animator trigger '{startAnimationTrigger}' set.");
        }
        
        // Використовуємо Time.unscaledDeltaTime, оскільки Time.timeScale може бути 0 з інших причин
        float timer = activeShakeWindowDuration;
        while (timer > 0)
        {
            timer -= Time.unscaledDeltaTime;
            yield return null;
        }

        _isShakeInputActive = false; 
        
        if (shakeAnimator != null)
        {
            shakeAnimator.SetTrigger(endAnimationTrigger); 
            Debug.Log($"ShakeController: Animator trigger '{endAnimationTrigger}' set.");
        }
        
        EndShakeSequence();
    }

    private IEnumerator CountdownCoroutine(float time)
    {
        countdownText.gameObject.SetActive(true); 
        DOTween.Sequence()
            .Append(countdownText.rectTransform.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.OutBack)) 
            .Append(countdownText.rectTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad))      
            .SetLoops(-1, LoopType.Restart) 
            .SetUpdate(UpdateType.Normal); 

        float timer = time;
        while (timer > 0)
        {
            countdownText.text = Mathf.CeilToInt(timer).ToString(); 
            timer -= Time.unscaledDeltaTime; 
            yield return null;
        }
        DOTween.Kill(countdownText.transform); 
        countdownText.gameObject.SetActive(false); 
        countdownText.transform.localScale = Vector3.one; 
    }

    private void ApplyShakeToLabubuObjects(Vector3 deltaAcceleration)
    {
        GameObject[] labubuObjects = GameObject.FindGameObjectsWithTag(shakeableObjectsTag); 
        foreach (GameObject obj in labubuObjects)
        {
            ObjectShaker objShaker = obj.GetComponent<ObjectShaker>();
            if (objShaker != null)
            {
                Vector2 impulseDirection = (Vector2)Random.insideUnitCircle.normalized; 
                objShaker.ApplyShakeImpulse(impulseDirection * objectShakeImpulseForce * deltaAcceleration.magnitude); 
            }
        }
    }

    public void EndShakeSequence()
    {
        if (!_isSequenceRunning) return; 

        _isSequenceRunning = false;

        shakeMinigamePanel.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        
        if (spawnManagerRef != null)
        {
            spawnManagerRef._test = _originalSpawnManagerTestValue; 
            Debug.Log($"ShakeController: SpawnManager._test restored to {_originalSpawnManagerTestValue}."); 
        }
        else
        {
            Debug.LogError("ShakeController: SpawnManagerRef став NULL при спробі відновлення."); 
        }

        foreach (var entry in _originalScriptStates)
        {
            if (entry.Key != null)
            {
                entry.Key.enabled = entry.Value; // Відновлюємо скрипти
                Debug.Log($"ShakeController: Скрипт {entry.Key.name} відновлено до enabled={entry.Value}."); 
            }
            else
            {
                Debug.LogWarning("ShakeController: _originalScriptStates містить NULL скрипт (можливо, об'єкт був знищений)."); 
            }
        }
        _originalScriptStates.Clear(); 

        if (targetCollider != null)
        {
            StartCoroutine(RestoreColliderStateAfterDelay()); 
        }
        else
        {
            Debug.LogError("ShakeController: TargetCollider став NULL при спробі відновити його після Shake. Можливо, об'єкт був знищений."); 
        }

        Debug.Log("ShakeController: Shake Minigame Ended. Game Resumed.");
        // Викликаємо подію про завершення
        OnShakeSequenceEnded?.Invoke();
    }

    private IEnumerator RestoreColliderStateAfterDelay()
    {
        Debug.Log("ShakeController: Починаю RestoreColliderStateAfterDelay (1 сек затримки)."); 
        yield return new WaitForSeconds(1f);

        if (targetCollider != null)
        {
            targetCollider.isTrigger = _originalColliderIsTriggerValue; 
            Debug.Log($"Collider '{targetCollider.name}' isTrigger restored to {_originalColliderIsTriggerValue}."); 
        }
        else
        {
            Debug.LogError("ShakeController: TargetCollider став NULL при спробі відновлення."); 
        }
    }

    void OnDisable()
    {
        if (_isSequenceRunning)
        {
            EndShakeSequence(); 
        }
        if (mainCamera != null && _originalCameraLocalPos != Vector3.zero)
        {
            mainCamera.transform.localPosition = _originalCameraLocalPos;
        }
    }
}