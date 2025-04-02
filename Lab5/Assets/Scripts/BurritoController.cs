using UnityEngine;

public class BurritoController : MonoBehaviour
{
    AudioSource audioSource;
    float delay = 0.5f;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            audioSource = GetComponent<AudioSource>();

            audioSource.Play();

            PlayerMovement.Instance.stamina += 30;
            
            GameObject.Destroy(gameObject, delay);

        }
    }








    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
