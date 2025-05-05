using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelSelectManager : MonoBehaviour
{
    public Button[] levelButtons;

    void Start()
    {
        SetupLevelButtons();
    }

    void SetupLevelButtons()
    {
        int maxLevelUnlocked = PlayerPrefs.GetInt("MaxLevel", 1);
        Debug.Log("MaxLevel (PlayerPrefs): " + maxLevelUnlocked);

        for (int i = 0; i < levelButtons.Length; i++)
        {
            int levelNumber = i + 1;
            levelButtons[i].interactable = (levelNumber <= maxLevelUnlocked);

            int capturedLevelNumber = levelNumber;
            levelButtons[i].onClick.RemoveAllListeners();
            levelButtons[i].onClick.AddListener(() => LoadLevelScene(capturedLevelNumber));
        }
    }

    void LoadLevelScene(int levelNumber)
    {
        // Sahne yüklenmeden önce zamaný normale döndür
        Time.timeScale = 1f;
        int sceneIndex = levelNumber + 1;
        SceneManager.LoadScene(sceneIndex);
    }
}
