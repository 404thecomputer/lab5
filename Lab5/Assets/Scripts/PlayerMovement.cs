using System;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance {get; private set;}

    [Header("Player Stats")]
    public float stamina = 100;
    public float maxStamina = 100;
    public float stamRecoverySpeed = 1f;

    [Header("Movement Settings")]
    public float maxSpeed = 10f;
    public float defaultSpeed = 10f;
    public float adaptSpeed = 0.5f;
    public float decaySpeed = 5f;
    public float dashMultiplier = 5f;
    public float dashStaminaCost = 10f;
    public float dashDuration = 0.2f;

    [Header("Visual Effects")]
    public ParticleSystem sprintParticles;
    public TrailRenderer movementTrail;
    public SpriteRenderer sealSprite;
    public Animator animator;

    [Header("Audio")]
    public AudioClip dashSound;
    public AudioClip staminaLowSound;
    public AudioClip pickupSound;

    // Internal variables
    private Vector3 currentSpeed = Vector3.zero;
    private Rigidbody2D thisBody;
    private AudioSource audioSource;
    private bool isDashing = false;
    private float dashTimer = 0f;
    private bool staminaWasLow = false;
    private float lowStaminaWarningInterval = 2f;
    private float lowStaminaTimer = 0f;

    void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Get components
        thisBody = GetComponent<Rigidbody2D>();
        if (thisBody == null)
        {
            thisBody = gameObject.AddComponent<Rigidbody2D>();
            thisBody.gravityScale = 0;
            thisBody.linearDamping = 0.5f;
            thisBody.angularDamping = 0.5f;
            thisBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (sprintParticles == null)
        {
            sprintParticles = GetComponentInChildren<ParticleSystem>();
        }
        
        if (movementTrail == null)
        {
            movementTrail = GetComponentInChildren<TrailRenderer>();
        }
        
        if (sealSprite == null)
        {
            sealSprite = GetComponent<SpriteRenderer>();
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        // Start with full stamina
        stamina = maxStamina;
    }

    void Update()
    {
        HandleMovementInput();
        HandleDashing();
        RegenerateStamina();
        UpdateVisuals();
        CheckStaminaWarning();
    }

    void HandleMovementInput()
    {
        if (isDashing)
            return;
            
        float goalSpeedY = 0;
        float goalSpeedX = 0;

        // Read movement input
        if (Input.GetKey(KeyCode.W)) {
            goalSpeedY = defaultSpeed;
        }
        if (Input.GetKey(KeyCode.S)) {
            goalSpeedY = defaultSpeed * -1;
        }
        if (Input.GetKey(KeyCode.A)) {
            goalSpeedX = defaultSpeed * -1;
            
            // Flip sprite to face left
            if (sealSprite != null)
            {
                sealSprite.flipX = true;
            }
        }
        if (Input.GetKey(KeyCode.D)) {
            goalSpeedX = defaultSpeed;
            
            // Flip sprite to face right
            if (sealSprite != null)
            {
                sealSprite.flipX = false;
            }
        }

        // Calculate new speed with smooth acceleration/deceleration
        float xDif = goalSpeedX - currentSpeed.x;
        float yDif = goalSpeedY - currentSpeed.y;
        float xSpeed;
        float ySpeed;
        
        if (Math.Abs(xDif) < 0.1f) {
            xSpeed = goalSpeedX;
        } else if (goalSpeedX != 0){
            if (Math.Sign(goalSpeedX) == Math.Sign(currentSpeed.x)) {
                if (Math.Abs(goalSpeedX) > Math.Abs(currentSpeed.x)) {
                    xSpeed = currentSpeed.x + (Math.Sign(xDif) * decaySpeed * Time.deltaTime);
                } else {
                    xSpeed = currentSpeed.x + (Math.Sign(xDif) * adaptSpeed * Time.deltaTime);
                }
            } else {
                xSpeed = currentSpeed.x + (Math.Sign(xDif) * adaptSpeed * Time.deltaTime);
            }
        } else {
            xSpeed = currentSpeed.x + (Math.Sign(xDif) * decaySpeed * Time.deltaTime);
        }
        
        if (Math.Abs(yDif) < 0.1f) {
            ySpeed = goalSpeedY;
        } else if (goalSpeedY != 0){
            if (Math.Sign(goalSpeedY) == Math.Sign(currentSpeed.y)) {
                if (Math.Abs(goalSpeedY) > Math.Abs(currentSpeed.y)) {
                    ySpeed = currentSpeed.y + (Math.Sign(yDif) * decaySpeed * Time.deltaTime);
                } else {
                    ySpeed = currentSpeed.y + (Math.Sign(yDif) * adaptSpeed * Time.deltaTime);
                }
            } else {
                ySpeed = currentSpeed.y + (Math.Sign(yDif) * adaptSpeed * Time.deltaTime);
            }
        } else {
            ySpeed = currentSpeed.y + (Math.Sign(yDif) * decaySpeed * Time.deltaTime);
        }
        
        currentSpeed = new Vector3(xSpeed, ySpeed, 0);
        
        // Apply movement
        thisBody.linearVelocity = currentSpeed;
    }

    void HandleDashing()
    {
        // Dash input
        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && stamina >= dashStaminaCost) 
        {
            // Consume stamina
            stamina -= dashStaminaCost;
            
            // Start dash
            isDashing = true;
            dashTimer = dashDuration;
            
            // Set dash velocity
            Vector3 dashDirection;
            if (currentSpeed.magnitude > 0.1f)
            {
                dashDirection = currentSpeed.normalized;
            }
            else
            {
                // Default dash forward if not moving
                dashDirection = new Vector3(0, 1, 0);
            }
            
            currentSpeed = dashDirection * defaultSpeed * dashMultiplier;
            thisBody.linearVelocity = currentSpeed;
            
            // Visual and audio effects
            if (sprintParticles != null)
            {
                sprintParticles.Play();
            }
            
            if (movementTrail != null)
            {
                movementTrail.emitting = true;
            }
            
            if (audioSource != null && dashSound != null)
            {
                audioSource.PlayOneShot(dashSound);
            }
        }
        
        // Update dash timer
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            
            if (dashTimer <= 0)
            {
                // End dash
                isDashing = false;
                
                // Stop trail
                if (movementTrail != null)
                {
                    movementTrail.emitting = false;
                }
            }
        }
    }

    void RegenerateStamina()
    {
        if (stamina < maxStamina)
        {
            stamina += stamRecoverySpeed * Time.deltaTime;
            if (stamina > maxStamina)
            {
                stamina = maxStamina;
            }
        }
    }

    void UpdateVisuals()
    {
        // Update animator parameters if available
        if (animator != null)
        {
            animator.SetFloat("Speed", currentSpeed.magnitude / defaultSpeed);
            animator.SetBool("IsDashing", isDashing);
        }
    }
    
    void CheckStaminaWarning()
    {
        bool isStaminaLow = stamina < (maxStamina * 0.25f);
        
        // Play warning sound when stamina gets low
        if (isStaminaLow && !staminaWasLow)
        {
            if (audioSource != null && staminaLowSound != null)
            {
                lowStaminaTimer = lowStaminaWarningInterval;
                audioSource.PlayOneShot(staminaLowSound);
            }
        }
        
        // Repeat warning sound at intervals
        if (isStaminaLow)
        {
            lowStaminaTimer -= Time.deltaTime;
            if (lowStaminaTimer <= 0)
            {
                if (audioSource != null && staminaLowSound != null)
                {
                    audioSource.PlayOneShot(staminaLowSound, 0.5f);
                }
                lowStaminaTimer = lowStaminaWarningInterval;
            }
        }
        
        staminaWasLow = isStaminaLow;
    }
    
    public void PlayPickupSound()
    {
        if (audioSource != null && pickupSound != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }
    
    // Method to reset player for a new game
    public void ResetPlayer()
    {
        stamina = maxStamina;
        currentSpeed = Vector3.zero;
        thisBody.linearVelocity = Vector3.zero;
        isDashing = false;
        staminaWasLow = false;
    }
}