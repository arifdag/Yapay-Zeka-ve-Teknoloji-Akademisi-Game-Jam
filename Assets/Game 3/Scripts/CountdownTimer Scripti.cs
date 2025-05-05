using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float totalTime = 120f;  // Baþlangýç zamaný
    public TextMeshProUGUI timerText;
    public GameObject gameOverPanel;
    public TextMeshProUGUI finalScoreText;

    private bool isGameOver = false;

    void Start()
    {
        gameOverPanel.SetActive(false);  // Baþlangýçta panel gizli
        Time.timeScale = 1f; // Oyuna her baþlangýçta zamaný sýfýrdan baþlat
    }

    void Update()
    {
        if (!isGameOver)
        {
            if (totalTime > 0)
            {
                totalTime -= Time.deltaTime;
                timerText.text = Mathf.CeilToInt(totalTime).ToString();  // Sadece saniyeleri göster
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
        finalScoreText.text = "Süre bitti! Puanýnýz: " + ScoreManager.instance.GetScore().ToString();
        Time.timeScale = 0; // Oyun durur
    }

    public void RestartGame()
    {
        // Yeni sahneye geçmeden önce zamaný normale döndür
        Time.timeScale = 1f;
        // Þu anki sahneyi yeniden yükle
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
