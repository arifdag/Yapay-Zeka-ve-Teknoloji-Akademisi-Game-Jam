using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartVoice : MonoBehaviour
{
    [Header("Audio Settings")] [Tooltip("Assign the audio clip (MP3, WAV, OGG, etc.) you want to play here.")]
    public AudioClip audioClipToPlay;

    [Tooltip("Check this box if you want the audio to loop.")]
    public bool loopAudio = false;

    private AudioSource audioSource;

    void Awake()
    {
        // Get the AudioSource component attached to this same GameObject
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            Debug.LogError("AudioPlayerOnStart requires an AudioSource component, but none was found!", this);
        }


        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (audioClipToPlay != null)
        {
            if (audioSource != null)
            {
                audioSource.clip = audioClipToPlay;


                audioSource.loop = loopAudio;

                // Tell the AudioSource to play the assigned clip
                audioSource.Play();

                Debug.Log($"AudioPlayerOnStart: Playing '{audioClipToPlay.name}'{(loopAudio ? " (Looping)" : "")}",
                    this);
            }
        }
        else
        {
            // If no clip was assigned, log a warning so the user knows why nothing is playing
            Debug.LogWarning("AudioPlayerOnStart: No AudioClip assigned in the Inspector. Nothing to play.", this);
        }
    }
}