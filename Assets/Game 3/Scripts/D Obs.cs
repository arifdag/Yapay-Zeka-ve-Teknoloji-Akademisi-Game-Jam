using UnityEngine;

public class DObstacle : MonoBehaviour
{
    public AudioClip coinSound;
    public int scoreValue = 10;  // D harfine �arp�nca eklenecek puan
    private AudioSource audioSource;

    public float speedIncreaseAmount = 2.0f; // H�z art�� miktar�
    private PlayerController playerController;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = coinSound;
        audioSource.playOnAwake = false;  // Sesin spawn oldu�unda �almamas�n� sa�lar

        // PlayerController'� bulmak i�in karakteri etiketliyoruz
        playerController = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!audioSource.isPlaying)  // Sesin tekrar �almas�n� engelle
            {
                audioSource.Play();
            }

            // Puan ekle
            ScoreManager.instance.AddScore(scoreValue);

            // Karakterin h�z�n� art�r
            if (playerController != null)
            {
                playerController.IncreaseSpeed(speedIncreaseAmount);
            }

            // Obstacle'�n i�inden ge�mesi i�in Collider'� devre d��� b�rak
            GetComponent<Collider>().enabled = false;
        }
    }
}
