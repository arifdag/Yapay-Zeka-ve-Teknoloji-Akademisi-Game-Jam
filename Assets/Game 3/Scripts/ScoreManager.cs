using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public TextMeshProUGUI scoreText;
    private int score = 0;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        score = 0;
        UpdateScoreText();
    }

    public void AddScore(int value)
    {
        score += value;
        UpdateScoreText();

        // Eðer skor 100 veya daha fazla ise, level açma iþlemi baþlasýn.
        if (score >= 100)
        {
            int currentLevelNumber = SceneManager.GetActiveScene().buildIndex - 1;
            int maxLevelUnlocked = PlayerPrefs.GetInt("MaxLevel", 1);

            // Sadece bir sonraki leveli açýyoruz
            if (currentLevelNumber == maxLevelUnlocked)
            {
                PlayerPrefs.SetInt("MaxLevel", maxLevelUnlocked + 1);
                PlayerPrefs.Save();
                Debug.Log("Yeni Level Açýldý: " + (maxLevelUnlocked + 1));
            }
        }
    }

    public void SubtractScore(int value)
    {
        score -= value;
        if (score < 0)
        {
            score = 0;
        }
        UpdateScoreText();
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    public int GetScore()
    {
        return score;
    }
}
