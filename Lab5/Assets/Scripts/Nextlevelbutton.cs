using UnityEngine;
using UnityEngine.UI;

public class NextLevelButton : MonoBehaviour
{
    void Start()
    {
        // Get the button component
        Button button = GetComponent<Button>();
        if (button != null)
        {
            // Add a listener that calls the NextLevel function on the game manager
            button.onClick.AddListener(OnNextLevelButtonClick);
        }
    }

    void OnNextLevelButtonClick()
    {
        // Find the game manager and call its NextLevel function
        if (BurritoGameManager.Instance != null)
        {
            Debug.Log("Button clicked - calling NextLevel on BurritoGameManager");
            BurritoGameManager.Instance.NextLevel();
        }
        else
        {
            Debug.LogError("Cannot find BurritoGameManager.Instance!");
        }
    }
}