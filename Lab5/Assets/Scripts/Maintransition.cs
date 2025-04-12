using UnityEngine;
using UnityEngine.SceneManagement;

public class MainTransition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure normal time scale when starting a scene
        Time.timeScale = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ReactToClick() 
    {
        // Use SceneManager directly instead of SceneFader
        SceneManager.LoadScene("main");
    }
}