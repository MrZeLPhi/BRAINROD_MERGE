using UnityEngine;

// Цей скрипт додається до ігрових об'єктів (з тегом "Labubu")
// які повинні трястися під час міні-гри.
[RequireComponent(typeof(Rigidbody2D))] // Вимагаємо Rigidbody2D для фізичної тряски
public class ObjectShaker : MonoBehaviour
{
    private Rigidbody2D _rb;
    private Vector2 _currentShakeForce = Vector2.zero; // Сила, що застосовується до об'єкта

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    // Цей метод викликатиметься з ShakeController для застосування сили тряски
    public void ApplyShakeImpulse(Vector2 impulse)
    {
        if (_rb != null)
        {
            _rb.AddForce(impulse, ForceMode2D.Impulse); // Застосовуємо миттєву силу
        }
    }

    // Можна додати для затухання тряски, якщо вона має бути постійною
    // void FixedUpdate()
    // {
    //     // Приклад: плавне повернення об'єкта до стану спокою після імпульсу
    //     // _rb.velocity = Vector2.Lerp(_rb.velocity, Vector2.zero, Time.fixedDeltaTime * 0.5f);
    // }
}