using UnityEngine;
using TMPro; // Assuming you are using TextMeshPro for UI Text

public class ScoreManager : MonoBehaviour
{
    // Singleton pattern for easy access from other scripts
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _scoreText; // UI Text to display the score
    private int _currentScore;

    public int CurrentScore
    {
        get { return _currentScore; }
        private set
        {
            _currentScore = value;
            UpdateScoreUI(); // Update UI whenever score changes
        }
    }

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
        }

        if (_scoreText == null)
        {
            Debug.LogError("Score Text (TextMeshProUGUI) is not assigned in ScoreManager!");
        }

        // Initialize score to 0 and update UI
        CurrentScore = 0;
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
    /// Resets the score to 0.
    /// </summary>
    public void ResetScore()
    {
        CurrentScore = 0;
    }
}