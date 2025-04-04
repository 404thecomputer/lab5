using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class BurritoGameManager : MonoBehaviour
{
    public static BurritoGameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public int currentLevel = 1;
    public int maxLevel = 3;
    public int burritosToCollect = 10;
    public float timeLimit = 60f;
    
    [Header("Burrito Spawning")]
    public GameObject burritoPrefab;
    public float minSpawnDistance = 5f;
    public float maxSpawnDistance = 15f;
    public int maxBurritosActive = 3;
    public float spawnInterval = 2f;
    
    [Header("Difficulty Settings")]
    public float[] speedMultipliers = { 1f, 1.5f, 2.2f };
    public float[] spawnDistanceMultipliers = { 1f, 1.3f, 1.6f };
    public float[] spawnIntervalDividers = { 1f, 1.2f, 1.5f };
    public int[] burritosRequiredPerLevel = { 10, 15, 20 };
    
    [Header("UI References")]
    public TextMeshProUGUI levelText; // Changed to TextMeshProUGUI
    public TextMeshProUGUI timerText; // Changed to TextMeshProUGUI
    public TextMeshProUGUI burritoCountText; // Changed to TextMeshProUGUI
    public TextMeshProUGUI staminaText; // Changed to TextMeshProUGUI
    public GameObject levelCompletePanel;
    public GameObject gameOverPanel;
    
    [Header("Audio")]
    public AudioClip levelCompleteSound;
    public AudioClip gameOverSound;
    public AudioClip backgroundMusic;
    
    // Internal variables
    private int burritosCollected = 0;
    private float remainingTime;
    private List<GameObject> activeBurritos = new List<GameObject>();
    private float spawnTimer;
    private bool isLevelActive = false;
    private AudioSource audioSource;
    
    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Setup audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Play background music
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.Play();
        }
        
        // Initialize UI
        UpdateUI();
        
        // Hide panels
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Start the first level
        StartLevel(currentLevel);
    }
    
    void Update()
    {
        if (!isLevelActive)
            return;
            
        // Update timer
        remainingTime -= Time.deltaTime;
        UpdateUI();
        
        // Check for game over condition
        if (remainingTime <= 0)
        {
            GameOver();
            return;
        }
        
        // Spawn burritos if needed
        ManageBurritoSpawning();
        
        // Check for level completion
        if (burritosCollected >= GetBurritosRequiredForLevel(currentLevel))
        {
            LevelComplete();
        }
    }
    
    void ManageBurritoSpawning()
    {
        // Remove null references (collected burritos)
        activeBurritos.RemoveAll(item => item == null);
        
        // Spawn new burritos if needed
        if (activeBurritos.Count < maxBurritosActive)
        {
            spawnTimer -= Time.deltaTime;
            
            if (spawnTimer <= 0)
            {
                SpawnBurrito();
                spawnTimer = spawnInterval / GetSpawnIntervalDivider(currentLevel);
            }
        }
    }
    
    void SpawnBurrito()
    {
        if (burritoPrefab == null || PlayerMovement.Instance == null)
            return;
            
        // Get player position
        Vector3 playerPos = PlayerMovement.Instance.transform.position;
        
        // Calculate random spawn position around player
        float spawnDistance = Random.Range(
            minSpawnDistance * GetSpawnDistanceMultiplier(currentLevel), 
            maxSpawnDistance * GetSpawnDistanceMultiplier(currentLevel)
        );
        Vector2 randomDirection = Random.insideUnitCircle.normalized;
        Vector3 spawnPos = playerPos + new Vector3(randomDirection.x, randomDirection.y, 0) * spawnDistance;
        
        // Instantiate burrito
        GameObject burrito = Instantiate(burritoPrefab, spawnPos, Quaternion.identity);
        
        // Set speed based on current level
        BurritoController controller = burrito.GetComponent<BurritoController>();
        if (controller != null)
        {
            controller.moveSpeed *= GetSpeedMultiplier(currentLevel);
            
            // Adjust other parameters based on level
            if (currentLevel > 1)
            {
                controller.changeDirectionInterval /= 1 + ((currentLevel - 1) * 0.2f);
            }
        }
        
        // Add to active list
        activeBurritos.Add(burrito);
    }
    
    void UpdateUI()
    {
        // Update level text
        if (levelText != null)
        {
            levelText.text = "Level: " + currentLevel;
        }
        
        // Update timer text
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
        }
        
        // Update burrito count text
        if (burritoCountText != null)
        {
            burritoCountText.text = "Burritos: " + burritosCollected + " / " + GetBurritosRequiredForLevel(currentLevel);
        }
        
        // Update stamina text
        if (staminaText != null && PlayerMovement.Instance != null)
        {
            staminaText.text = "Stamina: " + Mathf.RoundToInt(PlayerMovement.Instance.stamina);
        }
    }
    
    public void AddBurritoCollected()
    {
        burritosCollected++;
        UpdateUI();
    }
    
    public void StartLevel(int level)
    {
        // Set level parameters
        currentLevel = Mathf.Clamp(level, 1, maxLevel);
        burritosCollected = 0;
        remainingTime = timeLimit;
        isLevelActive = true;
        spawnTimer = 0.1f; // Spawn first burrito quickly
        
        // Clear any existing burritos
        ClearAllBurritos();
        
        // Update UI
        UpdateUI();
        
        // Hide panels
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }
    
    void LevelComplete()
    {
        isLevelActive = false;
        
        // Play completion sound
        if (audioSource != null && levelCompleteSound != null)
        {
            audioSource.PlayOneShot(levelCompleteSound);
        }
        
        // Show completion panel
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            
            // Optionally update stats on the level complete panel
            UpdateLevelCompleteStats();
        }
        
        // We'll advance to next level when the Next Level button is clicked
        // No need for LoadSceneAfterDelay anymore
    }

    void UpdateLevelCompleteStats()
    {
        // Find TextMeshPro components in the level complete panel
        TextMeshProUGUI[] texts = levelCompletePanel.GetComponentsInChildren<TextMeshProUGUI>();
        
        foreach (TextMeshProUGUI text in texts)
        {
            if (text.name.Contains("BurritosCollected"))
            {
                text.text = "Burritos Collected: " + burritosCollected + "/" + GetBurritosRequiredForLevel(currentLevel);
            }
            else if (text.name.Contains("TimeRemaining"))
            {
                int minutes = Mathf.FloorToInt(remainingTime / 60);
                int seconds = Mathf.FloorToInt(remainingTime % 60);
                text.text = string.Format("Time Remaining: {0:00}:{1:00}", minutes, seconds);
            }
        }
    }
    
    void GameOver()
    {
        isLevelActive = false;
        
        // Play game over sound
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }
        
        // Show game over panel instead of loading a scene
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            
            // Optionally update stats on the game over panel
            UpdateGameOverStats();
        }
        else
        {
            Debug.LogError("Game Over Panel is not assigned!");
        }
    }

    void UpdateGameOverStats()
    {
        // Find TextMeshPro components in the game over panel
        TextMeshProUGUI[] texts = gameOverPanel.GetComponentsInChildren<TextMeshProUGUI>();
        
        foreach (TextMeshProUGUI text in texts)
        {
            // Update specific texts based on their names or tags
            if (text.name.Contains("BurritosCollected"))
            {
                text.text = "Burritos Collected: " + burritosCollected;
            }
            else if (text.name.Contains("LevelReached"))
            {
                text.text = "Level Reached: " + currentLevel;
            }
        }
    }
    
    void ClearAllBurritos()
    {
        foreach (GameObject burrito in activeBurritos)
        {
            if (burrito != null)
            {
                Destroy(burrito);
            }
        }
        
        activeBurritos.Clear();
    }
    
    System.Collections.IEnumerator LoadNextLevel()
    {
        yield return new WaitForSeconds(2f);
        currentLevel++;
        SceneManager.LoadScene("Level" + currentLevel);
        yield return new WaitForSeconds(0.5f);
        StartLevel(currentLevel);
    }
    
    // Add these methods for the buttons on your panels:
    public void NextLevel()
    {
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        
        // Increment level
        currentLevel++;
        
        // Check if we've completed all levels
        if (currentLevel > maxLevel)
        {
            // Victory! Show victory message or panel
            ShowVictoryPanel();
        }
        else
        {
            // Start the next level
            StartLevel(currentLevel);
        }
    }

    public void ShowVictoryPanel()
    {
        Debug.Log("Victory! All levels complete!");
        QuitToMainMenu();
    }
    
    // Helper methods to get level-specific difficulty values
    private float GetSpeedMultiplier(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, speedMultipliers.Length - 1);
        return speedMultipliers[index];
    }
    
    private float GetSpawnDistanceMultiplier(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, spawnDistanceMultipliers.Length - 1);
        return spawnDistanceMultipliers[index];
    }
    
    private float GetSpawnIntervalDivider(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, spawnIntervalDividers.Length - 1);
        return spawnIntervalDividers[index];
    }
    
    private int GetBurritosRequiredForLevel(int level)
    {
        int index = Mathf.Clamp(level - 1, 0, burritosRequiredPerLevel.Length - 1);
        return burritosRequiredPerLevel[index];
    }
    
    // Public methods for UI buttons
    public void RestartGame()
    {
        // Reset player
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.ResetPlayer();
        }
        
        // Load first level
        SceneManager.LoadScene("Level1");
        
        // Start fresh
        currentLevel = 1;
        StartLevel(1);
    }
    
    public void QuitToMainMenu()
    {
        ClearAllBurritos();
        SceneManager.LoadScene("MainMenu");
    }
}