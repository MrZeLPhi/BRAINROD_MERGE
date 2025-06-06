using UnityEngine;
using System.Collections; // Для Coroutine

[RequireComponent(typeof(Collider2D))] // Переконаємось, що є Collider2D
public class MergeDetector : MonoBehaviour
{
    private MergeableObject _mergeableObject; // Посилання на власний компонент MergeableObject
    private SpawnManager _spawnManager; // Посилання на SpawnManager

    private void Awake()
    {
        _mergeableObject = GetComponent<MergeableObject>();
        if (_mergeableObject == null)
        {
            Debug.LogError("MergeDetector вимагає компонента MergeableObject на тому ж GameObject!");
            enabled = false; // Вимкнути скрипт, якщо немає MergeableObject
        }
    }

    private void Start()
    {
        // Знаходимо SpawnManager у сцені. Рекомендовано, щоб він був Singleton або посилання передавалось.
        _spawnManager = FindObjectOfType<SpawnManager>(); 
        if (_spawnManager == null)
        {
            Debug.LogError("SpawnManager не знайдено в сцені. MergeDetector не зможе працювати!");
            enabled = false;
        }
    }

    // Це спрацює, коли два об'єкти з Collider2D стикаються
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Перевіряємо, чи ми не перебуваємо в процесі злиття
        if (_mergeableObject.IsBeingMerged) return;

        // Отримуємо компонент MergeableObject з об'єкта, з яким зіткнулися
        MergeableObject otherMergeableObject = collision.gameObject.GetComponent<MergeableObject>();

        // Перевіряємо, чи зіткнулися з іншим MergeableObject
        if (otherMergeableObject != null)
        {
            // Перевіряємо, чи інший об'єкт також не перебуває в процесі злиття
            if (otherMergeableObject.IsBeingMerged) return;

            // Перевіряємо, чи рівні об'єднання однакові
            if (_mergeableObject.MergeLevel == otherMergeableObject.MergeLevel)
            {
                // Запускаємо корутину злиття
                StartCoroutine(HandleMergeRoutine(otherMergeableObject, collision.contacts[0].point));
            }
        }
    }

    /// <summary>
    /// Корутина для обробки процесу злиття двох об'єктів.
    /// </summary>
    /// <param name="otherObject">Інший об'єкт, з яким відбувається злиття.</param>
    /// <param name="mergePoint">Точка зіткнення для спавну нового об'єкта.</param>
    private IEnumerator HandleMergeRoutine(MergeableObject otherObject, Vector2 mergePoint)
    {
        // Позначаємо обидва об'єкти як такі, що зливаються, щоб уникнути подальших зіткнень
        _mergeableObject.SetBeingMerged(true);
        otherObject.SetBeingMerged(true);

        // Тимчасово вимикаємо колайдери, щоб уникнути багів з фізикою під час анімації
        GetComponent<Collider2D>().enabled = false;
        otherObject.GetComponent<Collider2D>().enabled = false;

        // ==== Тут можна додати візуальні та звукові ефекти злиття ====
        // Наприклад, відтворення Particle System або невеликої анімації
        Debug.Log($"Об'єкти рівня {_mergeableObject.MergeLevel} зливаються!");

        yield return new WaitForSeconds(0.2f); // Невелика затримка для візуального ефекту

        // Розраховуємо новий рівень
        int newLevel = _mergeableObject.MergeLevel * 2;

        // Повідомляємо SpawnManager створити новий об'єкт
        _spawnManager.SpawnMergedObject(mergePoint, newLevel);

        // Знищуємо початкові об'єкти
        Destroy(otherObject.gameObject);
        Destroy(this.gameObject);
    }
}
