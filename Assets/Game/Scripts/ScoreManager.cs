using UnityEngine;
using TMPro; 
using UnityEngine.UI;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("UI Text References")]
    [SerializeField] private TextMeshProUGUI _t1; // T1: Current score
    [SerializeField] private TextMeshProUGUI _t2; // T2: All-time high score (live display)
    [SerializeField] private TextMeshProUGUI _t3; // T3: Session high score (Game Over panel)
    [SerializeField] private TextMeshProUGUI _t4; // T4: All-time high score (Game Over panel)
    
    [Header("Save High Score Buttons")]
    [Tooltip("Список кнопок, натискання на які збереже рекорд, якщо він новий.")]
    public List<Button> saveHighScoreButtons;

    private int _currentScore;
    private int _sessionHighScore; 
    private int _allTimeHighScore; 

    private const string AllTimeHighScoreKey = "AllTimeHighScore"; 

    public int CurrentScore
    {
        get { return _currentScore; }
        private set
        {
            _currentScore = value;
            UpdateCurrentScoreUI(); 

            if (_currentScore > _sessionHighScore)
            {
                _sessionHighScore = _currentScore;
                UpdateLiveHighScoreUI();
            }
        }
    }

    public int SessionHighScore => _sessionHighScore;
    public int AllTimeHighScore => _allTimeHighScore;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        if (_t1 == null) Debug.LogError("T1 Score Text (TextMeshProUGUI) is not assigned in ScoreManager!");
        if (_t2 == null) Debug.LogWarning("T2 Max Score Text (TextMeshProUGUI) is not assigned! Max score won't be displayed on start.");
        
        _allTimeHighScore = PlayerPrefs.GetInt(AllTimeHighScoreKey, 0);

        ResetScore(); 
        
        UpdateLiveHighScoreUI();

        AddListenersToSaveButtons();
    }

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

    private void UpdateCurrentScoreUI()
    {
        if (_t1 != null)
        {
            _t1.text = $"{CurrentScore}";
        }
    }

    public void UpdateLiveHighScoreUI()
    {
        if (_t2 != null)
        {
            _t2.text = $"{(CurrentScore > _allTimeHighScore ? CurrentScore : _allTimeHighScore)}";
        }
    }

    public void ResetScore()
    {
        CurrentScore = 0; 
        _sessionHighScore = 0; 
    }

    public void SaveHighScores()
    {
        if (_sessionHighScore > _allTimeHighScore)
        {
            _allTimeHighScore = _sessionHighScore;
            PlayerPrefs.SetInt(AllTimeHighScoreKey, _allTimeHighScore);
            PlayerPrefs.Save(); 
            Debug.Log($"New All-Time High Score: {_allTimeHighScore}");
            
            UpdateLiveHighScoreUI();
        }
        else
        {
             Debug.Log("Current high score is not a new record. No save needed.");
        }
    }
    
    private void AddListenersToSaveButtons()
    {
        if (saveHighScoreButtons == null) return;

        foreach (Button button in saveHighScoreButtons)
        {
            if (button != null)
            {
                button.onClick.AddListener(SaveHighScores);
            }
        }
    }
}