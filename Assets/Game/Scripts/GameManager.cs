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

    [Header("Game Over Settings")]
    [SerializeField] private GameObject _gameOverUIPanel; 
    [SerializeField] private TextMeshProUGUI _t3; 
    [SerializeField] private TextMeshProUGUI _t4; 
    
    [Header("UI Panels")]
    [SerializeField] private GameObject _deleteUIPanel;

    [Header("Debugging / Performance")] 
    [SerializeField] private GameObject _graphyUIRoot; 
    [SerializeField] private int _targetFrameRate = 60; 

    private bool _isGameOver = false; 

    // Методів паузи/відновлення більше немає.
    // _isGamePaused також видалено.

    void Awake()
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
        
        if (_gameOverUIPanel != null)
        {
            _gameOverUIPanel.SetActive(false);
        }
        
        if (_graphyUIRoot != null)
        {
            _graphyUIRoot.SetActive(false); 
        }
    }

    public void GameOver()
    {
        if (!_isGameOver)
        {
            _isGameOver = true;
            Time.timeScale = 0f; // Залишаємо заморозку тільки для екрану Game Over
            if (_gameOverUIPanel != null)
            {
                _gameOverUIPanel.SetActive(true);
            }
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        Time.timeScale = 1f;
    }
}