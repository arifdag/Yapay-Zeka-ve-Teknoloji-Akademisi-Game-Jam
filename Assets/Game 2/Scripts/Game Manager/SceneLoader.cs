using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] [Tooltip("The build index of the currently loaded scene.")]
    private int currentSceneIndex = -1; // Initialize to -1 (invalid index)

    void Awake()
    {
        // Get the currently active scene
        Scene currentScene = SceneManager.GetActiveScene();

        // Get its index from the Build Settings
        currentSceneIndex = currentScene.buildIndex;


        Debug.Log($"SceneLoader initialized. Current Scene: '{currentScene.name}', Build Index: {currentSceneIndex}");


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

        Debug.Log($"Restarting Scene: {SceneManager.GetActiveScene().name} (Index: {currentSceneIndex})");
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

    // Public property to safely get the index if needed elsewhere
    public int CurrentSceneIndex => currentSceneIndex;
}