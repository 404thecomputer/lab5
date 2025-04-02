using UnityEngine;

public class CameraFollows : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(PlayerMovement.Instance.transform.position.x, PlayerMovement.Instance.transform.position.y, -20);
    }
}
