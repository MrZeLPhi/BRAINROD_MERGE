using UnityEngine;
using UnityEngine.Audio; // Для AudioMixerGroup

// Базовий клас для всіх звукових даних.
// Використовуйте CreateAssetMenu, щоб легко створювати нові ассети SoundData.
public abstract class SoundData : ScriptableObject
{
    [Tooltip("Аудіо кліп, який буде відтворюватися.")]
    public AudioClip clip;

    [Tooltip("Група Audio Mixer, до якої належить цей звук. Обов'язково призначити!")]
    public AudioMixerGroup outputAudioMixerGroup;

    [Header("Volume")]
    [Tooltip("Гучність відтворення кліпу (0.0 - 1.0).")]
    [Range(0f, 1f)]
    public float volume = 1.0f;

    [Header("Pitch Variation")]
    [Tooltip("Мінімальне значення тональності (pitch) для рандомізації.")]
    [Range(-3f, 3f)] // Діапазон тональності (pitch)
    public float minPitch = 0.9f;
    [Tooltip("Максимальне значення тональності (pitch) для рандомізації.")]
    [Range(-3f, 3f)]
    public float maxPitch = 1.1f;

    [Header("Looping")]
    [Tooltip("Чи має звук відтворюватися по колу (зазвичай для музики або фонових звуків).")]
    public bool loop = false;

    [Header("Spatial Blend")]
    [Tooltip("Просторове змішування (0 = 2D, 1 = 3D).")]
    [Range(0f, 1f)]
    public float spatialBlend = 0f; // 0 = 2D, 1 = 3D

    // Унікальний ID для звуку. Зручно для PlaySound(SoundType.Explosion)
    // АБО для String-based access, якщо ви віддаєте перевагу
    [Tooltip("Унікальний ідентифікатор для цього звуку. Може бути використаний для викликів з інших скриптів.")]
    public string soundID; 
}