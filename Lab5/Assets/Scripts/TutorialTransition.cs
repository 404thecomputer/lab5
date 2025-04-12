using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialTransition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public void ReactToClick() 
    {
        // Use the scene fader to load the Tutorial scene
        SceneManager.LoadScene("Tutorial");
    }
}