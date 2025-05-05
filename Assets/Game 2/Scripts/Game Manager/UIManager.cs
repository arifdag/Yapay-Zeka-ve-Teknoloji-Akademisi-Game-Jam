using UnityEngine;
using UnityEngine.Assertions;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")] [Tooltip("Assign the parent GameObject for the In-Game UI elements here.")] [SerializeField]
    private GameObject inGameUIPanel;

    [Tooltip("Assign the parent GameObject for the Game Over UI elements here.")] [SerializeField]
    private GameObject gameOverUIPanel;


    void Start()
    {
        Assert.IsNotNull(inGameUIPanel, $"UIManager Error: In-Game UI Panel is not assigned on {gameObject.name}!");
        Assert.IsNotNull(gameOverUIPanel, $"UIManager Error: Game Over UI Panel is not assigned on {gameObject.name}!");

        if (inGameUIPanel == null || gameOverUIPanel == null)
        {
            Debug.LogError(
                $"UIManager: A UI Panel is not assigned in the Inspector on {gameObject.name}! UI might not function correctly.",
                this);
        }

        // Set the initial state when the scene starts
        ShowGameUI();
    }

    /// Activates the In-Game UI panel and deactivates the Game Over panel.
    public void ShowGameUI()
    {
        if (inGameUIPanel != null)
        {
            inGameUIPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("UIManager: In-Game UI Panel reference is missing.", this);
        }

        if (gameOverUIPanel != null)
        {
            gameOverUIPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("UIManager: Game Over UI Panel reference is missing.", this);
        }
    }

    /// Activates the Game Over UI panel and deactivates the In-Game panel.
    public void ShowGameOverUI()
    {
        if (inGameUIPanel != null)
        {
            inGameUIPanel.SetActive(false);
        }
        else
        {
            Debug.LogWarning("UIManager: In-Game UI Panel reference is missing.", this);
        }

        if (gameOverUIPanel != null)
        {
            gameOverUIPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("UIManager: Game Over UI Panel reference is missing.", this);
        }

        Debug.Log("UIManager: Activated Game Over UI");
    }
}