using UnityEngine;
using UnityEngine.UI;
using TMPro; // Add this for TextMeshPro

public class StaminaBar : MonoBehaviour
{
    [Header("Bar References")]
    public Image staminaFill;
    public Image staminaBackground;
    public TextMeshProUGUI staminaText; // Changed from Text to TextMeshProUGUI
    
    [Header("Visual Settings")]
    public Color fullStaminaColor = new Color(0.2f, 0.8f, 0.2f);
    public Color mediumStaminaColor = new Color(0.9f, 0.9f, 0.2f);
    public Color lowStaminaColor = new Color(0.9f, 0.2f, 0.2f);
    public float lowStaminaThreshold = 0.3f;
    public float mediumStaminaThreshold = 0.6f;
    public float pulseSpeed = 2f;
    public float pulseMinScale = 0.9f;
    public float pulseMaxScale = 1.1f;
    
    // Internal variables
    private Vector3 originalScale;
    private bool isPulsing = false;
    
    void Start()
    {
        // Store original scale
        originalScale = transform.localScale;
        
        // Check required references
        if (staminaFill == null)
        {
            Debug.LogError("StaminaBar is missing staminaFill reference");
        }
        
        // Try to find TextMeshPro component if not assigned
        if (staminaText == null)
        {
            staminaText = GetComponentInChildren<TextMeshProUGUI>();
        }
    }
    
    void Update()
    {
        if (PlayerMovement.Instance == null)
            return;
        
        // Get current stamina percentage
        float staminaPercentage = PlayerMovement.Instance.stamina / PlayerMovement.Instance.maxStamina;
        
        // Update stamina fill amount
        if (staminaFill != null)
        {
            staminaFill.fillAmount = staminaPercentage;
            
            // Update color based on stamina level
            if (staminaPercentage <= lowStaminaThreshold)
            {
                staminaFill.color = lowStaminaColor;
                
                // Start pulsing when low
                isPulsing = true;
            }
            else if (staminaPercentage <= mediumStaminaThreshold)
            {
                staminaFill.color = mediumStaminaColor;
                isPulsing = false;
                
                // Reset scale when not pulsing
                transform.localScale = originalScale;
            }
            else
            {
                staminaFill.color = fullStaminaColor;
                isPulsing = false;
                
                // Reset scale when not pulsing
                transform.localScale = originalScale;
            }
        }
        
        // Update stamina text if available
        if (staminaText != null)
        {
            staminaText.text = Mathf.RoundToInt(PlayerMovement.Instance.stamina).ToString() + "/" + 
                              Mathf.RoundToInt(PlayerMovement.Instance.maxStamina).ToString();
        }
        
        // Handle pulsing effect
        if (isPulsing)
        {
            float pulseFactor = Mathf.Lerp(pulseMinScale, pulseMaxScale, 
                               (Mathf.Sin(Time.time * pulseSpeed) + 1) * 0.5f);
            transform.localScale = originalScale * pulseFactor;
        }
    }
}