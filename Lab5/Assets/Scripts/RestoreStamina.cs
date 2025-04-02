using UnityEngine;

public class RestoreStamina : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        print("Coffee room entered");
        if (collision.gameObject.tag == "Player") {
            PlayerMovement.Instance.stamina = 100;
        }
    }
}
