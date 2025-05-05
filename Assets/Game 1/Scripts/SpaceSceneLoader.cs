using UnityEngine;
using UnityEngine.SceneManagement;

public class SpaceSceneLoader : MonoBehaviour
{
    [SerializeField] [Tooltip("The build index of the currently loaded scene.")]
    private int currentSceneIndex = -1; // Initialize to -1 (invalid index)

    void Awake()
    {
        // Get the currently active scene
        Scene currentScene = SceneManager.GetActiveScene();

        // Get its index from the Build Settings
        currentSceneIndex = currentScene.buildIndex;
        

        if (currentSceneIndex < 0)
        {
            Debug.LogError(
                "SceneLoader Error: The current scene is not found in the Build Settings! Please add it via File -> Build Settings.");
        }
    }


    /// Reloads the currently active scene.
    public void RestartScene()
    {
        if (currentSceneIndex < 0)
        {
            Debug.LogError("Cannot restart scene: Current scene is not in Build Settings.");
            return;
        }
        
        Time.timeScale = 1f;
        SceneManager.LoadScene(currentSceneIndex);
    }


    /// Loads the next scene in the Build Settings order.
    /// If this is the last scene, it will log a warning and do nothing by default.
    public void LoadNextScene()
    {
        if (currentSceneIndex < 0)
        {
            Debug.LogError("Cannot load next scene: Current scene is not in Build Settings.");
            return;
        }

        // Calculate the index of the next scene
        int nextSceneIndex = currentSceneIndex + 1;

        // Check if the next scene index is valid (exists in the build settings)
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            // This is the last scene in the build settings
            Debug.LogWarning(
                "LoadNextScene called, but this is already the last scene in the Build Settings. No scene loaded.");
        }
    }
    
    /// Loads the scene at build index 0 (commonly the Main Menu).
    public void LoadMainMenuScene()
    {
        Debug.Log("Loading Main Menu Scene (Index 0)...");
        Time.timeScale = 1f; // Ensure time is running
        SceneManager.LoadScene(0); // Load scene with build index 0
    }
    
    /// Loads the scene with the specified build index.
    /// Performs checks to ensure the index is valid.
    public void LoadSceneByIndex(int sceneIndex)
    {
        // Check if the provided index is within the valid range of scenes in Build Settings
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            Debug.Log($"Loading Scene by Index: {sceneIndex}...");
            Time.timeScale = 1f; // Ensure time is running
            SceneManager.LoadScene(sceneIndex);
        }
        else
        {
            Debug.LogError($"Cannot load scene: Invalid scene index {sceneIndex}. Max index is {SceneManager.sceneCountInBuildSettings - 1}. Check Build Settings.");
        }
    }

    // Public property to safely get the index if needed elsewhere
    public int CurrentSceneIndex => currentSceneIndex;
}