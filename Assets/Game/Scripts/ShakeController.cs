using UnityEngine;
using UnityEngine.UI;
using TMPro; // Assuming TextMeshPro for UI text
using System.Collections;
using System.Collections.Generic;
using DG.Tweening; // For DOTween (ensure it's imported in your project)

public class ShakeController : MonoBehaviour
{
    public static ShakeController Instance { get; private set; }

    [Header("Dependencies")]
    [Tooltip("Посилання на ваш SpawnManager. Перетягніть GameObject з SpawnManager сюди.")]
    public SpawnManager spawnManagerRef; // Посилання на SpawnManager
    [Tooltip("Колайдер, який потрібно модифікувати (isTrigger). Перетягніть сюди Collider або GameObject з ним.")]
    public BoxCollider2D targetCollider;

    [Tooltip("Список MonoBehaviour скриптів, які потрібно вимкнути на час міні-гри (наприклад, PlayerController, EnemyAI).")]
    public List<MonoBehaviour> gameLogicScriptsToPause; 

    [Header("UI References")]
    [Tooltip("Головна панель UI для міні-гри тряски.")]
    public GameObject shakeMinigamePanel;
    [Tooltip("TextMeshPro текст для зворотного відліку.")]
    public TextMeshProUGUI countdownText; 
    [Tooltip("Animator на UI-панелі для анімації картинки.")]
    public Animator shakeAnimator; 
    [Tooltip("Назва тригера в Animator для запуску анімації тряски (наприклад, 'StartAnimation').")]
    public string startAnimationTrigger = "StartAnimation";
    [Tooltip("Назва тригера в Animator для завершення анімації тряски (наприклад, 'EndAnimation').")]
    public string endAnimationTrigger = "EndAnimation";

    [Header("Minigame Timings")]
    [Tooltip("Час зворотного відліку перед початком активної тряски (у секундах).")]
    public float preShakeCountdownTime = 3f;
    [Tooltip("Тривалість активного вікна для тряски телефону та анімації (у секундах).")]
    public float activeShakeWindowDuration = 3f; 

    [Header("Shake Physics Settings")]
    [Tooltip("The camera to shake. Should be assigned automatically or manually.")]
    public Camera mainCamera;
    [Tooltip("How quickly camera returns to original position after a shake impulse.")]
    public float shakeDampening = 5f; 
    private Vector3 _originalCameraLocalPos; // Saved original camera position

    [Tooltip("The sensitivity of camera shake to device acceleration.")] 
    public float shakeSensitivity = 0.05f; // How much acceleration translates to camera movement

    [Tooltip("Minimum acceleration magnitude change to trigger a shake impulse.")]
    public float shakeThreshold = 0.5f; 

    [Tooltip("Тег об'єктів, які мають трястися (завжди 'Labubu').")] 
    public string shakeableObjectsTag = "Labubu"; 
    [Tooltip("Сила імпульсу, що застосовується до об'єктів при виявленні тряски.")] 
    public float objectShakeImpulseForce = 0.5f;

    // Внутрішні змінні стану
    private Vector3 _lastAcceleration = Vector3.zero; 
    private Vector3 _currentCameraShakeOffset = Vector3.zero; 
    private bool _isShakeInputActive = false; 
    private bool _isSequenceRunning = false; 

    // Зберігаємо початковий стан SpawnManager._test та Collider.isTrigger
    private bool _originalSpawnManagerTestValue;
    private bool _originalColliderIsTriggerValue;
    // Зберігаємо початковий стан активованих скриптів
    private Dictionary<MonoBehaviour, bool> _originalScriptStates = new Dictionary<MonoBehaviour, bool>();


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); 

        // Ініціалізація камери та її позиції
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

        // Приховуємо панель міні-гри на старті
        if (shakeMinigamePanel != null)
        {
            shakeMinigamePanel.SetActive(false);
        }
    }

    // Update викликається кожен кадр, але ми будемо обробляти акселерометр лише тоді, коли _isShakeInputActive = true.
    void Update()
    {
        // Камера завжди повертається до початкового положення, якщо немає активної тряски
        if (!_isShakeInputActive && mainCamera != null && _currentCameraShakeOffset.magnitude > 0.001f)
        {
             _currentCameraShakeOffset = Vector3.Lerp(_currentCameraShakeOffset, Vector3.zero, Time.deltaTime * shakeDampening * 2); 
             mainCamera.transform.localPosition = _originalCameraLocalPos + _currentCameraShakeOffset;
        }
        else if (!_isShakeInputActive && mainCamera != null && _currentCameraShakeOffset.magnitude <= 0.001f && mainCamera.transform.localPosition != _originalCameraLocalPos)
        {
            mainCamera.transform.localPosition = _originalCameraLocalPos;
        }


        if (!_isShakeInputActive || mainCamera == null || !Application.isMobilePlatform)
        {
            // Обробляємо акселерометр лише під час активного вікна тряски і лише на мобільних пристроях
            return;
        }

        Vector3 currentAcceleration = Input.acceleration;
        Vector3 deltaAcceleration = currentAcceleration - _lastAcceleration;
        
        _lastAcceleration = currentAcceleration;

        // Перевіряємо, чи сила ривка (зміна прискорення) перевищує поріг
        if (deltaAcceleration.magnitude > shakeThreshold) 
        {
            // Застосовуємо силу до об'єктів Labubu
            ApplyShakeToLabubuObjects(deltaAcceleration);

            // Додаємо імпульс до зміщення камери
            _currentCameraShakeOffset += (Vector3)Random.insideUnitCircle * (deltaAcceleration.magnitude * shakeSensitivity); 
        }
    }


    /// <summary>
    /// Запускає повну послідовність міні-гри тряски.
    /// Цей метод викликається з кнопки, після реклами тощо.
    /// </summary>
    public void StartShakeSequence() 
    {
        if (_isSequenceRunning)
        {
            Debug.LogWarning("Shake sequence is already running. Ignoring new request.");
            return;
        }

        // Перевірка необхідних посилань (тепер вони є полями класу)
        if (spawnManagerRef == null || targetCollider == null || shakeMinigamePanel == null || shakeAnimator == null || countdownText == null)
        {
            Debug.LogError("ShakeController: Missing required references. Please assign all fields in Inspector. Aborting.");
            return;
        }
        
        _isSequenceRunning = true;
        Debug.Log("Shake Sequence Started!");

        // --- 1. Призупинення ігрової логіки ---
        // Зберігаємо початкові значення SpawnManager._test та Collider.isTrigger
        if (spawnManagerRef != null)
        {
            _originalSpawnManagerTestValue = spawnManagerRef._test; 
            spawnManagerRef._test = false; 
            Debug.Log($"SpawnManager._test set to false.");
        }

        if (targetCollider != null)
        {
            _originalColliderIsTriggerValue = targetCollider.isTrigger;
            targetCollider.isTrigger = false; 
            Debug.Log($"Collider '{targetCollider.name}' isTrigger set to False.");
        }

        // Вимикаємо інші ігрові скрипти
        _originalScriptStates.Clear();
        foreach (MonoBehaviour script in gameLogicScriptsToPause)
        {
            if (script != null)
            {
                _originalScriptStates[script] = script.enabled; 
                script.enabled = false; 
            }
        }

        // Показуємо UI панель міні-гри
        shakeMinigamePanel.SetActive(true);
        countdownText.gameObject.SetActive(true); 
        
        // --- 2. Запускаємо послідовність міні-гри (корутини) ---
        StartCoroutine(FullShakeSequenceCoroutine());
    }

    private IEnumerator FullShakeSequenceCoroutine()
    {
        // Зворотний відлік
        yield return StartCoroutine(CountdownCoroutine(preShakeCountdownTime));

        // Активне вікно для тряски та анімації
        _isShakeInputActive = true; 
        _lastAcceleration = Input.acceleration; 
        
        // Запускаємо анімацію UI
        if (shakeAnimator != null)
        {
            shakeAnimator.SetTrigger(startAnimationTrigger); 
            Debug.Log($"Animator trigger '{startAnimationTrigger}' set.");
        }
        
        // Чекаємо завершення активного вікна тряски
        yield return new WaitForSeconds(activeShakeWindowDuration); 

        // 3. Завершення фази тряски
        _isShakeInputActive = false; 
        
        // Завершуємо анімацію UI
        if (shakeAnimator != null)
        {
            shakeAnimator.SetTrigger(endAnimationTrigger); 
            Debug.Log($"Animator trigger '{endAnimationTrigger}' set.");
        }
        
        // --- 4. Завершення міні-гри ---
        EndShakeSequence();
    }

    // Корутина для зворотного відліку
    private IEnumerator CountdownCoroutine(float time)
    {
        countdownText.gameObject.SetActive(true); 
        // Анімація відліку через DOTween
        DOTween.Sequence()
            .Append(countdownText.rectTransform.DOScale(Vector3.one * 1.5f, 0.2f).SetEase(Ease.OutBack)) 
            .Append(countdownText.rectTransform.DOScale(Vector3.one, 0.2f).SetEase(Ease.OutQuad))      
            .SetLoops(-1, LoopType.Restart) 
            .SetUpdate(UpdateType.Normal); 

        float timer = time;
        while (timer > 0)
        {
            countdownText.text = Mathf.CeilToInt(timer).ToString(); 
            timer -= Time.deltaTime; 
            yield return null;
        }
        DOTween.Kill(countdownText.transform); 
        countdownText.gameObject.SetActive(false); 
        countdownText.transform.localScale = Vector3.one; 
    }

    // Застосування фізичної тряски до об'єктів Labubu
    private void ApplyShakeToLabubuObjects(Vector3 deltaAcceleration)
    {
        GameObject[] labubuObjects = GameObject.FindGameObjectsWithTag(shakeableObjectsTag); 
        foreach (GameObject obj in labubuObjects)
        {
            ObjectShaker objShaker = obj.GetComponent<ObjectShaker>();
            if (objShaker != null)
            {
                // Застосовуємо імпульс, що залежить від сили тряски телефону
                Vector2 impulseDirection = (Vector2)Random.insideUnitCircle.normalized; 
                objShaker.ApplyShakeImpulse(impulseDirection * objectShakeImpulseForce * deltaAcceleration.magnitude); 
            }
        }
    }

    // --- ЗМІНА ТУТ: Виклик нової корутини для затримки відновлення колайдера ---
    public void EndShakeSequence()
    {
        if (!_isSequenceRunning) return; 

        _isSequenceRunning = false;

        // Приховуємо UI панель
        shakeMinigamePanel.SetActive(false);
        if (countdownText != null) countdownText.gameObject.SetActive(false);
        
        // Відновлюємо SpawnManager._test
        if (spawnManagerRef != null)
        {
            spawnManagerRef._test = _originalSpawnManagerTestValue; 
            Debug.Log($"SpawnManager._test restored to {_originalSpawnManagerTestValue}.");
        }

        // Запускаємо корутину для затримки відновлення колайдера
        if (targetCollider != null)
        {
            StartCoroutine(RestoreColliderStateAfterDelay());
        }
        
        // Відновлюємо інші ігрові скрипти
        foreach (var entry in _originalScriptStates)
        {
            if (entry.Key != null)
            {
                entry.Key.enabled = entry.Value; 
            }
        }
        _originalScriptStates.Clear(); 

        Debug.Log("Shake Minigame Ended. Game Resumed.");
    }

    // --- НОВА КОРУТИНА: Чекає 1 секунду і відновлює стан колайдера ---
    private IEnumerator RestoreColliderStateAfterDelay()
    {
        // Чекаємо одну секунду
        yield return new WaitForSeconds(1f);

        // Відновлюємо Collider.isTrigger
        if (targetCollider != null)
        {
            targetCollider.isTrigger = _originalColliderIsTriggerValue; 
            Debug.Log($"Collider '{targetCollider.name}' isTrigger restored to {_originalColliderIsTriggerValue}.");
        }
    }

    // Прибирання при вимкненні об'єкта ShakeController (наприклад, при зміні сцени або Destroy)
    void OnDisable()
    {
        if (_isSequenceRunning)
        {
            EndShakeSequence(); 
        }
        // Забезпечуємо, що камера повертається в початкове положення
        if (mainCamera != null && _originalCameraLocalPos != Vector3.zero)
        {
            mainCamera.transform.localPosition = _originalCameraLocalPos;
        }
    }
}