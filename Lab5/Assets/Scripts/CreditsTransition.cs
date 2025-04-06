using UnityEngine;

public class CreditsTransition : MonoBehaviour
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
        // Use the scene fader to load the Credits scene
        SceneFader.Instance.FadeToScene("Credits");
    }
}