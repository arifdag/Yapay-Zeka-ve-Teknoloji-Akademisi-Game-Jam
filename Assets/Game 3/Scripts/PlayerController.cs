using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Speeds")]
    public float speed = 6.0f;
    public float horizontalSpeed = 8.0f;
    public float maxSpeed = 20.0f;
    public float minSpeed = 1.0f;

    [Header("Jump & Gravity")]
    public float jumpHeight = 2.0f;
    public float gravity = -19.62f;

    [Header("Lane Limits")]
    public float xLimit = 5.0f;

    [Header("Slide (Roll) Settings")]
    [Tooltip("Collider yüksekliğinin orijinalin bu kadar katına düşecektir.")]
    [Range(0.1f, 1f)]
    public float slideHeightRatio = 0.5f;

    private CharacterController controller;
    private Animator animator;
    private AudioSource audioSource;
    private Vector3 verticalVelocity;
    private bool isGrounded;

    // Collider original values
    private float ccOriginalHeight;
    private Vector3 ccOriginalCenter;

    private float slideDuration;

    // Speed management
    private float originalSpeed;
    private bool isReducedSpeed = false;
    private float reducedSpeedTimer = 0f;

    // Animator parameters
    private const string SLIDE_TRIGGER = "Slide";
    private const string JUMP_TRIGGER = "Jump";
    private const string IS_GROUNDED = "IsGrounded";

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Save collider values
        ccOriginalHeight = controller.height;
        ccOriginalCenter = controller.center;

        // Save speed
        originalSpeed = speed;

        // Detect slide clip length
        slideDuration = 0.8f;
        if (animator.runtimeAnimatorController != null)
        {
            foreach (var clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip.name.ToLower().Contains(SLIDE_TRIGGER.ToLower()))
                {
                    slideDuration = clip.length;
                    break;
                }
            }
        }
    }

    void Update()
    {
        // Ground check
        isGrounded = controller.isGrounded;
        animator.SetBool(IS_GROUNDED, isGrounded);
        if (isGrounded && verticalVelocity.y < 0)
            verticalVelocity.y = -2f;

        // Horizontal input
        float hInput = 0f;
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) hInput = -1f;
        else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) hInput = 1f;

        // Movement
        Vector3 forwardMove = transform.forward * speed;
        Vector3 horizontalMove = transform.right * hInput * horizontalSpeed;
        Vector3 moveVector = forwardMove + horizontalMove + verticalVelocity;
        controller.Move(moveVector * Time.deltaTime);

        // Clamp X
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -xLimit, xLimit);
        transform.position = pos;

        // Jump
        if (isGrounded && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)))
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetTrigger(JUMP_TRIGGER);
        }

        // Slide
        if (isGrounded && (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)))
        {
            animator.SetTrigger(SLIDE_TRIGGER);
            StartCoroutine(DoSlide());
        }

        // Gravity
        verticalVelocity.y += gravity * Time.deltaTime;

        // Speed reset logic
        if (isReducedSpeed)
        {
            reducedSpeedTimer -= Time.deltaTime;
            if (reducedSpeedTimer <= 0f)
            {
                speed = originalSpeed;
                isReducedSpeed = false;
            }
        }
    }

    private IEnumerator DoSlide()
    {
        // Shrink collider
        controller.height = ccOriginalHeight * slideHeightRatio;
        controller.center = ccOriginalCenter + Vector3.down * (ccOriginalHeight * (1 - slideHeightRatio) * 0.5f);

        yield return new WaitForSeconds(slideDuration);

        // Restore collider
        controller.height = ccOriginalHeight;
        controller.center = ccOriginalCenter;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("y"))
        {
            // Y obstacle behavior
            YObstacle y = other.GetComponent<YObstacle>();
            if (y != null)
            {
                DecreaseSpeedToOne(y.slowDuration);
                // Subtract score via ScoreManager
                ScoreManager.instance.SubtractScore(y.penaltyValue);
                // Play obstacle sound
                audioSource.PlayOneShot(y.soundEffect);
            }
        }
        else if (other.CompareTag("d"))
        {
            DObstacle d = other.GetComponent<DObstacle>();
            if (d != null)
            {
                ScoreManager.instance.AddScore(d.scoreValue);
                audioSource.PlayOneShot(d.coinSound);
                IncreaseSpeed(d.speedIncreaseAmount);
            }
        }
        else if (other.CompareTag("s"))
        {
            SObstacle s = other.GetComponent<SObstacle>();
            if (s != null)
                audioSource.PlayOneShot(s.soundEffect);
        }

        // Disable obstacle collider so it won't retrigger
        other.GetComponent<Collider>().enabled = false;
    }

    public void DecreaseSpeedToOne(float duration)
    {
        if (!isReducedSpeed)
            originalSpeed = speed;
        speed = 1f;
        isReducedSpeed = true;
        reducedSpeedTimer = duration;
    }

    public void IncreaseSpeed(float amount)
    {
        if (isReducedSpeed)
            originalSpeed = Mathf.Clamp(originalSpeed + amount, minSpeed, maxSpeed);
        else
        {
            speed = Mathf.Clamp(speed + amount, minSpeed, maxSpeed);
            originalSpeed = speed;
        }
    }
}
