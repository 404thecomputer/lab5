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
        Debug.Log("BurritoGameManager Start called");
        
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
        
        // Make sure panels are hidden
        Debug.Log("Hiding panels in Start");
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        // Initialize game state
        Debug.Log("Setting initial game values");
        currentLevel = 1;
        burritosCollected = 0;
        remainingTime = timeLimit;
        isLevelActive = true;
        
        // Initialize UI
        UpdateUI();
        
        // Start the first level
        Debug.Log("Starting first level from Start");
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
        // Debug what we're updating to
        Debug.Log($"UpdateUI - Level: {currentLevel}, Time: {remainingTime}, Burritos: {burritosCollected}/{GetBurritosRequiredForLevel(currentLevel)}");
        
        // Update level text
        if (levelText != null)
        {
            levelText.text = "Level: " + currentLevel;
            Debug.Log($"Set levelText to: {levelText.text}");
        }
        else
        {
            Debug.LogWarning("levelText is null in UpdateUI");
        }
        
        // Update timer text
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
            Debug.Log($"Set timerText to: {timerText.text}");
        }
        else
        {
            Debug.LogWarning("timerText is null in UpdateUI");
        }
        
        // Update burrito count text
        if (burritoCountText != null)
        {
            burritoCountText.text = "Burritos: " + burritosCollected + " / " + GetBurritosRequiredForLevel(currentLevel);
            Debug.Log($"Set burritoCountText to: {burritoCountText.text}");
        }
        else
        {
            Debug.LogWarning("burritoCountText is null in UpdateUI");
        }
        
        // Update stamina text
        if (staminaText != null && PlayerMovement.Instance != null)
        {
            staminaText.text = "Stamina: " + Mathf.RoundToInt(PlayerMovement.Instance.stamina);
            Debug.Log($"Set staminaText to: {staminaText.text}");
        }
        else if (staminaText == null)
        {
            Debug.LogWarning("staminaText is null in UpdateUI");
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
        // Hide panels
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        
        // Clear ALL burritos before changing scenes
        GameObject[] allBurritos = GameObject.FindGameObjectsWithTag("Burrito");
        foreach (GameObject burrito in allBurritos)
        {
            Destroy(burrito);
        }
        
        // Clear the active burritos list
        activeBurritos.Clear();
        
        // Store the CURRENT level before clearing references
        int nextLevelNumber = currentLevel + 1;
        Debug.Log("Going to level: " + nextLevelNumber);
        
        // IMPORTANT: Clear all UI references so they'll be found in the new scene
        levelText = null;
        timerText = null;
        burritoCountText = null;
        staminaText = null;
        levelCompletePanel = null;
        gameOverPanel = null;
        
        // Set the current level to the next level BEFORE loading the scene
        currentLevel = nextLevelNumber;
        
        // Load the appropriate scene
        string nextSceneName = "Level" + nextLevelNumber;
        Debug.Log("Loading scene: " + nextSceneName + " for level: " + currentLevel);
        
        SceneManager.LoadScene(nextSceneName);
        
        // Wait for scene to load, then start new level
        StartCoroutine(SetupNewLevel());
    }
    
    private void ForceUpdateLevelAndTime()
    {
        // Debug what we're trying to set
        Debug.Log($"Forcing update for Level: {currentLevel}, Time: {remainingTime}");
        
        // Find text elements more aggressively if they're null
        if (levelText == null || timerText == null)
        {
            // Find ALL UI text elements
            TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
            
            foreach (TextMeshProUGUI text in allTexts)
            {
                // Log each text element we find
                Debug.Log($"Found text: {text.name} with text: {text.text}");
                
                // Try to identify by contents and name
                if (text.name.ToLower().Contains("level"))
                {
                    levelText = text;
                    Debug.Log($"Set levelText to {text.name}");
                }
                else if (text.name.ToLower().Contains("time") || text.name.ToLower().Contains("timer"))
                {
                    timerText = text;
                    Debug.Log($"Set timerText to {text.name}");
                }
            }
        }
        
        // Directly set values regardless of previous content
        if (levelText != null)
        {
            levelText.text = "Level: " + currentLevel;
            Debug.Log($"Updated levelText to: {levelText.text}");
        }
        else
        {
            Debug.LogError("Still couldn't find levelText!");
        }
        
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = string.Format("Time: {0:00}:{1:00}", minutes, seconds);
            Debug.Log($"Updated timerText to: {timerText.text}");
        }
        else
        {
            Debug.LogError("Still couldn't find timerText!");
        }
    }

    private IEnumerator SetupNewLevel()
    {
        // Wait for TWO frames to ensure the scene is fully loaded
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        
        Debug.Log("Setting up new level: " + currentLevel);
        
        // Try to find the exact "Level Text" element specifically
        GameObject levelTextObj = GameObject.Find("Level Text");
        if (levelTextObj != null)
        {
            levelText = levelTextObj.GetComponent<TextMeshProUGUI>();
            Debug.Log("Found Level Text directly: " + (levelText != null));
        }
        
        // Find UI elements in the new scene - more robust approach for other elements
        TextMeshProUGUI[] allTexts = FindObjectsOfType<TextMeshProUGUI>(true);
        Debug.Log("Found " + allTexts.Length + " TextMeshProUGUI components");
        
        // Try to find by name (case insensitive)
        foreach (TextMeshProUGUI text in allTexts)
        {
            string textName = text.gameObject.name.ToLower();
            
            if (levelText == null && textName.Contains("level") && !textName.Contains("complete"))
            {
                levelText = text;
                Debug.Log("Found LevelText: " + text.name);
            }
            else if (textName.Contains("timer") || textName.Contains("time"))
            {
                timerText = text;
                Debug.Log("Found TimerText: " + text.name);
            }
            else if (textName.Contains("burrito") && textName.Contains("count"))
            {
                burritoCountText = text;
                Debug.Log("Found BurritoCountText: " + text.name);
            }
            else if (textName.Contains("stamina") && !textName.Contains("bar"))
            {
                staminaText = text;
                Debug.Log("Found StaminaText: " + text.name);
            }
        }
        
        // Find panels by searching all GameObjects with specific names
        GameObject[] allObjects = FindObjectsOfType<GameObject>(true);
        foreach (GameObject obj in allObjects)
        {
            string objName = obj.name.ToLower();
            
            if (objName.Contains("levelcompletepanel"))
            {
                levelCompletePanel = obj;
                Debug.Log("Found LevelCompletePanel: " + obj.name);
            }
            else if (objName.Contains("gameoverpanel"))
            {
                gameOverPanel = obj;
                Debug.Log("Found GameOverPanel: " + obj.name);
            }
        }
        
        // Make sure panels are hidden
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
            
        // Start the new level
        burritosCollected = 0;
        remainingTime = timeLimit; // Reset to full time
        isLevelActive = true;
        spawnTimer = 0.1f;
        
        // Force update level text specifically
        if (levelText != null)
        {
            levelText.text = "Level: " + currentLevel;
            Debug.Log("Force set level text to: " + levelText.text);
        }
        
        // Update UI using the standard method
        UpdateUI();
        
        // Log final UI reference status
        Debug.Log("UI references after setup: LevelText=" + (levelText != null) + 
                ", TimerText=" + (timerText != null) + 
                ", BurritoCountText=" + (burritoCountText != null) + 
                ", StaminaText=" + (staminaText != null));
    }

    // Helper method to find child objects recursively by name
    private Transform FindChildRecursively(Transform parent, string name)
    {
        // Check direct children first
        Transform child = parent.Find(name);
        if (child != null)
            return child;
        
        // Check children of children
        for (int i = 0; i < parent.childCount; i++)
        {
            child = FindChildRecursively(parent.GetChild(i), name);
            if (child != null)
                return child;
        }
        
        return null;
    }

    public void ShowVictoryPanel()
    {
        // Create a victory UI or modify existing one
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(true);
            
            // Find and modify the title to show victory
            TextMeshProUGUI[] texts = levelCompletePanel.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (TextMeshProUGUI text in texts)
            {
                if (text.name.Contains("Title"))
                {
                    text.text = "Victory! All Levels Complete!";
                    text.color = Color.yellow; // Make it fancy
                }
            }
            
            // Play victory sound
            if (audioSource != null && levelCompleteSound != null)
            {
                audioSource.PlayOneShot(levelCompleteSound, 1.5f); // Play at higher volume
            }
        }
        else
        {
            Debug.Log("Victory! All levels complete!");
            QuitToMainMenu();
        }
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
        // Set our instance to null to allow a new one to be created
        Instance = null;
        
        // Find and destroy any player objects
        if (PlayerMovement.Instance != null)
        {
            Destroy(PlayerMovement.Instance.gameObject);
        }
        
        // Destroy this game manager
        Destroy(gameObject);
        
        // Load the main scene
        SceneManager.LoadScene("Main");
    }
    private IEnumerator ResetAfterSceneLoad()
    {
        // Wait for the end of the frame to ensure scene is loaded
        yield return new WaitForEndOfFrame();
        
        // Make absolutely sure panels are disabled after scene load
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        
        // Reset the timer again
        remainingTime = timeLimit;
        
        // Update the UI
        UpdateUI();
        
        // Start the first level
        StartLevel(1);
    }
    
    public void QuitToMainMenu()
    {
        // Deactivate any active panels
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
        
        // Clear any active burritos
        ClearAllBurritos();
        
        // Load main menu scene
        SceneManager.LoadScene("Title");
    }
}