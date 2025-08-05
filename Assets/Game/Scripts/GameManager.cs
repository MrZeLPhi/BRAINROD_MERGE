using UnityEngine;
using UnityEngine.SceneManagement; 
using TMPro; 
using System.Collections.Generic; 

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Controllers")]
    [SerializeField] private PlayerController _playerController; 
    [SerializeField] private SpawnManager _spawnManager;
    [SerializeField] private ScoreManager _scoreManager;

    [Tooltip("Список MonoBehaviour скриптів, які потрібно вимкнути при паузі або Game Over.")]
    public List<MonoBehaviour> gameLogicScriptsToPause; 

    [Header("Game Over Settings")]
    [SerializeField] private GameObject _gameOverUIPanel; 
    [SerializeField] private TextMeshProUGUI _t3; // T3: Session high score (Game Over panel)
    [SerializeField] private TextMeshProUGUI _t4; // T4: All-time high score (Game Over panel)
    
    [Header("Debugging / Performance")] 
    [SerializeField] private GameObject _graphyUIRoot; 
    [SerializeField] private int _targetFrameRate = 60; 

    private bool _isGameOver = false; 
    private bool _isGamePaused = false; 
    private Dictionary<MonoBehaviour, bool> _originalScriptStates = new Dictionary<MonoBehaviour, bool>(); 

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

        Application.targetFrameRate = _targetFrameRate; 

        if (_playerController == null) Debug.LogError("GameManager: PlayerController is not assigned in GameManager!");
        if (_spawnManager == null) Debug.LogError("GameManager: SpawnManager is not assigned in GameManager!");
        if (_scoreManager == null) Debug.LogError("GameManager: ScoreManager is not assigned in GameManager!"); 
        
        if (_t3 == null) Debug.LogWarning("GameManager: T3 Score Text (TextMeshProUGUI) is not assigned in GameManager!");
        if (_t4 == null) Debug.LogWarning("GameManager: T4 Score Text (TextMeshProUGUI) is not assigned in GameManager!");

        if (_gameOverUIPanel != null)
        {
            _gameOverUIPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("GameManager: Game Over UI Panel is not assigned. It won't be displayed!");
        }

        if (_graphyUIRoot != null)
        {
            _graphyUIRoot.SetActive(false); 
        }
    }

    private void Start()
    {
        StartGame();
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_graphyUIRoot != null)
            {
                _graphyUIRoot.SetActive(!_graphyUIRoot.activeSelf);
            }
        }
    }

    public void StartGame()
    {
        Debug.Log("GameManager: Game started!");
        _isGameOver = false;
        _isGamePaused = false;
        
        if (_scoreManager != null)
        {
            _scoreManager.ResetScore(); 
            _scoreManager.UpdateLiveHighScoreUI(); // <-- ВИПРАВЛЕНО: Правильна назва методу
        }
        else
        {
            Debug.LogError("GameManager: ScoreManager is not assigned. Cannot update scores.");
        }

        if (_gameOverUIPanel != null)
        {
            _gameOverUIPanel.SetActive(false);
        }
        
        SetGameLogicEnabled(true);
    }

    public void PauseGame()
    {
        if (_isGameOver || _isGamePaused) return; 
        _isGamePaused = true;
        SetGameLogicEnabled(false); 
        Debug.Log("GameManager: Game paused.");
    }

    public void ResumeGame()
    {
        if (_isGameOver || !_isGamePaused) return; 
        _isGamePaused = false;
        SetGameLogicEnabled(true); 
        Debug.Log("GameManager: Game resumed.");
    }

    public void GameOver()
    {
        if (_isGameOver) return; 
        _isGameOver = true;

        Debug.Log("GameManager: Game Over!");
        SetGameLogicEnabled(false); 

        if (_scoreManager != null)
        {
            _scoreManager.SaveHighScores();
        }

        if (_gameOverUIPanel != null)
        {
            _gameOverUIPanel.SetActive(true); 
            
            if (_t3 != null && _scoreManager != null)
            {
                _t3.text = $"Your Score: {_scoreManager.SessionHighScore}";
            }
            if (_t4 != null && _scoreManager != null)
            {
                _t4.text = $"Best Score: {_scoreManager.AllTimeHighScore}";
            }
        }
        else
        {
            Debug.LogWarning("GameManager: Game Over UI Panel is not assigned. Reloading scene...");
            ReloadCurrentScene();
        }

        if (_graphyUIRoot != null)
        {
            _graphyUIRoot.SetActive(false); 
        }
    }

    public void ReloadCurrentScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void SetGameLogicEnabled(bool enable)
    {
        if (enable)
        {
            Debug.Log("GameManager: SetGameLogicEnabled(true) викликано. Відновлюю скрипти."); 
            
            foreach (var entry in _originalScriptStates)
            {
                if (entry.Key != null)
                {
                    entry.Key.enabled = entry.Value;
                    Debug.Log($"GameManager: Скрипт {entry.Key.name} відновлено до enabled={entry.Value}"); 
                }
                else
                {
                    Debug.LogWarning("GameManager: _originalScriptStates містить NULL скрипт. Це може бути видалений об'єкт."); 
                }
            }
            _originalScriptStates.Clear(); 
        }
        else 
        {
            Debug.Log("GameManager: SetGameLogicEnabled(false) викликано. Вимикаю скрипти."); 
            _originalScriptStates.Clear(); 
            
            if (_playerController != null && !gameLogicScriptsToPause.Contains(_playerController)) gameLogicScriptsToPause.Insert(0, _playerController); 
            if (_spawnManager != null && !gameLogicScriptsToPause.Contains(_spawnManager)) gameLogicScriptsToPause.Insert(0, _spawnManager);

            foreach (MonoBehaviour script in gameLogicScriptsToPause)
            {
                if (script != null)
                {
                    _originalScriptStates[script] = script.enabled; 
                    script.enabled = false; 
                    Debug.Log($"GameManager: Скрипт {script.name} вимкнено."); 
                }
                else
                {
                    Debug.LogWarning("GameManager: gameLogicScriptsToPause містить NULL скрипт. Будь ласка, перевірте Inspector."); 
                }
            }
        }
    }
}