using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class SpaceUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    [Tooltip("Assign the parent GameObject for the In-Game UI elements here.")]
    [SerializeField] private GameObject inGameUIPanel;

    [Tooltip("Assign the parent GameObject for the Game Over UI elements here.")]
    [SerializeField] private GameObject gameOverUIPanel;

    [Header("Game Timer Settings")]
    [Tooltip("The TextMeshProUGUI element displaying the game timer.")]
    [SerializeField] private TextMeshProUGUI gameTimerText; 

    [Tooltip("Total duration of the game round in seconds (e.g., 60).")]
    [SerializeField] private float gameDuration = 60f;
    
    [Header("Scene Loading")]
    [Tooltip("Assign the GameObject that has the SpaceSceneLoader script.")]
    [SerializeField] private SpaceSceneLoader sceneLoader;

    // Private state variables for the timer
    private float currentTime;
    private bool timerIsRunning = false;
    

    void Start()
    {
        Assert.IsNotNull(inGameUIPanel, $"UIManager Error: In-Game UI Panel is not assigned on {gameObject.name}!");
        Assert.IsNotNull(gameOverUIPanel, $"UIManager Error: Game Over UI Panel is not assigned on {gameObject.name}!");
        Assert.IsNotNull(gameTimerText, $"UIManager Error: Game Timer Text is not assigned on {gameObject.name}!"); // Check the timer text reference

        if (inGameUIPanel == null || gameOverUIPanel == null || gameTimerText == null)
        {
            Debug.LogError($"UIManager: A required UI element (Panel or Game Timer Text) is not assigned in the Inspector on {gameObject.name}! UI might not function correctly.", this);
            this.enabled = false; // Disable script if essential refs are missing
            return;
        }

        // Set the initial state when the scene starts
        ShowGameUI(); // Show the main game UI
        StartGameTimer(); // Start the main game timer
    }

    void Update()
    {
        // Update the timer logic only if it's running
        if (timerIsRunning)
        {
            currentTime -= Time.deltaTime; // Decrease time by time elapsed since last frame

            if (currentTime <= 0f)
            {
                // Timer reached zero
                currentTime = 0f; // Clamp to 0
                timerIsRunning = false;
                UpdateTimerDisplay(); // Update display one last time to show 00:00
                HandleTimeUp(); // Trigger game over actions
            }
            else
            {
                // Update the display while timer is running
                UpdateTimerDisplay();
            }
        }
    }

    /// Starts the main gameplay timer.
    public void StartGameTimer()
    {
        currentTime = gameDuration; // Reset time to the full duration
        timerIsRunning = true;
        if (gameTimerText != null)
        {
            gameTimerText.gameObject.SetActive(true); // Ensure timer text is visible
        }
        UpdateTimerDisplay(); // Show initial time immediately
        Time.timeScale = 1f; // Ensure game is running
    }

    /// Updates the TextMeshPro element with the formatted time.
    void UpdateTimerDisplay()
    {
        if (gameTimerText != null)
        {
            // Calculate minutes and seconds from the float time
            float minutes = Mathf.FloorToInt(currentTime / 60f);
            float seconds = Mathf.FloorToInt(currentTime % 60f);

            // Format the string as MM:SS (e.g., "01:05" or "00:32")
            gameTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    /// Called when the timer runs out.
    void HandleTimeUp()
    {
        timerIsRunning = false;
        if (sceneLoader != null)
        {
            sceneLoader.LoadNextScene();
        }
        
    }


    /// Activates the In-Game UI panel and deactivates the Game Over panel.
    public void ShowGameUI()
    {
        if (inGameUIPanel != null) inGameUIPanel.SetActive(true);
        else Debug.LogWarning("UIManager: In-Game UI Panel reference is missing.", this);

        if (gameOverUIPanel != null) gameOverUIPanel.SetActive(false);
        else Debug.LogWarning("UIManager: Game Over UI Panel reference is missing.", this);

        // Ensure timer text starts active if it's part of InGameUI
        if (gameTimerText != null) gameTimerText.gameObject.SetActive(true);
    }

    /// Activates the Game Over UI panel and deactivates the In-Game panel.
    public void ShowGameOverUI()
    {
        timerIsRunning = false; // Stop the timer if game over is triggered by a crash

        if (inGameUIPanel != null) inGameUIPanel.SetActive(false);
        else Debug.LogWarning("UIManager: In-Game UI Panel reference is missing.", this);

        if (gameOverUIPanel != null) gameOverUIPanel.SetActive(true);
        else Debug.LogWarning("UIManager: Game Over UI Panel reference is missing.", this);
        

        // Note: The PlayerCollisionHandler script already sets Time.timeScale = 0f
        // If this script were the *only* thing causing Game Over, you'd add:
        // Time.timeScale = 0f;
    }
}