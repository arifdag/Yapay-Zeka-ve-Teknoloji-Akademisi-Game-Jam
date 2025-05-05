using UnityEngine;

public class YObstacle : MonoBehaviour
{
    public AudioClip soundEffect;         // �arpma ses efekti
    public int penaltyValue = 5;          // Y obstacle'a �arpt���nda azalt�lacak puan
    public float slowAmount = 1.0f;       // Y obstacle'a �arpt���nda yava�lama miktar�
    public float slowDuration = 2.0f;     // Y obstacle'a �arpt���nda yava�lama s�resi (2 saniye)
    public float normalSpeedDecrease = 2.0f; // �arpma sonras� normal h�z�n d��me miktar�
    private AudioSource audioSource;

    void Start()
    {
        // AudioSource bile�enini ekler ve ses dosyas�n� atar
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = soundEffect;
        audioSource.playOnAwake = false;  // Sesin hemen �almas�n� engeller
    }

    private void OnTriggerEnter(Collider other)
    {
        // E�er �arpan obje "Player" tag'ine sahipse
        if (other.CompareTag("Player"))
        {
            // Ses zaten �alm�yorsa ses efektini �al
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }

            // Puan azalt
            ScoreManager.instance.SubtractScore(penaltyValue);

            // Karakterin h�z�n� yava�lat ve normal h�z�n� azalt
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController != null)
            {
                // Normal h�zdan 2 birim azalt (�u an kullan�lm�yor)
                // playerController.DecreaseSpeed(normalSpeedDecrease, slowDuration);

                // H�z� ge�ici olarak 1.0'a d���r
                playerController.DecreaseSpeedToOne(slowDuration);
            }

            // Mobilde telefonu titre�tir (Android ve iOS platformlar� i�in)
#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif

            // Obstacle'�n Collider'�n� devre d��� b�rak, b�ylece i�inden ge�ilebilir olur
            GetComponent<Collider>().enabled = false;
        }
    }
}
