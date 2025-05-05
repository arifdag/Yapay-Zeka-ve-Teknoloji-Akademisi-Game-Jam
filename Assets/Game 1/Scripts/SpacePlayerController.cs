using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class SpacePlayerController : MonoBehaviour
{
    private CharacterController controller;
    private Animator animator; // Animator Reference
    public float speed = 6.0f; // Character's initial forward speed (Z-axis)
    public float horizontalSpeed = 8.0f; // Character's sideways movement speed (X-axis)
    public float maxSpeed = 20.0f; // Maximum forward speed
    public float minSpeed = 1.0f; // Minimum forward speed
    [Tooltip("Maximum distance the player can move left/right from the center (X=0)")]
    public float xLimit = 5.0f; // X-axis movement limit 
    public float jumpHeight = 2.0f; // Jump height
    public float gravity = -19.62f; // Gravity force

    private Vector3 verticalVelocity; // To track vertical speed (jump/gravity)
    private bool isGrounded; // Checks if grounded

    private float originalSpeed; // Speed before collision slowdown
    private bool isReducedSpeed = false; // Flag for slowdown state
    private float reducedSpeedTimer = 2f; // Duration of slowdown

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>(); // Get the Animator component

        originalSpeed = speed; // Store the initial speed

        // Error handling if components are missing
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the player!", this);
        }
        if (controller == null)
        {
            Debug.LogError("CharacterController component not found on the player!", this);
        }
    }

    void Update()
    {
        // Ground Check
        isGrounded = controller.isGrounded;
        animator.SetBool("IsGrounded", isGrounded); // Inform Animator about ground status

        if (isGrounded && verticalVelocity.y < 0)
        {
            // Reset vertical velocity when grounded
            verticalVelocity.y = -2f;
        }

        // Horizontal Input
        float horizontalInput = 0f;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            horizontalInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            horizontalInput = 1f;
        }

        // Boundary Check BEFORE Calculating Horizontal Move
        float currentX = transform.position.x;

        // Prevent moving further left if already at or past the left limit AND trying to move left
        if (horizontalInput < 0 && currentX <= -xLimit)
        {
            horizontalInput = 0;
        }
        // Prevent moving further right if already at or past the right limit AND trying to move right
        else if (horizontalInput > 0 && currentX >= xLimit)
        {
            horizontalInput = 0;
        }

        // Calculate Movement Vectors
        Vector3 forwardMove = transform.forward * speed; // Forward movement based on current speed
        Vector3 horizontalMove = transform.right * horizontalInput * horizontalSpeed; // Sideways movement

        // Jump Input (W, Up Arrow)
        if (isGrounded && (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)))
        {
            // Physics formula: v = sqrt(h * -2 * g)
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger("Jump"); // Trigger jump animation
        }

        // Slide Input (S, Down Arrow)
        if (isGrounded && (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)))
        {
            animator.SetTrigger("Slide"); // Trigger slide animation
            // Change collider size during slide
            StartCoroutine(SlideColliderChange());
        }

        // Apply Gravity
        verticalVelocity.y += gravity * Time.deltaTime;

        // Apply Combined Movement
        Vector3 moveVector = forwardMove + horizontalMove + verticalVelocity;
        controller.Move(moveVector * Time.deltaTime);

        // Speed Reduction Logic
        if (isReducedSpeed)
        {
            reducedSpeedTimer -= Time.deltaTime;
            if (reducedSpeedTimer <= 0f)
            {
                speed = originalSpeed; // Restore original speed
                isReducedSpeed = false;
            }
        }
    }

    // Function to decrease speed temporarily
    public void DecreaseSpeedToOne(float duration)
    {
        if (!isReducedSpeed)
        {
            originalSpeed = speed; // Store current speed only if not already slowed down
        }
        speed = 1.0f;
        isReducedSpeed = true;
        reducedSpeedTimer = duration;
    }

    // Function to increase speed
    public void IncreaseSpeed(float amount)
    {
        if (isReducedSpeed)
        {
            // If slowed down, increase the speed we will return to
            originalSpeed = Mathf.Clamp(originalSpeed + amount, minSpeed, maxSpeed);
        }
        else
        {
            // If not slowed down, increase current speed directly
            speed = Mathf.Clamp(speed + amount, minSpeed, maxSpeed);
            originalSpeed = speed; // Keep originalSpeed synced with the current non-slowed speed
        }
    }


    IEnumerator SlideColliderChange()
    {
        float originalHeight = controller.height;
        Vector3 originalCenter = controller.center;
        float slideDuration = 1.5f;

        controller.height = originalHeight / 2f;
        controller.center = new Vector3(originalCenter.x, originalCenter.y / 2f, originalCenter.z);

        yield return new WaitForSeconds(slideDuration); // Wait for slide duration


        controller.height = originalHeight;
        controller.center = originalCenter;
    }

}