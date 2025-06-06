using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton-патерн для легкого доступу з інших скриптів
    public static GameManager Instance { get; private set; }

    [Header("Game Controllers")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private SpawnManager _spawnManager;

    private void Awake()
    {
        // Реалізація Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Якщо GameManager повинен існувати між сценами
        }

        // Перевірка наявності контролерів
        if (_playerController == null) Debug.LogError("PlayerController не призначений в GameManager!");
        if (_spawnManager == null) Debug.LogError("SpawnManager не призначений в GameManager!");
    }

    private void Start()
    {
        StartGame();
    }

    /// <summary>
    /// Запускає гру.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("Гра почалася!");
        // Можливо, ініціалізувати рахунок, UI тощо
        // _spawnManager.SpawnNextPlayerObject(_playerController); // Вже викликається у PlayerController.Start()
    }

    /// <summary>
    /// Зупиняє гру.
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f; // Зупинити час
        Debug.Log("Гра на паузі.");
        // Показати меню паузи
    }

    /// <summary>
    /// Відновлює гру.
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f; // Відновити час
        Debug.Log("Гра відновлена.");
        // Сховати меню паузи
    }

    /// <summary>
    /// Обробляє завершення гри.
    /// </summary>
    public void GameOver()
    {
        Debug.Log("Гра закінчена!");
        // Показати екран завершення гри, зупинити спавн, тощо.
        Time.timeScale = 0f;
    }
}
