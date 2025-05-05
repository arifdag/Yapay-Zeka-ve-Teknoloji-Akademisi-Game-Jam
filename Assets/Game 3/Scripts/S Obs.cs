using UnityEngine;

public class SObstacle : MonoBehaviour
{
    public AudioClip soundEffect;
    private AudioSource audioSource;

    void Start()
    {
        // Eðer prefab'da AudioSource yoksa, ekle
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;

        // Sesin hacmini ve 3D ayarlarýný kontrol et
        audioSource.spatialBlend = 0.0f; // 2D ses olarak çalmasý için
        audioSource.volume = 1.0f; // Ses seviyesini tam yap
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (soundEffect != null && !audioSource.isPlaying)
            {
                // PlayOneShot kullanarak sesi çal
                audioSource.PlayOneShot(soundEffect);

                // Collider'ý devre dýþý býrak
                GetComponent<Collider>().enabled = false;
            }
            else
            {
                Debug.LogWarning("SoundEffect null veya AudioSource zaten ses çalýyor.");
            }
        }
    }
}
