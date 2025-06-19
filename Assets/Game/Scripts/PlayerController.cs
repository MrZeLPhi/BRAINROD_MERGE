using UnityEngine;
using UnityEngine.UI; // For working with Slider and Image

public class PlayerController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TouchSlider _touchSlider; // Reference to our TouchSlider
    [SerializeField] private Image _nextObjectUIImageView; // UI Image to display the next object
    
    // Reference to the spawn manager
    [SerializeField] private SpawnManager _spawnManager;

    private MergeableObject _currentActiveObject; // The object currently controlled by the player

    private void Awake()
    {
        if (_touchSlider == null) Debug.LogError("TouchSlider is not assigned in PlayerController!");
        if (_spawnManager == null) Debug.LogError("SpawnManager is not assigned in PlayerController!");
        // Check for Image reference
        if (_nextObjectUIImageView == null) Debug.LogWarning("Next Object UI Image View is not assigned in PlayerController! The next object won't be displayed.");


        // Subscribe to events from TouchSlider
        _touchSlider.OnPointerDragEvent += OnSliderDrag;
        _touchSlider.OnPointerUpEvent += OnSliderRelease;
        _touchSlider.OnPointerDownEvent += OnSliderPress; // Optional, if something needs to be done on press
    }

    private void Start()
    {
        // Start spawning the first object at the beginning of the game
        _spawnManager.SpawnNextPlayerObject(this);
        // Update the UI for the next object immediately after game start
        UpdateNextObjectUI();
    }

    private void OnDisable()
    {
        // It's important to unsubscribe from events to avoid memory leaks
        _touchSlider.OnPointerDragEvent -= OnSliderDrag;
        _touchSlider.OnPointerUpEvent -= OnSliderRelease;
        _touchSlider.OnPointerDownEvent -= OnSliderPress;
    }

    /// <summary>
    /// Sets the current active object controlled by the player.
    /// Called by SpawnManager.
    /// </summary>
    public void SetCurrentActiveObject(MergeableObject obj)
    {
        _currentActiveObject = obj;
        if (_currentActiveObject != null)
        {
            // Set the initial position of the object according to the current slider position (0 - center)
            float initialX = _touchSlider.GetComponent<Slider>().value; // Get the current slider value (which should be 0)
            _currentActiveObject.transform.position = new Vector3(
                initialX,
                _currentActiveObject.transform.position.y,
                _currentActiveObject.transform.position.z
            );
        }
        // Update the UI for the next object every time a new current object is set
        UpdateNextObjectUI();
    }

    /// <summary>
    /// Handles the slider press event.
    /// </summary>
    private void OnSliderPress()
    {
        // Add logic here for when the player first presses the slider
        // E.g., a visual highlight or sound
        // Debug.Log("Slider pressed!");
    }

    /// <summary>
    /// Handles the slider drag event.
    /// The object precisely follows the slider's X-position, using its value as a coordinate.
    /// </summary>
    /// <param name="sliderValue">The slider value (from -1 to 1).</param>
    private void OnSliderDrag(float sliderValue)
    {
        if (_currentActiveObject != null && _currentActiveObject.IsPlayerControlled)
        {
            // Set the object's position directly, without delay
            // Use the slider value as the X-coordinate in the game world
            _currentActiveObject.transform.position = new Vector3(
                sliderValue, // sliderValue is already in the range of -1 to 1
                _currentActiveObject.transform.position.y,
                _currentActiveObject.transform.position.z
            );
        }
    }

    /// <summary>
    /// Handles the slider release event.
    /// Releases the current object and requests a new one.
    /// </summary>
    private void OnSliderRelease()
    {
        if (_currentActiveObject == null) return;

        // Check if the object was actually controlled by the player until now
        if (_currentActiveObject.IsPlayerControlled)
            ReleaseCurrentObject();
    }

    /// <summary>
    /// Releases the current object, activates its physics, and requests a new object.
    /// </summary>
    private void ReleaseCurrentObject()
    {
        if (_currentActiveObject == null) return;

        _currentActiveObject.ActivatePhysics(); // Activate the object's physics
        _currentActiveObject = null; // Clear the reference to the current object

        // Request a new object for the player from SpawnManager
        _spawnManager.SpawnNextPlayerObject(this);
    }

    /// <summary>
    /// Updates the UI Image, displaying the sprite of the next object to be spawned.
    /// </summary>
    private void UpdateNextObjectUI()
    {
        if (_nextObjectUIImageView != null && _spawnManager != null)
        {
            Sprite nextSprite = _spawnManager.GetNextPlayerObjectSprite();
            if (nextSprite != null)
            {
                _nextObjectUIImageView.sprite = nextSprite;
                _nextObjectUIImageView.gameObject.SetActive(true);
            }
            else
            {
                _nextObjectUIImageView.gameObject.SetActive(false); // Hide if no next sprite
            }
        }
    }
}
