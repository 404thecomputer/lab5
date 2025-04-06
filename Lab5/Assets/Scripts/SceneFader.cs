using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public static SceneFader Instance { get; private set; }
    
    public Image fadePanel;
    public float fadeSpeed = 1.5f;
    
    private bool isFading = false;
    private Canvas fadeCanvas;
    
    private void Awake()
    {
        // Set up singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Make sure we also preserve the canvas that contains the fade panel
            if (fadePanel != null)
            {
                fadeCanvas = fadePanel.GetComponentInParent<Canvas>();
                if (fadeCanvas != null)
                {
                    DontDestroyOnLoad(fadeCanvas.gameObject);
                }
                else
                {
                    Debug.LogError("No Canvas found in the parent hierarchy of the fade panel!");
                }
                
                // Make sure the panel starts transparent and doesn't block raycasts
                Color panelColor = fadePanel.color;
                panelColor.a = 0f;
                fadePanel.color = panelColor;
                fadePanel.raycastTarget = false;
            }
            else
            {
                Debug.LogError("Fade panel reference is missing!");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void FadeToScene(string sceneName)
    {
        if (!isFading && fadePanel != null)
        {
            StartCoroutine(FadeAndLoadScene(sceneName));
        }
        else if (fadePanel == null)
        {
            Debug.LogError("Fade panel is null! Cannot fade to scene: " + sceneName);
            // Just load the scene directly as fallback
            SceneManager.LoadScene(sceneName);
        }
    }
    
    System.Collections.IEnumerator FadeAndLoadScene(string sceneName)
    {
        isFading = true;
        
        // Enable raycast blocking only when we start fading
        fadePanel.raycastTarget = true;
        
        // Fade to black
        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * fadeSpeed;
            if (fadePanel != null)
            {
                Color panelColor = fadePanel.color;
                panelColor.a = alpha;
                fadePanel.color = panelColor;
            }
            else
            {
                break; // Exit if panel was destroyed
            }
            yield return null;
        }
        
        // Load the scene
        SceneManager.LoadScene(sceneName);
        
        // Wait one frame for the scene to load
        yield return null;
        
        // Fade back in
        alpha = 1f;
        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            if (fadePanel != null)
            {
                Color panelColor = fadePanel.color;
                panelColor.a = alpha;
                fadePanel.color = panelColor;
            }
            else
            {
                break; // Exit if panel was destroyed
            }
            yield return null;
        }
        
        // Disable raycast blocking when we're done fading
        if (fadePanel != null)
        {
            fadePanel.raycastTarget = false;
        }
        
        isFading = false;
    }
}