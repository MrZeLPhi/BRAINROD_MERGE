using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShakeManager : MonoBehaviour
{
    // Посилання на головну камеру, яку ми будемо трясти
    [Tooltip("Головна камера сцени.")]
    public Camera mainCamera;

    [Header("Shake Configuration")]
    [Tooltip("Базова інтенсивність тряски.")]
    public float shakeIntensity = 0.1f;
    [Tooltip("Швидкість затухання тряски.")]
    public float shakeFadeOutSpeed = 1.0f;
    [Tooltip("Порогове значення прискорення для спрацьовування тряски (для мобільних пристроїв).")]
    public float shakeThreshold = 2.0f; 

    // Зберігаємо початкову позицію камери
    private Vector3 originalCameraPosition;
    private bool isShaking = false;

    // Внутрішня змінна для керування вимкненням об'єктів
    private List<GameObject> disabledObjects = new List<GameObject>();

    void Start()
    {
        // Знаходимо головну камеру, якщо вона не призначена вручну
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera != null)
        {
            // Зберігаємо початкову позицію камери
            originalCameraPosition = mainCamera.transform.localPosition;
        }
    }

  

    public void Shake()
    {
        // Приклад обробки вводу для тряски (для мобільних пристроїв)
        // Якщо гра працює на мобільному пристрої, перевіряємо прискорення
        if (Application.isMobilePlatform)
        {
            Vector3 acceleration = Input.acceleration;
            // Перевіряємо, чи абсолютне значення прискорення перевищує поріг
            if (acceleration.magnitude > shakeThreshold)
            {
                // Якщо пристрій трясуть, ми можемо викликати функцію тряски сцени
                // В реальній грі, можливо, цю функцію варто викликати в іншому місці,
                // наприклад, як відповідь на ігрову подію (вибух, землетрус).
                
                // Якщо тряска ще не триває, ми можемо почати її
                // Якщо ви хочете, щоб тряска спрацьовувала лише від "потряхування" пристрою:
                // if (!isShaking) {
                //     StartSceneShake(5.0f, shakeIntensity); // Приклад: 5 секунд
                // }
            }
        }
        else
        {
            // Приклад для ПК: запуск тряски при натисканні пробілу
            if (Input.GetKeyDown(KeyCode.Space) && !isShaking)
            {
                Debug.Log("Початок тряски сцени (симуляція Shake Input).");
                // Викликаємо функцію з прикладом даних (масив об'єктів, тривалість 2 секунди)
                GameObject[] exampleObjects = GetObjectsToDisable(); 
                ShakeAndDisableObjects(exampleObjects, 2.0f);
            }
        }
        
    }

    // --- Основна функція, яку ви запросили ---
    /// <summary>
    /// Вимикає об'єкти на час тряски сцени, виконує тряску, і знову вмикає об'єкти.
    /// </summary>
    /// <param name="objectsToDisable">Масив GameObjects, які потрібно вимкнути.</param>
    /// <param name="duration">Тривалість тряски та вимкнення об'єктів (у секундах).</param>
    public void ShakeAndDisableObjects(GameObject[] objectsToDisable, float duration)
    {
        if (isShaking)
        {
            Debug.LogWarning("Сцена вже трясеться. Ігноруємо новий запит.");
            return;
        }
        
        // Перевіряємо, чи масив не null та чи є в ньому об'єкти
        if (objectsToDisable == null || objectsToDisable.Length == 0)
        {
            Debug.LogWarning("Масив об'єктів для вимкнення порожній або null. Тряска сцени все одно відбудеться.");
        }

        // Зберігаємо та вимикаємо об'єкти
        disabledObjects.Clear();
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null && obj.activeSelf)
            {
                obj.SetActive(false);
                disabledObjects.Add(obj); // Додаємо до списку вимкнених
            }
        }

        // Запускаємо корутину для тряски сцени та керування тривалістю
        StartCoroutine(SceneShakeCoroutine(duration));
    }


    // --- Логіка тряски сцени (Camera Shake) ---
    private IEnumerator SceneShakeCoroutine(float duration)
    {
        if (mainCamera == null)
        {
            Debug.LogError("Головна камера не призначена. Тряска сцени неможлива.");
            yield break;
        }

        isShaking = true;
        float elapsed = 0.0f;
        float currentIntensity = shakeIntensity;

        // Перевіряємо, чи ми вже зберегли початкову позицію
        if (originalCameraPosition == Vector3.zero) 
        {
            originalCameraPosition = mainCamera.transform.localPosition;
        }

        while (elapsed < duration)
        {
            // Рандомний зсув позиції камери
            float x = Random.Range(-1f, 1f) * currentIntensity;
            float y = Random.Range(-1f, 1f) * currentIntensity;

            // Змінюємо локальну позицію камери
            mainCamera.transform.localPosition = originalCameraPosition + new Vector3(x, y, 0f);

            // Знижуємо інтенсивність тряски з часом
            currentIntensity = Mathf.Lerp(currentIntensity, 0, Time.deltaTime * shakeFadeOutSpeed);

            elapsed += Time.deltaTime;
            yield return null; // Чекаємо наступного кадру
        }

        // Повертаємо камеру в початкове положення після тряски
        mainCamera.transform.localPosition = originalCameraPosition;
        isShaking = false;

        // Вмикаємо об'єкти, які були вимкнені
        EnableObjectsAfterShake();
    }

    private void EnableObjectsAfterShake()
    {
        foreach (GameObject obj in disabledObjects)
        {
            if (obj != null)
            {
                obj.SetActive(true);
            }
        }
        disabledObjects.Clear();
        Debug.Log("Тряска завершена. Об'єкти знову увімкнено.");
    }


    // --- Допоміжний метод для прикладу (не частина основної логіки) ---
    // Цей метод потрібен, якщо ми хочемо протестувати функціонал вимкнення об'єктів.
    // У реальній грі, масив об'єктів буде передаватися з іншого класу.
    private GameObject[] GetObjectsToDisable()
    {
        // Приклад: знаходимо всі об'єкти з тегом "ShakeSensitive" на сцені
        // GameObject[] objects = GameObject.FindGameObjectsWithTag("ShakeSensitive");
        
        // Для простоти, повернемо тут будь-який об'єкт, який ви можете вимкнути
        // Якщо у вас є ігрові об'єкти (наприклад, вороги), ви можете повернути їх масив.
        
        // Якщо ви використовуєте MainMenu, ви можете спробувати вимкнути CampaignPanel, наприклад
        // АБО краще: просто створіть масив та перетягніть об'єкти в Inspector, якщо ви додасте це поле.
        
        return null; // Повертаємо null, якщо не хочемо вимикати об'єкти в цьому прикладі.
    }
}