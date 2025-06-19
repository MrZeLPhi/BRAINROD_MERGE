using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton pattern for easy access from other scripts
    public static GameManager Instance { get; private set; }

    [Header("Game Controllers")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private ScoreManager _scoreManager; // NEW: Reference to ScoreManager

    private void Awake()
    {
        // Singleton implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // If GameManager should persist between scenes
        }

        // Check for controller references
        if (_playerController == null) Debug.LogError("PlayerController is not assigned in GameManager!");
        if (_spawnManager == null) Debug.LogError("SpawnManager is not assigned in GameManager!");
        if (_scoreManager == null) Debug.LogError("ScoreManager is not assigned in GameManager!"); // NEW check
    }

    private void Start()
    {
        StartGame();
    }

    /// <summary>
    /// Starts the game.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("Game started!");
        _scoreManager.ResetScore(); // NEW: Reset score at game start
        // Possible to initialize score, UI etc.
    }

    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f; // Stop time
        Debug.Log("Game paused.");
        // Show pause menu
    }

    /// <summary>
    /// Resumes the game.
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f; // Resume time
        Debug.Log("Game resumed.");
        // Hide pause menu
    }

    /// <summary>
    /// Handles game over.
    /// </summary>
    public void GameOver()
    {
        Debug.Log("Game Over!");
        // Show game over screen, stop spawning, etc.
        Time.timeScale = 0f;
    }
}
