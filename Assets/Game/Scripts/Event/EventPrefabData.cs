using UnityEngine;

[CreateAssetMenu(fileName = "NewEventPrefabData", menuName = "Event System/Event Prefab Data")]
public class EventPrefabData : ScriptableObject
{
    [Tooltip("Префаб, який буде спавнитися.")]
    public GameObject prefab;
    [Tooltip("Мінімальна сила імпульсу для цього префаба.")]
    public float minImpulseForce = 10f;
    [Tooltip("Максимальна сила імпульсу для цього префаба.")]
    public float maxImpulseForce = 30f;
    [Tooltip("Вага спавну. Вища вага = вищий шанс появи.")]
    [Range(1, 100)]
    public int spawnWeight = 10;
}