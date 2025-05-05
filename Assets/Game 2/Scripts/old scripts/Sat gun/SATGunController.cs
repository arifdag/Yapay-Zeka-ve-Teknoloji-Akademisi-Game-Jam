using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SATGunController : MonoBehaviour
{
    [Header("References")] [Tooltip("The bullet prefab to be fired.")]
    public GameObject bulletPrefab;

    [Tooltip("The point where bullets will spawn.")]
    public Transform muzzlePoint;

    [Tooltip("The part of the gun model that should visually rebound.")]
    public Transform gunModelTransform;

    [Header("Firing Settings")] [Tooltip("Time (in seconds) between consecutive shots.")]
    public float fireRate = 0.1f; // e.g., 0.1 = 10 shots per second

    [Tooltip("Speed of the fired bullets.")]
    public float bulletSpeed = 50f;

    [Tooltip("How long (in seconds) bullets exist before being destroyed.")]
    public float bulletLifetime = 5.0f;

    [Header("Rebound Settings")] [Tooltip("How far back the gun model moves during rebound (in local units).")]
    public float reboundDistance = 0.1f; 

    [Tooltip("Total duration (in seconds) for the rebound animation (back and forth).")]
    public float
        reboundDuration =
            0.2f; 

    // Private variables
    private float nextFireTime = 0f;
    private Vector3 originalGunModelPosition;
    private bool isRebounding = false;
    private Coroutine reboundCoroutine = null;

    public bool start = false;

    void Start()
    {
        if (bulletPrefab == null)
        {
            Debug.LogError("SATGunController: Bullet Prefab not assigned!", this);
            enabled = false;
            return;
        }

        if (muzzlePoint == null)
        {
            Debug.LogError("SATGunController: Muzzle Point Transform not assigned!", this);
            enabled = false;
            return;
        }

        if (gunModelTransform == null)
        {
            Debug.LogWarning(
                "SATGunController: Gun Model Transform not assigned. Defaulting to this GameObject's transform for rebound.",
                this);
            gunModelTransform = this.transform;
        }

        // Store the initial local position for the rebound animation
        originalGunModelPosition = gunModelTransform.localPosition;
    }

    void Update()
    {
        if (start && !isRebounding)
            TryFire();
    }
    
    /// Attempts to fire the gun if the fire rate cooldown has passed.
    public void TryFire()
    {
        if (Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireRate; // Set cooldown for the next shot
        }
    }
    
    /// Handles the actual firing logic: instantiating bullets and starting rebound.
    private void Fire()
    {
        if (bulletPrefab == null || muzzlePoint == null) return;
        
        GameObject bullet = Instantiate(bulletPrefab, muzzlePoint.position, muzzlePoint.rotation);

        // Get the Rigidbody and set its velocity
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = muzzlePoint.forward * bulletSpeed;
        }
        else
        {
            Debug.LogWarning(
                $"Bullet prefab '{bulletPrefab.name}' is missing a Rigidbody component. Cannot set velocity.",
                bulletPrefab);
        }

        // Schedule bullet destruction
        Destroy(bullet, bulletLifetime);

        // Start the rebound animation
        if (gunModelTransform != null && reboundCoroutine == null)
        {
            reboundCoroutine = StartCoroutine(ReboundSequence());
        }
        else if (gunModelTransform != null && reboundCoroutine != null)
        {
            // Optional: If you want rapid fire to 'reset' the rebound animation
            StopCoroutine(reboundCoroutine);
            reboundCoroutine = StartCoroutine(ReboundSequence());
        }
    }
    
    /// Coroutine to handle the visual rebound animation of the gun model.
    private IEnumerator ReboundSequence()
    {
        isRebounding = true;
        Vector3 reboundTargetPosition =
            originalGunModelPosition - (Vector3.forward * reboundDistance); // Move back along local Z
        float halfDuration = reboundDuration / 2.0f;
        float elapsedTime = 0f;

        // Move Backwards
        while (elapsedTime < halfDuration)
        {
            gunModelTransform.localPosition = Vector3.Lerp(
                originalGunModelPosition,
                reboundTargetPosition,
                elapsedTime / halfDuration // Normalized time (0 to 1)
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        gunModelTransform.localPosition = reboundTargetPosition;

        // Move Forwards
        elapsedTime = 0f;
        while (elapsedTime < halfDuration)
        {
            gunModelTransform.localPosition = Vector3.Lerp(
                reboundTargetPosition,
                originalGunModelPosition,
                elapsedTime / halfDuration // Normalized time (0 to 1)
            );

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure it snaps back exactly to the original position
        gunModelTransform.localPosition = originalGunModelPosition;

        isRebounding = false;
        reboundCoroutine = null;
    }
}