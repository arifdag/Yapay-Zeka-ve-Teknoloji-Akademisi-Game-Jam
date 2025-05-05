using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class CarController : MonoBehaviour
{
    [Header("Movement Speeds")]
    [Tooltip("Base speed of forward movement along the Z-axis (units per second).")]
    [SerializeField]
    private float baseForwardSpeed = 10.0f;

    [Tooltip("Base speed of horizontal (left/right) movement (units per second).")] [SerializeField]
    private float baseHorizontalSpeed = 5.0f;

    [Tooltip("Base speed of vertical (up/down) movement (units per second).")] [SerializeField]
    private float baseVerticalSpeed = 4.0f;

    [Header("Movement Limits")] [Tooltip("Maximum allowed deviation from the starting X position.")] [SerializeField]
    private float xLimit = 15.0f;

    [Tooltip("Maximum allowed deviation from the starting Y position.")] [SerializeField]
    private float yLimit = 8.0f;

    [Header("Speed Boost")]
    [Tooltip("Multiplier applied to speeds during boost (e.g., 1.5 for 50% faster).")]
    [SerializeField]
    private float boostSpeedMultiplier = 1.5f;

    [Tooltip("Duration of the speed boost in seconds.")] [SerializeField]
    private float boostDuration = 5.0f;

    [Tooltip("Particle system for speed lines effect. Should be a child or assigned.")] [SerializeField]
    private ParticleSystem speedLinesEffect;

    // Private Variables
    private Vector3 initialPosition; // Store the starting position for limit calculations

    // Current effective speeds (can be modified by boost)
    private float currentForwardSpeed;
    private float currentHorizontalSpeed;
    private float currentVerticalSpeed;

    // Boost state
    private bool isBoosting = false;
    private Coroutine boostCoroutine = null;

    void Start()
    {
        initialPosition = transform.position;

        // Initialize current speeds to base speeds
        currentForwardSpeed = baseForwardSpeed;
        currentHorizontalSpeed = baseHorizontalSpeed;
        currentVerticalSpeed = baseVerticalSpeed;

        // Assertions and Validation
        Assert.IsNotNull(speedLinesEffect, $"CarController: Speed Lines Effect not assigned on {gameObject.name}!");
        if (speedLinesEffect == null)
        {
            Debug.LogError(
                $"CarController: Speed Lines Effect not assigned on {gameObject.name}! Boost effect will be missing.",
                this);
        }
        else
        {
            // Ensure speed lines are off initially
            speedLinesEffect.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        if (xLimit < 0)
        {
            Debug.LogWarning("xLimit should be non-negative. Using absolute value.");
            xLimit = Mathf.Abs(xLimit);
        }

        if (yLimit < 0)
        {
            Debug.LogWarning("yLimit should be non-negative. Using absolute value.");
            yLimit = Mathf.Abs(yLimit);
        }

        if (baseForwardSpeed < 0)
        {
            Debug.LogWarning("baseForwardSpeed is negative. The car will move backward.");
        }

        if (boostSpeedMultiplier <= 0)
        {
            Debug.LogWarning("boostSpeedMultiplier should be positive. Setting to 1.");
            boostSpeedMultiplier = 1f;
        }

        if (boostDuration <= 0)
        {
            Debug.LogWarning("boostDuration should be positive. Setting to 1.");
            boostDuration = 1f;
        }
    }

    void Update()
    {
        // Get Input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate Target Position Changes (Using CURRENT speeds)
        float deltaZ = currentForwardSpeed * Time.deltaTime;
        float deltaX = horizontalInput * currentHorizontalSpeed * Time.deltaTime;
        float deltaY = verticalInput * currentVerticalSpeed * Time.deltaTime;

        // Calculate the NEXT potential position
        Vector3 currentPos = transform.position;
        Vector3 targetPosition = currentPos + new Vector3(deltaX, deltaY, deltaZ);

        // Apply Limits (Clamping)
        float clampedX = Mathf.Clamp(targetPosition.x, initialPosition.x - xLimit, initialPosition.x + xLimit);
        float clampedY = Mathf.Clamp(targetPosition.y, initialPosition.y - yLimit, initialPosition.y + yLimit);

        // Construct the Final New Position
        Vector3 newPosition = new Vector3(clampedX, clampedY, targetPosition.z);

        // Apply the Movement
        transform.position = newPosition;
    }


    /// Activates the speed boost for a configured duration.
    /// If already boosting, resets the timer.
    public void ActivateSpeedBoost()
    {
        // Stop any existing boost coroutine to reset the timer
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
            Debug.Log("Resetting boost timer.");
        }
        else
        {
            Debug.Log("Activating speed boost!");
        }

        // Start the new boost timer
        boostCoroutine = StartCoroutine(BoostTimerCoroutine());
    }

    /// Immediately stops the speed boost and reverts to normal speed.
    /// Called externally (e.g., on crash).
    public void StopBoost()
    {
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
            boostCoroutine = null;
            RevertSpeed();
            Debug.Log("Boost stopped prematurely (e.g., crash).");
        }
    }


    private IEnumerator BoostTimerCoroutine()
    {
        try
        {
            isBoosting = true;

            // Apply boosted speeds
            currentForwardSpeed = baseForwardSpeed * boostSpeedMultiplier;
            currentHorizontalSpeed = baseHorizontalSpeed * boostSpeedMultiplier; // Boost horizontal too? Optional.
            currentVerticalSpeed = baseVerticalSpeed * boostSpeedMultiplier; // Boost vertical too? Optional.

            // Activate speed lines
            if (speedLinesEffect != null)
            {
                speedLinesEffect.Play();
            }

            // Wait for the duration
            yield return new WaitForSeconds(boostDuration);

            // Timer finished naturally
            Debug.Log("Boost ended.");
        }
        finally // This block executes whether the coroutine finishes OR is stopped
        {
            RevertSpeed();
            isBoosting = false;
            boostCoroutine = null;
        }
    }

    private void RevertSpeed()
    {
        // Revert to base speeds
        currentForwardSpeed = baseForwardSpeed;
        currentHorizontalSpeed = baseHorizontalSpeed;
        currentVerticalSpeed = baseVerticalSpeed;

        // Deactivate speed lines
        if (speedLinesEffect != null && speedLinesEffect.isPlaying) // Check if playing before stopping
        {
            speedLinesEffect.Stop();
        }
    }


    // Visualize Limits in Scene View
    void OnDrawGizmosSelected()
    {
        Vector3 center = Application.isPlaying ? initialPosition : transform.position;
        center.z = transform.position.z;
        Vector3 size = new Vector3(xLimit * 2, yLimit * 2, 1.0f);
        Gizmos.color = new Color(0.0f, 1.0f, 0.0f, 0.3f);
        Gizmos.DrawWireCube(center, size);
    }

    // Public property to check if currently boosting (optional, might be useful elsewhere)
    public bool IsBoosting => isBoosting;
}