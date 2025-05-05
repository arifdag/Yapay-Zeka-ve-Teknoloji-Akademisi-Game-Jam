using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    public TextMeshProUGUI scoreText;
    private int score = 0;
    
    [Header("Scene Loading")]
    [Tooltip("Assign the GameObject that has the SpaceSceneLoader script.")]
    [SerializeField] private SpaceSceneLoader sceneLoader;

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
        if (score >= 145)
        {
           sceneLoader.LoadNextScene();
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
