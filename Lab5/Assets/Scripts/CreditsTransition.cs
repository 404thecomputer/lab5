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
    public void ReactToClick() {
        Initiate.Fade("Credits", Color.black, 0.5);
    }
}
