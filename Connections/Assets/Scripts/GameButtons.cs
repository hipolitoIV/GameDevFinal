using UnityEngine;
using UnityEngine.SceneManagement;

public class GameButtons : MonoBehaviour
{

    // Start game by scene name
    public void StartGameLevel1()
    {
        SceneManager.LoadScene("Level1");
    }

    // Optional: Quit button
    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Game Quit"); // Works in editor only
    }
}
