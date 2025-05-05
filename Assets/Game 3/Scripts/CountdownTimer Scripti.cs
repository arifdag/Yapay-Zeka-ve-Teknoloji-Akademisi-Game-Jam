using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float totalTime = 120f;  // Ba�lang�� zaman�
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    private bool isGameOver = false;

    void Start()
    {
        gameOverPanel.SetActive(false);  // Ba�lang��ta panel gizli
        Time.timeScale = 1f; // Oyuna her ba�lang��ta zaman� s�f�rdan ba�lat
    }

    void Update()
    {
        if (!isGameOver)
        {
            if (totalTime > 0)
            {
                totalTime -= Time.deltaTime;
                timerText.text = Mathf.CeilToInt(totalTime).ToString();  // Sadece saniyeleri g�ster
            }
            else
            {
                EndGame();
            }
        }
    }

    void EndGame()
    {
        isGameOver = true;
        gameOverPanel.SetActive(true);
        finalScoreText.text = "S�re bitti! Puan�n�z: " + ScoreManager.instance.GetScore().ToString();
        Time.timeScale = 0; // Oyun durur
    }

    public void RestartGame()
    {
        // Yeni sahneye ge�meden �nce zaman� normale d�nd�r
        Time.timeScale = 1f;
        // �u anki sahneyi yeniden y�kle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
