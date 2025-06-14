using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TouchSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Події, які викликатимуться PlayerController
    public UnityAction OnPointerDownEvent;
    public UnityAction<float> OnPointerDragEvent; // Передаємо значення слайдера
    public UnityAction OnPointerUpEvent;

    private Slider uiSlider;

    private void Awake()
    {
        uiSlider = GetComponent<Slider>();
        // Додаємо слухача для OnValueChanged, щоб викликати OnPointerDragEvent
        uiSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    // Викликається при натисканні на слайдер
    public void OnPointerDown(PointerEventData eventData)
    {
        // Викликаємо подію початку дотику
        OnPointerDownEvent?.Invoke();

        // Викликаємо подію перетягування з поточним значенням слайдера
        // Це потрібно, щоб об'єкт одразу зайняв правильну позицію при першому дотику
        OnPointerDragEvent?.Invoke(uiSlider.value);
        // BackgroundttttAudio._instance.HealSound(); // Закоментовано, бо це з іншого проекту
    }

    // Викликається при зміні значення слайдера (під час перетягування)
    private void OnSliderValueChanged(float value)
    {
        // Викликаємо подію перетягування з новим значенням слайдера
        OnPointerDragEvent?.Invoke(value);
        // BackgroundttttAudio._instance.HealSound(); // Закоментовано, бо це з іншого проекту
    }

    // Викликається при відпусканні пальця зі слайдера
    public void OnPointerUp(PointerEventData eventData)
    {
        // Викликаємо подію завершення дотику
        OnPointerUpEvent?.Invoke();
        
        // Скидаємо значення слайдера до центру після відпускання
        // Тепер це 0f, відповідно до налаштувань слайдера (-1 до 1, центр 0)
        uiSlider.value = 0f; 
    }

    private void OnDestroy()
    {
        // Видаляємо слухача подій, щоб уникнути витоків пам'яті
        uiSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
    }
}
