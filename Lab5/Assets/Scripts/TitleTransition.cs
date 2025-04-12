using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleTransition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Time.timeScale = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ReactToClick() 
    {
        // Use the scene fader to load the Title scene
        SceneManager.LoadScene("Title");
    }
}