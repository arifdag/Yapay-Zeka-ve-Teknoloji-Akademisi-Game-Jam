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

        // E�er skor 100 veya daha fazla ise, level a�ma i�lemi ba�las�n.
        if (score >= 100)
        {
            int currentLevelNumber = SceneManager.GetActiveScene().buildIndex - 1;
            int maxLevelUnlocked = PlayerPrefs.GetInt("MaxLevel", 1);

            // Sadece bir sonraki leveli a��yoruz
            if (currentLevelNumber == maxLevelUnlocked)
            {
                PlayerPrefs.SetInt("MaxLevel", maxLevelUnlocked + 1);
                PlayerPrefs.Save();
                Debug.Log("Yeni Level A��ld�: " + (maxLevelUnlocked + 1));
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
