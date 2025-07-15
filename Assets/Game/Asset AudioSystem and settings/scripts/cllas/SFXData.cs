using UnityEngine;

[CreateAssetMenu(fileName = "NewSFX", menuName = "Audio/SFX Data")]
public class SFXData : SoundData
{
    [Header("SFX Specific")]
    [Tooltip("Чи можна цей SFX переривати іншим SFX того ж типу. (Зазвичай true для пострілів)")]
    public bool canOverlap = true;
    [Tooltip("Затримка перед відтворенням кліпу (у секундах).")]
    public float delay = 0f;
    [Tooltip("Чи зупиняти попередній SFX цього типу перед відтворенням нового (зазвичай false).")]
    public bool stopPreviousOfSameType = false; // Наприклад, якщо це звук UI, який не має накладатися.
}