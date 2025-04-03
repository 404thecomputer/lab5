using UnityEngine;

public class BurritoController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float changeDirectionInterval = 2f;
    public float boundsRadius = 15f;
    public bool shouldWrap = true;
    
    [Header("Collection Settings")]
    public float staminaRestoreAmount = 30f;
    public float destroyDelay = 0.5f;
    
    [Header("Visual Effects")]
    public GameObject collectEffectPrefab;
    public ParticleSystem moveParticles;
    
    // Internal variables
    private Vector2 moveDirection;
    private float directionTimer;
    private AudioSource audioSource;
    private Rigidbody2D rb;
    private Transform playerTransform;
    private bool isCollected = false;
    
    void Start()
    {
        // Get components
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.linearDamping = 0.5f;
            rb.angularDamping = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        // Find player
        playerTransform = PlayerMovement.Instance.transform;
        
        // Initialize movement
        ChangeDirection();
        
        // Start particles if available
        if (moveParticles != null)
        {
            moveParticles.Play();
        }
    }
    
    void Update()
    {
        if (isCollected)
            return;
            
        // Check if it's time to change direction
        directionTimer -= Time.deltaTime;
        if (directionTimer <= 0)
        {
            ChangeDirection();
        }
        
        // Apply movement
        rb.linearVelocity = moveDirection * moveSpeed;
        
        // Rotate to face movement direction
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
        
        // Check if burrito is too far from player and wrap or adjust
        if (playerTransform != null)
        {
            Vector2 toPlayer = playerTransform.position - transform.position;
            float distanceToPlayer = toPlayer.magnitude;
            
            if (distanceToPlayer > boundsRadius)
            {
                if (shouldWrap)
                {
                    // Wrap to opposite side of play area (teleport)
                    Vector2 newPosition = (Vector2)playerTransform.position - toPlayer.normalized * (boundsRadius * 0.8f);
                    transform.position = new Vector3(newPosition.x, newPosition.y, transform.position.z);
                }
                else
                {
                    // Turn back toward the bounds
                    moveDirection = toPlayer.normalized;
                    directionTimer = changeDirectionInterval;
                }
            }
        }
    }
    
    void ChangeDirection()
    {
        // Pick a random direction
        moveDirection = Random.insideUnitCircle.normalized;
        directionTimer = changeDirectionInterval;
        
        // Add some variety to the interval
        directionTimer += Random.Range(-0.5f, 0.5f);
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player" && !isCollected)
        {
            isCollected = true;
            
            // Play collection sound
            if (audioSource != null)
            {
                audioSource.Play();
            }
            
            // Restore player's stamina
            PlayerMovement.Instance.stamina += staminaRestoreAmount;
            
            // Clamp stamina to max value
            if (PlayerMovement.Instance.stamina > PlayerMovement.Instance.maxStamina)
            {
                PlayerMovement.Instance.stamina = PlayerMovement.Instance.maxStamina;
            }
            
            // Increment burrito count
            if (BurritoGameManager.Instance != null)
            {
                BurritoGameManager.Instance.AddBurritoCollected();
            }
            
            // Spawn collection effect if available
            if (collectEffectPrefab != null)
            {
                Instantiate(collectEffectPrefab, transform.position, Quaternion.identity);
            }
            
            // Stop movement
            rb.linearVelocity = Vector2.zero;
            
            // Disable collider and renderer
            Collider2D collider = GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false;
            }
            
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }
            
            // Stop particles if available
            if (moveParticles != null)
            {
                moveParticles.Stop();
            }
            
            // Destroy after delay
            Destroy(gameObject, destroyDelay);
        }
    }
}