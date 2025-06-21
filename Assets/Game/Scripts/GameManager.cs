using UnityEngine;
using UnityEngine.SceneManagement; // For scene management
using TMPro; // Assuming you are using TextMeshPro for UI Text

public class GameManager : MonoBehaviour
{
    // Singleton pattern for easy access from other scripts
    public static GameManager Instance { get; private set; }

    [Header("Game Controllers")]
    [SerializeField] private PlayerController _playerController;
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private ScoreManager _scoreManager;

    [Header("Game Over Settings")]
    [SerializeField] private GameObject _gameOverUIPanel; // Reference to your Game Over UI Panel/Image
    [SerializeField] private TextMeshProUGUI _sessionHighScoreText; // UI Text for session high score
    [SerializeField] private TextMeshProUGUI _allTimeHighScoreText; // UI Text for all-time high score
    
    [Header("Debugging / Performance")] // NEW HEADER
    [SerializeField] private GameObject _graphyUIRoot; // NEW: Reference to the root GameObject of Graphy's UI
    [SerializeField] private int _targetFrameRate = 60; // NEW: Desired target frame rate

    private bool _isGameOver = false; // Flag to prevent multiple game over calls

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

        // Set target frame rate
        Application.targetFrameRate = _targetFrameRate; // Using the serialized field for flexibility

        // Check for controller references
        if (_playerController == null) Debug.LogError("PlayerController is not assigned in GameManager!");
        if (_spawnManager == null) Debug.LogError("SpawnManager is not assigned in GameManager!");
        if (_scoreManager == null) Debug.LogError("ScoreManager is not assigned in GameManager!"); 
        
        // Check for UI text references for Game Over panel
        if (_sessionHighScoreText == null) Debug.LogWarning("Session High Score Text (TextMeshProUGUI) is not assigned in GameManager!");
        if (_allTimeHighScoreText == null) Debug.LogWarning("All-Time High Score Text (TextMeshProUGUI) is not assigned in GameManager!");

        // Ensure Game Over UI is initially hidden
        if (_gameOverUIPanel != null)
        {
            _gameOverUIPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("Game Over UI Panel is not assigned in GameManager. It won't be displayed!");
        }

        // NEW: Ensure Graphy UI is initially hidden or shown based on your preference
        if (_graphyUIRoot != null)
        {
            _graphyUIRoot.SetActive(false); // Initially hide Graphy
        }
        else
        {
            Debug.LogWarning("Graphy UI Root is not assigned in GameManager. Graphy toggle won't work!");
        }
    }

    private void Start()
    {
        StartGame();
    }

    private void Update() // NEW: For input handling
    {
        // Toggle Graphy UI with 'E' key
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("E key pressed!"); // Додайте цей лог
            if (_graphyUIRoot != null)
            {
                _graphyUIRoot.SetActive(!_graphyUIRoot.activeSelf);
                Debug.Log($"Graphy UI active status: {_graphyUIRoot.activeSelf}"); // Додайте цей лог
            }
            else
            {
                Debug.LogWarning("Graphy UI Root is NULL when E key pressed!"); // Додайте цей лог
            }
        }
    }

    /// <summary>
    /// Starts the game.
    /// </summary>
    public void StartGame()
    {
        Debug.Log("Game started!");
        _isGameOver = false;
        Time.timeScale = 1f; // Ensure time is running
        _scoreManager.ResetScore(); // Reset score at game start

        // Hide Game Over UI if it was shown
        if (_gameOverUIPanel != null)
        {
            _gameOverUIPanel.SetActive(false);
        }
        
        // (Optional) Re-enable player controls, spawning etc. if they were disabled on game over
        if (_playerController != null) _playerController.enabled = true;
        if (_spawnManager != null) _spawnManager.enabled = true;
    }

    /// <summary>
    /// Pauses the game.
    /// </summary>
    public void PauseGame()
    {
        if (_isGameOver) return; // Don't pause if game is already over
        Time.timeScale = 0f; // Stop time
        Debug.Log("Game paused.");
        // Show pause menu
    }

    /// <summary>
    /// Resumes the game.
    /// </summary>
    public void ResumeGame()
    {
        if (_isGameOver) return; // Don't resume if game is over
        Time.timeScale = 1f; // Resume time
        Debug.Log("Game resumed.");
        // Hide pause menu
    }

    /// <summary>
    /// Handles game over logic.
    /// </summary>
    public void GameOver()
    {
        if (_isGameOver) return; // Prevent multiple calls
        _isGameOver = true;

        Debug.Log("Game Over!");
        Time.timeScale = 0f; // Stop time on the scene

        // Save high scores before displaying them
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.SaveHighScores();
        }

        // Show Game Over UI and update stats
        if (_gameOverUIPanel != null)
        {
            _gameOverUIPanel.SetActive(true); // Show the Game Over UI

            // Update high score display on the Game Over Panel
            if (_sessionHighScoreText != null && ScoreManager.Instance != null)
            {
                _sessionHighScoreText.text = $"Сесія: {ScoreManager.Instance.SessionHighScore}";
            }
            if (_allTimeHighScoreText != null && ScoreManager.Instance != null)
            {
                _allTimeHighScoreText.text = $"Рекорд: {ScoreManager.Instance.AllTimeHighScore}";
            }
        }
        else
        {
            // Current implementation: Reload scene if no UI panel for game over
            Debug.LogWarning("Game Over UI Panel is not assigned. Reloading scene...");
            ReloadCurrentScene();
        }

        // Disable player controls and spawning
        if (_playerController != null) _playerController.enabled = false;
        if (_spawnManager != null) _spawnManager.enabled = false;

        // Hide Graphy when game is over
        if (_graphyUIRoot != null)
        {
            _graphyUIRoot.SetActive(false); 
        }

        // In a real game, you might wait for a button press on the UI before reloading.
    }

    /// <summary>
    /// Reloads the current active scene.
    /// </summary>
    public void ReloadCurrentScene()
    {
        Time.timeScale = 1f; // Reset timeScale before reloading scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
