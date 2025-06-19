using UnityEngine;
using TMPro; // Assuming you are using TextMeshPro for UI Text

public class ScoreManager : MonoBehaviour
{
    // Singleton pattern for easy access from other scripts
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _scoreText; // UI Text to display the current score
    private int _currentScore;
    private int _sessionHighScore; // NEW: Max score achieved in the current game session
    private int _allTimeHighScore; // NEW: Max score achieved across all game sessions

    // Key for PlayerPrefs to store all-time high score
    private const string AllTimeHighScoreKey = "AllTimeHighScore"; 

    public int CurrentScore
    {
        get { return _currentScore; }
        private set
        {
            _currentScore = value;
            UpdateScoreUI(); // Update UI whenever score changes

            // Update session high score if current score surpasses it
            if (_currentScore > _sessionHighScore)
            {
                _sessionHighScore = _currentScore;
            }
        }
    }

    // NEW: Public accessor for session high score
    public int SessionHighScore => _sessionHighScore;

    // NEW: Public accessor for all-time high score
    public int AllTimeHighScore => _allTimeHighScore;


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
            // DontDestroyOnLoad(gameObject); // If ScoreManager should persist between scenes
        }

        if (_scoreText == null)
        {
            Debug.LogError("Score Text (TextMeshProUGUI) is not assigned in ScoreManager!");
        }

        // Load all-time high score from PlayerPrefs when the game starts
        _allTimeHighScore = PlayerPrefs.GetInt(AllTimeHighScoreKey, 0);

        // Initialize session high score and current score
        ResetScore(); 
    }

    /// <summary>
    /// Adds points to the global score.
    /// </summary>
    /// <param name="pointsToAdd">The amount of points to add.</param>
    public void AddPoints(int pointsToAdd)
    {
        if (pointsToAdd < 0)
        {
            Debug.LogWarning("Attempted to add negative points. Please provide a positive value.");
            return;
        }
        CurrentScore += pointsToAdd;
        Debug.Log($"Added {pointsToAdd} points. New score: {CurrentScore}");
    }

    /// <summary>
    /// Updates the score display in the UI.
    /// </summary>
    private void UpdateScoreUI()
    {
        if (_scoreText != null)
        {
            _scoreText.text = $"{CurrentScore}";
        }
    }

    /// <summary>
    /// Resets the current score to 0 and session high score.
    /// Called at the start of a new game.
    /// </summary>
    public void ResetScore()
    {
        CurrentScore = 0; // Will also update _sessionHighScore to 0 if CurrentScore becomes 0
        _sessionHighScore = 0; // Explicitly reset session high score for new game
    }

    /// <summary>
    /// Saves the current session's high score to all-time high score if it's greater.
    /// Should be called when the game ends (Game Over).
    /// </summary>
    public void SaveHighScores()
    {
        // Check if session high score is greater than all-time high score
        if (_sessionHighScore > _allTimeHighScore)
        {
            _allTimeHighScore = _sessionHighScore;
            PlayerPrefs.SetInt(AllTimeHighScoreKey, _allTimeHighScore);
            PlayerPrefs.Save(); // Save changes to disk
            Debug.Log($"New All-Time High Score: {_allTimeHighScore}");
        }
        Debug.Log($"Session High Score: {_sessionHighScore}");
    }
}
