using UnityEngine;

[CreateAssetMenu(fileName = "NewMusic", menuName = "Audio/Music Data")]
public class MusicData : SoundData
{
    [Header("Music Specific")]
    [Tooltip("Плавне затухання на початку треку (у секундах).")]
    public float fadeInTime = 0.5f;
    [Tooltip("Плавне затухання в кінці треку (у секундах).")]
    public float fadeOutTime = 0.5f;
    [Tooltip("Чи зупиняти попередній музичний трек перед відтворенням нового.")]
    public bool stopPreviousMusic = true; // Зазвичай музика не має накладатися
}