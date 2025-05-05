using UnityEngine;

public class YObstacle : MonoBehaviour
{
    public AudioClip soundEffect;         // Çarpma ses efekti
    public int penaltyValue = 5;          // Y obstacle'a çarptýðýnda azaltýlacak puan
    public float slowAmount = 1.0f;       // Y obstacle'a çarptýðýnda yavaþlama miktarý
    public float slowDuration = 2.0f;     // Y obstacle'a çarptýðýnda yavaþlama süresi (2 saniye)
    public float normalSpeedDecrease = 2.0f; // Çarpma sonrasý normal hýzýn düþme miktarý
    private AudioSource audioSource;

    void Start()
    {
        // AudioSource bileþenini ekler ve ses dosyasýný atar
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = soundEffect;
        audioSource.playOnAwake = false;  // Sesin hemen çalmasýný engeller
    }

    private void OnTriggerEnter(Collider other)
    {
        // Eðer çarpan obje "Player" tag'ine sahipse
        if (other.CompareTag("Player"))
        {
            // Ses zaten çalmýyorsa ses efektini çal
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            // Puan azalt
            ScoreManager.instance.SubtractScore(penaltyValue);

            // Karakterin hýzýný yavaþlat ve normal hýzýný azalt
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Normal hýzdan 2 birim azalt (þu an kullanýlmýyor)
                // playerController.DecreaseSpeed(normalSpeedDecrease, slowDuration);

                // Hýzý geçici olarak 1.0'a düþür
                playerController.DecreaseSpeedToOne(slowDuration);
            }

            // Mobilde telefonu titreþtir (Android ve iOS platformlarý için)
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif

            // Obstacle'ýn Collider'ýný devre dýþý býrak, böylece içinden geçilebilir olur
            GetComponent<Collider>().enabled = false;
        }
    }
}
