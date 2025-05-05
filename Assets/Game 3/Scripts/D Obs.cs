using UnityEngine;

public class DObstacle : MonoBehaviour
{
    public AudioClip coinSound;
    public int scoreValue = 10;  // D harfine çarpýnca eklenecek puan
    private AudioSource audioSource;

    public float speedIncreaseAmount = 2.0f; // Hýz artýþ miktarý
    private PlayerController playerController;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = coinSound;
        audioSource.playOnAwake = false;  // Sesin spawn olduðunda çalmamasýný saðlar

        // PlayerController'ý bulmak için karakteri etiketliyoruz
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!audioSource.isPlaying)  // Sesin tekrar çalmasýný engelle
            {
                audioSource.Play();
            }

            // Puan ekle
            ScoreManager.instance.AddScore(scoreValue);

            // Karakterin hýzýný artýr
            if (playerController != null)
            {
                playerController.IncreaseSpeed(speedIncreaseAmount);
            }

            // Obstacle'ýn içinden geçmesi için Collider'ý devre dýþý býrak
            GetComponent<Collider>().enabled = false;
        }
    }
}
