using UnityEngine;
using TMPro;
using UnityEngine.Assertions;

public class PlayerCollisionHandler : MonoBehaviour
{
    [Header("References")] [Tooltip("Assign the UIManager GameObject from the scene here.")] [SerializeField]
    private UIManager uiManager;

    [Tooltip("The CarController script, usually on the same GameObject or a parent.")] [SerializeField]
    private CarController carController;

    [Header("Collectible Settings")]
    [Tooltip("The TextMeshProUGUI element displaying the collectible count.")]
    [SerializeField]
    private TextMeshProUGUI collectibleText;

    [Tooltip("The maximum number of collectibles to collect (e.g., the '10' in 'x/10').")] [SerializeField]
    private int maxCollectibles = 10;

    [Header("Effects (Optional)")] [Tooltip("Particle system to play on collectible pickup.")] [SerializeField]
    private ParticleSystem collectEffect;

    [Tooltip("Particle system to play on crash.")] [SerializeField]
    private ParticleSystem crashEffect;

    [Tooltip("Audio clip to play on collectible pickup.")] [SerializeField]
    private AudioClip collectSound;

    [Tooltip("Audio clip to play on crash.")] [SerializeField]
    private AudioClip crashSound;
    
    [Header("Scene Loading")]
    [Tooltip("Assign the GameObject that has the SceneLoader script.")]
    [SerializeField] private SceneLoader sceneLoader;


    // Private Variables
    private int currentCollectibles = 0;
    private AudioSource audioSource;

    void Start()
    {
        currentCollectibles = 0;
        UpdateCollectibleText();

        // Attempt to get AudioSource if not assigned
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Assertions and Validation
        Assert.IsNotNull(uiManager, $"PlayerCollisionHandler: UI Manager reference not assigned on {gameObject.name}!");
        Assert.IsNotNull(collectibleText,
            $"PlayerCollisionHandler: Collectible Text not assigned on {gameObject.name}!");
        Assert.IsNotNull(carController,
            $"PlayerCollisionHandler: Car Controller reference not assigned on {gameObject.name}!"); // <-- ADDED ASSERTION

        if (uiManager == null)
            Debug.LogError(
                $"PlayerCollisionHandler: UI Manager not assigned on {gameObject.name}! Game Over UI will not work.",
                this);
        if (collectibleText == null)
            Debug.LogError(
                $"PlayerCollisionHandler: Collectible Text not assigned on {gameObject.name}! Count display will not work.",
                this);
        if (carController == null)
        {
            Debug.LogError(
                $"PlayerCollisionHandler: Car Controller not assigned on {gameObject.name}! Speed boost will not work.",
                this);
            // Attempt to find it on the same GameObject as a fallback
            carController = GetComponent<CarController>();
            if (carController == null)
            {
                Debug.LogError(
                    $"PlayerCollisionHandler: Could not find Car Controller component on {gameObject.name} either!",
                    this);
            }
        }
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Debug.Log("Player collided with Obstacle!");
            HandleCrash(other.transform.position);
        }
        else if (other.CompareTag("Collectible"))
        {
            HandleCollectible(other.gameObject);
        }
    }


    void HandleCrash(Vector3 crashPosition)
    {
        // Stop Boost First
        if (carController != null)
        {
            carController.StopBoost(); // <-- STOP BOOST ON CRASH
        }

        if (uiManager != null)
        {
            uiManager.ShowGameOverUI();
        }
        else
        {
            Debug.LogError("UIManager reference not set on PlayerCollisionHandler! Cannot show Game Over UI.");
        }

        PlayEffect(crashEffect, crashPosition);
        PlaySound(crashSound);

        // Stop game time *after* handling effects and boost stoppage
        Time.timeScale = 0f;
    }

    void HandleCollectible(GameObject collectibleObject)
    {
        
        // Collect logic
        if (currentCollectibles < maxCollectibles)
        {
            currentCollectibles++;
            if(currentCollectibles == maxCollectibles)
                sceneLoader.LoadNextScene();
            UpdateCollectibleText();
        }

        // Effects
        PlayEffect(collectEffect, collectibleObject.transform.position);
        PlaySound(collectSound);

        // Activate Boost
        if (carController != null)
        {
            carController.ActivateSpeedBoost(); // <-- ACTIVATE BOOST ON COLLECT
        }
        else
        {
            Debug.LogWarning("CarController reference missing, cannot activate boost.", this);
        }
        // --------------------

        Destroy(collectibleObject);
    }


    void UpdateCollectibleText()
    {
        if (collectibleText != null)
        {
            collectibleText.text = $"{Mathf.Min(currentCollectibles, maxCollectibles)}/{maxCollectibles}";
        }
    }

    void PlayEffect(ParticleSystem effectPrefab, Vector3 position)
    {
        if (effectPrefab != null)
        {
            ParticleSystem instance = Instantiate(effectPrefab, position, effectPrefab.transform.rotation);
            var main = instance.main;
            main.stopAction = ParticleSystemStopAction.Destroy;
            instance.Play(); // Ensure it plays if not playing automatically
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
        else if (clip != null)
        {
            Debug.LogWarning("AudioSource component missing, cannot play sound.", this);
        }
    }
}