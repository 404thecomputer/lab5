using UnityEngine;

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
    public void ReactToClick() {
        Initiate.Fade("Tutorial", Color.black, 0.5);
    }
}
