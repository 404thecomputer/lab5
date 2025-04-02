using UnityEngine;

public class StaminaBar : MonoBehaviour
{
    public GameObject mainBar;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float perc = PlayerMovement.Instance.stamina/PlayerMovement.Instance.maxStamina;
        this.transform.localScale = new Vector3(perc, 1, 1);
    }
}
