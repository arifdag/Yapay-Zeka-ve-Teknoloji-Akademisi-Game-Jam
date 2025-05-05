using UnityEngine;
using TMPro;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

public class SpacePlayerCollisionHandler : MonoBehaviour
{
    [Header("References")] [Tooltip("Assign the SpaceUIManager GameObject from the scene here.")] [SerializeField]
    private SpaceUIManager spaceUIManager;

    [Tooltip("The SpacePlayerController script, usually on the same GameObject or a parent.")] [SerializeField]
    private SpacePlayerController playerController;



 
    
    [Tooltip("Particle system to play on crash.")] [SerializeField]
    private ParticleSystem crashEffect;
    

    [Tooltip("Audio clip to play on crash.")] [SerializeField]
    private AudioClip crashSound;


    // Private Variables
    private int currentCollectibles = 0;
    private AudioSource audioSource;

    void Start()
    {
        currentCollectibles = 0;
        UpdateCountdownText();

        // Attempt to get AudioSource if not assigned
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        // Assertions and Validation
        Assert.IsNotNull(spaceUIManager, $"PlayerCollisionHandler: UI Manager reference not assigned on {gameObject.name}!");
        
        Assert.IsNotNull(playerController,
            $"PlayerCollisionHandler: Car Controller reference not assigned on {gameObject.name}!"); // <-- ADDED ASSERTION

        if (spaceUIManager == null)
            Debug.LogError(
                $"PlayerCollisionHandler: UI Manager not assigned on {gameObject.name}! Game Over UI will not work.",
                this);
        if (playerController == null)
        {
            Debug.LogError(
                $"PlayerCollisionHandler: Car Controller not assigned on {gameObject.name}! Speed boost will not work.",
                this);
            // Attempt to find it on the same GameObject as a fallback
            playerController = GetComponent<SpacePlayerController>();
            if (playerController == null)
            {
                Debug.LogError(
                    $"PlayerCollisionHandler: Could not find Car Controller component on {gameObject.name} either!",
                    this);
            }
        }
    }


    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if the object we hit has the "Obstacle" tag
        if (hit.gameObject.CompareTag("Obstacle"))
        {
            HandleCrash(hit.point); // Use hit.point for accurate effect position
        }

    }


    void HandleCrash(Vector3 crashPosition)
    {
        
        if (spaceUIManager != null)
        {
            spaceUIManager.ShowGameOverUI();
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
    


    void UpdateCountdownText()
    {
        
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